using UnityEngine;

namespace TextBuddy.Core
{

    public enum ConnectError
    {
        None = 0,
        Network = 1,          // no HTTP status (DNS/socket), generic transport
        Timeout = 2,          // best-effort detect via error text
        Http4xx = 3,
        Http5xx = 4,
        JsonParse = 5,
        SignatureValidation = 6,
        Unknown = 99
    }
}