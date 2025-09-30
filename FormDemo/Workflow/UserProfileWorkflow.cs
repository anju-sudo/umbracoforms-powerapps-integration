using FormDemo.CustomModel;
using FormDemo.PowerAppsIntegration.Interface;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core;

public class UserProfileWorkflow : WorkflowType
{
    private readonly ILogger<UserProfileWorkflow> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UserProfileWorkflow(
        ILogger<UserProfileWorkflow> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        this.Id = new Guid("8f9a7b34-2c5d-4c9a-bf2b-6a1e0f1c5678");
        this.Name = "User Profile Flow";
        this.Description = "userprofileflow";
        this.Icon = "icon-flash";
    }

    public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var powerAppsService = scope.ServiceProvider.GetRequiredService<IPowerAppsFormEntryService>();

            var formModel = MapToUserProfileFormModel(context.Record);
            await powerAppsService.SaveUserProfileFormdataAsync(context.Form.Id, formModel);

            return WorkflowExecutionStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving record to Dataverse for form {FormName}", context.Form?.Name);
            return WorkflowExecutionStatus.Failed;
        }
    }

    private UserProfileModel MapToUserProfileFormModel(Record record)
    {
        return new UserProfileModel
        {
            FullName = record.GetRecordFieldByAlias("fullName")?.ValuesAsString(),
            Email = record.GetRecordFieldByAlias("emailAddress")?.ValuesAsString(),
            PhoneNumber = record.GetRecordFieldByAlias("phoneNumber")?.ValuesAsString(),
            DateOfBirth = record.GetRecordFieldByAlias("dateOfBirth")?.ValuesAsString(),
            Gender = record.GetRecordFieldByAlias("gender")?.ValuesAsString(),
            ProfilePicture = record.GetRecordFieldByAlias("profilePicture")?.ValuesAsString(),
            Subscription = record.GetRecordFieldByAlias("subscribeToNewsletter")?.ValuesAsString(),
        };
    }

    public override List<Exception> ValidateSettings() => new();
}
