using ISMSE_REST_API.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Infrastructure
{
    public abstract class TestUtils
    {
        protected void _ninjectStart() => NinjectWebCommon.Start();
        protected void _ninjectStop() => NinjectWebCommon.Stop();
        protected T _ninjectGetService<T>() => NinjectWebCommon.GetService<T>();
        protected readonly ITestOutputHelper _output;
        public TestUtils(ITestOutputHelper output)
        {
            _output = output;
        }
    }
}
