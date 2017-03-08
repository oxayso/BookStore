using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficialBookStore.Models.Data;
using OfficialBookStore.Models.ViewModels.Pages;

namespace OfficialBookStore.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare a list of PageVM
            List<PageVM> pagesList;


            using (Db db = new Db())
            {
                //Init list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();

            }

            //Return List
            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Declare slug
                string slug;

                // Init pageDTO
                PageDTO dto = new PageDTO();

                // DTO title
                dto.Title = model.Title;

                // Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                // Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug you entered already exists.");
                    return View(model);
                }

                // DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                // Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "Successfully added a new page!";

            // Redirect
            return RedirectToAction("AddPage");
        }


    }
}