using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.App
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello World!");
            RabbitMqServer server = new RabbitMqServer();
            if(!await server.Startup().ConfigureAwait(false))
            {
                Console.WriteLine("Failed to start.  Exiting...");
                return;
            }
            Console.WriteLine("Started server");
            await Task.Delay(Timeout.Infinite);

            //Console.WriteLine("Press enter to exit");
            //Console.ReadLine();

            //Console.WriteLine("Shutting Down...");
            //await server.ShutdownAsync().ConfigureAwait(false);
        }
    }
}
