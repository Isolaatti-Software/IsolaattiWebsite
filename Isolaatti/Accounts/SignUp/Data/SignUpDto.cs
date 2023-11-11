using System.ComponentModel.DataAnnotations;

namespace Isolaatti.Accounts.SignUp.Data;
public class SignUpDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string DisplayName { get; set; }
    public string Code { get; set; }
}