using FluentAssertions;
using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Services.MedactProcesses.Verification;
using ISMSE_REST_API.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Delegates
{
    public class DuplicateControlServiceResolverTests : TestUtils
    {
        public DuplicateControlServiceResolverTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Ninject cannot work in parallel format. Use only individual. Only to be sure that DI correctly injected")]
        public void InvokeChild_WhenInvoked_Returns_ChildControl()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<DuplicateControlServiceResolver>();

            //Act
            var duplicateControl = sut("Child");
            _ninjectStop();

            //Assert
            duplicateControl.Should().NotBeNull();
            duplicateControl.Should().BeOfType<ChildControl>();
        }
    }
}
