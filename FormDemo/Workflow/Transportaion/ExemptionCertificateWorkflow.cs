using FormDemo.CustomModel;
using FormDemo.PowerAppsIntegration.Interface;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace FormDemo.Workflow.Transportaion
{
    public class ExemptionCertificateWorkflow : WorkflowType
    {
        private readonly ILogger<ExemptionCertificateWorkflow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ExemptionCertificateWorkflow(ILogger<ExemptionCertificateWorkflow> logger,
            IServiceProvider serviceProvider, IUmbracoContextFactory umbracoContextFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            this.Id = new Guid("f0a3e6c4-3c67-4b18-bef2-8c15f0a292e5");
            this.Name = "Exemtion Certification";
            this.Description = "Exemtion Certification";
            this.Icon = "icon-flash";
            _umbracoContextFactory = umbracoContextFactory;
        }
        public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var powerAppsService = scope.ServiceProvider.GetRequiredService<IPowerAppsFormEntryService>();

                var formModel = MapToExcemptionCertificationModel(context.Record);
                await powerAppsService.SaveExemtionCertificationDataAsync(context.Form.Id, formModel);
                using var cref = _umbracoContextFactory.EnsureUmbracoContext();
                var umbraco = cref.UmbracoContext;

                IPublishedContent? paymentPage = umbraco.Content
                    .GetAtRoot()                          
                    .SelectMany(x => x.DescendantsOrSelfOfType("paymentPage"))
                    .FirstOrDefault();

                var paymentPageUrl = paymentPage?.Url() ?? string.Empty;
                if (paymentPage != null)
                {
                    context.Form.GoToPageOnSubmit = paymentPage.Id;
                }
                return WorkflowExecutionStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving record to Dataverse for form {FormName}", context.Form?.Name);
                return WorkflowExecutionStatus.Failed;
            }
        }

        private ExemtionCertificationModel MapToExcemptionCertificationModel(Record record)
        {
            var declaration = record.GetRecordFieldByAlias("declaration")?.ValuesAsString();

            return new ExemtionCertificationModel
            {
                NameOfShip = record.GetRecordFieldByAlias("nameOfShip")?.ValuesAsString(),
                OfficialNumber = record.GetRecordFieldByAlias("officialNumber")?.ValuesAsString(),
                IMONUmber = record.GetRecordFieldByAlias("iMONumber")?.ValuesAsString(),
                NameofOwner = record.GetRecordFieldByAlias("nameOfOwner")?.ValuesAsString(),
                IMOOwnerNumber = record.GetRecordFieldByAlias("iMOOwnerNumber")?.ValuesAsString(),
                EquipmentandExemption = record.GetRecordFieldByAlias("equipmentAndorRegulationExemption")?.ValuesAsString(),
                ResounforRequest = record.GetRecordFieldByAlias("reasonsForRequestingTheAbove")?.ValuesAsString(),
                RegulationGrandExemtion = record.GetRecordFieldByAlias("regulationThatGrantsTheRightToAnExemptionDispensation")?.ValuesAsString(),
                ValidityStatutory = record.GetRecordFieldByAlias("validityOfTheStatutoryOrOtherCertificate")?.ValuesAsString(),
                ClassificationSocienty = record.GetRecordFieldByAlias("classificationSocietyRecognizedOrganization")?.ValuesAsString(),
                SpecialConditions = record.GetRecordFieldByAlias("specialConditionsOrFurtherRemarks")?.ValuesAsString(),
                
            };
        }
        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }

    }
}
