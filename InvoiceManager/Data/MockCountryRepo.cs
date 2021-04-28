using System.Collections.Generic;
using InvoiceManager.Models;
using System.Linq;
using System;

namespace InvoiceManager.Data
{
    public class MockCountryRepo : ICountryRepo
    {
        public Country GetCountryByName(string name)
        {
            var countries = MockCountryData();
            
            try
            {
                return countries.Where(x => x.Name.ToLower() == name.Trim().ToLower()).FirstOrDefault();
            }
            catch
            {
                throw new System.ArgumentException("Invalid Country");
            }
        }

        public string GetRegionFromCountryByName(string name)
        {
            var countries = MockCountryData();

           return countries.Where(x => x.Name.ToLower() == name.Trim().ToLower()).Select(x => x.Region).FirstOrDefault() ?? throw new System.ArgumentException("Invalid Country");
        }

        public double GetVatCodeFromCountryByName(string name)
        {
            var countries = MockCountryData();
            try
            {
                return countries.Where(x => x.Name.ToLower() == name.Trim().ToLower()).Select(x => x.VATCode).FirstOrDefault();
            }
            catch
            {
                throw new System.ArgumentException("Invalid Country");
            }
        }

        private List<Country> MockCountryData()
        {
            var countries = new List<Country>();
            countries.Add(new Country(){Id = 0, Name = "Lithuania", Region = "EU", VATCode = 21});
            countries.Add(new Country(){Id = 1, Name = "China", Region = "Azija", VATCode = 17});
            countries.Add(new Country(){Id = 2, Name = "Poland", Region = "EU", VATCode = 23});

            return countries;
        }
    }
}