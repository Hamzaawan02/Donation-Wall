using DonationWall.Database;
using DonationWall.Entities;
using DonationWall.Services;
using DonationWall.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace DonationWall.Controllers
{
    public class ProductController : Controller
    {
        private readonly DSContext _context;

        public ProductController()
        {
            _context = DSContext.Create();
        }



        // GET: Product
        public ActionResult DonateProduct(int id=0)
        {
            var itemtypes = ItemServices.Instance.GetAllItems();
            return View(itemtypes);
        }
        [HttpPost]
        public ActionResult DonateProduct()
        {
            try
            {
                string name = Request.Form["name"]?.ToString();
                string location = Request.Form["location"]?.ToString();
                string description = Request.Form["description"]?.ToString();

                var itemId = int.Parse(Request.Form["itemType"]);
                var item = ItemServices.Instance.GetItemById(itemId);

                Post mypost = new Post
                {
                    Post_ID = DateTime.Now.ToString("ddMMyyyyHHmmss"),
                    Name = name,
                    Location = location,
                    Description = description,
                    Item = item,
                    User_ID = User.Identity.GetUserId(),
                    Status = "Available"
                };

                PostServices.Instance.SavePost(mypost);

                var video = Request.Files["video"];
                if (video != null && video.ContentLength > 0)
                {
                    PostVideos postvideo = new PostVideos
                    {
                        VideoURL = SaveFile(video, "~/PostVideos/"),
                        Post_ID = mypost.Post_ID
                    };
                    PostVideoServices.Instance.SavePostVideo(postvideo);
                }

                var imagecounts = int.Parse(Request.Form["counts"]);
                for (int i = 0; i <= imagecounts; i++)
                {
                    var file = Request.Files["file_" + i];
                    if (file != null && file.ContentLength > 0)
                    {
                        PostImages postimage = new PostImages
                        {
                            ImageURL = SaveFile(file, "~/PostImages/"),
                            Post_ID = mypost.Post_ID
                        };
                        PostImageServices.Instance.SavePostimage(postimage);
                    }
                }

                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request. Please try again.";
                return RedirectToAction("DonateProduct");
            }
        }

        private string SaveFile(HttpPostedFileBase file, string path)
        {
            string physicalPath = Server.MapPath(path);

            if (!System.IO.Directory.Exists(physicalPath))
            {
                System.IO.Directory.CreateDirectory(physicalPath);
            }

            string filename = DateTime.Now.ToString("HHmmss") + "_" + Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(physicalPath, filename);

            file.SaveAs(fullPath);

            return Path.Combine(path, filename);
        }



        [HttpGet]
        public ActionResult PostDetail(string postID)
        {
            PostDetailViewModel model = new PostDetailViewModel
            {
                PostDetail = PostServices.Instance.GetAllPosts().FirstOrDefault(x => x.Post_ID == postID),
                PostImages = PostImageServices.Instance.GetAllPostImages(postID),
                PostVideo = PostVideoServices.Instance.GetAllPostVideos(postID)
            };
            return View(model);
        }


        
        [HttpPost]
        public ActionResult ExpressInterest(string id)
        {
            var userId = User.Identity.GetUserId();
            using (var context = DSContext.Create())
            {
                var post = context.Posts.FirstOrDefault(p => p.Post_ID == id);

                if (post != null)
                {
                    var interest = new Interest
                    {
                        Post_ID = id,
                        Accepter_ID = userId,
                        Status = "Pending",
                        ExpressedAt = DateTime.Now
                    };

                    context.Interests.Add(interest);
                    context.SaveChanges();

                    post.Status = "Hidden";
                    context.Entry(post).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("RequestStatus", "Home");
        }

        public void UnhideExpiredPosts()
        {
            using (var context = DSContext.Create())
            {
                var cutoffTime = DateTime.Now.AddHours(-24);
                var expiredPosts = context.Posts
                    .Where(p => p.Status == "Hidden" && p.HiddenAt <= cutoffTime)
                    .ToList();

                foreach (var post in expiredPosts)
                {
                    post.Status = "Available";
                    post.HiddenAt = null;  
                    context.Entry(post).State = System.Data.Entity.EntityState.Modified;
                }

                context.SaveChanges();
            }
        }


        // Delete Post Action
        [HttpPost]
        [Authorize(Roles = "Donor")]
        public ActionResult DeletePost(string postID)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Post_ID == postID);
            if (post != null)
            {
                var interests = _context.Interests.Where(i => i.Post_ID == postID).ToList();
                foreach (var interest in interests)
                {
                    _context.Interests.Remove(interest);
                }

                var postImages = _context.PostImages.Where(pi => pi.Post_ID == postID).ToList();
                foreach (var image in postImages)
                {
                    var imagePath = Server.MapPath("~/PostImages/" + image.ImageURL);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    _context.PostImages.Remove(image);
                }

                var postVideo = _context.PostVideos.FirstOrDefault(pv => pv.Post_ID == postID);
                if (postVideo != null)
                {
                    var videoPath = Server.MapPath("~/PostVideos/" + postVideo.VideoURL);
                    if (System.IO.File.Exists(videoPath))
                    {
                        System.IO.File.Delete(videoPath);
                    }
                    _context.PostVideos.Remove(postVideo);
                }

                _context.Posts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard", "Home");
        }

    }        
}
