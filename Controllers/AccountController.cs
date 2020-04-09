using Store.Models.Data;
using Store.Models.ViewModels.Account;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }


        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // Post: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //Проверяем соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match");
                return View("CreateAccount", model);
            }
            using (Db db = new Db())
            {
                //проверяем имя на уникальность
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", $"Username {model.UserName} is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                if (db.Users.Any(x => x.EmailAdress.Equals(model.EmailAdress)))
                {
                    ModelState.AddModelError("", $"Email {model.EmailAdress} is taken.");
                    model.EmailAdress = "";
                    return View("CreateAccount", model);
                }
                //Создаем экземлпяр класса контекста данных UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                //Добавляем данные в экземпляр класса
                db.Users.Add(userDTO);
                //Сохраняем данные
                db.SaveChanges();

                //Добавляем роль пользователю
                int id = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            //Записываем сообщение в TempData["SM"]
            TempData["SM"] = "You have been successfully registered";

            //переадресовываем пользователя

            return RedirectToAction("Login");
        }


        //Get: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Подтвердить что пользователь не авторизован
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }
            //Возвращаем представление
            return View();
        }


        //Post: Account/Login
        [HttpPost]
        public ActionResult Login(UserLoginVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Проверяем пользователя на валидность
            bool isValid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username of password.");
                    return View(model);

                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;
            //Обьялвяем модель UserNavPartialVM
            UserNavPartialVM model;
            using (Db db = new Db())
            {
                //Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);
                //Заполняем модель данными из контекста ( DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            //Возвращаем частичное представление с моделью
            return PartialView(model);
        }

        //Get: /account/user-profile
        [HttpGet]
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;
            //Обьявляем модель
            UserProfileVM model;
            using (Db db = new Db())
            {
                //Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                //Инициализируем модель данными
                model = new UserProfileVM(dto);
            }
            //Возвращаем модель в представление
            return View("UserProfile",model);
        }


        //Post: /account/user-profile
        [HttpPost]
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;

            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            //Проверяем пароль если пользователь его меняет
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                //Проверяем соответствие пароля
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password do not match");
                    return View("UserProfile", model);
                }
            }
            using (Db db = new Db())
            {
                //Полуачем имя пользователя
                string userName = User.Identity.Name;

                //Провекряем изменилось ли имя пользователя
                if (userName != model.UserName)
                {
                    userName = model.UserName;
                    userNameIsChanged = true;
                }

                //Проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x=> x.UserName == userName))
                {
                    ModelState.AddModelError("", $"Username {model.UserName} already exist.");
                    model.UserName = "";
                    return View("UserProfile", model);
                }
                //изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                //Сохраняем изменения
                db.SaveChanges();
            }

            //Устанавливаем сообщение в ТЕмпдата
            TempData["SM"] = "You have edited yout profile";
            if (!userNameIsChanged)
            {
                //Возвращаем предстваление с молделью
                return View("UserProfile", model);
            }
            else
            {
                return RedirectToAction("Logout");
            }
        }

        [HttpGet]
        [Authorize(Roles ="User")]
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();
            using (Db db = new Db())
            {
                //Получаем Id пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
                int userId = user.Id;

                //Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираем список товаров в OrderVM
                foreach (var order in orders)
                {
                    //Инициализируем словать товаро
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    //Обьявляем переменную конечно суммы
                    decimal total = 0m;
                    //Иницаилизруем модель OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Перебираем список OrderDetailsDTO
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
                    //Добавляем полученные данные в модель OrdersForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQuantity = productsAndQuantity,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //Возвращаем представление с моделью
            return View(ordersForUser);

        }


    }
}