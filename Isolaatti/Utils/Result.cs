using System;

namespace Isolaatti.Utils;

public class Result<D, RT>
{
    public RT ResultType { get; set; }
    public D? Data { get; set; }
}