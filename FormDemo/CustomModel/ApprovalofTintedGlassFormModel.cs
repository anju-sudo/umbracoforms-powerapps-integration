using Newtonsoft.Json;

namespace FormDemo.CustomModel
{
    public class ApprovalofTintedGlassFormModel
    {
        public string NameOfVehicleOwner { get; set; }
        public string IdentificationOfOwner { get; set; }
        public string IsApplicantOwnerOfVehicle { get; set; }
        public string ApplicantName { get; set; }
        public string IdentificationOfApplicant { get; set; }
        public string VehicleIdentificationMark { get; set; }
        public string VehicleChassisNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool Declaration { get; set; }
    }
}
