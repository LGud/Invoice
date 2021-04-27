using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Controllers
{
    [Route("api/invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(ILogger<InvoiceController> logger)
        {
            _logger = logger;
        }

        //api/invoice/{ProviderId}/issue/{ClientId}
        [HttpGet]
        [Route("api/invoice/{providerid}/issue/{clientid}/amount/{amount}")]
        public ActionResult<double> CreateClientInvoice(int providerid, int clientid)
        {
            return new double();
        }

    }
}
