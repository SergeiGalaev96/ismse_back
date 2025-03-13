using FluentAssertions;
using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Exceptions;
using ISMSE_REST_API.Services.MedactProcesses;
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
    public class AdultFacadeTests : TestUtils
    {
        public AdultFacadeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateNew_WhenCalled_ThrowsDuplicateException()
        {
            //Arrange
            var mockDataService = Mock.Of<IDataService>();
            var mockDuplicateControlResolver = new Mock<DuplicateControlServiceResolver>();
            var mockDuplicateControl = new Mock<IDuplicateControl>();
            var errorMessage = "some error";
            mockDuplicateControl.Setup(svc => svc.VerifyExisting(It.IsAny<Guid>())).Throws(() => new DuplicateControlException(errorMessage));
            mockDuplicateControlResolver.Setup(d => d("Adult")).Returns(mockDuplicateControl.Object);
            var mockPersonVerifier = Mock.Of<IPersonVerification>();
            IAdultFacade sut = new AdultFacadeImpl(mockDataService, mockDuplicateControlResolver.Object, mockPersonVerifier);

            //Act & Assert
            var ex = Assert.Throws<DuplicateControlException>(() => sut.CreateNew(new document(), Guid.Empty));
        }

        [Fact]
        public void CreateNew_WhenCalled_ThrowsPersonVerificationException()
        {
            //Arrange
            var mockDataService = Mock.Of<IDataService>();
            var mockDuplicateControlResolver = Mock.Of<DuplicateControlServiceResolver>();
            IPersonVerification personVerifier = new PersonVerificationImpl();
            IAdultFacade sut = new AdultFacadeImpl(mockDataService, mockDuplicateControlResolver, personVerifier);

            //Act & Assert
            var ex = Assert.Throws<PersonVerificationException>(() => sut.CreateNew(new document { attributes = new[] { new attribute() } }, Guid.Empty));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void CreateNew_WhenCalled_Returns_Data()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var newDocumentId = Guid.NewGuid();
            mockDataService.Setup(svc => svc.CreateWithNo(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<document>())).Returns(newDocumentId);
            mockDataService.Setup(svc => svc.SetState(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
            var mockDuplicateControlResolver = new Mock<DuplicateControlServiceResolver>();
            var mockDuplicateControl = Mock.Of<IDuplicateControl>();
            mockDuplicateControlResolver.Setup(d => d("Adult")).Returns(mockDuplicateControl);
            var mockPersonVerifier = Mock.Of<IPersonVerification>();
            IAdultFacade sut = new AdultFacadeImpl(mockDataService.Object, mockDuplicateControlResolver.Object, mockPersonVerifier);

            //Act
            var result = sut.CreateNew(new document { attributes = new[] { new attribute() } }, Guid.Empty);

            //Assert
            mockDataService.Verify(svc => svc.SetState(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            result.Should().NotBeNull();
            result.id.Should().Be(newDocumentId);
        }
    }
}
