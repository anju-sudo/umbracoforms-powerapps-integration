using FormDemo.CustomModel;
using FormDemo.PowerAppsIntegration.Interface;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace FormDemo.Workflow.Transportaion
{
    public class ApprovalofTintedGlassFormWorkflow : WorkflowType
    {
        private readonly ILogger<ApprovalofTintedGlassFormWorkflow> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ApprovalofTintedGlassFormWorkflow(
            ILogger<ApprovalofTintedGlassFormWorkflow> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            this.Id = new Guid("0e7a5c31-61d5-4d9b-96a1-4b9b7a9f43f4");
            this.Name = "Approval of TintedGlass";
            this.Description = "ApprovalofTintedGlass";
            this.Icon = "icon-flash";
        }

        public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var powerAppsService = scope.ServiceProvider.GetRequiredService<IPowerAppsFormEntryService>();

                var formModel = MapToApprovalofTintedGlassFormModel(context.Record);
                await powerAppsService.SaveApprovalofTintedGlassdataAsync(context.Form.Id, formModel);

                return WorkflowExecutionStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving record to Dataverse for form {FormName}", context.Form?.Name);
                return WorkflowExecutionStatus.Failed;
            }
        }

        private ApprovalofTintedGlassFormModel MapToApprovalofTintedGlassFormModel(Record record)
        {
            var declaration = record.GetRecordFieldByAlias("declaration")?.ValuesAsString();

            return new ApprovalofTintedGlassFormModel
            {
                NameOfVehicleOwner = record.GetRecordFieldByAlias("nameOfVehicleOwner")?.ValuesAsString(),
                IdentificationOfOwner = record.GetRecordFieldByAlias("identificationNoTypeOfVehicleOwner")?.ValuesAsString(),
                IsApplicantOwnerOfVehicle = record.GetRecordFieldByAlias("isApplicantTheOwnerOfTheVehicle")?.ValuesAsString(),
                ApplicantName = record.GetRecordFieldByAlias("applicantsName")?.ValuesAsString(),
                IdentificationOfApplicant = record.GetRecordFieldByAlias("identificationNoTypeOfApplicant")?.ValuesAsString(),
                VehicleIdentificationMark = record.GetRecordFieldByAlias("vehicleIdentificationMark")?.ValuesAsString(),
                VehicleChassisNumber = record.GetRecordFieldByAlias("vehicleChassisNumber")?.ValuesAsString(),
                PhoneNumber = record.GetRecordFieldByAlias("phoneNumber")?.ValuesAsString(),
                Email = record.GetRecordFieldByAlias("eMailAddress")?.ValuesAsString(),
                Declaration = declaration == "True" ? true : false
            };
        }
        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }


    }
}
