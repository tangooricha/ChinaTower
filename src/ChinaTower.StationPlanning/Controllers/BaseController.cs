using Microsoft.AspNet.Mvc;
using CodeComb.Security.Aes;
using CodeComb.Net.EmailSender;
using ChinaTower.StationPlanning.Models;

namespace ChinaTower.StationPlanning.Controllers
{
    public class BaseController : BaseController<ChinaTowerContext, User, string>
    {
        [FromServices]
        public AesCrypto Aes { get; set; }

        [FromServices]
        public SmtpEmailSender Mail { get; set; }
    }
}
