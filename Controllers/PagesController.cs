using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        [HttpGet]
        public ActionResult Index(string page = "")
        {
            //Получаем или устанавливаем краткий заголовок (Slug)
            if (page == "")
            {
                page = "home";
            }

            //Обьявляем  модель и класс DTO
            PageVM model;
            PagesDTO dto;

            //Проверяем доступна ли текущая страница
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToActionPermanent("Index", new { page = "" });
                }
            }

            //Получаем контекст данных (DTO страницы)
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //Устанавливам закоголовок страницы (Title)
            ViewBag.PageTitle = dto.Title;

            //Проверяем боковую панель
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //Заполняем модель данными
            model = new PageVM(dto);

            //Возвращаем модель в представление
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //Инициализируем list PageVM
            List<PageVM> pageVMList;
            //Получаем все страницы, кроме home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x))
                    .ToList();
            }
            //Возвращаем частичное представление с List`ом данных
            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //Обьявляем модель
            SidebarVM model;
            //Инициализируем молдель данными
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);
                model = new SidebarVM(dto);
            }
            //Возвращаем модель в частичное представление
            return PartialView("_SidebarPartial", model);
        }
    }
}