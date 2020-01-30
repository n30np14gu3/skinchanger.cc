using System;

namespace skinchanger_loader.SDK.Api.Structs
{
    [Serializable]
    public class ServerRequest<T>
    {
        public string type;
        public T data;
    }
}