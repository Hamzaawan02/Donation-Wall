using Microsoft.AspNet.Identity.EntityFramework;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DonationWall.ViewModels
{
    public class UsersListingViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public string RoleID { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public string SearchTerm { get; set; }
    }

    public class UserActionModel
    {

        public string Role { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Contact { get; set; }
        public string Password { get; set; }
        public List<IdentityRole> Roles { get; set; }

    }

    public class UserRoleModel
    {
        public string UserID { get; set; }
        public IEnumerable<IdentityRole> UserRoles { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }

    }
}