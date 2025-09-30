using FormDemo.CustomModel;
using FormDemo.PowerAppsIntegration.Interface;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace FormDemo.PowerAppsIntegration
{
    public class PowerAppsFormEntryService : IPowerAppsFormEntryService
    {
        private readonly IConfiguration _configuration;
        private readonly IDataverseWebApiService _dataverse;
        private readonly ILogger<PowerAppsFormEntryService> _logger;
        
        public PowerAppsFormEntryService(IConfiguration configuration, IDataverseWebApiService dataverse, ILogger<PowerAppsFormEntryService> logger)
        {
            _dataverse = dataverse;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<Guid> SaveUserProfileFormdataAsync(Guid formId, UserProfileModel form)
        {
            var tableName = $"new_umbracoform_{formId.ToString().Replace("-", "_").ToLower()}";
            
            // Map to exact Dataverse schema names produced by GetSafeColumnName and primary name column
            var attributes = new Dictionary<string, object>
            {
                ["new_recordname"] = $"UserProfile - {DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            // Match aliases used in the form so the safe name becomes: new_fullname, new_emailaddress, etc.
            attributes["new_fullname"] = form.FullName;
            attributes["new_emailaddress"] = form.Email;
            attributes["new_phonenumber"] = form.PhoneNumber;

            if (DateTime.TryParse(form.DateOfBirth, out var dob))
                attributes["new_dateofbirth"] = dob;

            attributes["new_gender"] = form.Gender;
            attributes["new_profilepicture"] = form.ProfilePicture;

            // Checkbox mapped as Boolean when created; parse string to bool
            //if (TryToBool(form.Subscription, out var subscribe))
            //    attributes["new_subscribetonewsletter"] = subscribe;

            // Insert row
            try
            {
                return await _dataverse.CreateRowAsync(tableName, attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dataverse insert failed for table {Table}", tableName);
                throw;
            }
        }
       
        public async Task<Guid> ContactUsFormSubmit(Guid formId, ContactUsModel form)
        {
            var tableName = $"new_umbracoform_{formId.ToString().Replace("-", "_").ToLower()}";

            // Map to exact Dataverse schema names produced by GetSafeColumnName and primary name column
            var attributes = new Dictionary<string, object>
            {
                ["new_recordname"] = $"Contact Us Form - {DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            attributes["new_fullname"] = form.FullName;
            attributes["new_emailaddress"] = form.Email;
            attributes["new_phonenumber"] = form.PhoneNumber;
            attributes["new_subject"] = form.Subject;
            attributes["new_message"] = form.Message;

            try
            {
                return await _dataverse.CreateRowAsync(tableName, attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dataverse insert failed for table {Table}", tableName);
                throw;
            }
        }

        public async Task<Guid> SaveApprovalofTintedGlassdataAsync(Guid formId, ApprovalofTintedGlassFormModel form)
        {
            var tableName = $"new_umbracoform_{formId.ToString().Replace("-", "_").ToLower()}";

            // Map to exact Dataverse schema names produced by GetSafeColumnName and primary name column
            var attributes = new Dictionary<string, object>
            {
                ["new_recordname"] = $"Tinted Glass - {DateTime.UtcNow:yyyyMMddHHmmss}"
            };
            attributes["new_nameofvehicleowner"] = form.NameOfVehicleOwner;
            attributes["new_identificationnotypeofvehicleowner"] = form.IdentificationOfOwner;
            attributes["new_isapplicanttheownerofthevehicle"] = form.IsApplicantOwnerOfVehicle;
            attributes["new_applicantsname"] = form.ApplicantName;
            attributes["new_identificationnotypeofapplicant"] = form.IdentificationOfApplicant;
            attributes["new_vehicleidentificationmark"] = form.VehicleIdentificationMark;
            attributes["new_vehiclechassisnumber"] = form.VehicleChassisNumber;
            attributes["new_phonenumber"] = form.PhoneNumber;
            attributes["new_emailaddress"] = form.Email;
            attributes["new_declaration"] = form.Declaration;

            // Insert row
            try
            {
                return await _dataverse.CreateRowAsync(tableName, attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dataverse insert failed for table {Table}", tableName);
                throw;
            }
        }

        public async Task<Guid> SaveExemtionCertificationDataAsync(Guid formId, ExemtionCertificationModel form)
        {
            var tableName = $"new_umbracoform_{formId.ToString().Replace("-", "_").ToLower()}";

            // Map to exact Dataverse schema names produced by GetSafeColumnName and primary name column
            var attributes = new Dictionary<string, object>
            {
                ["new_recordname"] = $"ExemtionCert - {DateTime.UtcNow:yyyyMMddHHmmss}"
            };
            attributes["new_nameofship"] = form.NameOfShip;
            attributes["new_officialnumber"] = form.OfficialNumber;
            attributes["new_imonumber"] = form.IMONUmber;
            attributes["new_nameofowner"] = form.NameofOwner;
            attributes["new_imoownernumber"] = form.IMOOwnerNumber;
            attributes["new_equipmentandorregulationexemption"] = form.EquipmentandExemption;
            attributes["new_reasonsforrequestingtheabove"] = form.ResounforRequest;
            attributes["new_regulationthatgrantstherighttoanexemptiondispensation"] = form.RegulationGrandExemtion;
            attributes["new_validityofthestatutoryorothercertificate"] = form.ValidityStatutory;
            attributes["new_classificationsocietyrecognizedorganization"] = form.ClassificationSocienty;
            attributes["new_specialconditionsorfurtherremarks"] = form.SpecialConditions;

            // Insert row
            try
            {
                return await _dataverse.CreateRowAsync(tableName, attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dataverse insert failed for table {Table}", tableName);
                throw;
            }
        }

    }

}

