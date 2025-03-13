using FluentAssertions;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Services.Documents;
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
    public class DocumentServiceTests : TestUtils
    {
        public DocumentServiceTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateWithNo_WhenCalled_Returns_ID()
        {
            //Arrange
            var mockCissaDAL = new Mock<ICissaDataAccessLayer>();

            var defId = Guid.Empty;
            var dto = new document();
            var userId = Guid.Empty;
            var hasNo = true;
            var noAttrName = "No";

            var expctedDocumentId = Guid.NewGuid();
            mockCissaDAL.Setup(svc => svc.CreateWithNo(defId, dto, userId, hasNo, noAttrName)).Returns(expctedDocumentId);
            IDataService sut = new DataServiceImpl(mockCissaDAL.Object);

            //Act
            var actualDocumentId = sut.CreateWithNo(defId, userId, noAttrName, dto);

            //Assert
            actualDocumentId.Should().Be(expctedDocumentId);

        }
    }
}
