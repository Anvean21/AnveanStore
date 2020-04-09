using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    
    public class PagesController : Controller
    {
       
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //С бд получить инфу о колве страниц (Обьявляем список для представления(PageVM))
            List<PageVM> pageList;

            //Инициализацируем список (DB)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList(); //Выборка из бд
            }

            //Возвращаем список в представление
            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // Post: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                //Обьявляем переменную для краткого описания (slug)
                string slug;

                //Инициализируем класс PageDTO
                PagesDTO dto = new PagesDTO();

                //Присваемаем заголовок модели
                dto.Title = model.Title.ToUpper();
                //Проверяем есть ли краткое описание, если нет присваиваем его
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Проверка заголовка и к.о. на уникальность
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //Присваевам оставшиеся значения нашей модели
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100; //чтобы довлялось сразу в конец списка

                //Сохраняем модель в бд
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Передаем сообщение через TempData
            TempData["SM"] = "You has added a new page!";

            //Переадресация пользователя на метод INDEX
            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {

            //Обьявляем модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаем Id страницы (саму страницу по Id)
                PagesDTO dto = db.Pages.Find(id);

                //Проверяем доступна ли страница (Валидация)
                if(dto == null)
                {
                    return Content("The page does not exist.");
                }

                // Инициализируем модель данными DTO через конструктор в классе pageVM
                model = new PageVM(dto);
            }
            //Возвращаем модель в представление
            return View(model);
        }

        // Post: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Получаем id страницы
                int id = model.Id;

                //Обьявляем переменную краткого заголовка
                string slug = "home";
                //Получаем страницу по Id
                PagesDTO dto = db.Pages.Find(id);
                //Присваиваем название из полученной модели в DTO
                dto.Title = model.Title;

                //Проверяем Slug (краткий заголовок) и присваиваем его, если это необходимо
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

                //Проверяем Slug и Title на уникальность
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                //Присвоить остальные значения в класс DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Сохраняем изменения в базу
                db.SaveChanges();
            }

            //Установить сообщение в TempData
            TempData["SM"] = "You have edited the page.";

            //Переадресация пользователя
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //Oбьявим модель PageVM
            PageVM model;

            using(Db db = new Db())
            {
                //Получаем страницу 
                PagesDTO dto = db.Pages.Find(id);

                //Подтверждаем что страница доступна
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Присваиваем модели информации из базы
                model = new PageVM(dto);
            }
            //Возвращаем модель представления
            return View(model);

        }

        //GET: Admin/Pages/DeletePage/id
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Удаляем страницу
                db.Pages.Remove(dto);

                //Сохраним изменения в базе
                db.SaveChanges();
            }
            //Добавляем сообщение об успешном удалении станицы
            TempData["SM"] = "You`ve deleted a page";

            //Переадресация пользователя
            return RedirectToAction("Index");
        }

        //Post: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //Начальный Счетчик 
                int count = 1;
                //Инициализируем моддель данных
                PagesDTO dto;
                //Устанавлтиваем сортировку для каждой страницы
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //Get: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Обьявляем модель
            SidebarVM model;

            //Получаем данные из бд (DTO)
            using(Db db = new Db())
            {
                //Заполнить модель которую мы обьявили
                SidebarDTO dto = db.Sidebars.Find(1); //Исправить на гибкое значение
                model = new SidebarVM(dto);
            }
            //Вернуть представление с моделью
            return View(model);
        }
        //Post: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Получить данные из DTO
                SidebarDTO dto = db.Sidebars.Find(1); //ID
                //Присвоить данные в Body
                dto.Body = model.Body;
                //Сохраняем
                db.SaveChanges();
            }
            //Темпдата сообщение
            TempData["SM"] = "You have edited sidebar";
            //Редирект
            return RedirectToAction("EditSidebar");
        }


    }
}