using System;

namespace skinchanger_loader.SDK.Api.Structs
{
    [Serializable]
    internal class ServerResponse
    {
        public ServerCodes status;
        public int last_update;
        public int all;
        public int online;
    }
}