using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DonationWall.ViewModels
{
    public class AdminViewModel
    {
        public User SignedInUser { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ProfilePictureUrl { get; set; }

    }

    
}

