using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using OfficialBookStore.Models.Data;
using OfficialBookStore.Models.ViewModels.Account;

namespace OfficialBookStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            // Confirm user is not logged in

            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            // Return view
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the user is valid

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.User.Any(x => x.UserName.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }


        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.User.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                // Add the DTO
                db.User.Add(userDTO);

                // Save
                db.SaveChanges();

                // Add to UserRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRole.Add(userRolesDTO);
                db.SaveChanges();
            }

            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // Redirect
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }


        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.User.FirstOrDefault(x => x.UserName == username);

                // Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            // Return partial view with model
            return PartialView(model);
        }

        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.User.FirstOrDefault(x => x.UserName == username);

                // Build model
                model = new UserProfileVM(dto);
            }

            // Return view with model
            return View("UserProfile", model);
        }
    }
}