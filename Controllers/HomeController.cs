using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OtpNet;
using System.Diagnostics;
using System.Text;
using XAccess2.Models;


namespace XAccess2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            ViewBag.Message = "XACCESS - Powered by Xstra";

            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> CheckCode(AuthRequest request)
        {
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            request.IPAddress = ip;
            if (request.DealNumber is null) request.DealNumber = string.Empty;
            if (request.EmailAddress is null) request.EmailAddress = string.Empty;
            if (request.MFACode is null) request.MFACode = string.Empty;


            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapi.x.direct/Authenticate");
            var content = new StringContent(JsonConvert.SerializeObject(request), null, "application/json");
            httpRequest.Content = content;
            var response = await client.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthStatusResponse>(responseString);
            if (result != null)
            {
                if (result.RedirectURL == "https://xaccess.x.direct") return View("Index"); else return Redirect(result.RedirectURL);
            }
            else return View("Index");
        }

        public ActionResult RegisterForXAccess(RegisterRequest registerRequest)
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            var token = Base32Encoding.ToString(key);
            registerRequest.Token = token;

            ViewBag.SuccessMessage = "Check your email for further instructions";
            return View("Index");
        }

        [HttpPost]
        public async Task<JsonResult> CheckMailDomain(MailRequest request)
        {
            var client = new HttpClient();
            if(request.emailAddress is null) request.emailAddress = string.Empty;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapi.x.direct/CheckMailDomain");
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            httpRequest.Content = content;
            var response = await client.SendAsync(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MailDomainResponse>(responseString);
            if (result != null)
            {
                return Json(result);
            }
            return null;
        }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
