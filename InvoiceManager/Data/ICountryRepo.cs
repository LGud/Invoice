using System.Collections.Generic;
using InvoiceManager.Models;

namespace InvoiceManager.Data
{
    public interface ICountryRepo
    {       
        Country GetCountryByName(string name); 
        string GetRegionFromCountryByName(string name);
        double GetVatCodeFromCountryByName(string name);

         
    }
}