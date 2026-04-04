namespace PhotoMover.Tests.Converters;

using PhotoMover.Converters;
using System.Globalization;
using System.Windows;
using Xunit;
using FluentAssertions;

public class BooleanToThicknessConverterTests
{
    [Fact]
    public void Convert_WhenBoolIsFalseWith0Pipe2Parameter_ReturnsThicknessZero()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();
        var parameter = "0|2";

        // Act
        var result = converter.Convert(false, typeof(Thickness), parameter, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(0);
        thickness.Top.Should().Be(0);
        thickness.Right.Should().Be(0);
        thickness.Bottom.Should().Be(0);
    }

    [Fact]
    public void Convert_WhenBoolIsTrueWith0Pipe2Parameter_ReturnsThickness2()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();
        var parameter = "0|2";

        // Act
        var result = converter.Convert(true, typeof(Thickness), parameter, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(2);
        thickness.Top.Should().Be(2);
        thickness.Right.Should().Be(2);
        thickness.Bottom.Should().Be(2);
    }

    [Fact]
    public void Convert_WhenParameterIsNull_ReturnsThicknessZero()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();

        // Act
        var result = converter.Convert(true, typeof(Thickness), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(0);
    }

    [Fact]
    public void Convert_WhenBoolValueIsNotBoolean_ReturnsThicknessZero()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();
        var parameter = "0|2";

        // Act
        var result = converter.Convert("not a bool", typeof(Thickness), parameter, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(0);
    }

    [Fact]
    public void Convert_WhenParameterWithTwoSidedValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();
        var parameter = "0|1,2";  // When true: 1 horizontal, 2 vertical

        // Act
        var result = converter.Convert(true, typeof(Thickness), parameter, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(1);
        thickness.Top.Should().Be(2);
        thickness.Right.Should().Be(1);
        thickness.Bottom.Should().Be(2);
    }

    [Fact]
    public void Convert_WhenParameterWithFourSidedValues_ParsesCorrectly()
    {
        // Arrange
        var converter = new BooleanToThicknessConverter();
        var parameter = "0|1,2,3,4";  // When true: left=1, top=2, right=3, bottom=4

        // Act
        var result = converter.Convert(true, typeof(Thickness), parameter, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var thickness = (Thickness)result;
        thickness.Left.Should().Be(1);
        thickness.Top.Should().Be(2);
        thickness.Right.Should().Be(3);
        thickness.Bottom.Should().Be(4);
    }
}
