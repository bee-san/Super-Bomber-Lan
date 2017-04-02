using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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

        private Timer timer;

        private Dictionary<BombEntity, Tuple<Int32, Int32, DateTime>> PlacedBombs = new Dictionary<BombEntity, Tuple<Int32, Int32, DateTime>>();

        public Server(int port) : base(port)
        {
            base.ClientConnected += ServerOnClientConnected;
            base.StateChanged += ServerOnStateChanged;
            timer = new Timer(TimerCallback, null, 0, 100);
        }

#region Event handlers

        private void TimerCallback(object state)
        {
            List<Tuple<BombEntity, Int32, Int32>> bombsToExplode = new List<Tuple<BombEntity, Int32, Int32>>();
            foreach (var bomb in PlacedBombs)
            {
                if (DateTime.UtcNow.Subtract(bomb.Value.Item3).TotalMilliseconds > 1000)
                {
                    bombsToExplode.Add(new Tuple<BombEntity, int, int>(bomb.Key, bomb.Value.Item1, bomb.Value.Item2));
                }
            }

            foreach (Tuple<BombEntity, int, int> bombEntity in bombsToExplode)
            {
                PlacedBombs.Remove(bombEntity.Item1);

                List<Tuple<Int32, Int32>> blocksToBreak = new List<Tuple<int, int>>();
                List<PlayerEntity> playersToKill = new List<PlayerEntity>();

                //Simulate explosion right
                for (int i = 1; i <= bombEntity.Item1.Radius; i++)
                {
                    if (bombEntity.Item2 + i > Program.Map.Width - 1) break;

                    var entities = Program.Map.Tiles[bombEntity.Item2 + i, bombEntity.Item3].Entities;
                    if (entities.Any(x => x.EntityType == EntityType.Box))
                    {
                        blocksToBreak.Add(new Tuple<int, int>(bombEntity.Item2 + i, bombEntity.Item3));
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Wall))
                    {
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Player))
                    {
                        playersToKill.Add(entities.First(x => x.EntityType == EntityType.Player) as PlayerEntity);
                    }
                }

                //Simulate explosion down
                for (int i = 1; i <= bombEntity.Item1.Radius; i++)
                {
                    if (bombEntity.Item2 + i > Program.Map.Height - 1) break;

                    var entities = Program.Map.Tiles[bombEntity.Item2, bombEntity.Item3 + i].Entities;
                    if (entities.Any(x => x.EntityType == EntityType.Box))
                    {
                        blocksToBreak.Add(new Tuple<int, int>(bombEntity.Item2, bombEntity.Item3 + i));
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Wall))
                    {
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Player))
                    {
                        playersToKill.Add(entities.First(x => x.EntityType == EntityType.Player) as PlayerEntity);
                    }
                }

                //Simulate explosion left
                for (int i = 1; i <= bombEntity.Item1.Radius; i++)
                {
                    if (bombEntity.Item2 - i < 0) break;

                    var entities = Program.Map.Tiles[bombEntity.Item2 - i, bombEntity.Item3].Entities;
                    if (entities.Any(x => x.EntityType == EntityType.Box))
                    {
                        blocksToBreak.Add(new Tuple<int, int>(bombEntity.Item2 - i, bombEntity.Item3));
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Wall))
                    {
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Player))
                    {
                        playersToKill.Add(entities.First(x => x.EntityType == EntityType.Player) as PlayerEntity);
                    }
                }

                //Simulate explosion up
                for (int i = 1; i <= bombEntity.Item1.Radius; i++)
                {
                    if (bombEntity.Item2 - i < 0) break;

                    var entities = Program.Map.Tiles[bombEntity.Item2, bombEntity.Item3 - i].Entities;
                    if (entities.Any(x => x.EntityType == EntityType.Box))
                    {
                        blocksToBreak.Add(new Tuple<int, int>(bombEntity.Item2, bombEntity.Item3 - i));
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Wall))
                    {
                        break;
                    }

                    if (entities.Any(x => x.EntityType == EntityType.Player))
                    {
                        playersToKill.Add(entities.First(x => x.EntityType == EntityType.Player) as PlayerEntity);
                    }
                }

                if (blocksToBreak.Any())
                {
                    ArrayList array = new ArrayList();

                    foreach (var blockToBreak in blocksToBreak)
                    {
                        array.Add(new { x = blockToBreak.Item1, y = blockToBreak.Item2 });
                    }

                    Packet packet = new Packet
                    {
                        OpCode = 7,
                        Data = array
                    };

                    var value = JsonConvert.SerializeObject(packet);

                    Program.Server.Broadcast(value);
                }

                if (playersToKill.Any())
                {
                    ArrayList array = new ArrayList();

                    foreach (var playerToKill in playersToKill)
                    {
                        array.Add(new { id = playerToKill.ConnectionId });
                    }

                    Packet packet = new Packet
                    {
                        OpCode = 8,
                        Data = array
                    };

                    var value = JsonConvert.SerializeObject(packet);

                    Program.Server.Broadcast(value);
                }
            }
        }

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

            var test = new { map = Program.Map, id = source.ID };

            switch (decodedData.OpCode)
            {
                case 1:
                    packetToSend = new Packet
                    {
                        OpCode = 2,
                        Data = test
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

                        List<JObject> objects = new List<JObject>();

                        Random random = new Random();
                        foreach (Connection client in connectedClients)
                        {
                            if (connectedClients.First().ID == client.ID) continue;

                            int index = random.Next(0, freeLocations.Count - 1);

                            Program.Map.Tiles[freeLocations[index].Item1, freeLocations[index].Item2].Entities.Add(new PlayerEntity { ConnectionId = client.ID });

                            JObject jobject = new JObject();
                            jobject.Add("id", client.ID);
                            jobject.Add("x", freeLocations[index].Item1);
                            jobject.Add("y", freeLocations[index].Item2);

                            objects.Add(jobject);

                            freeLocations.RemoveAt(index);
                        }

                        packetToSend = new Packet
                        {
                            OpCode = 4,
                            Data = objects
                        };

                        result = JsonConvert.SerializeObject(packetToSend, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Program.Server.Broadcast(result);
                    }
                    break;

                case 5:
                    if (Program.GameState == GameState.Running)
                    {
                        switch (decodedData.Data.ToString().ToUpper())
                        {
                            case "LEFT":
                                for (int i = 0; i < Program.Map.Width; i++)
                                {
                                    for (int j = 0; j < Program.Map.Height; j++)
                                    {
                                        var entity = Program.Map.Tiles[i, j].Entities.OfType<PlayerEntity>().FirstOrDefault(x => x.ConnectionId == source.ID);
                                        if (entity != null)
                                        {
                                            Program.Map.Tiles[i, j].Entities.Remove(entity);
                                            Program.Map.Tiles[i - 1, j].Entities.Add(entity);

                                            i = Int32.MaxValue - 1; //Break out of outer loop
                                            break;
                                        }
                                    }
                                }

                                Program.Server.Broadcast($"{{\"op\": 5, \"data\": {{ \"id\" : \"{ source.ID }\", \"direction\" : \"LEFT\"  }}");
                                break;
                            case "RIGHT":
                                for (int i = 0; i < Program.Map.Width; i++)
                                {
                                    for (int j = 0; j < Program.Map.Height; j++)
                                    {
                                        var entity = Program.Map.Tiles[i, j].Entities.OfType<PlayerEntity>().FirstOrDefault(x => x.ConnectionId == source.ID);
                                        if (entity != null)
                                        {
                                            Program.Map.Tiles[i, j].Entities.Remove(entity);
                                            Program.Map.Tiles[i + 1, j].Entities.Add(entity);

                                            i = Int32.MaxValue - 1; //Break out of outer loop
                                            break;
                                        }
                                    }
                                }

                                Program.Server.Broadcast($"{{\"op\": 5, \"data\": {{ \"id\" : \"{ source.ID }\", \"direction\" : \"RIGHT\"  }}");
                                break;
                            case "UP":
                                for (int i = 0; i < Program.Map.Width; i++)
                                {
                                    for (int j = 0; j < Program.Map.Height; j++)
                                    {
                                        var entity = Program.Map.Tiles[i, j].Entities.OfType<PlayerEntity>().FirstOrDefault(x => x.ConnectionId == source.ID);
                                        if (entity != null)
                                        {
                                            Program.Map.Tiles[i, j].Entities.Remove(entity);
                                            Program.Map.Tiles[i, j - 1].Entities.Add(entity);

                                            i = Int32.MaxValue - 1; //Break out of outer loop
                                            break;
                                        }
                                    }
                                }

                                Program.Server.Broadcast($"{{\"op\": 5, \"data\": {{ \"id\" : \"{ source.ID }\", \"direction\" : \"UP\"  }}");
                                break;
                            case "DOWN":
                                for (int i = 0; i < Program.Map.Width; i++)
                                {
                                    for (int j = 0; j < Program.Map.Height; j++)
                                    {
                                        var entity = Program.Map.Tiles[i, j].Entities.OfType<PlayerEntity>().FirstOrDefault(x => x.ConnectionId == source.ID);
                                        if (entity != null)
                                        {
                                            Program.Map.Tiles[i, j].Entities.Remove(entity);
                                            Program.Map.Tiles[i, j + 1].Entities.Add(entity);

                                            i = Int32.MaxValue - 1; //Break out of outer loop
                                            break;
                                        }
                                    }
                                }

                                Program.Server.Broadcast($"{{\"op\": 5, \"data\": {{ \"id\" : \"{ source.ID }\", \"direction\" : \"DOWN\"  }}");
                                break;
                        }
                    }
                    break;

                case 6:
                    if (Program.GameState == GameState.Running)
                    {
                        int x = (int)((JObject) decodedData.Data)["x"].ToObject(typeof(Int32));
                        int y = (int)((JObject)decodedData.Data)["y"].ToObject(typeof(Int32));
                        BombEntity bomb = new BombEntity();
                        Program.Map.Tiles[x, y].Entities.Add(bomb);
                        PlacedBombs.Add(bomb, new Tuple<int, int, DateTime>(x, y, DateTime.UtcNow));

                        Packet packet = new Packet
                        {
                            OpCode = 6,
                            Data = decodedData.Data
                        };

                        result = JsonConvert.SerializeObject(packet);
                        Program.Server.Broadcast(result);
                    }

                    break;
            }
        }

        #endregion
    }
}
