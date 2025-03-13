using FluentAssertions;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Contracts.Status;
using ISMSE_REST_API.Controllers;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.Documents;
using ISMSE_REST_API.Services.Infrastructure;
using ISMSE_REST_API.Services.MedactProcesses;
using ISMSE_REST_API.Services.MedactProcesses.Verification;
using ISMSE_REST_API.Tests.Infrastructure;
using Moq;
using Raven.Imports.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Controllers
{
    [Collection("Sequential")]
    public class DocumentControllerTests : TestUtils
    {
        public DocumentControllerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void SetTrashStatus_WhenCalled_Returns_OK_200()
        {
            //Arrange
            var mockStatusRepo = Mock.Of<IStatusRepository>();
            var mockConverter = Mock.Of<IConverterPresenter>();
            var mockTransferChildToAdult = Mock.Of<ITransferChildToAdult>();
            var mockChildFacade = Mock.Of<IChildFacade>();
            var mockAdultFacade = Mock.Of<IAdultFacade>();
            var sut = new DocumentController(mockStatusRepo, mockConverter, mockTransferChildToAdult, mockChildFacade, mockAdultFacade);

            //Act
            var response = sut.SetTrashStatus(Guid.Empty, Guid.Empty);

            //Assert
            Assert.IsType<OkNegotiatedContentResult<OperationResult>>(response);
            var content = response as OkNegotiatedContentResult<OperationResult>;
            content.Content.Should().NotBeNull();
            Assert.True(content.Content.isSuccess);

        }

        [Fact]
        public void SetStatusManually_WhenCalled_Returns_OK_200()
        {
            //Arrange
            var mockStatusRepo = Mock.Of<IStatusRepository>();
            var mockConverter = Mock.Of<IConverterPresenter>();
            var mockTransferChildToAdult = Mock.Of<ITransferChildToAdult>();
            var mockChildFacade = Mock.Of<IChildFacade>();
            var mockAdultFacade = Mock.Of<IAdultFacade>();
            var sut = new DocumentController(mockStatusRepo, mockConverter, mockTransferChildToAdult, mockChildFacade, mockAdultFacade);
            var dto = new Models.Status.SetStatusManuallyRequestDTO();
            //Act
            var response = sut.SetStatusManually(dto, Guid.Empty);

            //Assert
            Assert.IsType<OkNegotiatedContentResult<OperationResult>>(response);
            var content = response as OkNegotiatedContentResult<OperationResult>;
            content.Content.Should().NotBeNull();
            Assert.True(content.Content.isSuccess);

        }

        [Fact]
        public void ExtendWithNo_WhenCalled_Returns_OK_Data()
        {
            //Arrange
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
            var newGuid = Guid.NewGuid();
            var mockTransferChildToAdult = new Mock<ITransferChildToAdult>();
            mockTransferChildToAdult.Setup(svc => svc.TransferFromChildToAdult(dto, It.IsAny<Guid>())).Returns(newGuid);
            var sut = new DocumentController(Mock.Of<IStatusRepository>(), Mock.Of<IConverterPresenter>(),
                mockTransferChildToAdult.Object, Mock.Of<IChildFacade>(), Mock.Of<IAdultFacade>());
            //Act
            var response = sut.ExtendWithNo(dto, Guid.Empty);

            //Assert
            Assert.IsType<OkNegotiatedContentResult<OperationResult>>(response);
            var content = response as OkNegotiatedContentResult<OperationResult>;
            content.Content.Should().NotBeNull();
            Assert.True(content.Content.isSuccess);
            var obj = content.Content.data as document;
            obj.id.Should().Be(newGuid);

        }

        [Fact]
        public void CreateAdult_WhenCalled_Returns_OK()
        {
            try
            {
                _ninjectStart();
                //Arrange
                var docDefId = CustomExportAdultState.APPROVED_AND_REGISTERED.GetDefId();
                var inStates = CustomExportAdultState.APPROVED_AND_REGISTERED.GetValueId().Concat(CustomExportAdultState.ON_REGISTERING.GetValueId());
                var duplicateCountFound = 0;
                var personId = Guid.NewGuid();
                var converterPresenter = _ninjectGetService<IConverterPresenter>();
                var personVerification = _ninjectGetService<IPersonVerification>();
                var mockCissaDAL = new Mock<ICissaDataAccessLayer>();
                var newAdultMedactId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var dto = new document
                {
                    attributes = new[]
                    {
                        new attribute {
                            name = "Field1",
                            type = "Text",
                            value = "sample string 7"
                        },
                        new attribute{
                            name = "Field2",
                            type = "Date",
                            value = "yyyy-MM-dd"
                        },
                        new attribute{
                            name = "Person",
                            type = "Doc",
                            value = personId.ToString()
                        }
                    }
                };

                mockCissaDAL.Setup(svc =>
                svc.CreateWithNo(docDefId, dto, userId, true, "No"))
                    .Returns(newAdultMedactId);
                mockCissaDAL.Setup(svc => svc.SetState(newAdultMedactId, CustomExportAdultState.ON_REGISTERING.GetValueId()[0], userId));

                IDataService dataService = new DataServiceImpl(mockCissaDAL.Object);
                var serviceResolver = new Mock<DuplicateControlServiceResolver>();
                mockCissaDAL.Setup(svc => svc.CountDocumentsByPersonIdAndInStates(docDefId, personId, inStates)).Returns(duplicateCountFound);
                IDuplicateControl adultControl = new AdultControl(mockCissaDAL.Object);
                serviceResolver.Setup(d => d("Adult")).Returns(adultControl);
                IAdultFacade adultFacade = new AdultFacadeImpl(dataService, serviceResolver.Object, personVerification);

                var sut = new DocumentController(Mock.Of<IStatusRepository>(), converterPresenter, Mock.Of<ITransferChildToAdult>(), Mock.Of<IChildFacade>(), adultFacade);

                //Act
                var response = sut.CreateAdult(dto, userId);

                //Assert
                Assert.IsType<OkNegotiatedContentResult<OperationResult>>(response);
                var content = response as OkNegotiatedContentResult<OperationResult>;
                content.Content.Should().NotBeNull();
                if (!content.Content.isSuccess)
                    _output.WriteLine(content.Content.errorMessage);
                content.Content.isSuccess.Should().BeTrue();
                var obj = content.Content.data as document;
                obj.id.Should().Be(newAdultMedactId);
                mockCissaDAL.Verify(svc => svc.SetState(newAdultMedactId, CustomExportAdultState.ON_REGISTERING.GetValueId()[0], userId), Times.Once());

            }
            finally
            {
                _ninjectStop();
            }
        }
    }
}
