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
        // admin pages
        public ActionResult Index()
        {
            List<PageVM> pagesList;


            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            return View(pagesList);
        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                PageDTO dto = new PageDTO();

                dto.Title = model.Title;

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title you entered already exists.");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "Successfully added a new page!";

            return RedirectToAction("AddPage");
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }



        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                int id = model.Id;

                string slug = "home";

                PageDTO dto = db.Pages.Find(id);

                dto.Title = model.Title;

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                     db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug you entered already exists.");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                db.SaveChanges();
            }

            TempData["SM"] = "Successfully edited the page!";

            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                db.Pages.Remove(dto);

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                PageDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }

        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;

            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                dto.Body = model.Body;

                db.SaveChanges();
            }

            TempData["SM"] = "Successfully edited the sidebar!";

            return RedirectToAction("EditSidebar");
        }


    }
}