using FluentAssertions;
using IdeaBranch.Domain.Timeline;

namespace IdeaBranch.UnitTests.Timeline;

/// <summary>
/// Comprehensive tests for TemporalRange creation and validation.
/// </summary>
public class TemporalRangeTests
{
    [Test]
    public void Constructor_WithStartOnly_ShouldCreatePointRange()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act
        var range = new TemporalRange(start);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().BeNull();
    }

    [Test]
    public void Constructor_WithStartAndEnd_ShouldCreateDurationRange()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 20));

        // Act
        var range = new TemporalRange(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Test]
    public void Constructor_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 20));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new TemporalRange(start, end));
        exception!.Message.Should().Contain("End date must be after or equal to start date");
        exception.ParamName.Should().Be("end");
    }

    [Test]
    public void Constructor_WithEqualStartAndEnd_ShouldCreateValidRange()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act
        var range = new TemporalRange(start, end);

        // Assert
        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Test]
    public void Point_WithTemporalInstant_ShouldCreatePointRange()
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
    public void Point_WithDateTime_ShouldCreatePointRangeWithDayPrecision()
    {
        // Arrange
        var dateTime = new DateTime(2024, 3, 15, 14, 30, 0);

        // Act
        var range = TemporalRange.Point(dateTime);

        // Assert
        range.Start.Date.Should().Be(new DateTime(2024, 3, 15));
        range.Start.Precision.Should().Be(TemporalPrecision.Day);
        range.End.Should().BeNull();
    }

    [Test]
    public void Duration_WithTemporalInstants_ShouldCreateDurationRange()
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
    public void Duration_WithDateTime_ShouldCreateDurationRangeWithDayPrecision()
    {
        // Arrange
        var start = new DateTime(2024, 3, 15, 10, 0, 0);
        var end = new DateTime(2024, 3, 20, 15, 30, 0);

        // Act
        var range = TemporalRange.Duration(start, end);

        // Assert
        range.Start.Date.Should().Be(new DateTime(2024, 3, 15));
        range.Start.Precision.Should().Be(TemporalPrecision.Day);
        range.End!.Date.Should().Be(new DateTime(2024, 3, 20));
        range.End.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void Duration_WithEqualDateTimes_ShouldCreateValidRange()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var range = TemporalRange.Duration(date, date);

        // Assert
        range.Start.Date.Should().Be(date);
        range.End.Should().NotBeNull();
        range.End!.Date.Should().Be(date);
    }

    [Test]
    public void Duration_WithDifferentPrecisions_ShouldPreservePrecisions()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15), TemporalPrecision.Month);
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 6, 20), TemporalPrecision.Day);

        // Act
        var range = TemporalRange.Duration(start, end);

        // Assert
        range.Start.Precision.Should().Be(TemporalPrecision.Month);
        range.End!.Precision.Should().Be(TemporalPrecision.Day);
    }

    [Test]
    public void Point_WithYearPrecision_ShouldNormalizeCorrectly()
    {
        // Arrange
        var instant = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15), TemporalPrecision.Year);

        // Act
        var range = TemporalRange.Point(instant);

        // Assert
        range.Start.Date.Should().Be(new DateTime(2024, 1, 1));
        range.Start.Precision.Should().Be(TemporalPrecision.Year);
    }

    [Test]
    public void Duration_WithDateTimeEndBeforeStart_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTime(2024, 3, 20);
        var end = new DateTime(2024, 3, 15);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => TemporalRange.Duration(start, end));
        exception!.Message.Should().Contain("End date must be after or equal to start date");
    }

    [Test]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var start = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));
        var end = TemporalInstant.FromDateTime(new DateTime(2024, 3, 20));

        // Act
        var range1 = TemporalRange.Duration(start, end);
        var range2 = TemporalRange.Duration(start, end);

        // Assert
        range1.Should().BeEquivalentTo(range2);
        (range1 == range2).Should().BeTrue();
    }

    [Test]
    public void RecordEquality_PointRange_WithSameInstant_ShouldBeEqual()
    {
        // Arrange
        var instant = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act
        var range1 = TemporalRange.Point(instant);
        var range2 = TemporalRange.Point(instant);

        // Assert
        range1.Should().BeEquivalentTo(range2);
        (range1 == range2).Should().BeTrue();
    }

    [Test]
    public void RecordEquality_PointAndDuration_ShouldNotBeEqual()
    {
        // Arrange
        var instant = TemporalInstant.FromDateTime(new DateTime(2024, 3, 15));

        // Act
        var pointRange = TemporalRange.Point(instant);
        var durationRange = TemporalRange.Duration(instant, instant);

        // Assert
        pointRange.Should().NotBeEquivalentTo(durationRange);
        (pointRange == durationRange).Should().BeFalse();
    }

    [Test]
    public void Duration_WithLeapYearBoundary_ShouldHandleCorrectly()
    {
        // Arrange
        var start = new DateTime(2024, 2, 28);
        var end = new DateTime(2024, 2, 29);

        // Act
        var range = TemporalRange.Duration(start, end);

        // Assert
        range.Start.Date.Should().Be(new DateTime(2024, 2, 28));
        range.End!.Date.Should().Be(new DateTime(2024, 2, 29));
    }
}

