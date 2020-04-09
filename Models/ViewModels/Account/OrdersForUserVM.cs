using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Store.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        [DisplayName("Total Price")]
        public decimal Total { get; set; }
        [DisplayName("Order Details")]
        public Dictionary<string, int> ProductsAndQuantity { get; set; }
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}