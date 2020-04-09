using Store.Models.Data;
using Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Обьвляем List<CartVM>
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Проверяем не пуста ли корзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is Empty.";
                return View();
            }
            
            //Складываем сумму и передаем в ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;

            //Возвращаем лист в представление
            return View(cart);
        }
        public ActionResult CartPartial()
        {
            //Обьявляем модель CartVM
            CartVM model = new CartVM();

            //Переменная общего количевства
            int qty = 0;

            //Обьявляем переменную цены
            decimal price = 0m;

            //Проверяем сессию корзины
            if (Session["cart"] != null)
            {
                //Получаем общее количевсто товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //Или устанавливаем количество и цену в 0
                model.Quantity = 0;
                model.Price = 0m;
            }


            //Возвращаем частичное представление с моделью
            return PartialView("_CartPartial",model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Обьявляем List<CartVM>
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Обьялвяем модель CartVM
            CartVM model = new CartVM();

           
            using (Db db = new Db())
            {
                //Получаем продукт по Id
                ProductDTO product = db.Products.Find(id);

                //Проверяем находится ли товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.Id == id);

                //Если нет, то добавляем этот товар
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        Id = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                //Если да, добавляем единицу товара
                else
                {
                    productInCart.Quantity++;
                }
            }
            //Получаем общее количество товаров, цену, и добавляем данные в модель
            int qty = 0;
            decimal price = 0m;
            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Сохраняем состояние корзины в сессию
            Session["cart"] = cart;
            //Возвращаем частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }


        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Обьявлвяем List<CartVM>
            List<CartVM> cart = Session["cart"] as List<CartVM>;
                using (Db db = new Db())
            {
                //Получаем CartVM из List`а
                CartVM model = cart.FirstOrDefault(x => x.Id == productId);

                //Добавляем количество
                model.Quantity++;

                //Сохранеяем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                //Вернуть JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DecrementProduct(int productId)
        {
            //Обьявлвяем List<CartVM>
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                //Получаем CartVM из List`а
                CartVM model = cart.FirstOrDefault(x => x.Id == productId);

                //Отнимаем количество
                if (model.Quantity>1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                //Сохранеяем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                //Вернуть JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        
        public void RemoveProduct(int productId)
        {
            //Обьявлвяем List<CartVM>
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Получаем CartVM из List`а
                CartVM model = cart.FirstOrDefault(x => x.Id == productId);

                cart.Remove(model);
            }
        }
        
        //Обьявновление
        public ActionResult PaypalPartial()
        {
            //Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            //Возвращаем Частичное Представление с списком товаров
            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            //Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Получаем имя пользоваткля
            string userName = User.Identity.Name;

            //Инициализируем переменную для OrderId
            int orderId = 0;

            using (Db db = new Db())
            {
                //Обьявляем модель OrderDTO
                OrderDTO orderDto = new OrderDTO();

                //Получаем Id пользователя
                var q = db.Users.FirstOrDefault(x => x.UserName == userName);
                int userId = q.Id;

                //Заполняем модель OrderDTO данными
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;

                //добавляем модель в бд
                db.Orders.Add(orderDto);

                //Сохраняем модель
                db.SaveChanges();
                //Получаем Id заказа (OrderId)
                orderId = orderDto.OrderId;
                //обьявляем  модель OrderDetailsDTO 
                OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO();

                //Инициализируем модель OrderDetailsDTO данными
                foreach (var item in cart)
                {
                    orderDetailsDto.OrderId = orderId;
                    orderDetailsDto.UserId = userId;
                    orderDetailsDto.ProductId = item.Id;
                    orderDetailsDto.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDto);
                    db.SaveChanges();
                }
            }
            //Отправвляем письмо о заказе на почту Администратора
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("8b25f1503a65d0", "7ef26bc5a1bb69"),
                EnableSsl = true
            };
            client.Send("AnveanShop@example.com", "admin@example.com", "New Order", $"You have a new order. Order number: {orderId}");

            //Обновить сессию
            Session["cart"] = null;
        }
    }
}