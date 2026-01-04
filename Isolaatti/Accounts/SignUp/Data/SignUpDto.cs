using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Isolaatti.Accounts.SignUp.Data;
public class SignUpDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Code { get; set; }
}