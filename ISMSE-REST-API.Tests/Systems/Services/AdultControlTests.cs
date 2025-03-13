using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Exceptions;
using ISMSE_REST_API.Services.MedactProcesses.Verification;
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
    public class AdultControlTests : TestUtils
    {
        public AdultControlTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void VerifyExisting_WhenCalled_Returns_OK_Empty()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
            mockCissaDAL.Setup(svc => svc.CountDocumentsByPersonIdAndInStates(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(0);

            IDuplicateControl sut = new AdultControl(mockCissaDAL.Object);
            //Act
            sut.VerifyExisting(Guid.Empty);

            //Assert

        }

        [Fact]
        public void VerifyExisting_WhenCalled_Throws_DuplicateException()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
            mockCissaDAL.Setup(svc => svc.CountDocumentsByPersonIdAndInStates(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())).Returns(1);

            IDuplicateControl sut = new AdultControl(mockCissaDAL.Object);
            //Act & Assert
            Assert.Throws<DuplicateControlException>(() => sut.VerifyExisting(Guid.Empty));
        }
    }
}
