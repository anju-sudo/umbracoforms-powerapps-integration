using FormDemo.CustomModel;

namespace FormDemo.PowerAppsIntegration.Interface
{
    public interface IPowerAppsFormEntryService
    {
        Task<Guid> SaveUserProfileFormdataAsync(Guid formId, UserProfileModel form);
        Task<Guid> ContactUsFormSubmit(Guid formId, ContactUsModel form);
        Task<Guid> SaveApprovalofTintedGlassdataAsync(Guid formId, ApprovalofTintedGlassFormModel form);
        Task<Guid> SaveExemtionCertificationDataAsync(Guid formId, ExemtionCertificationModel form);
    }
}
