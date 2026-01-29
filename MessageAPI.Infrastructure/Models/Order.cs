using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Infrastructure.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
