using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using Jakkes.WebSockets.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperBomberLanServer.Network.Packets;
using WebSocketState = Jakkes.WebSockets.Server.WebSocketState;

namespace SuperBomberLanServer.Network
{
    public class Server : WebSocketServer
    {
        private List<Connection> connectedClients = new List<Connection>();

        public Server(int port) : base(port)
        {
            base.ClientConnected += ServerOnClientConnected;
            base.StateChanged += ServerOnStateChanged;
        }

#region Event handlers

        private void ServerOnStateChanged(WebSocketServer source, WebSocketServerState state)
        {
            Console.WriteLine("Server state changed: " + state);
        }

        private void ServerOnClientConnected(WebSocketServer source, Connection conn)
        {
            Console.WriteLine("Client connected: " + conn.ID);
            connectedClients.Add(conn);
            conn.MessageReceived += ConnOnMessageReceived;
            conn.StateChanged += ConnOnStateChanged;
        }

        private void ConnOnStateChanged(Connection source, WebSocketState state)
        {
            Console.WriteLine("Client " + source.ID + " state changed: " + state);
            if (state == WebSocketState.Closed)
            {
                connectedClients.Remove(source);
            }
        }

        private void ConnOnMessageReceived(Connection source, string data)
        {
            Console.WriteLine("Client" + source.ID + " sent data: " + data);
            var decodedData = JsonConvert.DeserializeObject<Packet>(data);
            switch (decodedData.OpCode)
            {
                case 1:
                    Packet packetToSend = new Packet
                    {
                        OpCode = 2,
                        Data = Program.Map
                    };

                    var result = JsonConvert.SerializeObject(packetToSend);
                    source.Send(result);

                    if (connectedClients.FirstOrDefault()?.ID == source.ID)
                    {
                        packetToSend = new Packet
                        {
                            OpCode = 3
                        };

                        result = JsonConvert.SerializeObject(packetToSend, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
                        source.Send(result);
                    }
                    break;
            }
        }

        #endregion
    }
}
