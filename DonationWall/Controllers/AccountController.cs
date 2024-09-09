using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DonationWall.Models;
using DonationWall.Services;
using DonationWall.ViewModels;
using DonationWall.Entities;
using System.Net.Mail;
using System.IO;
using DonationWall.Database;

namespace DonationWall.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;

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

        private readonly DSContext _context;

        public AccountController()
        {
            _context = DSContext.Create();
        }


        [Authorize(Roles = "Accepter")]
        public ActionResult Oath()
        {
            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);

            if (user != null && user.HasAcceptedOath)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Accepter")]
        public ActionResult AcceptOath()
        {
            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId);

            if (user != null)
            {
                user.HasAcceptedOath = true;
                UserManager.Update(user);
                _context.SaveChanges(); 
            }

            return RedirectToAction("Dashboard", "Home");
        }




        private AMRolesManager _rolesManager;
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

        public AccountController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }



        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        Session["User"] = user.Name;
                        Session["ID"] = user.Id;

                        var cookie = new HttpCookie("UserProfile")
                        {
                            ["UserName"] = user.UserName,
                            ["ProfilePictureUrl"] = user.ProfilePictureUrl,
                            Expires = DateTime.Now.AddYears(1)
                        };
                        Response.Cookies.Add(cookie);

                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid email or password.");
                        return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);

            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.Roles = RolesManager.Roles.ToList();
            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await RolesManager.FindByIdAsync(model.RoleID);
                var user = new User {CNIC = model.CNIC, Address = model.Address, UserName = model.Email, Email = model.Email, PhoneNumber = model.Contact, Name = model.Name, Role = role.Name, Password = model.Password };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, role.Name);
                    //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    Random rand = new Random();
                    var randomcode = rand.Next(9999, 99999);
                    Session["code"] = randomcode;
                    var userdetail = UserManager.FindByEmail(model.Email);
                    EmailVerificationCodeSender(model.Email, randomcode);
                    Session["Userid"] = userdetail.Id;
                    return RedirectToAction("VerifyEmail");
                }
                AddErrors(result);
            } 

            // If we got this far, something failed, redisplay form
            return RedirectToAction("", "");
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult VerifyEmail(string Code)
        {
            if (Code == Session["code"].ToString())
            {
                var user = UserManager.FindById(Session["Userid"].ToString());
                user.EmailConfirmed = true;
                UserManager.Update(user);
                return RedirectToAction("", "");
            }
            else
            {
                return RedirectToAction("VerifyEmail");
            }

        }
        public void EmailVerificationCodeSender(string email, int code)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("donationdevar@gmail.com", "Donation Wall", System.Text.Encoding.UTF8);
                mail.Subject = "Signup Verification";
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = "<div style='background-color:#f2f7f2;padding:30px'><div style='background-color:#14a800;padding:30px'></div><div style='background-color:#ffffff;padding:30px;color:#001e00'><p dir='ltr'></p><h2>Hi, </h2><p dir='ltr'>You have requested to signup on our Portal so Your Verification Code => " + code + "<p dir='ltr'>Thanks,<br>Donation Wall Team</p></div><div style='padding:30px 30px 0px'><div style='color:#65735b;text-align:center'>Wall of Kindess 💕</div><div style='color:#65735b;text-align:center'>© 2024 DonationWall® Inc.</div></div></div>";
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("donationdevar@gmail.com", "bnolqzdkhkwlxhnm");
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Send(mail);
            }
            catch (Exception ex)
            {
                // Handle exception if needed
                // ex.Message can be logged or shown if necessary
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null && user.CNIC == model.CNIC)
                {
                    return RedirectToAction("ResetPassword", new { email = model.Email });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid CNIC or Email.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> UserProfile()
        {
            string userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new AdminViewModel
            {
                ID = user.Id,
                Name = user.UserName,
                Email = user.Email,
                Contact = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl ?? "/Content/UserProfiles/default.jpg"
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(AdminViewModel model, HttpPostedFileBase ProfilePicture)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.ID);
                if (user != null)
                {
                    user.UserName = model.Name;
                    user.Email = model.Email;
                    user.PhoneNumber = model.Contact;

                    if (ProfilePicture != null && ProfilePicture.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(ProfilePicture.FileName);
                        string extension = Path.GetExtension(ProfilePicture.FileName);
                        fileName = user.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                        string directoryPath = Server.MapPath("~/Content/UserProfiles/");

                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        string path = Path.Combine(directoryPath, fileName);
                        ProfilePicture.SaveAs(path);

                        user.ProfilePictureUrl = "/Content/UserProfiles/" + fileName;
                    }

                    if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                    {
                        var changePasswordResult = await UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);
                        if (!changePasswordResult.Succeeded)
                        {
                            AddErrors(changePasswordResult);
                            return View(model);
                        }
                    }

                    var updateResult = await UserManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        var cookie = new HttpCookie("UserProfile")
                        {
                            ["UserName"] = user.UserName,
                            ["ProfilePictureUrl"] = user.ProfilePictureUrl,
                            Expires = DateTime.Now.AddYears(1)
                        };
                        Response.Cookies.Add(cookie);

                        return RedirectToAction("UserProfile");
                    }

                    AddErrors(updateResult);
                }
                else
                {
                    ModelState.AddModelError("", "User not found.");
                }
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadProfilePicture(HttpPostedFileBase ProfilePicture)
        {
            if (ProfilePicture != null && ProfilePicture.ContentLength > 0)
            {
                string userId = User.Identity.GetUserId();
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(ProfilePicture.FileName);
                    string extension = Path.GetExtension(ProfilePicture.FileName);
                    fileName = userId + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                    string directoryPath = Server.MapPath("~/Content/UserProfiles/");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string path = Path.Combine(directoryPath, fileName);
                    ProfilePicture.SaveAs(path);

                    user.ProfilePictureUrl = "/Content/UserProfiles/" + fileName;
                    var result = await UserManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("UserProfile");
                    }
                    AddErrors(result);
                }
            }

            return RedirectToAction("UserProfile");
        }


        //
        // POST: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                ModelState.AddModelError("", "Invalid attempt.");
                return View();
            }

            // Reset the user's password
            var result = await UserManager.RemovePasswordAsync(user.Id);
            if (result.Succeeded)
            {
                result = await UserManager.AddPasswordAsync(user.Id, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
            }

            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Clear();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }




        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            var userID = Session["ID"].ToString();
            var user = UserManager.FindById(userID);
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            if ((UserManager.IsInRole(userID, "Accepter") || UserManager.IsInRole(userID, "Donor")) && user.EmailConfirmed)
            {
                return RedirectToAction("Dashboard", "Admin");

            }
            
            else
            {
                return RedirectToAction("", "");

            }
        }
    

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}