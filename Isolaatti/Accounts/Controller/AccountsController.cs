using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Service;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Accounts.Controller;

[ApiController]
[Route("/api/profile")]
public class AccountsController(ProfileService accounts) : ProfileRequiredController
{
    
    /// <summary>
    /// Perform profile creation for authenticated user. Call this method only when you get 409 Conflict status code
    /// </summary>
    [HttpPost]
    [Route("perform_creation")]
    public async Task MakeProfile()
    {
        
    }
}