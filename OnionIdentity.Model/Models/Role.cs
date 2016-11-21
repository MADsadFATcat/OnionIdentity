using Microsoft.AspNet.Identity;

namespace OnionIdentity.Model.Models
{
    public class Role : IRole<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
