using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chaos
{
    public class MessageLatencyProvider : IBootstrapProvider
    {
        public string Name { get; private set; }

        public Logger Logger { get; private set; }

        InvokeInterceptor innerInterceptor = null;

        Random rand = new Random();

        int MaxLatency = 0;


        public Task Close()
        {
            return TaskDone.Done;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Logger = providerRuntime.GetLogger(nameof(MessageLatencyProvider));
            this.Logger.Warn(666, $"You have enabled the {nameof(MessageLatencyProvider)}. This should NOT be used in a production environment. Expect reduced performance!");
            this.Name = name;

            this.MaxLatency = config.GetIntProperty("MaxLatency", 0);

            this.Logger.Warn(666, $"Maximum latency set to {MaxLatency}ms");

            this.innerInterceptor = providerRuntime.GetInvokeInterceptor();
            providerRuntime.SetInvokeInterceptor(this.InvokeInterceptor);

            return TaskDone.Done;
        }


        int GetLatency()
        {
            return rand.Next(this.MaxLatency);
        }

        async Task<object> InvokeInterceptor(MethodInfo targetMethod, InvokeMethodRequest request, IGrain target, IGrainMethodInvoker invoker)
        {
            var amount = this.GetLatency() / 2;

            // latency is symetrical, and added before and after the call
            await Task.Delay(amount);

            object result = null;
            if (this.innerInterceptor != null)
            {
                result = await this.innerInterceptor(targetMethod, request, target, invoker);
            }
            else
            {
                result = await invoker.Invoke(target, request);
            }

            await Task.Delay(amount);

            return result;
        }
    }
}
