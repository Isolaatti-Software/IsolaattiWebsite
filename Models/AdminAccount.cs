/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
namespace isolaatti_API.Models
{
    public class AdminAccount
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }
}