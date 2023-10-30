using System.ComponentModel.DataAnnotations;

namespace Isolaatti.Accounts.SignUp.Data;
public class SignUpDto
{
    [EmailAddress]
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string DisplayName { get; set; }
}