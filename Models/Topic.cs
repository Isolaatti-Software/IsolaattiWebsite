using System;

namespace isolaatti_API.Models
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string About { get; set; }
    }
}