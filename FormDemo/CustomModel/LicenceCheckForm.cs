using Newtonsoft.Json;

namespace FormDemo.CustomModel
{
    public class LicenceCheckForm
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("dob")]
        public string DateOfbirth { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }


        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("languagesKnown")]
        public List<string> LanguagesKnown { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("licence_number")]
        public string LicenseNumber { get; set; }

        [JsonProperty("english")]
        public bool English { get; set; } = false;

        [JsonProperty("french")]
        public bool French { get; set; } = false;

        [JsonProperty("hindi")]
        public bool Hindi { get; set; } = false;

        [JsonProperty("japanese")]
        public bool Japanese { get; set; } = false;

        [JsonProperty("korean")]
        public bool Korean { get; set; } = false;
        [JsonProperty("chinese")]
        public bool Chinese { get; set; } = false;

        [JsonProperty("occupation")]
        public string Occupation { get; set; }

        [JsonProperty("yearsOfExperience")]
        public string YearsOfExperience { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("skills")]
        public List<string> Skills { get; set; }

        [JsonProperty("currentRole")]
        public string CurrentRole { get; set; }


        //skills
        [JsonProperty(".Net")]
        public bool dotNET { get; set; }
        [JsonProperty("php")]
        public bool PHP { get; set; }
        [JsonProperty("java")]
        public bool Java { get; set; }



    }
}
