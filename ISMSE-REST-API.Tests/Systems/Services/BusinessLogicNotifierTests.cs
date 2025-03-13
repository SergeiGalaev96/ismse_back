using FluentAssertions;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Notifications;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Services.Notifications;
using ISMSE_REST_API.Tests.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Services
{
    public class BusinessLogicNotifierTests : TestUtils
    {
        public BusinessLogicNotifierTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void FetchGreaterThan18YearsChildMedacts_WhenCalled_Returns_Data()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
            var greaterThan18YearsEntries = new document[] { new document(), new document() };
            mockCissaDAL.Setup(svc => svc.FetchGreaterThan18YearsChildMedacts(It.IsAny<document>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(greaterThan18YearsEntries);
            IBusinessLogicNotifier sut = new BusinessLogicNotifierImpl(mockCissaDAL.Object);

            //Act
            var result = sut.FetchGreaterThan18YearsChildMedacts(It.IsAny<document>(), Guid.Empty);

            //Assert
            result.Length.Should().Be(greaterThan18YearsEntries.Length);
        }
    }
}
