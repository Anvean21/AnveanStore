using PagedList;
using Store.Areas.Admin.Models.VIewModels.Shop;
using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // Обьявляем модель типа List
            List<CategoryVM> categoryVMlist;
            //Инициализируем модель данными
            using(Db db = new Db())
            {
                categoryVMlist = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            //Возвращаем List в представление
            return View(categoryVMlist);
        }
        [HttpPost]
        // Post: Admin/Shop/AddNewCategory
        public string AddNewCategory(string catName)
        {
            //Обьявляем строковую переменную ID
            string id;

            using(Db db = new Db())
            {
                //проверить имя категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }
                //Иниц. модель DTO
                CategoryDTO dto = new CategoryDTO();
                //заполняем данными модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                //Сохраняем
                db.Categories.Add(dto);
                db.SaveChanges();
                //Получаем Id чтобы вернутть его в представление
                id = dto.Id.ToString();
            }

            //Возвращаем Id в представление
            return id;
        }
        //Post: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Начальный Счетчик 
                int count = 1;
                //Обьявлякем модель данных
                CategoryDTO dto;
                //Устанавлтиваем сортировку для каждой категории
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }
        //GET: Admin/Shop/DeleteCategory/id
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получаем категорию
                CategoryDTO dto = db.Categories.Find(id);

                //Удаляем категорию
                db.Categories.Remove(dto);

                //Сохраним изменения в базе
                db.SaveChanges();
            }
            //Добавляем сообщение об успешном удалении категории
            TempData["SM"] = "You`ve deleted a category";

            //Переадресация пользователя
            return RedirectToAction("Categories");
        }
        //Post: Admin/Shop/RenameCategory/newcatname/id
        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using(Db db = new Db())
            {
            //Проверяем имя на уникальность
            if (db.Categories.Any(x => x.Name == newCatName))
            {
                    return "titletaken";
            }
                //получаем модель DTO
                CategoryDTO dto = db.Categories.Find(id);
                //Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                //Cохранить изменения
                db.SaveChanges();
            }
            //возвращаем слово
            return "ok";
        }

        //Метод добавления товаров
        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Обьявляем модель данных 
            ProductVM model = new ProductVM();
            //Добавляем список категорий из базы в модель
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            //Возвращаем модель в представление
            return View(model);
        }

        //Post: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
                
            }
            //Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is already taken!");
                    return View(model);
                }
                if (model.Price <= 0)
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Price cann`t be less than 0!");
                    return View(model);
                }
            }
            
            //Обьявляем переменную ProductId
            int id;

            //Инициалоизируем и сохраняем в базу модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Price = model.Price;
                product.Description = model.Description;
                product.CategoryId = model.CategoryId;


                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }
            //Добавляем temрdata

            TempData["SM"] = "You`ve added a product!";
            #region Upload Image

            //Создаём необходимые ссылки дерикторий
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");

            var pathString2 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString());

            var pathString3 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Thumbs");

            var pathString4 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Gallery");

            var pathString5 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Проверяем наличии директорий (если нет, создаём)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Проверяем был ли файл загружен 
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();
                //Проверяем расширение файла
                if (ext != "image/jpg" && 
                    ext != "image/jpeg" &&
                    ext != "image/pgpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The imge was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
           
            // обьявляем Переменную с именем изоброжения
            string imageName = file.FileName;

            //Добавляем имя изоб. в модель DTO
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.ImageName = imageName;

                db.SaveChanges();
            }
                //Назначаем путu к ориг и уменьш изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //Сохраняем ориг изображение
                file.SaveAs(path);

                //Создем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }
            #endregion

            //Редирект пользователя
            return RedirectToAction("AddProduct");
        }

        //Get:Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Обьявляем ProductVM типa List
            List<ProductVM> listOfProduvtVM;

            //Устанавливаем номер страницы
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Инициализируем List и заполняем данными
                listOfProduvtVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //Заполнить наши категории для сортировки
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }

            //Устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProduvtVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //Возвращаем представление с данными
            return View(listOfProduvtVM);
        }

        //Get:Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Обьявляем модель ProductVM
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                //Получаем продукт
                ProductDTO dto = db.Products.Find(id);

                //Проверяем доступен ли продукт
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                //Инициализируем модель данными
                model = new ProductVM(dto);

                //Создаём список наших категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Получить все изображения из гарелеи
                model.GalleryImeges = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Product/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            //Вернуть модель в представление
            return View(model);
        }

        //Post:Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Получаем Id продукта
            int id = model.Id;
            //Заполнить наш список категориями и изображениями
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
                model.GalleryImeges = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Product/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
           
            using (Db db = new Db())
            {
                //Проверяем продукт и имя на уникпальность
                if (db.Products.Where(x => x.Id != id ).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
                if (model.Price <= 0)
                {
                    ModelState.AddModelError("", "Price cann`t be less than 0!");
                    return View(model);
                }
            }
            //Oбновляем продукт
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Price = model.Price;
                dto.Description = model.Description;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            //Устанавливаем сообщение в TempData
            TempData["SM"] = "You have edited the product!";

            //Логика обработки изображений
            #region ImageUpload

            //Проверяем был ли файл загружен 
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();
                //Проверяем расширение файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pgpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The imge was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                //Устанавливаем пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Thumbs");

                //Удаляем существующие файлы и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                
                // Cохраняем имя изображения
                string imageName = file.FileName;

                //Добавляем имя изоб. в модель DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //Назначаем путu к ориг и уменьш изображению
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Сохраняем ориг изображение
                file.SaveAs(path);

                //Создем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion
            //Редирект пользователя
            return RedirectToAction("EditProduct");
        }

        //Get:Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            //удаляем товар из БД
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id); ;
                db.Products.Remove(dto);
                db.SaveChanges();

            }
            //Удаляем директории товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }
            //редирект
            return RedirectToAction("Products");
        }

        //Post:Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Перебираем все полученные нами файлы
            foreach (string fileName in Request.Files)
            {
                //Инициализируем эти файлы
                HttpPostedFileBase file = Request.Files[fileName];

                //Проверяем на null
                if (file != null && file.ContentLength > 0)
                {
                    //Назначаем пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                    var pathString1 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Gallery");
                    var pathString2 = Path.Combine(originalDirectory.ToString(), "Product\\" + id.ToString() + "\\Gallery\\Thumbs");
                    //Назначаем пути самих изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    //Сохраняем оригинальный и уменьшенные копии
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }
            }
        }

        //Post:Admin/Shop/DeleteImage/id/imageName
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Product/" + id.ToString() + "/Gallery" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Product/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
            {
                System.IO.File.Delete(fullPath1);
            }
            if (System.IO.File.Exists(fullPath2))
            {
                System.IO.File.Delete(fullPath2);
            }
        }

        //Метод вывода всех заказов для администратора
        //Get: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                //Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();


                //Перебираем данные модели OrderVM 
                foreach (var order in orders)
                {
                    //Инициализируем словарь товаров
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    //Создаем переменную для общей суммы
                    decimal total = 0m;
                    //Инициализируем List<OrderDetailsDTO>
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Получаем имя пользователя

                    // UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string userName = user.UserName;

                    //Перебираем список товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Получаем товар который относится к нашему юзеру
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем цену товара
                        decimal price = product.Price;
                        //Получаем название товара
                        string productName = product.Name;
                        //Добавлвяем товар в словарь
                        productsAndQuantity.Add(productName, orderDetails.Quantity);
                        //Получаем полную стоимость товаров пользователя
                        total += orderDetails.Quantity * price;
                    }

                    //Добавлвяем данные в модель OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductsAndQuantity = productsAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            //Вернуть наше представление вместе с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }
}