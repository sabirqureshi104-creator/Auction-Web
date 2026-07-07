namespace Auction_web.Models
{
    public class EditProfileViewModel
    {
        public string Username { get; set; } // Add this
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}