using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Entities
{
    public class Interest : BaseEntity
    {
        public string Post_ID { get; set; }
        public string Accepter_ID { get; set; }
        public string Status { get; set; }
        public DateTime ExpressedAt { get; set; }
       public string AccepterEmail { get; set; }
        public string AccepterContact { get; set; }
        public string AccepterName { get; set; } 
        public Post Post { get; set; }
    }
}
