using System.Collections.Generic;

namespace _25DH110387_LTW.Models.ViewModel
{
    public class OrderGroupVM
    {
        public string Status { get; set; }
        public int Total { get; set; }
        public bool Expanded { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class MyOrderVM
    {
        public string Tab { get; set; }
        public string Query { get; set; }
        public List<string> Statuses { get; set; }
        public List<OrderGroupVM> Groups { get; set; }
    }
}