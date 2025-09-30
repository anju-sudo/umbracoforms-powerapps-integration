using FormDemo.Interface;
using FormDemo.PowerAppsIntegration;
using FormDemo.PowerAppsIntegration.Interface;
using FormDemo.Services;
using FormDemo.Workflow;
using FormDemo.Workflow.Transportaion;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Forms.Core.Providers;
using Umbraco.Forms.Core.Services.Notifications;

namespace FormDemo.DI
{
    public class DIComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<ILicenseExpiryCheck, LicenseExpiryCheckService>();
            builder.Services.AddScoped<IDataverseWebApiService, DataverseWebApiService>();
            builder.Services.AddScoped<IPowerAppsFormEntryService, PowerAppsFormEntryService>();
            
            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
             .Add<LicenseExpiryCheckWorkflow>();

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
            .Add<ApprovalWorkflow>();

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
            .Add<UserProfileWorkflow>();

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
            .Add<ContactUsWorkflow>();

            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
           .Add<ApprovalofTintedGlassFormWorkflow>();
            builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
          .Add<ExemptionCertificateWorkflow>();

            builder.AddNotificationHandler<FormSavedNotification, FormSaveNotificationHandler>();

        }
    }
}
