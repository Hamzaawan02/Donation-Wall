using DonationWall.Entities;

namespace DonationWall.ViewModels
{
    public class RequestStatusViewModel
    {
        public Post Post { get; set; }
        public Interest Interest { get; set; }
        public string ImageUrl { get; set; }
        public string AccepterName { get; set; }  
        public string AccepterEmail { get; set; }
        public string AccepterContact { get; set; }
    }
}
