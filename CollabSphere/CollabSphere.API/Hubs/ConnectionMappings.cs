using System.Collections.Concurrent;

namespace CollabSphere.API.Hubs
{
    public class ConnectionMappings
    {
        private static readonly ConcurrentDictionary<string, TeamWorkConnectionInfo> _teamBoardConnections = new();

        private static readonly ConcurrentDictionary<string, ChatHubConnectionInfo> _chatConnections = new();


        public ConcurrentDictionary<string, TeamWorkConnectionInfo> TeamBoardMapping => _teamBoardConnections;

        public ConcurrentDictionary<string, ChatHubConnectionInfo> ChatHubMapping => _chatConnections;
    }
}
