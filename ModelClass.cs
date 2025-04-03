using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InlineArryBenchmark
{
    public class Order
    {
        // Parameterless constructor for EF Core
        private Order() { }

        public Order(string customerName, DateTime orderDate, decimal amount, bool isShipped, List<OrderItem> Items)
        {
            CustomerName = customerName;
            OrderDate = orderDate;
            Amount = amount;
            IsShipped = isShipped;
            Items = new List<OrderItem>();
        }

        public int Id { get; private set; }
        public string CustomerName { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal Amount { get; private set; }
        public bool IsShipped { get; private set; }
        public List<OrderItem> Items { get; private set; }
    }

    public class OrderItem
    {
        // Parameterless constructor for EF Core
        private OrderItem() { }

        public OrderItem(string productName, int quantity, decimal price)
        {
            ProductName = productName;
            Quantity = quantity;
            Price = price;
        }

        public int Id { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }
    }
}
