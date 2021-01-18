using System;
using System.Threading.Tasks;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.App
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World!");
            RabbitMqServer server = new RabbitMqServer();
            if(!await server.Startup())
            {
                Console.WriteLine("Failed to start.  Exiting...");
                return;
            }
            Console.WriteLine("Started server");

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

            Console.WriteLine("Shutting Down...");
            await server.ShutdownAsync();
        }
    }
}
