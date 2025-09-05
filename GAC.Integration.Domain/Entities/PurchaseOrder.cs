using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAC.Integration.Domain.Entities
{
    public class PurchaseOrder
    {
        public string OrderId { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string CustomerId { get; set; }
        public List<PurchaseOrderItem> Items { get; set; } = new();
    }

    public class PurchaseOrderItem
    {
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}

