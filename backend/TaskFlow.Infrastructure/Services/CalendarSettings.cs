namespace TaskFlow.Infrastructure.Services;

public sealed class CalendarSettings
{
    public const string SectionName = "Calendar";

    /// <summary>The time zone of the team as an IANA id, such as "Europe/Amsterdam"; Windows ids work too.</summary>
    public string TimeZone { get; set; } = "Europe/Amsterdam";
}
