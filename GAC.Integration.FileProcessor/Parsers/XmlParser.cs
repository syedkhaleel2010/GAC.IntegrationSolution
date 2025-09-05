using System.Xml.Linq;
using GAC.Integration.Domain.Entities;

namespace GAC.Integration.FileProcessor.Parsers
{
    public static class XmlParser
    {
        public static PurchaseOrder ParsePurchaseOrder(string xmlContent)
        {
            var xDoc = XDocument.Parse(xmlContent);

            var po = new PurchaseOrder
            {
                OrderId = xDoc.Root?.Element("LegacyOrderId")?.Value,
                ProcessingDate = DateTime.Parse(xDoc.Root?.Element("OrderDate")?.Value ?? DateTime.UtcNow.ToString()),
                CustomerId = xDoc.Root?.Element("CustID")?.Value
            };

            foreach (var item in xDoc.Descendants("Item"))
            {
                po.Items.Add(new PurchaseOrderItem
                {
                    ProductCode = item.Element("LegacyProductCode")?.Value,
                    Quantity = int.Parse(item.Element("Qty")?.Value ?? "0")
                });
            }

            return po;
        }
    }
}

