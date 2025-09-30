using FormDemo.CustomModel;

namespace FormDemo.Interface
{
    public interface ILicenseExpiryCheck
    {
        LicenseExpiryResult SaveFormdata(LicenceCheckForm form);
        bool CheckApproval(LicenceCheckForm form);
    }
}
