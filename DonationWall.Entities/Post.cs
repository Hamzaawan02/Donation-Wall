using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Entities
{
    public class Post : BaseEntity
    {
        public string Post_ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public virtual Item Item { get; set; }
        public string User_ID { get; set; }
        public string Status { get; set; }
        public DateTime? HiddenAt { get; set; }
        public virtual ICollection<PostImages> PostImages { get; set; }
        public virtual ICollection<PostVideos> PostVideos { get; set; }

    }
}
