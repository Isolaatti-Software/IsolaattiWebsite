namespace Isolaatti.Classes.ApiEndpointsRequestDataModels
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