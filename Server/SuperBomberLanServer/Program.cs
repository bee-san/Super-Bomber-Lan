using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperBomberLanServer.Local;
using SuperBomberLanServer.Network;

namespace SuperBomberLanServer
{
    class Program
    {
        public static Server Server;
        public static Map Map;
        public static GameState GameState = GameState.StartingUp;

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            Console.WriteLine(@"/-----------------------------------------------------------------------------------------\");
            Console.WriteLine(@"|                                        Welcome to                                      |");
            Console.WriteLine(@"|                               _____                                                    |");
            Console.WriteLine(@"|                              / ____|                                                   |");
            Console.WriteLine(@"|                             | (___  _   _ _ __   ___ _ __                              |");
            Console.WriteLine(@"|                              \___ \| | | | '_ \ / _ \ '__|                             |");
            Console.WriteLine(@"|                              ____) | |_| | |_) |  __/ |                                |");
            Console.WriteLine(@"|                           __|_____/ \__,_| .__/ \___|_|                                |");
            Console.WriteLine(@"|                          |  _ \          | |   | |                                     |");
            Console.WriteLine(@"|                          | |_) | ___  _ _|_|__ | |__   ___ _ __                        |");
            Console.WriteLine(@"|                          |  _ < / _ \| '_ ` _ \| '_ \ / _ \ '__|                       |");
            Console.WriteLine(@"|                          | |_) | (_) | | | | | | |_) |  __/ |                          |");
            Console.WriteLine(@"|                          |____/ \___/|_| |_| |_|_.__/ \___|_|                          |");
            Console.WriteLine(@"|                                | |        /\   | \ | |                                 |");
            Console.WriteLine(@"|                                | |       /  \  |  \| |                                 |");
            Console.WriteLine(@"|                                | |      / /\ \ | . ` |                                 |");
            Console.WriteLine(@"|                                | |____ / ____ \| |\  |                                 |");
            Console.WriteLine(@"|                                |______/_/    \_\_| \_|                                 |");
            Console.WriteLine("\\-----------------------------------------------------------------------------------------/\n\n");
            Console.WriteLine("Initialising....");

            //Generate the map
            Console.WriteLine("Generating map....");
            Map = Map.CreateMap(64, 64);

            //var test = JsonConvert.SerializeObject(map);
            
            Console.WriteLine("Starting server on port 12345....");
            Server = new Server(12345);
            Server.Start();

            Console.WriteLine("Server reported sucessful startup");

            Console.WriteLine("Ready");

            Console.WriteLine("Waiting for admin client to connect...");

            if (Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                Server.Close(false);
                Environment.Exit(0);
            }

            await Task.Delay(-1);
        }
    }

    public enum GameState
    {
        StartingUp,
        Running,
        Completed
    }
}