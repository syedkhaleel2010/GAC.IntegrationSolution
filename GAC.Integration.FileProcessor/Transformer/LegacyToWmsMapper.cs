using GAC.Integration.Domain.Entities;

namespace GAC.Integration.FileProcessor.Transformers
{
    public class LegacyToWmsMapper
    {
        public static object MapPurchaseOrderToWms(PurchaseOrder po)
        {
            return new
            {
                orderId = po.ExternalOrderID,
                processingDate = po.ProcessingDate,
                customer = po.CustomerID,
                products = po.PurchaseOrderLineDto.Select(i => new
                {
                    code = i.ProductID,
                    qty = i.Quantity
                }).ToList()
            };
        }
    }
}
