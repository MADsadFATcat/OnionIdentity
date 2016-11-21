using System.Data.Entity.ModelConfiguration;
using OnionIdentity.Model.Models;

namespace OnionIdentity.Data.Configuration
{
    internal class UserRoleConfiguration : EntityTypeConfiguration<UserRole>
    {
        public UserRoleConfiguration()
        {
            ToTable("UserRoles");
            HasKey(c => new { c.UserId, c.RoleId });
        }
    }
}
