using InvoiceManager.Models;

namespace InvoiceManager.Services
{
    public class InvoiceService : IInvoiceService
    {
        public double CalculateAmountWithVAT(Individual provider, Individual client, double amount)
        {
            if (provider.VATPayer)
            {
                if (client.Country.Trim().ToLower() == provider.Country.Trim().ToLower())
                    return amount * (1 - client.VATCode / 100);
                if (client.Region.Trim().ToLower() != "europe")
                    return amount;
                if (!client.VATPayer && client.Country.Trim().ToLower() != provider.Country.Trim().ToLower())
                    return amount * (1 - client.VATCode / 100);
                if (client.VATPayer && client.Country.Trim().ToLower() != provider.Country.Trim().ToLower())
                    return amount;
            }
            return amount;
        }
    }
}
