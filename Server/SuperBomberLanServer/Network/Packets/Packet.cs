using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SuperBomberLanServer.Network.Packets
{
    public class Packet
    {
        [JsonProperty("op")]
        public int OpCode { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
