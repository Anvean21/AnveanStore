using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            //Обьявляем модель типа List<CategoryVM>
            List<CategoryVM> categoryVMList;
            //Инициализируем модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        // GET: Shop/Category/name
        public ActionResult Category(string name)
        {
            //Список типа лист
            List<ProductVM> productVMList;
            
            using (Db db = new Db())
            {
                //Получаем Id категорий
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //Инициализируем список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //Получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //Проверка на null
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.Category.Name;
                }
            }
            //Вернуть представление с моделью
            return View(productVMList);
        }

        // GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Обьявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            //обьявляем и нициализируем Id продукта
            int id = 0;
            using (Db db= new Db())
            {
                //Проверяем доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //Иницализируем модель productDTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Получаем Id
                id = dto.Id;
                //Инициализируем модель productVM данными
                model = new ProductVM(dto);
            }
            //Получаем изображение из галлереи
            model.GalleryImeges = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Product/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
            //Возвращаем модель в представление
            return View("ProductDetails",model);
        }
    }
}