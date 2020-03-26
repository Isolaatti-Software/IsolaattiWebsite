namespace isolaatti_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Uid { get; set; }
        public bool emailValidated { get; set; }
    }
}