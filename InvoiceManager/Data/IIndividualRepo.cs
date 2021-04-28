using InvoiceManager.Models;

namespace InvoiceManager.Data
{
    public interface IIndividualRepo
    {
         Individual GetIndividualById(int id);
    }
}