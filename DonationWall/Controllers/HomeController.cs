using DonationWall.Database;
using DonationWall.Entities;
using DonationWall.Services;
using DonationWall.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace DonationWall.Controllers
{
    public class HomeController : Controller
    {
        private readonly DSContext _context;

        public HomeController()
        {
            _context = DSContext.Create();
        }


        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            ViewBag.HasAcceptedOath = user?.HasAcceptedOath ?? false;

            if (User.IsInRole("Accepter") && user != null && !user.HasAcceptedOath)
            {
                return RedirectToAction("Oath", "Account");
            }
            ViewBag.ProfilePictureUrl = user?.ProfilePictureUrl ?? "/Content/UserProfiles/default.jpg";
            ViewBag.Name = user?.Name;
            var itemTypes = _context.Items.Select(it => it.ItemType).Distinct().ToList();
            ViewBag.ItemTypes = itemTypes;

            List<Post> posts = new List<Post>();

            if (User.IsInRole("Donor"))
            {
                posts = PostServices.Instance.GetAllPostsByUserID(userId)
                                             .Where(p => p.Status == "Available" || p.Status == "Pending" || p.Status == "Accepted")
                                             .ToList();
            }
            else if (User.IsInRole("Accepter"))
            {
                posts = PostServices.Instance.GetAllPosts()
                                             .Where(p => p.Status == "Available" &&
                                                         !_context.Interests.Any(i => i.Post_ID == p.Post_ID && i.Status != "Rejected"))
                                             .ToList();
            }

            List<PostViewModel> model = new List<PostViewModel>();

            foreach (var item in posts)
            {
                var postImages = PostImageServices.Instance.GetAllPostImages(item.Post_ID);
                model.Add(new PostViewModel
                {
                    PostDetails = item,
                    PostImages = postImages
                });
            }

            return View("~/Views/Admin/Dashboard.cshtml", model);
        }




        [Authorize(Roles = "Donor")]
        public ActionResult PendingRequests()
        {
            string loggedInUserId = User.Identity.GetUserId();

            var userPosts = _context.Posts
                .Where(p => p.User_ID == loggedInUserId)
                .Select(p => p.Post_ID)
                .ToList();

            var pendingRequests = _context.Interests
                .Where(i => (i.Status == "Pending" || i.Status == "Accepted") && userPosts.Contains(i.Post_ID))
                .ToList()
                .Select(i => new PendingRequestViewModel
                {
                    Post = _context.Posts.FirstOrDefault(p => p.Post_ID == i.Post_ID),
                    Interest = i,
                    AccepterName = _context.Users.Where(u => u.Id == i.Accepter_ID).Select(u => u.Name).FirstOrDefault(),
                    AccepterEmail = _context.Users.FirstOrDefault(u => u.Id == i.Accepter_ID)?.Email,
                    AccepterContact = _context.Users.FirstOrDefault(u => u.Id == i.Accepter_ID)?.PhoneNumber,
                    ImageUrl = _context.PostImages.FirstOrDefault(pi => pi.Post_ID == i.Post_ID)?.ImageURL 
                }).ToList();

            return View(pendingRequests);
        }




        [Authorize(Roles = "Accepter")]
        public ActionResult RequestStatus()
        {
            var userId = User.Identity.GetUserId();

            var requests = _context.Interests
                .Where(i => i.Accepter_ID == userId && (i.Status == "Pending" || i.Status == "Accepted"))
                .ToList()
                .Select(i => new RequestStatusViewModel
                {
                    Post = _context.Posts.FirstOrDefault(p => p.Post_ID == i.Post_ID),
                    Interest = i,
                    AccepterName = _context.Users.FirstOrDefault(u => u.Id == i.Accepter_ID)?.Name,
                    AccepterEmail = _context.Users.FirstOrDefault(u => u.Id == i.Accepter_ID)?.Email,
                    AccepterContact = _context.Users.FirstOrDefault(u => u.Id == i.Accepter_ID)?.PhoneNumber,
                    ImageUrl = _context.PostImages.FirstOrDefault(pi => pi.Post_ID == i.Post_ID)?.ImageURL,
                }).ToList();

            return View(requests);
        }

        [Authorize(Roles = "Donor")]
        public ActionResult AcceptRequest(int interestId)
        {
            using (var context = DSContext.Create())
            {
                var interest = context.Interests.FirstOrDefault(i => i.ID == interestId);

                if (interest != null)
                {
                    interest.Status = "Accepted";
                    var post = context.Posts.FirstOrDefault(p => p.Post_ID == interest.Post_ID);
                    if (post != null)
                    {
                        post.Status = "Accepted";
                    }

                    context.SaveChanges();
                }
            }

            return RedirectToAction("PendingRequests", "Home");
        }


        [HttpPost]
        [Authorize(Roles = "Donor")]
        public ActionResult RejectRequest(int interestId)
        {
            using (var context = DSContext.Create())
            {
                var interest = context.Interests.FirstOrDefault(i => i.ID == interestId);
                if (interest != null)
                {
                    interest.Status = "Rejected";

                    var post = context.Posts.FirstOrDefault(p => p.Post_ID == interest.Post_ID);
                    if (post != null && post.Status == "Hidden")
                    {
                        post.Status = "Available";
                    }

                    context.SaveChanges();
                }
            }
            return RedirectToAction("PendingRequests", "Home");
        }


        public ActionResult DonationHistory()
        {
            using (var context = new DSContext())
            {
                string loggedInUserId = User.Identity.GetUserId();
                var model = new List<DonationHistoryViewModel>();

                if (User.IsInRole("Donor"))
                {
                    model = context.DonationHistories
                        .Where(d => d.Donor_ID == loggedInUserId)
                        .Select(d => new DonationHistoryViewModel
                        {
                            Post_ID = d.Post_ID,
                            Name = d.Name,
                            Description = d.Description,
                            AccepterName = d.AccepterName,
                            AccepterContact = d.AccepterContact,
                            AccepterEmail = d.AccepterEmail,
                            Date = d.Date,
                            Status = d.Status
                        }).ToList();
                }
                else if (User.IsInRole("Accepter"))
                {
                    model = context.DonationHistories
                        .Where(d => d.Accepter_ID == loggedInUserId && d.Status == "Completed")
                        .Select(d => new DonationHistoryViewModel
                        {
                            Post_ID = d.Post_ID,
                            Name = d.Name,
                            Description = d.Description,
                            Date = d.Date,
                            Status = d.Status,
                            Post = d.Post,
                            Location = d.Post != null ? d.Post.Location : "N/A"
                        }).ToList();
                }



                return View(model);
            }
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }
    


    [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoneRequest(int interestId)
        {
            using (var context = new DSContext())
            {
                var interest = context.Interests.FirstOrDefault(i => i.ID == interestId);

                if (interest != null)
                {
                    var post = context.Posts.FirstOrDefault(p => p.Post_ID == interest.Post_ID);
                    if (post != null)
                    {
                        var donationHistory = new DonationHistory
                        {
                            Post_ID = post.Post_ID,
                            Donor_ID = post.User_ID,
                            DonorName = context.Users.FirstOrDefault(u => u.Id == post.User_ID)?.Name,
                            Accepter_ID = interest.Accepter_ID,
                            AccepterName = context.Users.FirstOrDefault(u => u.Id == interest.Accepter_ID)?.Name,
                            AccepterEmail = context.Users.FirstOrDefault(u => u.Id == interest.Accepter_ID)?.Email,
                            AccepterContact = context.Users.FirstOrDefault(u => u.Id == interest.Accepter_ID)?.PhoneNumber,
                            Date = DateTime.Now,
                            Name = post.Name,
                            Description = post.Description,
                            Status = "Completed",
                            Location = post.Location
                        };


                        context.DonationHistories.Add(donationHistory);
                        interest.Status = "Completed";

                        context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("PendingRequests");
        }
    }

}