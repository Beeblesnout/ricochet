namespace Popcron.Networking.Shared
{
    public enum PipeMessageType : ushort
    {
        NetworkReceiveEvent,
        NetworkErrorEvent,
        PeerConnectedEvent,
        PeerDisconnectedEvent,
        NetworkReceiveUnconnectedEvent,
        NetworkLatencyUpdateEvent,
        ServerInitializedEvent,
        SendNetworkDataRequest,
        ServerInitializedFailedEvent,
        CloseConnectionRequest,
        DisconnectRequest,
        ConnectRequest,
        InitializeServerRequest,
        SendNetworkDataToConnectionRequest,
        ShutdownApplicationRequest,
        ServerClosedVoluntarilyEvent,
        ClientClosedVoluntarilyEvents,
        EditorPlayModeStateChangedRequest,
        EditorCompileStartedRequest,
        EditorFinishedCompilingRequest,
        ApplicationStartedRequest,
        ConsoleMessageRequest
    }
}
