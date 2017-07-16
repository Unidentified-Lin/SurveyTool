using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SurveyTool.Models;

namespace SurveyTool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //
        // GET: /Roles/
        public ActionResult Index()
        {
            var roles = context.Roles.OrderBy(r => r.Name).ToList();
            return View(roles);
        }

        //
        // GET: /Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                context.Roles.Add(new IdentityRole()
                {
                    Name = collection["RoleName"]
                });
                context.SaveChanges();
                ViewBag.ResultMessage = "Role created successfully !";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Roles/Edit/5
        public ActionResult Edit(string roleName)
        {
            var thisRole = context.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            return View(thisRole);
        }

        //
        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdentityRole role)
        {
            try
            {
                context.Entry(role).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Roles/Delete/5
        public ActionResult Delete(string RoleName)
        {
            var thisRole = context.Roles.Where(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            context.Roles.Remove(thisRole);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        //public ActionResult ManageUserRoles()
        //{
        //    var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
        //    ViewBag.Roles = list;
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult RoleAddToUser(string UserName, string RoleName)
        //{
        //    ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

        //    UserManager.AddToRole(user.Id, RoleName);

        //    ViewBag.ResultMessage = "Role created successfully !";

        //    // prepopulat roles for the view dropdown
        //    var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
        //    ViewBag.Roles = list;

        //    return View("ManageUserRoles");
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult GetRoles(string UserName)
        //{
        //    if (!string.IsNullOrWhiteSpace(UserName))
        //    {
        //        //ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        //        ApplicationUser user = context.Users.Where(u => u.UserName == UserName).FirstOrDefault();

        //        ViewBag.RolesForThisUser = user != null ? UserManager.GetRoles(user.Id) : null;

        //        // prepopulat roles for the view dropdown
        //        var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
        //        ViewBag.Roles = list;
        //    }

        //    return View("ManageUserRoles");
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteRoleForUser(string UserName, string RoleName)
        //{
        //    ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

        //    if (UserManager.IsInRole(user.Id, RoleName))
        //    {
        //        UserManager.RemoveFromRole(user.Id, RoleName);
        //        ViewBag.ResultMessage = "Role removed from this user successfully !";
        //    }
        //    else
        //    {
        //        ViewBag.ResultMessage = "This user doesn't belong to selected role.";
        //    }

        //    // prepopulat roles for the view dropdown
        //    var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
        //    ViewBag.Roles = list;

        //    return View("ManageUserRoles");
        //}

        public ActionResult UserList()
        {
            var allusers = context.Users.OrderBy(u => u.UserName).ToList();

            var users = new List<UserViewModel>();

            allusers.ForEach(u => users.Add(new UserViewModel
            {
                Id = u.Id,
                Username = u.Name,
                Account = u.UserName,
                Role = UserManager.GetRoles(u.Id).SingleOrDefault()
            }));

            var model = new UserListViewModel { Users = users };

            return View(model);
        }

        public ActionResult EditUserRole(string UserName, string RoleName)
        {
            ViewBag.UserName = UserName;
            ViewBag.RoleName = RoleName;
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            return View();
        }

        [HttpPost]
        public ActionResult EditUserRole(string UserName, string oldRoleName, string newRoleName)
        {
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (String.IsNullOrEmpty(newRoleName))
            {
                ViewBag.UserName = UserName;
                ViewBag.RoleName = oldRoleName;
                var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                ViewBag.Roles = list;
                return View();
            }
            else if (String.IsNullOrEmpty(oldRoleName))
            {
                //加新的
                UserManager.AddToRole(user.Id, newRoleName);
                return RedirectToAction("UserList");
            }
            else
            {
                if (oldRoleName != newRoleName && UserManager.IsInRole(user.Id, oldRoleName))
                {
                    //刪原本
                    UserManager.RemoveFromRole(user.Id, oldRoleName);
                    //加新的
                    UserManager.AddToRole(user.Id, newRoleName);
                }
                return RedirectToAction("UserList");
            }
        }
    }
}
