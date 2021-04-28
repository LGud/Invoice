using InvoiceManager.Models;
using InvoiceManager.Services;
using InvoiceManager.Data;
using NSubstitute;
using Xunit;

namespace InvoiceManagerTests
{
    public class InvoiceServiceTests
    {
        private readonly IInvoiceService _sut;
        private readonly ICountryRepo _countryRepo = Substitute.For<ICountryRepo>();
        private readonly IIndividualRepo _individualRepo = Substitute.For<IIndividualRepo>();
        
        public InvoiceServiceTests()
        {
            _sut = new InvoiceService(_countryRepo,_individualRepo);
        }

        //Kai paslaugų tiekėjas nėra juridinis asmuo 
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsPerson_ShouldReturnException()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "Tadas Padas", Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = false, IsPerson=true };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Country = "Lithuania", Adress = "Kaunas Kazkur.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;
            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);
            //act 
            var exception = Assert.Throws<System.ArgumentException>(()=> _sut.CalculateTotalAmount(provider.Id, client.Id, amount));

            //assert
            Assert.Equal("Provider can not be a person", exception.Message);
        }   

        //kai neranda salies
        [Fact]      
        public void CalculateTotalAmount_WhenCannotFindRegion_ShouldReturnException()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "Tadas Padas", Country = "Tunac", Adress = "Kaunas Kazkur.g 12", VATPayer = true, IsPerson=false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Country = "Dunac", Adress = "Kaunas Kazkur.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;
            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            _countryRepo.When(x=>x.GetRegionFromCountryByName(client.Country)).Do(x=>{throw new System.ArgumentException("Invalid Country");});

            //act 
            var exception = Assert.Throws<System.ArgumentException>(()=> _sut.CalculateTotalAmount(provider.Id, client.Id, amount));

            //assert
            Assert.Equal("Invalid Country", exception.Message);
        }        

        // Kai paslaugų tiekėjas nėra PVM mokėtojas - PVM mokestis nuo užsakymo sumos nėra skaičiuojamas.
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsNotVATPayer_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas",Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = false, IsPerson = false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo",Country = "Lithuania", Adress = "Kaunas Kazkur.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;
            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            //act       
            var result = _sut.CalculateTotalAmount(provider.Id, client.Id, amount);

            //assert
            Assert.Equal(amount, result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas už EU (Europos sąjungos) ribų - PVM taikomas 0%
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsVATPayerAndClientIsOutOfEuropeBorder_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas",Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = true, IsPerson = false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo",Country = "China", Adress = "King Kong.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;

            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            _countryRepo.GetRegionFromCountryByName(client.Country).Returns("Azija");

            //act          
            var result = _sut.CalculateTotalAmount(provider.Id, client.Id, amount);

            //assert
            Assert.Equal(amount, result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra ne PVM mokėtojas, bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. 
        // Taikomas PVM x%, kur x - toje šalyje taikomas PVM procentas, pvz.: Lietuva 21 % PVM
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsVATPayerAndClientLivesInEuropeAndClientIsNotVATPayerAndLiveInDifferentCountries_ShouldReturnAmoutWithVAT()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas",Country = "Poland", Adress = "Kaunas Kazkur.g 12", VATPayer = true, IsPerson = false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = false, IsPerson = false };
            var amount = 123.321;

            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            _countryRepo.GetRegionFromCountryByName(client.Country).Returns("EU");
            _countryRepo.GetVatCodeFromCountryByName(client.Country).Returns(21);

            //act          
            var result = _sut.CalculateTotalAmount(provider.Id, client.Id, amount);

            //assert
            Assert.Equal(amount * (1 - _countryRepo.GetVatCodeFromCountryByName(client.Country) / 100), result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra PVM mokėtojas, , bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. 
        //Taikomas 0% pagal atvirkštinį apmokęstinimą.
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsVATPayerAndClientLivesInEuropeAndClientIsVATPayerAndLiveInDifferentCountries_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas",Country = "Poland", Adress = "Kaunas Kazkur.g 12", VATPayer = true, IsPerson = false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo",Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;

            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            _countryRepo.GetRegionFromCountryByName(client.Country).Returns("EU");
            _countryRepo.GetVatCodeFromCountryByName(client.Country).Returns(21);

            //act          
            var result = _sut.CalculateTotalAmount(provider.Id, client.Id, amount);

            //assert
            Assert.Equal(amount, result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas kai užsakovas ir paslaugų tiekėjas gyvena toje pačioje šalyje - visada taikomas PVM
        [Fact]
        public void CalculateTotalAmount_WhenProviderIsVATPayerAndBothLiveInTheSameCountry_ShouldReturnAmoutWithVAT()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas",Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = true, IsPerson = false };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo",Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = true, IsPerson = false };
            var amount = 123.321;

            _individualRepo.GetIndividualById(provider.Id).Returns(provider);
            _individualRepo.GetIndividualById(client.Id).Returns(client);

            _countryRepo.GetRegionFromCountryByName(client.Country).Returns("EU");
            _countryRepo.GetVatCodeFromCountryByName(client.Country).Returns(21);

            //act          
            var result = _sut.CalculateTotalAmount(provider.Id, client.Id, amount);

            //assert
            Assert.Equal(amount * (1 - _countryRepo.GetVatCodeFromCountryByName(client.Country) / 100), result);
        }
    }
}
