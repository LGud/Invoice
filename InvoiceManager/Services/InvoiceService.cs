using InvoiceManager.Models;
using InvoiceManager.Data;

namespace InvoiceManager.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ICountryRepo _countryRepo;
        private readonly IIndividualRepo _individualRepo;

        public InvoiceService(ICountryRepo countryRepo,IIndividualRepo individualRepo)
        {
            _countryRepo = countryRepo;
            _individualRepo = individualRepo;
        }
        public double CalculateTotalAmount(int providerId, int clientId, double amount)
        {
            var provider = _individualRepo.GetIndividualById(providerId);
            var client =  _individualRepo.GetIndividualById(clientId);

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
