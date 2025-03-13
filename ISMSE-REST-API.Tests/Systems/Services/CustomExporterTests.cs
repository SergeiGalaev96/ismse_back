using FluentAssertions;
using ISMSE_REST_API.Contracts.CustomExporter;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.CustomExporter;
using ISMSE_REST_API.Tests.Infrastructure;
using Moq;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Services
{
    public class CustomExporterTests : TestUtils
    {
        public CustomExporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetData_WhenCalled_Returns_Data()
        {
            //Arrange
            var mockMyDataProvider = new Mock<INativeSqlDataProvider>();
            var state = new[] { CustomExportAdultState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray();
            var expectedResult = new List<Models.CustomExportModels.CustomExportItem[]>()
            {
                new Models.CustomExportModels.CustomExportItem[]
                {
                    new Models.CustomExportModels.CustomExportItem{ Key = "Some key", Value = "Some val"}
                }
            };
            mockMyDataProvider.Setup(svc => svc.FetchData(state, DateTime.Today, DateTime.Today, null, null, null))
                .Returns(expectedResult);

            ICustomExporter sut = new CustomExporterImpl(mockMyDataProvider.Object);

            //Act
            var result = sut.GetData(state, DateTime.Today, DateTime.Today, null, null, null);

            //Assert
            result.Should().HaveCount(expectedResult.Count);
            _output.WriteLine("expected result:");
            _output.WriteLine(JsonConvert.SerializeObject(expectedResult));
            _output.WriteLine("actual result:");
            _output.WriteLine(JsonConvert.SerializeObject(result));
            mockMyDataProvider.Verify(svc => svc.FetchData(state, DateTime.Today, DateTime.Today, null, null, null), Times.Once);
        }

        [Fact]
        public void GetData_WhenCalled_Returns_Throws_Exception()
        {
            //Arrange
            var mockMyDataProvider = new Mock<INativeSqlDataProvider>();
            var state = new[] { CustomExportAdultState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray();
            var expectedResult = new List<Models.CustomExportModels.CustomExportItem[]>()
            {
                new Models.CustomExportModels.CustomExportItem[]
                {
                    new Models.CustomExportModels.CustomExportItem{ Key = "Some key", Value = "Some val"}
                }
            };
            var errorMessage = "some errors in database";
            mockMyDataProvider.Setup(svc => svc.FetchData(state, DateTime.Today, DateTime.Today, null, null, null))
                .Throws(new Exception(errorMessage));

            ICustomExporter sut = new CustomExporterImpl(mockMyDataProvider.Object);

            //Act & Assert
            var ex = Assert.Throws<Exception>(() => sut.GetData(state, DateTime.Today, DateTime.Today, null, null, null));
            ex.Message.Should().Be(errorMessage);
            mockMyDataProvider.Verify(svc => svc.FetchData(state, DateTime.Today, DateTime.Today, null, null, null), Times.Once);
        }
    }
}
