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
using SuperBomberLanServer.Local;
using SuperBomberLanServer.Local.Entities;
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
            if (Program.GameState == GameState.StartingUp)
            {
                Console.WriteLine("Client connected: " + conn.ID);
                connectedClients.Add(conn);
                conn.MessageReceived += ConnOnMessageReceived;
                conn.StateChanged += ConnOnStateChanged;
            }
            else
            {
                Console.WriteLine("Rejected Client: " + conn.ID + " as game is already running");
                conn.Close();
            }
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
            Console.WriteLine("Client " + source.ID + " sent data: " + data);
            var decodedData = JsonConvert.DeserializeObject<Packet>(data);
            Packet packetToSend;
            switch (decodedData.OpCode)
            {
                case 1:
                    packetToSend = new Packet
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

                case 4:
                    if (connectedClients.FirstOrDefault()?.ID == source.ID && Program.GameState == GameState.StartingUp)
                    {
                        ConsoleColor originalColour = Console.ForegroundColor;
                        Console.ForegroundColor  = ConsoleColor.Green;
                        Console.WriteLine("Admin has triggered game start");
                        Console.ForegroundColor = originalColour;

                        Program.GameState = GameState.Running;

                        List<Tuple<int, int>> freeLocations = new List<Tuple<int, int>>();
                        for (int i = 0; i < Program.Map.Width; i++)
                        {
                            for (int j = 0; j < Program.Map.Height; j++)
                            {
                                if (!Program.Map.Tiles[i, j].Entities.Any())
                                {
                                    freeLocations.Add(new Tuple<int, int>(i, j));
                                }
                            }
                        }

                        Random random = new Random();
                        foreach (Connection client in connectedClients)
                        {
                            int index = random.Next(0, freeLocations.Count - 1);

                            Program.Map.Tiles[freeLocations[index].Item1, freeLocations[index].Item2].Entities.Add(new PlayerEntity { ConnectionId = client.ID });

                            JObject jobject = new JObject();
                            jobject.Add("x", freeLocations[index].Item1);
                            jobject.Add("y", freeLocations[index].Item2);

                            packetToSend = new Packet
                            {
                                OpCode = 4,
                                Data = jobject
                            };

                            result = JsonConvert.SerializeObject(packetToSend, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                            client.Send(result);

                            freeLocations.RemoveAt(index);
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
