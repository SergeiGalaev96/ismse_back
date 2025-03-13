using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Status;
using ISMSE_REST_API.Services.Status;
using ISMSE_REST_API.Tests.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Repos
{
    public class TestStatusRepository : TestUtils
    {
        public TestStatusRepository(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void SetTrashStatus_WhenCalled_Returns_Ok_Empty()
        {
            //Arrange
            _ninjectStart();
            var mockStatusDataProvider = new Mock<ICissaDataAccessLayer>();
            IStatusRepository sut = new StatusRepositoryImpl(mockStatusDataProvider.Object);

            //Act
            sut.SetTrashStatus(Guid.Empty, Guid.Empty);

            //Assert
        }

        [Fact]
        public void SetStatusManually_WhenCalled_Returns_Ok_Empty()
        {
            //Arrange
            _ninjectStart();
            var mockStatusDataProvider = new Mock<ICissaDataAccessLayer>();
            IStatusRepository sut = new StatusRepositoryImpl(mockStatusDataProvider.Object);

            //Act
            sut.SetStatusManually(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today);

            //Assert
        }
    }
}
