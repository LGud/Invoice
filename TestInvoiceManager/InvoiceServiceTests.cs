using InvoiceManager.Models;
using InvoiceManager.Services;
using NSubstitute;
using Xunit;

namespace TestInvoiceManager
{
    public class InvoiceServiceTests
    {
        private readonly IInvoiceService _sut;
        private readonly IInvoiceService _invoiceService = Substitute.For<IInvoiceService>();

        public InvoiceServiceTests()
        {
            _sut = new InvoiceService();
        }

        // Kai paslaugų tiekėjas nėra PVM mokėtojas - PVM mokestis nuo užsakymo sumos nėra skaičiuojamas.
        [Fact]
        public void CalculateAmountWithVAT_WhenProviderIsNotVATPayer_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas", Region = "Europe", Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = false, VATCode = 0.0 };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Region = "Europe", Country = "Lithuania", Adress = "Kaunas Kazkur.g 15", VATPayer = true, VATCode = 21 };
            var amount = 123.321;
            _invoiceService.CalculateAmountWithVAT(provider, client, amount).Returns(amount);

            //act       
            var result = _sut.CalculateAmountWithVAT(provider, client, amount);

            //assert
            Assert.Equal(_invoiceService.CalculateAmountWithVAT(provider, client, amount), result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas už EU (Europos sąjungos) ribų - PVM taikomas 0%
        [Fact]
        public void CalculateAmountWithVAT_WhenProviderIsVATPayerAndClientIsOutOfEuropeBorder_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas", Region = "Europe", Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = true, VATCode = 21.0 };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Region = "Azija", Country = "China", Adress = "King Kong.g 15", VATPayer = true, VATCode = 17.0 };
            var amount = 123.321;
            _invoiceService.CalculateAmountWithVAT(provider, client, amount).Returns(amount);

            //act          
            var result = _sut.CalculateAmountWithVAT(provider, client, amount);

            //assert
            Assert.Equal(_invoiceService.CalculateAmountWithVAT(provider, client, amount), result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra ne PVM mokėtojas, bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. 
        // Taikomas PVM x%, kur x - toje šalyje taikomas PVM procentas, pvz.: Lietuva 21 % PVM
        [Fact]
        public void CalculateAmountWithVAT_WhenProviderIsVATPayerAndClientLivesInEuropeAndClientIsNotVATPayerAndLiveInDifferentCountries_ShouldReturnAmoutWithVAT()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas", Region = "Europe", Country = "Poland", Adress = "Kaunas Kazkur.g 12", VATPayer = true, VATCode = 23.0 };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Region = "Europe", Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = false, VATCode = 21.0 };
            var amount = 123.321;
            _invoiceService.CalculateAmountWithVAT(provider, client, amount).Returns(amount * (1 - client.VATCode / 100));

            //act          
            var result = _sut.CalculateAmountWithVAT(provider, client, amount);

            //assert
            Assert.Equal(_invoiceService.CalculateAmountWithVAT(provider, client, amount), result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra PVM mokėtojas, , bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. 
        //Taikomas 0% pagal atvirkštinį apmokęstinimą.
        [Fact]
        public void CalculateAmountWithVAT_WhenProviderIsVATPayerAndClientLivesInEuropeAndClientIsVATPayerAndLiveInDifferentCountries_ShouldReturnAmout()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas", Region = "Europe", Country = "Poland", Adress = "Kaunas Kazkur.g 12", VATPayer = true, VATCode = 23.0 };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Region = "Europe", Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = true, VATCode = 21.0 };
            var amount = 123.321;
            _invoiceService.CalculateAmountWithVAT(provider, client, amount).Returns(amount);

            //act          
            var result = _sut.CalculateAmountWithVAT(provider, client, amount);

            //assert
            Assert.Equal(_invoiceService.CalculateAmountWithVAT(provider, client, amount), result);
        }

        // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas kai užsakovas ir paslaugų tiekėjas gyvena toje pačioje šalyje - visada taikomas PVM
        [Fact]
        public void CalculateAmountWithVAT_WhenProviderIsVATPayerAndBothLiveInTheSameCountry_ShouldReturnAmoutWithVAT()
        {
            //arrange
            var provider = new Individual() { Id = 0, FullName = "UAB GerasTestas", Region = "Europe", Country = "Lithuania", Adress = "Kaunas Kazkur.g 12", VATPayer = true, VATCode = 21.0 };
            var client = new Individual() { Id = 1, FullName = "UAB GerasAsmuo", Region = "Europe", Country = "Lithuania", Adress = "King Kong.g 15", VATPayer = true, VATCode = 21.0 };
            var amount = 123.321;
            _invoiceService.CalculateAmountWithVAT(provider, client, amount).Returns(amount * (1 - client.VATCode / 100));

            //act          
            var result = _sut.CalculateAmountWithVAT(provider, client, amount);

            //assert
            Assert.Equal(_invoiceService.CalculateAmountWithVAT(provider, client, amount), result);
        }
    }
}
