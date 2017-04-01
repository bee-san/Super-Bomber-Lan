using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperBomberLanServer.Local;

namespace SuperBomberLanServer
{
    class Program
    {
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
            Map map = Map.CreateMap(64, 64);

            //
            var test = JsonConvert.SerializeObject(map);
            Debugger.Break();

            await Task.Delay(-1);
        }
    }
}