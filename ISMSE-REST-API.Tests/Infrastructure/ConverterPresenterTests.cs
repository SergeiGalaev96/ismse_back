using FluentAssertions;
using ISMSE_REST_API.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Infrastructure
{
    [Collection("Sequential")]
    public class ConverterPresenterTests : TestUtils
    {
        public ConverterPresenterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetHours_WhenCalled_Returns_INT32()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var correctFullTime = "22:45";
            var expectedHours = 22;

            //Act
            var result = sut.GetHours(correctFullTime);
            _ninjectStop();

            //Assert
            result.Should().Be(expectedHours);
        }

        [Fact]
        public void GetHours_WhenCalled_ThrowsArgumentNullException()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var incorrectFullTime = "";

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.GetHours(incorrectFullTime));
            _ninjectStop();
        }

        [Fact]
        public void GetHours_WhenCalled_ThrowsFormatException()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var incorrectFullTime = "22.45";

            //Act & Assert
            Assert.Throws<FormatException>(() => sut.GetHours(incorrectFullTime));
            _ninjectStop();
        }

        [Fact]
        public void GetMinutes_WhenCalled_Returns_INT32()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var correctFullTime = "22:45";
            var expectedHours = 45;

            //Act
            var result = sut.GetMinutes(correctFullTime);
            _ninjectStop();

            //Assert
            result.Should().Be(expectedHours);
        }

        [Fact]
        public void GetMinutes_WhenCalled_ThrowsArgumentNullException()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var incorrectFullTime = "";

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.GetMinutes(incorrectFullTime));
            _ninjectStop();
        }

        [Fact]
        public void GetMinutes_WhenCalled_ThrowsFormatException()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IConverterPresenter>();
            var incorrectFullTime = "22.45";

            //Act & Assert
            Assert.Throws<FormatException>(() => sut.GetMinutes(incorrectFullTime));
            _ninjectStop();
        }
    }
}
