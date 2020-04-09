using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Store.Areas.Admin.Models.VIewModels.Shop
{
    public class OrdersForAdminVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DisplayName("Total Price")]
        public decimal Total { get; set; }
        [DisplayName("Order Details")]
        public Dictionary<string,int> ProductsAndQuantity { get; set; }
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}