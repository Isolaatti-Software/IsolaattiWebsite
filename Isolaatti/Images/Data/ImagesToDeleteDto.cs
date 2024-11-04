using System;
using System.Collections.Generic;

namespace Isolaatti.Images.Data;

public class ImagesToDeleteDto
{
    public IEnumerable<Guid> ImageIds { get; set; }
}