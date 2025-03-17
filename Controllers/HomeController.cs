using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
//using System.Web.Http;
using Newtonsoft.Json;
using OtpNet;
using QRCoder;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using XAccess2.Models;
//using static System.Net.Mime.MediaTypeNames;


namespace XAccess2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
            var apiKey = _configuration["AppSettings:XAccessApiKey"];

            // Log or debug - make sure the API key isn't empty/null
            if (string.IsNullOrEmpty(apiKey))
            {
                return Content("API key is missing in configuration");
            }

            var baseAddress = _configuration["AppSettings:BaseUrl"];
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            request.IPAddress = ip;


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                var requestUrl = $"{baseAddress}Authenticate";

                var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // deserialize the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthStatusResponse>(responseContent);
                    // redirect to the URL provided in the response
                    return Redirect(authResponse.RedirectURL);
                }
                else return View("Index");
            } 
            
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
        public async Task<JsonResult> CheckMailDomain(ValidationRequest request)
        {
            var apiKey = _configuration["AppSettings:XAccessApiKey"];

            // Log or debug - make sure the API key isn't empty/null
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { error = "API key is missing in configuration" });
            }

            var baseAddress = _configuration["AppSettings:BaseUrl"];
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                var requestUrl = $"{baseAddress}Validate";

                if (request.identifier is null) request.identifier = string.Empty;
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                httpRequest.Content = content;
                var response = await client.SendAsync(httpRequest);
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ValidationResponse>(responseString);
                if (result != null)
                {
                    return Json(result);
                }
                return Json(new { error = "Failed to user record" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
