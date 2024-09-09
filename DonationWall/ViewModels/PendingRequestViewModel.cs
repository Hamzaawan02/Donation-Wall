using System;
using DonationWall.Entities;

namespace DonationWall.ViewModels
{
    public class PendingRequestViewModel
    {

        public Post Post { get; set; }
        public Interest Interest { get; set; }
        public string AccepterName { get; set; }
        public string AccepterEmail { get; set; }
        public string AccepterContact { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
    }
}
