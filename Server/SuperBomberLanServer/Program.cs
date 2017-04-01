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
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine("|                                         Welcome to                                      |");
            Console.WriteLine("|                                     SUPER BOMBER LAN                                    |");
            Console.WriteLine("-------------------------------------------------------------------------------------------\n\n");
            Console.WriteLine("Initialising....");

            //Generate the map
            Console.WriteLine("Generating map....");
            Map map = Map.CreateMap(64, 64);
            var test = JsonConvert.SerializeObject(map);
            Debugger.Break();

            await Task.Delay(-1);
        }
    }
}