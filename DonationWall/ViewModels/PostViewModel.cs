using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DonationWall.ViewModels
{
    public class PostViewModel
    {
        public Post  PostDetails { get; set; }
        public string ItemType { get; set; }
        public List<PostImages> PostImages { get; set; }
    }
    public class PostDetailViewModel
    {
        public Post PostDetail { get; set; }
        public List<PostImages> PostImages { get; set; }
        public List<PostVideos> PostVideo { get; set; }
    }
}