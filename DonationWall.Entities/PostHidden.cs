using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Entities
{
    public class PostHidden : BaseEntity
    {
        public string Post_ID { get; set; }
        public DateTime HiddenUntil { get; set; }
    }
}
