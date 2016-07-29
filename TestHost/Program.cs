using Orleans;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHost
{
    class Program
    {

        static void Main(string[] args)
        {
            using (var silo = new DevSilo())
            {

                GrainClient.Initialize(ClientConfiguration.LocalhostSilo());

                Console.WriteLine("Running Test");
                var rand = new Random();
                while (true)
                {
                    var id = rand.Next(100);
                    var grain = GrainClient.GrainFactory.GetGrain<ITestGrain>(id);
                    try
                    {
                        Console.WriteLine($"calling {id}");
                        grain.Test().Wait();
                        Console.WriteLine($"success");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                }
            }
        }

    }

    class DevSilo : IDisposable
    {
        private static OrleansHostWrapper hostWrapper;

        public DevSilo()
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = new string[0],
            });
        }

        static void InitSilo(string[] args = null)
        {
            hostWrapper = new OrleansHostWrapper();

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        public void Dispose()
        {
            if (hostWrapper == null) return;
            hostWrapper.Dispose();
            GC.SuppressFinalize(hostWrapper);
        }
    }

}
