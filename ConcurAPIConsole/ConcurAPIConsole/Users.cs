using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurAPIConsole
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Address
    {
        public string? country { get; set; }
        public string streetAddress { get; set; }
        public string postalCode { get; set; }
        public string locality { get; set; }
        public string type { get; set; }
        public string region { get; set; }
    }

    public class Email
    {
        public bool verified { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public bool notifications { get; set; }
    }

    public class EmergencyContact
    {
        public string country { get; set; }
        public string streetAddress { get; set; }
        public string postalCode { get; set; }
        public string name { get; set; }
        public string locality { get; set; }
        public List<string> phones { get; set; }
        public string region { get; set; }
        public string relationship { get; set; }
    }

    public class LocaleOverrides
    {
        public int preferenceEndDayViewHour { get; set; }
        public string preferenceFirstDayOfWeek { get; set; }
        public string preferenceDateFormat { get; set; }
        public string preferenceCurrencySymbolLocation { get; set; }
        public string preferenceHourMinuteSeparator { get; set; }
        public string preferenceDistance { get; set; }
        public string preferenceDefaultCalView { get; set; }
        public string preference24Hour { get; set; }
        public string preferenceNumberFormat { get; set; }
        public int preferenceStartDayViewHour { get; set; }
        public string preferenceNegativeCurrencyFormat { get; set; }
        public string preferenceNegativeNumberFormat { get; set; }
    }

    public class Manager
    {
        public string value { get; set; }
        public string employeeNumber { get; set; }
    }

    public class Meta
    {
        public string resourceType { get; set; }
        public DateTime created { get; set; }
        public DateTime lastModified { get; set; }
        public int version { get; set; }
        public string location { get; set; }
    }

    public class Name
    {
        public string honorificSuffix { get; set; }
        public string givenName { get; set; }
        public string familyName { get; set; }
        public object familyNamePrefix { get; set; }
        public string honorificPrefix { get; set; }
        public string middleName { get; set; }
        public string formatted { get; set; }
    }

    public class PhoneNumber
    {
        public string display { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string issuingCountry { get; set; }
        public bool? notifications { get; set; }
        public bool? primary { get; set; }
    }

    public class Resource
    {
        public LocaleOverrides localeOverrides { get; set; }
        public List<Address> addresses { get; set; }
        public string timezone { get; set; }
        public Meta meta { get; set; }
        public string displayName { get; set; }
        public Name name { get; set; }
        public List<PhoneNumber> phoneNumbers { get; set; }
        public List<EmergencyContact> emergencyContacts { get; set; }
        public string preferredLanguage { get; set; }
        public string title { get; set; }
        public string dateOfBirth { get; set; }
        public object nickName { get; set; }
        public List<string> schemas { get; set; }
        public bool active { get; set; }
        public string id { get; set; }
        public List<Email> emails { get; set; }
        public string userName { get; set; }

        [JsonProperty("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User")]
        public UrnIetfParamsScimSchemasExtensionEnterprise20User urnietfparamsscimschemasextensionenterprise20User { get; set; }
    }

    public class Root
    {
        public List<Resource> Resources { get; set; }
    }

    public class UrnIetfParamsScimSchemasExtensionEnterprise20User
    {
        public DateTime? terminationDate { get; set; }
        public string companyId { get; set; }
        public Manager manager { get; set; }
        public object costCenter { get; set; }
        public DateTime startDate { get; set; }
        public string employeeNumber { get; set; }
    }
}
