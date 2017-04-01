using System;
using System.Threading.Tasks;
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

            await Task.Delay(-1);
        }
    }
}