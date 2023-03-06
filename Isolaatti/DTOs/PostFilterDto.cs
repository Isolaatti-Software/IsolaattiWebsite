namespace Isolaatti.DTOs;

public class PostFilterDto
{
    public string IncludeAudio { get; set; }
    public string IncludeFromSquads { get; set; }
    public DateRange DateRange { get; set; }
}

public class DateRange
{
    public bool Enabled { get; set; }
    public string From { get; set; }
    public string To { get; set; }
}