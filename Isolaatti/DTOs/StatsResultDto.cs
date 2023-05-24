using System.Collections.Generic;

namespace Isolaatti.DTOs;

public class StatsResultDto
{
    public int LessThanOneMinute { get; set; }
    public int BetweenOneMinuteAndTwoMinutes { get; set; }
    public int MoreThanTwoMinutes { get; set; }
}