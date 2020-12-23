namespace C8F2740A.NetworkNode.RemoteApi.Nuget.Trace
{
    public class LocalConsolePoint
    {
        private readonly IExternalConsolePoint _externalConsolePoint;
        
        public LocalConsolePoint(IExternalConsolePoint externalConsolePoint)
        {
            _externalConsolePoint = externalConsolePoint;
            _externalConsolePoint.SetLocalStreaming(false);
            _externalConsolePoint.SetRemoteStreaming(true);
        }
    }
}