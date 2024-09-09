using DonationWall.Database;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DonationWall.Services
{
    public class PostServices
    {
        #region Singleton
        public static PostServices Instance
        {
            get
            {
                if (instance == null) instance = new PostServices();
                return instance;
            }
        }
        private static PostServices instance { get; set; }
        private PostServices()
        {
        }
        #endregion

        public List<Post> GetAllPostsByUserID(string id)
        {
            var context = new DSContext();
            return context.Posts.Where(x => x.User_ID == id && (x.Status == "Available" || x.Status == "Pending")).ToList();
        }

        public List<Post> GetAllPosts()
        {
            var context = new DSContext();
            return context.Posts.Where(x => x.Status == "Available").ToList();
        }

        public void SavePost(Post post)
        {
            var context = new DSContext();
            context.Posts.Add(post);
            context.SaveChanges();
        }

        public void DeletePost(int ID)
        {
            var context = new DSContext();
            var target = context.Posts.FirstOrDefault(x => x.ID == ID);
            if (target != null)
            {
                context.Posts.Remove(target);
                context.SaveChanges();
            }
        }

        public void UpdatePost(Post post, DSContext context)
        {
            var local = context.Set<Post>()
                .Local
                .FirstOrDefault(entry => entry.Post_ID == post.Post_ID);

            if (local != null)
            {
                context.Entry(local).State = System.Data.Entity.EntityState.Detached;
            }

            context.Entry(post).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
        }

    }

}
