using System.Collections.Generic;

namespace Isolaatti.Common;

public class ListPayload<T>
{
    public List<T> Items { get; set; }
}