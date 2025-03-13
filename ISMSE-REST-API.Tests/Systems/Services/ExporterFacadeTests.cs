using ISMSE_REST_API.Contracts.CustomExporter;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services;
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
    public class ExporterFacadeTests : TestUtils
    {
        public ExporterFacadeTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public void DownloadGrownList_WhenCalled_ReturnsByteData()
        {
            //Arrange

            var state = new[] { CustomExportAdultState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray();
            var expectedResult = new List<Models.CustomExportModels.CustomExportItem[]>()
            {
                new Models.CustomExportModels.CustomExportItem[]
                {
                    new Models.CustomExportModels.CustomExportItem{ Key = "Some key", Value = "Some val"}
                }
            };
            var byteData = new byte[0];
            var mockCustomExporter = new Mock<ICustomExporter>();
            mockCustomExporter.Setup(svc => svc.GetData(state, DateTime.Today, DateTime.Today, null, null, null)).Returns(expectedResult);
            mockCustomExporter.Setup(svc => svc.ConvertToFileInByteArray(expectedResult)).Returns(byteData);


            IExporterFacade sut = new ExporterFacadeImpl(mockCustomExporter.Object);
            //Act
            var result = sut.GetExcelAsByteArray(state, DateTime.Today, DateTime.Today, null, null, null);

            //Assert
            mockCustomExporter.Verify(svc => svc.GetData(state, DateTime.Today, DateTime.Today, null, null, null), Times.Once);
            mockCustomExporter.Verify(svc => svc.ConvertToFileInByteArray(expectedResult), Times.Once);
        }
    }
}
