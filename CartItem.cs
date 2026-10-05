using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayazitovObuv
{
    public class CartItem
    {
        public StockItems Stock { get; set; }  // конкретная позиция склада (товар + размер)
        public int Quantity { get; set; }

        public string ProductName => Stock?.Products?.ProductName;
        public string PhotoPath => Stock?.Products?.PhotoPath;
        public decimal Price => Stock?.Products?.ProductCost ?? 0;
        public decimal SizeValue => Stock?.Sizes?.Size ?? 0;
        public decimal Total => Price * Quantity;
    }
}
