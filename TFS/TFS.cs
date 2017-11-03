namespace RainforestExcavator.Services
{
    public partial class TFS
    {
        public Session Session { get; }
        public Operations Operations { get; }
        public TFS(Operations operations, Session session)
        {
            this.Operations = operations;
            this.Session = session;
        } 
    }
}
