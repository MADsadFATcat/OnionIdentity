namespace OnionIdentity.Model.Models
{
    public class UserClaim
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
