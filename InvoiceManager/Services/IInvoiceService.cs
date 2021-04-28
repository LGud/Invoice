using InvoiceManager.Models;

namespace InvoiceManager.Services
{
    public interface IInvoiceService
    {
        double CalculateTotalAmount(int providerId, int clientId, double amount);
    }
}
