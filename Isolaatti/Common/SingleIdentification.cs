namespace Isolaatti.Common
{
    public class SingleIdentification
    {
        public long Id { get; set; }
    }

    public class SingleIdentification<T>
    {
        public T Id { get; set; }
    }
}