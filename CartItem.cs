using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayazitovObuv
{
    public class CartItem
    {
        public StockItems Stock { get; set; }
        public int Quantity { get; set; }

        public string ProductName => Stock.Products.ProductName;
        public decimal Price => Stock.Products.ProductCost;
        public decimal Total => Price * Quantity;
        public string PhotoPath => Stock.Products.PhotoPath;
    }
}
