using System.Xml.Linq;
using System.Xml.Serialization;
using GAC.Integration.Domain.Entities;

namespace GAC.Integration.FileProcessor.Parsers
{
    public static class XmlParser
    {
        public static PurchaseOrder ParsePurchaseOrder(string xmlFilePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseOrder));
            using (FileStream fs = new FileStream(xmlFilePath, FileMode.Open))
            {
                return (PurchaseOrder)serializer.Deserialize(fs);
            }
        }

    }
}

