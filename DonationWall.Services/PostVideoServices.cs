using DonationWall.Database;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Services
{
    public class PostVideoServices
    {
        #region Singleton
        public static PostVideoServices Instance
        {
            get
            {
                if (instance == null) instance = new PostVideoServices();
                return instance;
            }
        }
        private static PostVideoServices instance { get; set; }
        private PostVideoServices()
        {
        }
        #endregion

        public List<PostVideos> GetAllPostVideos(string id)
        {
            var context = new DSContext();
            return context.PostVideos.Where(x => x.Post_ID == id).ToList();
        }
        public void SavePostVideo(PostVideos postvideo)
        {
            var context = new DSContext();
            context.PostVideos.Add(postvideo);
            context.SaveChanges();
        }
        public void DeletePostVideo(int ID)
        {
            var context = new DSContext();
            var target = context.PostVideos.Where(x => x.ID == ID).FirstOrDefault();
            context.PostVideos.Remove(target);
            context.SaveChanges();
        }
    }
}
