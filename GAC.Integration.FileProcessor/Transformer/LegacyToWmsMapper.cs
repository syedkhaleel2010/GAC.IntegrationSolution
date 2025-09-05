using GAC.Integration.Domain.Entities;

namespace GAC.Integration.FileProcessor.Transformers
{
    public class LegacyToWmsMapper
    {
        public static object MapPurchaseOrderToWms(PurchaseOrder po)
        {
            return new
            {
                orderId = po.OrderId,
                processingDate = po.ProcessingDate,
                customer = po.CustomerId,
                products = po.Items.Select(i => new
                {
                    code = i.ProductCode,
                    qty = i.Quantity
                }).ToList()
            };
        }
    }
}
