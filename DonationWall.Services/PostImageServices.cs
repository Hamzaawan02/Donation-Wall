using DonationWall.Database;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Services
{
    public class PostImageServices
    {
        #region Singleton
        public static PostImageServices Instance
        {
            get
            {
                if (instance == null) instance = new PostImageServices();
                return instance;
            }
        }
        private static PostImageServices instance { get; set; }
        private PostImageServices()
        {
        }
        #endregion

        public List<PostImages> GetAllPostImages(string id)
        {
            var context = new DSContext();
            return context.PostImages.Where(x => x.Post_ID == id).ToList();
        }
        public void SavePostimage(PostImages postimage)
        {
            var context = new DSContext();
            context.PostImages.Add(postimage);
            context.SaveChanges();
        }
        public void DeletePostImage(int ID)
        {
            var context = new DSContext();
            var target = context.PostImages.Where(x => x.ID == ID).FirstOrDefault();
            context.PostImages.Remove(target);
            context.SaveChanges();
        }
    }
}
