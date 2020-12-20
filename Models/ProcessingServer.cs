/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
namespace isolaatti_API.Models
{
    public class ProcessingServerList
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public int OnQueue { get; set; }
    }
}