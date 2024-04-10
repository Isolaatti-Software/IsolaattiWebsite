using System.Collections.Generic;

namespace Isolaatti.Images.Data;

public class ImagesToDeleteDto
{
    public IEnumerable<string> ImageIds { get; set; }
}