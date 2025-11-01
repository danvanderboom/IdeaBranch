using FluentAssertions;
using IdeaBranch.Domain.Timeline;

namespace IdeaBranch.UnitTests.Timeline;

public class TemporalPrecisionTests
{
    [Test]
    public void TemporalInstant_FromDateTime_WithDayPrecision_ShouldNormalizeCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(date, TemporalPrecision.Day);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 3, 15));
        instant.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void TemporalInstant_FromDateTime_WithMonthPrecision_ShouldNormalizeCorrectly()
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
    public void TemporalInstant_FromDateTime_WithYearPrecision_ShouldNormalizeCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var instant = TemporalInstant.FromDateTime(date, TemporalPrecision.Year);

        // Assert
        instant.Date.Should().Be(new DateTime(2024, 1, 1));
        instant.Precision.Should().Be(TemporalPrecision.Year);
    }

    [Test]
    public void TemporalRange_Point_ShouldCreatePointInTimeRange()
    {
        // Arrange
        var instant = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act
        var range = TemporalRange.Point(instant);

        // Assert
        range.Start.Should().Be(instant);
        range.End.Should().BeNull();
    }

    [Test]
    public void TemporalRange_Duration_ShouldCreateRangeWithStartAndEnd()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 20));

        // Act
        var range = TemporalRange.Duration(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Test]
    public void TemporalRange_Duration_WithEndBeforeStart_ShouldThrow()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 20));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TemporalRange(start, end));
    }
}

// Note: TemporalInstant and TemporalRange tests have been moved to their own files:
// - TemporalInstantTests.cs
// - TemporalRangeTests.cs

