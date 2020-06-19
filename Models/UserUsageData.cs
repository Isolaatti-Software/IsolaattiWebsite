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