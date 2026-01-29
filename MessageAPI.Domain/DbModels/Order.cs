using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Domain.DbModels
{
    public class Order : EntityBase
    {
        public string OrderNo { get; set; } = null!;
    }
}
