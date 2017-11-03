using System.IO;

namespace RainforestExcavator.Core.Data
{
    public enum StateEnum
    {
        Start,          // no stored files that have not yet been uploaded
        Ready,          // Aggregate completed, files ready to upload
        Incomplete      // the tool is still processing or failed to complete a previous operation
    }

    public partial class Aggregator
    {
        public static void UpdateAggregateState(StateEnum state)
        {
            using (StreamWriter fileStream = new StreamWriter($"{Core.Filepaths.AggrStateFile}", false))
            {
                fileStream.Write($"{(int)state}");
            }
        }
    }
}
