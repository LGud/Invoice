using InvoiceManager.Models;

namespace InvoiceManager.Services
{
    public interface IInvoiceService
    {
        double CalculateTotalAmount(Individual provider, Individual client, double amount);
    }
}
