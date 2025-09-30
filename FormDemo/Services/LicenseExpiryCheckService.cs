using FormDemo.CustomModel;
using FormDemo.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System.Text;
using System.Text.Json;

namespace FormDemo.Services
{
    public class LicenseExpiryCheckService : ILicenseExpiryCheck
    {
        private readonly ILogger<LicenseExpiryCheckService> _logger;
        private readonly HttpClient _httpClient;
        public LicenseExpiryCheckService(ILogger<LicenseExpiryCheckService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }
        public LicenseExpiryResult SaveFormdata(LicenceCheckForm form)
        {
            try
            {

                var languages = new List<string>();
                var skills = new List<string>();

                if (form.LanguagesKnown != null)
                    languages.AddRange(form.LanguagesKnown);

                if (form.English)
                    languages.Add("English");
                if (form.Hindi)
                    languages.Add("Hindi");
                if (form.French)
                    languages.Add("French");
                if (form.Chinese)
                    languages.Add("Chinese");
                if (form.Korean)
                    languages.Add("Korean");

                if (form.Skills != null)
                    skills.AddRange(form.Skills);
                if (form.PHP)
                    skills.Add("php");
                if (form.dotNET)
                    skills.Add(".net");
                if (form.Java)
                    skills.Add("java");

                var payload = new
                {
                    firstName = form.FirstName,
                    lastName = form.LastName,
                    dateOfBirth = form.DateOfbirth,
                    phoneNumber = form.PhoneNumber,
                    company = form.Company,
                    yearsOfExperience = form.YearsOfExperience,
                    occupation = form.Occupation,
                    department = form.Department,
                    skills = skills,
                    address = form.Address,
                    gender =form.Gender,
                    email = form.Email,
                    country = form.Country,
                    languagesKnown = languages,
                   // licenseNumber = form.LicenseNumber
                };


                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

               
                var response = _httpClient.PostAsync("https://webhook.site/7437d5c0-efe9-4255-8a5f-b96efa1c003a", jsonContent).Result;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Webhook.site returned {StatusCode}", response.StatusCode);
                    return null;
                }
                var fakeApiResponse = new LicenseExpiryResult
                {
                   // LicenseNumber = payload.licenseNumber,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd")
                };

                _logger.LogInformation("License expiry check: {@Response}", fakeApiResponse);

                return fakeApiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling license expiry check API");
                return null;
            }
        }

        public bool CheckApproval(LicenceCheckForm form)
        {
            // Implementation here
            return true; // or false
        }
    }
}
