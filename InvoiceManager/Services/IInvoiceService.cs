using InvoiceManager.Models;

namespace InvoiceManager.Services
{
    public interface IInvoiceService
    {
        double CalculateAmountWithVAT(Individual provider, Individual client, double amount);
    }
}
