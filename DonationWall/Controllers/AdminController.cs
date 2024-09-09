using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using DonationWall.Services;
using DonationWall.ViewModels;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DonationWall.Database;

namespace DonationWall.Controllers
{
    public class AdminController : Controller
    {
        private AMSignInManager _signInManager;
        private AMRolesManager _rolesManager;
        private AMUserManager _userManager;
        public AMUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AMUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public AMRolesManager RolesManager
        {
            get
            {
                return _rolesManager ?? HttpContext.GetOwinContext().GetUserManager<AMRolesManager>();
            }
            private set
            {
                _rolesManager = value;
            }
        }
        public AMSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<AMSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }


        // GET: Admin
        public ActionResult Index()
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            model.Name = user.Name;
            return View(model);
        }

        private readonly DSContext _context;

        public AdminController()
        {
            _context = DSContext.Create();
        }

        public ActionResult Dashboard(string type = "", string location = "")
        {
            // Fetching the item types from the database and passing them to the view
            var itemTypes = _context.Items.Select(i => i.ItemType).Distinct().ToList();
            ViewBag.ItemTypes = itemTypes;

            List<Post> posts = new List<Post>();

            // Filtering posts based on user role
            if (User.IsInRole("Donor"))
            {
                posts = PostServices.Instance.GetAllPostsByUserID(User.Identity.GetUserId());
            }
            else if (User.IsInRole("Accepter"))
            {
                posts = PostServices.Instance.GetAllPosts();
            }

            // Applying filters for type and location
            if (!string.IsNullOrEmpty(type))
            {
                posts = posts.Where(p => p.Item .ItemType == type).ToList();
            }

            if (!string.IsNullOrEmpty(location))
            {
                posts = posts.Where(p => p.Location.Contains(location)).ToList();
            }

            List<PostViewModel> Model = new List<PostViewModel>();

            foreach (var item in posts)
            {
                var postimages = PostImageServices.Instance.GetAllPostImages(item.Post_ID);
                Model.Add(new PostViewModel
                {
                    PostDetails = item,
                    PostImages = postimages
                });
            }

            ViewBag.Location = location;

            return View(Model);
        }



        [HttpPost]
        public ActionResult Dashboard(string SearchTerm)
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            model.SignedInUser = user;
            return View(model);
        }


    }
}