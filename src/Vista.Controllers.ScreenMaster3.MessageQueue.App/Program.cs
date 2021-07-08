using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.App
{
    class Program
    {
        static async Task Main()
        {
            string rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            string rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "devtest";
            string rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "devtest";
            string rabbitMqVHost = Environment.GetEnvironmentVariable("RABBITMQ_VDIR") ?? "/";
            
            Console.WriteLine("Initializing Server...");
            RabbitMqServer server = new RabbitMqServer(rabbitMqHost, rabbitMqUser, rabbitMqPassword, rabbitMqVHost);
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
