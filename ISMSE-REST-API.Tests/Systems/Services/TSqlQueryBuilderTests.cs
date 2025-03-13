using FluentAssertions;
using ISMSE_REST_API.App_Start;
using ISMSE_REST_API.Contracts.Builders;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.Builders;
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
    public class TSqlQueryBuilderTests : TestUtils
    {
        public TSqlQueryBuilderTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void BuildSql_WhenInvoked_Returns_GrownSQL()
        {
            //Arrange
            var sut = new TSqlQueryBuilderCustomImpl(Mock.Of<ILogManager>());

            //Act
            var result = sut.BuildSql(new[] { CustomExportAdultState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray(), new DateTime(2020, 1, 1), DateTime.MaxValue, new Guid("a3a83c6e-19b8-4cf7-b96e-b8a7be91476b"));
            //Assert
            _output.WriteLine(result);
            result.Should().NotBeNullOrEmpty();

            
        }

        [Fact]
        public void BuildSql_WhenInvoked_Returns_ChildSQL()
        {
            //Arrange
            var sut = new TSqlQueryBuilderCustomImpl(Mock.Of<ILogManager>());

            //Act
            var result = sut.BuildSql(new[] { CustomExportChildState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray(), new DateTime(2020, 1, 1), DateTime.MaxValue, new Guid("a3a83c6e-19b8-4cf7-b96e-b8a7be91476b"));
            //Assert
            _output.WriteLine(result);
            result.Should().NotBeNullOrEmpty();
        }
    }
}
