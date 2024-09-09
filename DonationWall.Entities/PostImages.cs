using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Entities
{
    public class PostImages : BaseEntity
    {
        public string ImageURL { get; set; }
        public string Post_ID { get; set; }
    }
}
