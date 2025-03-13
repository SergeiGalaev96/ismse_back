using FluentAssertions;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Exceptions;
using ISMSE_REST_API.Services.MedactProcesses;
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
    public class TransferChildToAdultTests : TestUtils
    {
        public TransferChildToAdultTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void TransferFromChildToAdult_WhenCalled_Returns_ID()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
            var childSignedList = new Guid[] { Guid.Empty, Guid.Empty };
            mockCissaDAL.Setup(svc => svc.GetSignedChildMedActsByPersonId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(childSignedList);
            mockCissaDAL.Setup(svc => svc.SetState(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            var mockDataSvc = new Mock<IDataService>();
            var expctedDocumentId = Guid.NewGuid();
            mockDataSvc.Setup(svc => svc.CreateWithNo(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<document>()))
                .Returns(expctedDocumentId);

            var dto = new document
            {
                attributes = new attribute[]
                {
                    new attribute
                    {
                        name = "Person",
                        value = Guid.Empty.ToString()
                    }
                }
            };
            var userId = Guid.Empty;
            ITransferChildToAdult sut = new TransferChildToAdultImpl(mockDataSvc.Object, mockCissaDAL.Object);

            //Act
            var actualDocumentId = sut.TransferFromChildToAdult(dto, userId);

            //Assert
            mockCissaDAL.Verify(svc =>
            svc.GetSignedChildMedActsByPersonId(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Once());
            var expirationAttempts = childSignedList.Length;
            var currentEntrySetStateAttempts = 1;
            mockCissaDAL.Verify(svc => svc.SetState(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(expirationAttempts + currentEntrySetStateAttempts));

            actualDocumentId.Should().Be(expctedDocumentId);

        }

        [Fact]
        public void TransferFromChildToAdult_WhenCalled_Throws_Exception()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
            var childSignedList = new Guid[] {  };
            mockCissaDAL.Setup(svc => svc.GetSignedChildMedActsByPersonId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(childSignedList);
            mockCissaDAL.Setup(svc => svc.SetState(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            var mockDataSvc = new Mock<IDataService>();
            var expctedDocumentId = Guid.NewGuid();
            mockDataSvc.Setup(svc => svc.CreateWithNo(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<document>()))
                .Returns(expctedDocumentId);

            var dto = new document
            {
                attributes = new attribute[]
                {
                    new attribute
                    {
                        name = "Person",
                        value = Guid.Empty.ToString()
                    }
                }
            };
            var userId = Guid.Empty;
            ITransferChildToAdult sut = new TransferChildToAdultImpl(mockDataSvc.Object, mockCissaDAL.Object);

            //Act & Assert
            Assert.Throws<TransferException>(() => sut.TransferFromChildToAdult(dto, userId));
        }
    }
}
