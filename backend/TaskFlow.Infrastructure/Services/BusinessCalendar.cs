using Microsoft.Extensions.Options;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Today's date in the team's time zone (Calendar:TimeZone): a task is overdue when it is not done and its due date lies
/// before this day. Timestamps such as DateTimeCreated stay in UTC.
/// </summary>
internal sealed class BusinessCalendar(TimeProvider timeProvider, IOptions<CalendarSettings> calendarSettings)
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(calendarSettings.Value.TimeZone);

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _timeZone).DateTime);
}
