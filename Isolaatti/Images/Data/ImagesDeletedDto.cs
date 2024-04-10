using System.Collections.Generic;

namespace Isolaatti.Images.Data;

public class ImagesDeletedDto
{
    public bool Success { get; set; }
    public List<string> UnSuccessIds { get; set; }
}