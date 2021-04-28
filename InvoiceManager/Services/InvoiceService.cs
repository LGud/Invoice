using InvoiceManager.Models;
using InvoiceManager.Data;

namespace InvoiceManager.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ICountryRepo _countryRepo;

        public InvoiceService(ICountryRepo countryRepo)
        {
            _countryRepo = countryRepo;
        }
        public double CalculateTotalAmount(Individual provider, Individual client, double amount)
        {
            if (provider.IsPerson)
                throw new System.ArgumentException("Provider can not be a person");
            if (!provider.VATPayer)
                return amount;
            if (client.Country.Trim().ToLower() == provider.Country.Trim().ToLower())
                return amount * (1 - _countryRepo.GetVatCodeFromCountryByName(client.Country) / 100);
            if (_countryRepo.GetRegionFromCountryByName(client.Country).ToLower() != "eu")
                return amount;
            if (!client.VATPayer && client.Country.Trim().ToLower() != provider.Country.Trim().ToLower())
                return amount * (1 - _countryRepo.GetVatCodeFromCountryByName(client.Country) / 100);
            if (client.VATPayer && client.Country.Trim().ToLower() != provider.Country.Trim().ToLower())
                return amount;

            return amount;
        }
    }
}
