using System;

namespace Isolaatti.Utils.Attributes;

/// <summary>
/// Indicates that the Controller must receive the "sessionToken" http header and perform user auth
/// </summary>
public class IsolaattiAuth : Attribute
{
    
}