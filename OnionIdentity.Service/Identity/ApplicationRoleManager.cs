using Microsoft.AspNet.Identity;
using OnionIdentity.Data.Identity;
using OnionIdentity.Model.Models;

namespace OnionIdentity.Service.Identity
{
    public class ApplicationRoleManager : RoleManager<Role, int>
    {
        public ApplicationRoleManager(IRoleStore store) : base(store)
        {
        }
    }
}
