using FormDemo.CustomModel;
using FormDemo.PowerAppsIntegration.Interface;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core;

public class ContactUsWorkflow : WorkflowType
{
    private readonly ILogger<ContactUsWorkflow> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ContactUsWorkflow(
        ILogger<ContactUsWorkflow> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        this.Id = new Guid("d4e2a8b7-1c3f-45f0-9a62-8f27cb3d9a4e");
        this.Name = "Contact Us Flow";
        this.Description = "contact us workflow";
        this.Icon = "icon-flash";
    }

    public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var powerAppsService = scope.ServiceProvider.GetRequiredService<IPowerAppsFormEntryService>();

            var formModel = MapToContactUsFormModel(context.Record);
            await powerAppsService.ContactUsFormSubmit(context.Form.Id, formModel);

            return WorkflowExecutionStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving record to Dataverse for form {FormName}", context.Form?.Name);
            return WorkflowExecutionStatus.Failed;
        }
    }

    private ContactUsModel MapToContactUsFormModel(Record record)
    {
        return new ContactUsModel
        {
            FullName = record.GetRecordFieldByAlias("fullName")?.ValuesAsString(),
            Email = record.GetRecordFieldByAlias("emailAddress")?.ValuesAsString(),
            PhoneNumber = record.GetRecordFieldByAlias("phoneNumber")?.ValuesAsString(),
            Subject = record.GetRecordFieldByAlias("subject")?.ValuesAsString(),
            Message = record.GetRecordFieldByAlias("message")?.ValuesAsString()
           
        };
    }

    public override List<Exception> ValidateSettings() => new();
}
