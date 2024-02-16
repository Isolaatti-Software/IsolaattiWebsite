using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.MediaStreaming;

[IsolaattiAuth]
public class MyStreamingStations : IsolaattiPageModel
{
    public void OnGet()
    {
        
    }
}