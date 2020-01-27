using System;

namespace skinchanger_loader.SDK.Api.Structs
{
    [Serializable]
    internal enum ServerCodes : uint
    {
        API_CODE_OK_PING = 1,
        API_CODE_ERROR_PING,

        API_CODE_PROHIBDED = 200,
        API_CODE_OK = 100,
    }
}