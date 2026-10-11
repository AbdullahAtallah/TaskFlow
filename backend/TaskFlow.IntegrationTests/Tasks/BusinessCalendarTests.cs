using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.IntegrationTests.Tasks;

/// <summary>"Today" for overdue tasks is the date in the team's time zone, not in UTC; no API or database involved.</summary>
public sealed class BusinessCalendarTests
{
    [Theory]
    [InlineData("Europe/Amsterdam", "2026-10-11T22:30:00Z", "2026-10-12")]
    [InlineData("Europe/Amsterdam", "2026-12-11T22:30:00Z", "2026-12-11")]
    [InlineData("America/New_York", "2026-10-12T02:30:00Z", "2026-10-11")]
    [InlineData("UTC", "2026-10-11T22:30:00Z", "2026-10-11")]
    public void Today_IsTheDateInTheConfiguredTimeZone(string timeZone, string utcNow, string expectedToday)
    {
        var timeProvider = new FixedTimeProvider(DateTimeOffset.Parse(utcNow, CultureInfo.InvariantCulture));
        var calendar = new BusinessCalendar(timeProvider, Options.Create(new CalendarSettings { TimeZone = timeZone }));

        calendar.Today.Should().Be(DateOnly.Parse(expectedToday, CultureInfo.InvariantCulture));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
