namespace InvoiceManager.Models
{
    public class Individual
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Adress { get; set; }
        public bool VATPayer { get; set; }
        public double VATCode { get; set; }
    }
}
