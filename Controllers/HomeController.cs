using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using OtpNet;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using XAccess2.Models;
//using static System.Net.Mime.MediaTypeNames;


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
        public ActionResult Activate(int xno, string token)
        {
            ViewBag.Message = "XACCESS - Powered by Xstra";

            if (!string.IsNullOrEmpty(token))
            {
                return View("Activate", token);
            }

            return View("Activate");
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
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapidev.x.direct/Authenticate");
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
        public ActionResult GenerateQrCode(string token)
        {
            try
            {
                var client = new HttpClient();
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapidev.x.direct/GenXAuth");
                XGenAuthRequest xGen = new XGenAuthRequest();
                xGen.TopSec = token;
                var content = new StringContent(JsonConvert.SerializeObject(xGen), null, "application/json");
                httpRequest.Content = content;
                var response = client.SendAsync(httpRequest).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AuthResponse>(responseString);
                if (result != null)
                {
                    LogAssist.LogMessage(string.Format("XNumber {0}, TopSec {1}", result.XNumber, result.TopSec, "GenerateQR"));
                    string otpDetail = "otpauth://totp/XACCESS%20-%20X" + result.XNumber.ToString() + "?secret=" + result.TopSec + "&issuer=XACCESS%20by%20Xstra%20Group%20Pty%20Ltd";

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpDetail, QRCodeGenerator.ECCLevel.Q);

                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] byteArray = qrCode.GetGraphic(20, true);

                    return File(byteArray, "image/jpeg");
                }

                return Ok();
            }
            catch (Exception e)
            {
                LogAssist.LogError(e, "QR Email");
                return Ok();
            }
        }

        public ActionResult RegisterForXAccess(RegisterRequest registerRequest)
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            var token = Base32Encoding.ToString(key);
            registerRequest.Token = token;
            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapidev.x.direct/XRegister");
            var content = new StringContent(JsonConvert.SerializeObject(registerRequest), null, "application/json");
            httpRequest.Content = content;
            var response = client.SendAsync(httpRequest).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;


            ViewBag.SuccessMessage = "Check your email for further instructions";
            return View("Index");
        }

        [HttpPost]
        public async Task<JsonResult> CheckMailDomain(MailRequest request)
        {
            var client = new HttpClient();
            if(request.emailAddress is null) request.emailAddress = string.Empty;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://xaccessapidev.x.direct/CheckMailDomain");
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
