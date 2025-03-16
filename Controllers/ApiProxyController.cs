using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace XAccess2.Controllers
{
    [RoutePrefix("api/ApiProxy")]
    public class ApiProxyController : ApiController
    {
        private readonly IConfiguration _configuration;

        public ApiProxyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Authenticate")]
        public async Task<IHttpActionResult> Authenticate(AuthRequest request)
        {
            try
            {
                var apiKey = _configuration["AppSettings:XAccessApiKey"];

                // Log or debug - make sure the API key isn't empty/null
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Content(HttpStatusCode.InternalServerError, "API key is missing in configuration");
                }

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                    var requestUrl = $"https://xaccessapidev.x.direct/api/Authenticate";

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
                    return Content(response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
