using FormDemo.CustomModel;
using FormDemo.Interface;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace FormDemo.Workflow
{
    public class ApprovalWorkflow : WorkflowType
    {
        private readonly ILogger<ApprovalWorkflow> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ApprovalWorkflow(ILogger<ApprovalWorkflow> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            this.Id = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            this.Name = "Form Approve";
            this.Description = "On approve actions";
            this.Icon = "icon-flash";
        }
        public override Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var licenseExpiryCheckService = scope.ServiceProvider.GetRequiredService<ILicenseExpiryCheck>();
                    var formModel = MapToLicenceCheckFormModel(context.Record);
                    var result = licenseExpiryCheckService.SaveFormdata(formModel);
                   
                }
                return Task.FromResult(WorkflowExecutionStatus.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in licencechecker form");
                throw;
            }
        }
        private LicenceCheckForm MapToLicenceCheckFormModel(Record record)
        {
            var languagesKnown = record.GetRecordFieldByAlias("languagesKnown")?.ValuesAsString();
           
            var english = false;
            var hindi = false;
            var french = false;
            var japanese = false;
            var chinese = false;
            var korean = false;

            var skills = record.GetRecordFieldByAlias("skills")?.ValuesAsString();

            var dotNet = false;
            var php = false;
            var java = false;


            if (!string.IsNullOrEmpty(languagesKnown))
            {
                var array = languagesKnown?.Split(',')?.ToList();
                english = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "english")?.Count() > 0 ? true : false;
                hindi = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "hindi")?.Count() > 0 ? true : false;
                french = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "french")?.Count() > 0 ? true : false;
                japanese = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "japanese")?.Count() > 0 ? true : false;
                chinese = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "chinese")?.Count() > 0 ? true : false;
                korean = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "korean")?.Count() > 0 ? true : false;
               
            }
            if (!string.IsNullOrEmpty(skills))
            {
                var array = skills?.Split(',')?.ToList();
                dotNet = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == ".net")?.Count() > 0 ? true : false;
                php = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "php")?.Count() > 0 ? true : false;
                java = array?.Where(x => x?.ToLower()?.Replace(@" ", string.Empty) == "java")?.Count() > 0 ? true : false;
              

            }
            return new LicenceCheckForm
            {
                FirstName = record.GetRecordFieldByAlias("firstName")?.ValuesAsString(),
                LastName = record.GetRecordFieldByAlias("lastName")?.ValuesAsString(),
                DateOfbirth = record.GetRecordFieldByAlias("dateOfBirth")?.ValuesAsString(),
                PhoneNumber = record.GetRecordFieldByAlias("phoneNumber")?.ValuesAsString(),
                Company = record.GetRecordFieldByAlias("company")?.ValuesAsString(),
                YearsOfExperience = record.GetRecordFieldByAlias("yearsOfExperience")?.ValuesAsString(),
                Occupation = record.GetRecordFieldByAlias("occupation")?.ValuesAsString(),
                Department = record.GetRecordFieldByAlias("department")?.ValuesAsString(),
                Address = record.GetRecordFieldByAlias("address")?.ValuesAsString(),
                Gender = record.GetRecordFieldByAlias("gender")?.ValuesAsString(),
                Country = record.GetRecordFieldByAlias("country")?.ValuesAsString(),
                Email = record.GetRecordFieldByAlias("email")?.ValuesAsString(),
                LicenseNumber = record.GetRecordFieldByAlias("licenseNumber")?.ValuesAsString(),
                English = english,
                Hindi = hindi,
                French = french,
                Japanese = japanese,
                Chinese = chinese,
                Korean = korean,
                dotNET =dotNet,
                PHP =php,
                Java =java
            };
        }
        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }
    }
}
