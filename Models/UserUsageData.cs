/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
namespace isolaatti_API.Models
{
    public class UserUsageData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        // longs representing number of clicks on solo button of each instrument
        public long SoloOnDrums { get; set; }
        public long SoloOnBass { get; set; }
        public long SoloOnVocals { get; set; }
        public long SoloOnOther { get; set; }
    }
}