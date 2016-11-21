using System.Data.Entity.ModelConfiguration;
using OnionIdentity.Model.Models;

namespace OnionIdentity.Data.Configuration
{
    internal class UserLoginConfiguration : EntityTypeConfiguration<UserLogin>
    {
        public UserLoginConfiguration()
        {
            ToTable("UserLogins");
            HasKey(c => new
            {
                c.LoginProvider, c.ProviderKey, c.UserId
            });
        }
    }
}
