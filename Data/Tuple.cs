namespace RainforestExcavator.Core.Data
{
    /// <summary>
    /// Custom Tuple variant that is not readonly once instantiated.
    /// </summary>
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(Tuple<T1, T2> tuple)
        {
            this.Item1 = tuple.Item1;
            this.Item2 = tuple.Item2;
        }
        public Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }
}
