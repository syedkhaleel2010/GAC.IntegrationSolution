using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GAC.Integration.Domain.Entities
{
    [XmlRoot("PurchaseOrder")]
    public class PurchaseOrder
    {
        public string ExternalOrderID { get; set; }
        public DateTime ProcessingDate { get; set; }
        public Guid CustomerID { get; set; }

        [XmlArray("PurchaseOrderLines")]
        [XmlArrayItem("LineItem")]
        public List<LineItem> PurchaseOrderLineDto { get; set; }
    }

    public class LineItem
    {
        public Guid PurchaseOrderID { get; set; }
        public Guid ProductID { get; set; }
        public int Quantity { get; set; }
    }



     
}

