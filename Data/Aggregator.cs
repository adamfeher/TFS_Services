using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RainforestExcavator.Core.Data
{
    /// <summary>
    /// The Aggregator handles the dataseeding portion of the tool in its entirety.
    /// </summary>
    public partial class Aggregator
    {
        // All filepaths used below should be absolute, useful paths have been defined at Core/FilePaths.cs

        // The task that handles the parsing + output of downloaded dataseed files, initialized in constructor
        public Task<List<ServiceException>> processingTask;

        // Collection of download tasks, each download has its own task
        private BlockingCollection<Task<string>> downloadQueue;

        private bool killSwitch;

        // Holds all the required info for data generation as needed for the aggregation process
        private DataProfile dataProfile;

        // Holds the ProjectCode (int) and shortened ProjectName (string)
        private KeyValuePair<int, string> projectInfoPair;

        // Counters used for creating unique GenericUser data and assigning in place where unique ${*} marker is used, respectively
        private int genericPoolCounter = 1;
        private int uniqueCounter = 1;

        public Aggregator(KeyValuePair<int, string> projectInfo)
        {
            this.downloadQueue = new BlockingCollection<Task<string>>();
            this.killSwitch = false;
            this.projectInfoPair = projectInfo;
            this.dataProfile = null;
            this.processingTask = AggregateData();
        }

        /// <summary>
        /// Adds a new file to be worked on to the aggregator pool by adding its respective task and filepath
        /// </summary>  
        public void AddDownload(Task<string> task)
        {
            downloadQueue.Add(task);
        }
        /// <summary>
        /// Signals to the aggregator to finish remaining work, indicating there will be no more incoming tasks.
        /// </summary>
        public void FinishAll()
        {
            this.killSwitch = true;
        }
        /// <summary>
        /// The background task which collects all of the completed downloads, parses them and created the correct files to upload.
        /// </summary>
        /// <param name="verbOutStream"></param>
        /// <param name="genericOutStream"></param>
        private Task<List<ServiceException>> AggregateData()
        {
            return Task.Run(async delegate
            {
                List<ServiceException> failures = new List<ServiceException>();

                // initialize the genericUserPool header
                using (StreamWriter genericOutStream = new StreamWriter(Filepaths.GenericOutFile, false))
                {
                    genericOutStream.WriteLine("username,email,password,firstname,lastname");
                }

                // wait for downloads to finish and queue up until all completed
                while ((!this.killSwitch) || this.downloadQueue.Any())
                {
                    // ensure download task is completed, get matching filepath
                    string filepath = await this.downloadQueue.Take();

                    // retrieve test case id from filepath
                    int lastBackslashIndex = filepath.LastIndexOf('\\');

                    // extract RFID from filename suffix consisting of "{rfid}_{tfsid}"
                    string fileNameSuffix = filepath.Substring(lastBackslashIndex, filepath.LastIndexOf('_') - lastBackslashIndex);
                    string[] fileNameSplit = fileNameSuffix.Split('_');
                    string outFilePath = $"{Filepaths.UploadDir}{fileNameSuffix}.csv";

                    using (StreamReader inStream = new StreamReader(filepath))
                    {
                        try
                        {
                            // parse the header section => resets data profile in prep for new dataseed file
                            if (ParseHeader(inStream))
                            {
                                // header existed. proceed with processing
                                // read the tab var section => pulls out the raw values into the data profile
                                ReadTabVars(inStream);
                                WriteTabVarHeader(outFilePath);

                                // always need to loops 10 times for RF duplication process
                                for (int dupNum = 0; dupNum < 10; dupNum++)
                                {
                                    // create a new InjectedHeaderDict for each user
                                    this.dataProfile.CreateUniqueInjectedCopy(ref this.uniqueCounter);

                                    // generate the user data for this iteration, result available in dataProfile.UserDataDict
                                    if (this.dataProfile.UserTypeIsGeneric)
                                    {
                                        GenerateGenericUser();
                                        genericPoolCounter++;
                                    }
                                    else { GenerateSpecificUser(fileNameSplit[1]); }

                                    // Write out the tab variables to file
                                    WriteTabVars(outFilePath);
                                    ParseCommands(inStream, fileNameSplit[1]);
                                }
                            }
                        }
                        catch (ServiceException e)
                        {
                            // add failure with affected TFSID attached
                            e.AffectedId = fileNameSplit[1];
                            failures.Add(e);
                        }
                    }

                    // remove the dataseed file once complete
                    File.Delete(filepath);
                    Thread.Sleep(1);
                }
                return failures;
            });    
        }
    }
}
