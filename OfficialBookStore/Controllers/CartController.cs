using OfficialBookStore.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using OfficialBookStore.Models.Data;

namespace OfficialBookStore.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }


            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            int qty = 0;

            decimal price = 0m;

            if (Session["cart"] != null)
            {
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
                model.Quantity = 0;
                model.Price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Product.Find(id);

                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            Session["cart"] = cart;

            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                model.Quantity++;

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }

        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                OrderDTO orderDTO = new OrderDTO();

                var q = db.User.FirstOrDefault(x => x.UserName == username);
                int userId = q.Id;

                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Order.Add(orderDTO);

                db.SaveChanges();

                orderId = orderDTO.OrderId;

                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("ea6b1bda758d46", "eac432355b0b26"),
                EnableSsl = true
            };
            client.Send("master@example.com", "master@example.com", "New Order", "MASTER, YOU HAVE A NEW ORDER! Order number: " + orderId);
            Session["cart"] = null;
        }
    }
}