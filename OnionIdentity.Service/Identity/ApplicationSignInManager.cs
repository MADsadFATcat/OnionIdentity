using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OnionIdentity.Model.Models;
using Microsoft.AspNet.Identity;

namespace OnionIdentity.Service.Identity
{
    public class ApplicationSignInManager : SignInManager<User, int>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return CreateUserIdentityAsync((ApplicationUserManager)UserManager, user);
        }

        public static async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUserManager manager, User user)
        {
            var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}
