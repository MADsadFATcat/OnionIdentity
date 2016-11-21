using System.Data.Entity.ModelConfiguration;
using OnionIdentity.Model.Models;

namespace OnionIdentity.Data.Configuration
{
    internal class UserClaimConfiguration : EntityTypeConfiguration<UserClaim>
    {
        public UserClaimConfiguration()
        {
            ToTable("UserClaims");
        }
    }
}
