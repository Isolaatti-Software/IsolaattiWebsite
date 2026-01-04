using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.MediaStreaming;

[Authorize]
public class MyStreamingStations : IsolaattiPageModel
{
    public void OnGet()
    {
        
    }
}