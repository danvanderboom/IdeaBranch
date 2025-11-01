using FluentAssertions;
using IdeaBranch.Domain.Timeline;

namespace IdeaBranch.UnitTests.Timeline;

/// <summary>
/// Comprehensive tests for TemporalInstant normalization and construction.
/// </summary>
public class TemporalInstantTests
{
    [Test]
    public void Constructor_WithDayPrecision_ShouldNormalizeToMidnight()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 45);

        // Act
        var instant = new TemporalInstant(date, TemporalPrecision.Day);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 15, 0, 0, 0));
        instant.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void Constructor_WithMonthPrecision_ShouldNormalizeToFirstOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 45);

        // Act
        var instant = new TemporalInstant(date, TemporalPrecision.Month);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 1, 0, 0, 0));
        instant.Precision.Should().Be(TemporalPrecision.Month);
    }

    [Test]
    public void Constructor_WithYearPrecision_ShouldNormalizeToFirstOfYear()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 45);

        // Act
        var instant = new TemporalInstant(date, TemporalPrecision.Year);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0));
        instant.Precision.Should().Be(TemporalPrecision.Year);
    }

    [Test]
    public void FromDateTime_WithoutPrecision_ShouldDefaultToDay()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(date);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 15));
        instant.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void FromDateTime_WithExplicitPrecision_ShouldUseSpecifiedPrecision()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(date, TemporalPrecision.Month);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 1));
        instant.Precision.Should().Be(TemporalPrecision.Month);
    }

    [Test]
    public void Normalization_WithLeapYearDay_ShouldPreserveDay()
    {
        // Arrange
        var leapDay = new DateTime(2024, 2, 29, 12, 0, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(leapDay, TemporalPrecision.Day);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 2, 29));
        instant.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void Normalization_WithYearPrecision_ShouldIgnoreLeapYear()
    {
        // Arrange
        var leapDay = new DateTime(2024, 2, 29, 12, 0, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(leapDay, TemporalPrecision.Year);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 1, 1));
        instant.Precision.Should().Be(TemporalPrecision.Year);
    }

    [Test]
    public void Normalization_WithMonthPrecision_ShouldPreserveMonth()
    {
        // Arrange
        var lastDayOfMonth = new DateTime(2024, 3, 31, 23, 59, 59);

        // Act
        var instant = TemporalInstant.FromDateTime(lastDayOfMonth, TemporalPrecision.Month);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 1));
        instant.Precision.Should().Be(TemporalPrecision.Month);
    }

    [Test]
    public void Normalization_WithDifferentMonths_ShouldNormalizeCorrectly()
    {
        // Arrange & Act
        var jan = TemporalInstant.FromDateTime(new DateTime(2024, 1, 15), TemporalPrecision.Month);
        var dec = TemporalInstant.FromDateTime(new DateTime(2024, 12, 15), TemporalPrecision.Month);

        // Assert
        jan.Date.Should().Be(new DateTime(2024, 1, 1));
        dec.Date.Should().Be(new DateTime(2024, 12, 1));
    }

    [Test]
    public void RecordEquality_WithSameDateAndPrecision_ShouldBeEqual()
    {
        // Arrange
        var date1 = new DateTime(2024, 3, 15, 10, 0, 0);
        var date2 = new DateTime(2024, 3, 15, 20, 0, 0);

        // Act
        var instant1 = TemporalInstant.FromDateTime(date1, TemporalPrecision.Day);
        var instant2 = TemporalInstant.FromDateTime(date2, TemporalPrecision.Day);

        // Assert
        instant1.Should().BeEquivalentTo(instant2);
        (instant1 == instant2).Should().BeTrue();
    }

    [Test]
    public void RecordEquality_WithDifferentPrecision_ShouldNotBeEqual()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var dayPrecision = TemporalInstant.FromDateTime(date, TemporalPrecision.Day);
        var monthPrecision = TemporalInstant.FromDateTime(date, TemporalPrecision.Month);

        // Assert
        dayPrecision.Should().NotBeEquivalentTo(monthPrecision);
        (dayPrecision == monthPrecision).Should().BeFalse();
    }

    [Test]
    public void Normalization_WithYearPrecision_ShouldHandleYearBoundary()
    {
        // Arrange
        var newYear = new DateTime(2024, 1, 1, 0, 0, 0);
        var endOfYear = new DateTime(2024, 12, 31, 23, 59, 59);

        // Act
        var instant1 = TemporalInstant.FromDateTime(newYear, TemporalPrecision.Year);
        var instant2 = TemporalInstant.FromDateTime(endOfYear, TemporalPrecision.Year);

        // Assert
        instant1.Date.Should().Be(new DateTime(2024, 1, 1));
        instant2.Date.Should().Be(new DateTime(2024, 1, 1));
        instant1.Should().BeEquivalentTo(instant2);
    }
}

