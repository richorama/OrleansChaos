using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHost
{
    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task Test();
    }

    public class TestGrain : Grain, ITestGrain
    {
        public Task Test()
        {
            return TaskDone.Done;
        }
    }
}
