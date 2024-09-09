using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Services
{
    public class AMSignInManager : SignInManager<User, string>
    {
        public AMSignInManager(AMUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((AMUserManager)UserManager);
        }

        public static AMSignInManager Create(IdentityFactoryOptions<AMSignInManager> options, IOwinContext context)
        {
            return new AMSignInManager(context.GetUserManager<AMUserManager>(), context.Authentication);
        }
    }
}
