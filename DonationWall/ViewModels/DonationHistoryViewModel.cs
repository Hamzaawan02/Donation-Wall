using DonationWall.Entities;
using System;

namespace DonationWall.ViewModels
{
    public class DonationHistoryViewModel
    {
        public string Post_ID { get; set; }
        public string Donor_ID { get; set; }
        public string DonorName { get; set; }
        public string Accepter_ID { get; set; }
        public string AccepterName { get; set; }
        public string AccepterContact { get; set; }
        public string AccepterEmail { get; set; }
        public DateTime Date { get; set; }
        public Post Post { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}
