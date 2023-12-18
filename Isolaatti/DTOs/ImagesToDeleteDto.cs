using System.Collections.Generic;

namespace Isolaatti.DTOs;

public class ImagesToDeleteDto
{
    public IEnumerable<string> ImageIds { get; set; }
}