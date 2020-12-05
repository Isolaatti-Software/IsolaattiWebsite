namespace isolaatti_API.Classes
{
    public class UserUsageStats
    {
        public UserUsageStats(long drums, long bass, long vocals, long other)
        {
            var total = drums + bass + vocals + other;
            Drums = (drums*100f)/total;
            Bass = (bass * 100f) / total;
            Vocals = (vocals * 100f) / total;
            Other = (other * 100f) / total;
        }
        public float Drums { get; }
        public float Bass { get; }
        public float Vocals { get; }
        public float Other { get; }
    }
}