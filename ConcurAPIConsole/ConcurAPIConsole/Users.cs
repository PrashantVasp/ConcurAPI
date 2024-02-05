using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurAPIConsole
{
    public class UserResult
    {
        public string LocaleOverrides_PreferenceEndDayViewHour { get; set; }
        public string LocaleOverrides_PreferenceFirstDayOfWeek { get; set; }
        public string LocaleOverrides_PreferenceDateFormat { get; set; }
        public string LocaleOverrides_PreferenceCurrencySymbolLocation { get; set; }
        public string LocaleOverrides_PreferenceHourMinuteSeparator { get; set; }
        public string LocaleOverrides_PreferenceDistance { get; set; }
        public string LocaleOverrides_PreferenceDefaultCalView { get; set; }
        public string LocaleOverrides_Preference24Hour { get; set; }
        public int LocaleOverrides_PreferenceNumberFormat { get; set; }
        public int LocaleOverrides_PreferenceStartDayViewHour { get; set; }
        public int LocaleOverrides_PreferenceNegativeCurrencyFormat { get; set; }
        public int LocaleOverrides_PreferenceNegativeNumberFormat { get; set; }
        public string Addresses_Country { get; set; }
        public string Addresses_StreetAddress { get; set; }
        public long Addresses_PostalCode { get; set; }
        public string Addresses_Locality { get; set; }
        public string Addresses_Type { get; set; }
        public string Addresses_Region { get; set; }
        public string Timezone { get; set; }
        public string Meta_ResourceType { get; set; }
        public string Meta_Created { get; set; }
        public string Meta_LastModified { get; set; }
        public long Meta_Version { get; set; }
        public string Meta_Location { get; set; }
        public string DisplayName { get; set; }
        public string Name_HonorificSuffix { get; set; }
        public string Name_GivenName { get; set; }
        public string Name_FamilyName { get; set; }
        public string Name_FamilyNamePrefix { get; set; }
        public string Name_HonorificPrefix { get; set; }
        public string Name_MiddleName { get; set; }
        public string Name_Formatted { get; set; }
        public long PhoneNumbers { get; set; }
        public long EmergencyContacts { get; set; }
        public string PreferredLanguage { get; set; }
        public string Title { get; set; }
        public string DateOfBirth { get; set; }
        public string NickName { get; set; }
        public string Schemas { get; set; }
        public string Active { get; set; }
        public string Id { get; set; }
        public string Emails_Verified { get; set; }
        public string Emails_Type { get; set; }
        public string Emails_Value { get; set; }
        public string Emails_Notifications { get; set; }
        public string UserName { get; set; }
        public string Urn_TerminationDate { get; set; }
        public string Urn_CompanyId { get; set; }
        public string Urn_Manager_Value { get; set; }
        public int Urn_Manager_EmployeeNumber { get; set; }
        public string Urn_CostCenter { get; set; }
        public string Urn_StartDate { get; set; }
        public string Urn_EmployeeNumber { get; set; }
    }
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
        public UrnIetfParamsScimSchemasExtensionEnterprise20User urn { get; set; }
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
