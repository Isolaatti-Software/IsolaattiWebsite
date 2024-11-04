using System.Collections.Generic;

namespace Isolaatti.Common;

public class ContentListWrapper<T>
{
    public List<T> Data { get; set; }
    public bool MoreContent { get; set; }
}