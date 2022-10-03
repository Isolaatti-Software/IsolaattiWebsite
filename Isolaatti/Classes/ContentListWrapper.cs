using System.Collections.Generic;

namespace Isolaatti.Classes;

public class ContentListWrapper<T> where T : class
{
    public List<T> Data { get; set; }
    public bool MoreContent { get; set; }
}