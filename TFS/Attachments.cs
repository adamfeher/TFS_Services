using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace RainforestExcavator.Services
{
    public partial class TFS
    {
        /// <summary>
        /// Accepts a WorkItem and returns the Attachment matching the given filename.
        /// </summary>
        public Attachment GetAttachment(WorkItem workItem, string fileName)
        {
            int desiredIndex = GetAttachmentIndexByName(workItem, fileName);
            if (desiredIndex == -1) { throw new Core.ServiceException(Core.Error.TFSAttachmentDoesNotExist, workItem.Id.ToString()); }
            return workItem.Attachments[desiredIndex];
        }

        /// <summary>
        /// Searches the attachments of a WorkItem for the specified filename
        /// </summary>
        /// <returns>The index of the first attachment with given filename, else -1 if not found.</returns>
        public int GetAttachmentIndexByName(WorkItem workItem, string fileName)
        {
            for(int i = 0; i < workItem.Attachments.Count; i++)
            {
                if (workItem.Attachments[i].Name.Equals(fileName)) { return i; }
            }
            return -1;
        }
    }
}
