// See https://aka.ms/new-console-template for more information
using ConcurAPIConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using log4net;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Security.Policy;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices.JavaScript;
using System.Reflection.PortableExecutable;
using System.Configuration;


// Get the API keys and connection string from congig file
//string LogPath = ConfigurationManager.AppSettings["LogPath"];
//string LogDirectory = ConfigurationManager.AppSettings["LogDirectory"];
string connectionstring = ConfigurationManager.ConnectionStrings["Concur"].ConnectionString;
string clientId = ConfigurationManager.AppSettings["clientId"];
string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
string baseURL = ConfigurationManager.AppSettings["baseURL"];
string username = ConfigurationManager.AppSettings["username"];
string password = ConfigurationManager.AppSettings["password"];
string refresh_token = ConfigurationManager.AppSettings["refresh_token"];
string companyId = ConfigurationManager.AppSettings["companyId"];


// Get Authorization token from Cocur
string AccessToken = await GetAndInsertAccessTokenAsync(refresh_token, username, password, clientId, clientSecret, baseURL,connectionstring);

//Get Users data using authorization token
//string status = await GetandInsertAllUsers(AccessToken, refresh_token, username, password, clientId, clientSecret, baseURL,connectionstring);

//Get Expense Reports
//string statusReport = await GetAndInsertReports(AccessToken, refresh_token, username, password, clientId, clientSecret, baseURL,connectionstring);

//Get Expense Reports Entries
string statusReportEntry = await GetandInsertReportEntries(AccessToken, refresh_token, username, password, clientId, clientSecret, baseURL, connectionstring);




log4net.Config.XmlConfigurator.Configure();

//Store token with expiration and such in a table called concur.authenticationTokens
static async Task<string> GetAndInsertAccessTokenAsync(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL,string connectionstring)
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    try
    {
        log.Info("In method GetAndInsertAccessTokenAsync");
        using (var client = new HttpClient())
        {
            var tokenRequest = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refresh_token },
                        { "username", username},
                        { "password", password},
                        { "client_id", clientId},
                        { "client_secret", clientSecret}
                    };

            //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
            //tokenResponse.EnsureSuccessStatusCode();

            HttpResponseMessage tokenResponse = await client.PostAsync(baseURL + "oauth2/v0/token", new FormUrlEncodedContent(tokenRequest));
            var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
            TokenResponse tok = JsonConvert.DeserializeObject<TokenResponse>(jsonContent);
            if (tok != null)
            {
                using (var conn = new SqlConnection(connectionstring))
                {
                    conn.Open();
                    var cmd = new SqlCommand("INSERT_AUTHTOKEN", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@token", tok.AccessToken);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                log.Error("GetAndInsertAccessTokenAsync: Token not created");
                return "Token not created";
            }
            log.Info("GetAndInsertAccessTokenAsync: Token created and added in to database successfully");
            return tok.AccessToken;
        }
      
    }
    catch (Exception ex)
    {
        log.Error("Error in GetAccessTokenAsync:" + ex.Message);
        return string.Empty;
    }
}

 //Download all Users utilizing the following API Endpoint
 static async Task<string> GetandInsertAllUsers(string AccessToken, string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL, string connectionstring)
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    try
    {
        log.Info("In method GetandInsertAllUsers");

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", AccessToken);
            //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
            //tokenResponse.EnsureSuccessStatusCode();

            var response = await client.GetAsync(baseURL + "profile/identity/v4/Users/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonString = await response.Content.ReadAsStringAsync();
            var usersResult = JsonConvert.DeserializeObject<object>(jsonString);
        
            log.Info("Successfully fetched all the users");
            var userDataList = new Root();

            var jsonContent = JsonConvert.SerializeObject(usersResult);
            userDataList = JsonConvert.DeserializeObject<Root>(jsonContent);

            // Insert data into the database
            //string insertStatus = await InsertUsers(userDataList);
            string insertStatus = "";
            log.Info("GetAllUsersAndInsert:");
            log.Info(insertStatus);

            // return insertStatus;
            using (var conn = new SqlConnection(connectionstring))
            {
                conn.Open();

                foreach (var userData in userDataList.Resources)
                {
                    var cmd = new SqlCommand("INSERT_USERS", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceEndDayViewHour", userData.localeOverrides.preferenceEndDayViewHour);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceFirstDayOfWeek", userData.localeOverrides.preferenceFirstDayOfWeek);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceDateFormat", userData.localeOverrides.preferenceDateFormat);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceCurrencySymbolLocation", userData.localeOverrides.preferenceCurrencySymbolLocation);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceHourMinuteSeparator", userData.localeOverrides.preferenceHourMinuteSeparator);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceDistance", userData.localeOverrides.preferenceDistance);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceDefaultCalView", userData.localeOverrides.preferenceDefaultCalView);
                    cmd.Parameters.AddWithValue("@localeOverrides_preference24Hour", userData.localeOverrides.preference24Hour);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceNumberFormat", userData.localeOverrides.preferenceNumberFormat);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceStartDayViewHour", userData.localeOverrides.preferenceStartDayViewHour);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceNegativeCurrencyFormat", userData.localeOverrides.preferenceNegativeCurrencyFormat);
                    cmd.Parameters.AddWithValue("@localeOverrides_preferenceNegativeNumberFormat", userData.localeOverrides.preferenceNegativeNumberFormat);
                    if (userData.addresses != null && userData.addresses.Count > 0)
                    {
                        if (userData.addresses[0].country == null)
                        {
                            cmd.Parameters.AddWithValue("@addresses_country", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_country", userData.addresses[0].country);
                        }
                        if (userData.addresses[0].streetAddress == null )
                        {
                            cmd.Parameters.AddWithValue("@addresses_streetAddress", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_streetAddress", userData.addresses[0].streetAddress);
                        }
                        if (userData.addresses[0].postalCode == null)
                        {
                            cmd.Parameters.AddWithValue("@addresses_postalCode", 0);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_postalCode", userData.addresses[0].postalCode);
                        }
                        if(userData.addresses[0].locality == null)
                        {
                            cmd.Parameters.AddWithValue("@addresses_locality", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_locality", userData.addresses[0].locality);
                        }
                        if (userData.addresses[0].type == null)
                        {
                            cmd.Parameters.AddWithValue("@addresses_type", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_type", userData.addresses[0].type);
                        }
                        if (userData.addresses[0].region == null)
                        {
                            cmd.Parameters.AddWithValue("@addresses_region", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@addresses_region", userData.addresses[0].region);
                        }
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@addresses_country", "");
                        cmd.Parameters.AddWithValue("@addresses_streetAddress", "");
                        cmd.Parameters.AddWithValue("@addresses_postalCode", 0);
                        cmd.Parameters.AddWithValue("@addresses_locality", "");
                        cmd.Parameters.AddWithValue("@addresses_type", "");
                        cmd.Parameters.AddWithValue("@addresses_region", "");
                    }
                    cmd.Parameters.AddWithValue("@timezone", userData.timezone);
                    cmd.Parameters.AddWithValue("@meta_resourceType", userData.meta.resourceType);
                    cmd.Parameters.AddWithValue("@meta_created", userData.meta.created);
                    cmd.Parameters.AddWithValue("@meta_lastModified", userData.meta.lastModified);
                    cmd.Parameters.AddWithValue("@meta_version", userData.meta.version);
                    cmd.Parameters.AddWithValue("@meta_location", userData.meta.location);
                    cmd.Parameters.AddWithValue("@displayName", userData.displayName);
                    if (userData.name.honorificSuffix == null)
                    {
                        cmd.Parameters.AddWithValue("@name_honorificSuffix", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_honorificSuffix", userData.name.honorificSuffix);
                    }
                    if (userData.name.givenName == null)
                    {
                        cmd.Parameters.AddWithValue("@name_givenName", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_givenName", userData.name.givenName);
                    }
                    if (userData.name.familyName == null)
                    {
                        cmd.Parameters.AddWithValue("@name_familyName", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_familyName", userData.name.familyName);
                    }
                    if (userData.name.familyNamePrefix == null)
                    {
                        cmd.Parameters.AddWithValue("@name_familyNamePrefix", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_familyNamePrefix", userData.name.familyNamePrefix);
                    }
                    if (userData.name.honorificPrefix == null)
                    {
                        cmd.Parameters.AddWithValue("@name_honorificPrefix", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_honorificPrefix", userData.name.honorificPrefix);
                    }
                    if (userData.name.middleName == null)
                    {
                        cmd.Parameters.AddWithValue("@name_middleName", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_middleName", userData.name.middleName);
                    }
                    if (userData.name.formatted == null)
                    {
                        cmd.Parameters.AddWithValue("@name_formatted", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@name_formatted", userData.name.formatted);
                    }
                    if (userData.phoneNumbers != null && userData.phoneNumbers.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("@phoneNumbers_value", userData.phoneNumbers[0].value);
                        cmd.Parameters.AddWithValue("@phoneNumbers_type", userData.phoneNumbers[0].type);
                        cmd.Parameters.AddWithValue("@phoneNumbers_display", userData.phoneNumbers[0].display);
                        if (userData.phoneNumbers[0].issuingCountry == null)
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_issuingCountry", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_issuingCountry", userData.phoneNumbers[0].issuingCountry);
                        }
                        if (userData.phoneNumbers[0].notifications == null)
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_notifications", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_notifications", userData.phoneNumbers[0].notifications);
                        }
                        if(userData.phoneNumbers[0].primary == null)
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_primary", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@phoneNumbers_primary", userData.phoneNumbers[0].primary);
                        }
                        
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@phoneNumbers_value", "");
                        cmd.Parameters.AddWithValue("@phoneNumbers_type", "");
                        cmd.Parameters.AddWithValue("@phoneNumbers_display", "");
                        cmd.Parameters.AddWithValue("@phoneNumbers_issuingCountry", "");
                        cmd.Parameters.AddWithValue("@phoneNumbers_notifications", "");
                        cmd.Parameters.AddWithValue("@phoneNumbers_primary", "");
                    }
                    if (userData.emergencyContacts != null && userData.emergencyContacts.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("@emergencyContacts_country", userData.emergencyContacts[0].country);
                        cmd.Parameters.AddWithValue("@emergencyContacts_streetAddress", userData.emergencyContacts[0].streetAddress);
                        cmd.Parameters.AddWithValue("@emergencyContacts_postalCode", userData.emergencyContacts[0].postalCode);
                        cmd.Parameters.AddWithValue("@emergencyContacts_name", userData.emergencyContacts[0].name);
                        cmd.Parameters.AddWithValue("@emergencyContacts_locality", userData.emergencyContacts[0].locality);
                        cmd.Parameters.AddWithValue("@emergencyContacts_phone", userData.emergencyContacts[0].phones[0]);
                        if(userData.emergencyContacts[0].region == null)
                        {
                            cmd.Parameters.AddWithValue("@emergencyContacts_region", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@emergencyContacts_region", userData.emergencyContacts[0].region);
                        }
                        
                        cmd.Parameters.AddWithValue("@emergencyContacts_relationship", userData.emergencyContacts[0].relationship);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@emergencyContacts_country", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_streetAddress", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_postalCode", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_name", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_locality", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_phone", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_region", "");
                        cmd.Parameters.AddWithValue("@emergencyContacts_relationship", "");
                    }

                    cmd.Parameters.AddWithValue("@preferredLanguage", userData.preferredLanguage);
                    if (userData.title == null)
                    {
                        cmd.Parameters.AddWithValue("@title", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@title", userData.title);
                    }
                    if (userData.dateOfBirth == null)
                    {
                        cmd.Parameters.AddWithValue("@dateOfBirth", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@dateOfBirth", userData.dateOfBirth);
                    }
                    if (userData.nickName == null)
                    {
                        cmd.Parameters.AddWithValue("@nickName", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@nickName", userData.nickName);
                    }
                    cmd.Parameters.AddWithValue("@schemas", userData.schemas[0]);
                    cmd.Parameters.AddWithValue("@active", userData.active);
                    cmd.Parameters.AddWithValue("@id", userData.id);
                    if (userData.emails != null && userData.emails.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("@emails_verified", userData.emails[0].verified);
                        cmd.Parameters.AddWithValue("@emails_type", userData.emails[0].type);
                        cmd.Parameters.AddWithValue("@emails_value", userData.emails[0].value);
                        cmd.Parameters.AddWithValue("@emails_notifications", userData.emails[0].notifications);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@emails_verified", "");
                        cmd.Parameters.AddWithValue("@emails_type", "");
                        cmd.Parameters.AddWithValue("@emails_value", "");
                        cmd.Parameters.AddWithValue("@emails_notifications", "");
                    }
                    cmd.Parameters.AddWithValue("@userName", userData.userName);
                    if (userData.urn.terminationDate == null)
                    {
                        cmd.Parameters.AddWithValue("@urn_terminationDate", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@urn_terminationDate", userData.urn.terminationDate);
                    }
                    if (userData.urn.companyId == null)
                    {
                        cmd.Parameters.AddWithValue("@urn_companyId", userData.urn.companyId);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@urn_companyId", userData.urn.companyId);
                    }
                    if (userData.urn.manager == null)
                    {
                        cmd.Parameters.AddWithValue("@urn_manager_value", "");
                        cmd.Parameters.AddWithValue("@urn_manager_employeeNumber", "");
                    }
                    
                    else
                    {
                        if (userData.urn.manager.value == null)
                        {
                            cmd.Parameters.AddWithValue("@urn_manager_value", "");

                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@urn_manager_value", userData.urn.manager.value);
                        }
                        if (userData.urn.manager.employeeNumber == null)
                        {
                            cmd.Parameters.AddWithValue("@urn_manager_employeeNumber", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@urn_manager_employeeNumber", userData.urn.manager.employeeNumber);
                        }
                    }
                  
                    if (userData.urn.costCenter == null)
                    {
                        cmd.Parameters.AddWithValue("@urn_costCenter", "");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@urn_costCenter", userData.urn.costCenter);

                    }
                    cmd.Parameters.AddWithValue("@urn_startDate", userData.urn.startDate);
                    cmd.Parameters.AddWithValue("@urn_employeeNumber", userData.urn.employeeNumber);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        log.Info("Successfully inserted User named: " + userData.userName);
                    }
                    catch(SqlException ex)
                    {
                        log.Error("Error while inserting users:: " + ex.Message);
                    }
                }
            }
            log.Info("Successfully inserted all the users::"+ userDataList.Resources.Count);
            return "Successfully fetched and inserted all the users";
        }
    }
    catch (Exception ex)
    {
        log.Error("Error in GetAllUsers:" + ex.Message);
        return string.Empty;
    }
}
//all approved Expense reports since the last run time
static async Task<string> GetAndInsertReports(string AccessToken, string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL, string connectionstring)
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    try
    {
        log.Info("In method GetAndInsertReports");
        // Make a request to the Concur API and get the XML response

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AccessToken);
            //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
            //tokenResponse.EnsureSuccessStatusCode();
            log.Info("Fetch first 25 reports from the consur API");
            var response = await client.GetAsync(baseURL + "api/v3.0/expense/reports?user=ALL");
            response.EnsureSuccessStatusCode();
            string xmlData = await response.Content.ReadAsStringAsync();

            // Parse the XML data using XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            // Now you can navigate through the XML structure
            // For example, if there is an 'expenses' element, you can iterate through its children
            XmlNodeList expenseNodes = xmlDoc.SelectNodes("//Items/Report");
           
            log.Info("Fetch last inserted date from the Expenses table");
            string modifiedDateAfter = "";
            log.Info("Fetch last inserted date from the Expenses table");
            using (var conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT_EXPENSES_LASTINSERTEDDATE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    modifiedDateAfter = cmd.ExecuteScalar().ToString();
                }
            }
            bool goAheadForInsertion = false;
            if (modifiedDateAfter == "" || modifiedDateAfter == null)
            {
                goAheadForInsertion = true;
            }
            else if (System.DateTime.Now.Date > System.DateTime.Parse(modifiedDateAfter))
            {
                goAheadForInsertion = true;
            }
            int insertedRows = 0;
            string offset = "";
        INSETNEXTPAGEROWS:
           
            if (offset != "")
            {
                log.Info("Fetch next page's reports from the concur API offset:" + offset);
                insertedRows = 0;
                var responseNextPage = await client.GetAsync(baseURL + "api/v3.0/expense/reports?user=ALL&offset" + offset);
                responseNextPage.EnsureSuccessStatusCode();
                string xmlDataNextPage = await responseNextPage.Content.ReadAsStringAsync();

                // Parse the XML data using XmlDocument
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlDataNextPage);

                // Now you can navigate through the XML structure
                // For example, if there is an 'expenses' element, you can iterate through its children
                expenseNodes = xmlDoc.SelectNodes("//Items/Report");
            }
           
            if(goAheadForInsertion == true) { 
                foreach (XmlNode expenseNode in expenseNodes)
                {
                    // Access specific elements within each expense
                    string ID = expenseNode.SelectSingleNode("ID").InnerText;
                    string URI = expenseNode.SelectSingleNode("URI").InnerText;
                    string Name = expenseNode.SelectSingleNode("Name").InnerText;
                    string Total = expenseNode.SelectSingleNode("Total").InnerText;
                    string CurrencyCode = expenseNode.SelectSingleNode("CurrencyCode").InnerText;
                    string Country = expenseNode.SelectSingleNode("Country").InnerText;
                    string CountrySubdivision = expenseNode.SelectSingleNode("CountrySubdivision").InnerText;
                    string CreateDate = expenseNode.SelectSingleNode("CreateDate").InnerText;
                    string SubmitDate = expenseNode.SelectSingleNode("SubmitDate").InnerText;
                    string ProcessingPaymentDate = expenseNode.SelectSingleNode("ProcessingPaymentDate").InnerText;
                    string PaidDate = expenseNode.SelectSingleNode("PaidDate").InnerText;
                    string ReceiptsReceived = expenseNode.SelectSingleNode("ReceiptsReceived").InnerText;
                    string UserDefinedDate = expenseNode.SelectSingleNode("UserDefinedDate").InnerText;
                    string LastComment = expenseNode.SelectSingleNode("LastComment").InnerText;
                    string OwnerLoginID = expenseNode.SelectSingleNode("OwnerLoginID").InnerText;
                    string OwnerName = expenseNode.SelectSingleNode("OwnerName").InnerText;
                    string ApproverLoginID = expenseNode.SelectSingleNode("ApproverLoginID").InnerText;
                    string ApproverName = expenseNode.SelectSingleNode("ApproverName").InnerText;
                    string ApprovalStatusName = expenseNode.SelectSingleNode("ApprovalStatusName").InnerText;
                    string ApprovalStatusCode = expenseNode.SelectSingleNode("ApprovalStatusCode").InnerText;
                    string PaymentStatusName = expenseNode.SelectSingleNode("PaymentStatusName").InnerText;
                    string PaymentStatusCode = expenseNode.SelectSingleNode("PaymentStatusCode").InnerText;
                    string LastModifiedDate = expenseNode.SelectSingleNode("LastModifiedDate").InnerText;
                    string PersonalAmount = expenseNode.SelectSingleNode("PersonalAmount").InnerText;
                    string AmountDueEmployee = expenseNode.SelectSingleNode("AmountDueEmployee").InnerText;
                    string AmountDueCompanyCard = expenseNode.SelectSingleNode("AmountDueCompanyCard").InnerText;
                    string TotalClaimedAmount = expenseNode.SelectSingleNode("TotalClaimedAmount").InnerText;
                    string TotalApprovedAmount = expenseNode.SelectSingleNode("TotalApprovedAmount").InnerText;
                    string LedgerName = expenseNode.SelectSingleNode("LedgerName").InnerText;
                    string PolicyID = expenseNode.SelectSingleNode("PolicyID").InnerText;
                    string EverSentBack = expenseNode.SelectSingleNode("EverSentBack").InnerText;
                    string HasException = expenseNode.SelectSingleNode("HasException").InnerText;
                    string WorkflowActionUrl = expenseNode.SelectSingleNode("WorkflowActionUrl").InnerText;
                    string OrgUnit1 = expenseNode.SelectSingleNode("OrgUnit1").InnerText;
                    string OrgUnit2 = expenseNode.SelectSingleNode("OrgUnit2").InnerText;
                    string OrgUnit3 = expenseNode.SelectSingleNode("OrgUnit3").InnerText;
                    string OrgUnit4 = expenseNode.SelectSingleNode("OrgUnit4").InnerText;
                    string OrgUnit5 = expenseNode.SelectSingleNode("OrgUnit5").InnerText;
                    string OrgUnit6 = expenseNode.SelectSingleNode("OrgUnit6").InnerText;
                    string Custom1 = expenseNode.SelectSingleNode("Custom1").InnerText;
                    string Custom2 = expenseNode.SelectSingleNode("Custom2").InnerText;
                    string Custom3 = expenseNode.SelectSingleNode("Custom3").InnerText;
                    string Custom4 = expenseNode.SelectSingleNode("Custom4").InnerText;
                    string Custom5 = expenseNode.SelectSingleNode("Custom5").InnerText;
                    string Custom6 = expenseNode.SelectSingleNode("Custom6").InnerText;
                    string Custom7 = expenseNode.SelectSingleNode("Custom7").InnerText;
                    string Custom8 = expenseNode.SelectSingleNode("Custom8").InnerText;
                    string Custom9 = expenseNode.SelectSingleNode("Custom9").InnerText;
                    string Custom10 = expenseNode.SelectSingleNode("Custom10").InnerText;
                    string Custom11 = expenseNode.SelectSingleNode("Custom11").InnerText;
                    string Custom12 = expenseNode.SelectSingleNode("Custom12").InnerText;
                    string Custom13 = expenseNode.SelectSingleNode("Custom13").InnerText;
                    string Custom14 = expenseNode.SelectSingleNode("Custom14").InnerText;
                    string Custom15 = expenseNode.SelectSingleNode("Custom15").InnerText;
                    string Custom16 = expenseNode.SelectSingleNode("Custom16").InnerText;
                    string Custom17 = expenseNode.SelectSingleNode("Custom17").InnerText;
                    string Custom18 = expenseNode.SelectSingleNode("Custom18").InnerText;
                    string Custom19 = expenseNode.SelectSingleNode("Custom19").InnerText;
                    string Custom20 = expenseNode.SelectSingleNode("Custom20").InnerText;
                   
                        // Do something with the extracted data

                        try
                    {
                        log.Info("In method Insert Expense Reports");

                        if (ApprovalStatusCode == "A_APPR" )
                        {
                            using (var conn = new SqlConnection(connectionstring))
                            {
                                using (SqlCommand cmd = new SqlCommand("INSERT_EXPENSES", conn))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = ID;
                                    cmd.Parameters.Add("@URI", SqlDbType.VarChar).Value = URI;
                                    cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Name;
                                    cmd.Parameters.Add("@Total", SqlDbType.Decimal).Value = Convert.ToDecimal(Total);
                                    cmd.Parameters.Add("@CurrencyCode", SqlDbType.VarChar).Value = CurrencyCode;
                                    cmd.Parameters.Add("@Country", SqlDbType.VarChar).Value = Country;
                                    cmd.Parameters.Add("@CountrySubdivision", SqlDbType.VarChar).Value = CountrySubdivision;
                                    cmd.Parameters.Add("@CreateDate", SqlDbType.VarChar).Value = CreateDate;
                                    cmd.Parameters.Add("@SubmitDate", SqlDbType.VarChar).Value = SubmitDate;
                                    cmd.Parameters.Add("@ProcessingPaymentDate", SqlDbType.VarChar).Value = ProcessingPaymentDate;
                                    cmd.Parameters.Add("@PaidDate", SqlDbType.VarChar).Value = PaidDate;
                                    cmd.Parameters.Add("@ReceiptsReceived", SqlDbType.VarChar).Value = ReceiptsReceived;
                                    cmd.Parameters.Add("@UserDefinedDate", SqlDbType.VarChar).Value = UserDefinedDate;
                                    cmd.Parameters.Add("@LastComment", SqlDbType.VarChar).Value = LastComment;
                                    cmd.Parameters.Add("@OwnerLoginID", SqlDbType.VarChar).Value = OwnerLoginID;
                                    cmd.Parameters.Add("@OwnerName", SqlDbType.VarChar).Value = OwnerName;
                                    cmd.Parameters.Add("@ApproverLoginID", SqlDbType.VarChar).Value = ApproverLoginID;
                                    cmd.Parameters.Add("@ApproverName", SqlDbType.VarChar).Value = ApproverName;
                                    cmd.Parameters.Add("@ApprovalStatusName", SqlDbType.VarChar).Value = ApprovalStatusName;
                                    cmd.Parameters.Add("@ApprovalStatusCode", SqlDbType.VarChar).Value = ApprovalStatusCode;
                                    cmd.Parameters.Add("@PaymentStatusName", SqlDbType.VarChar).Value = PaymentStatusName;
                                    cmd.Parameters.Add("@PaymentStatusCode", SqlDbType.VarChar).Value = PaymentStatusCode;
                                    cmd.Parameters.Add("@LastModifiedDate", SqlDbType.VarChar).Value = LastModifiedDate;
                                    cmd.Parameters.Add("@PersonalAmount", SqlDbType.Decimal).Value = Convert.ToDecimal(PersonalAmount);
                                    cmd.Parameters.Add("@AmountDueEmployee", SqlDbType.Decimal).Value = Convert.ToDecimal(AmountDueEmployee);
                                    cmd.Parameters.Add("@AmountDueCompanyCard", SqlDbType.Decimal).Value = Convert.ToDecimal(AmountDueCompanyCard);
                                    cmd.Parameters.Add("@TotalClaimedAmount", SqlDbType.Decimal).Value = Convert.ToDecimal(TotalClaimedAmount);
                                    cmd.Parameters.Add("@TotalApprovedAmount", SqlDbType.Decimal).Value = Convert.ToDecimal(TotalApprovedAmount);
                                    cmd.Parameters.Add("@LedgerName", SqlDbType.VarChar).Value = LedgerName;
                                    cmd.Parameters.Add("@PolicyID", SqlDbType.VarChar).Value = PolicyID;
                                    cmd.Parameters.Add("@EverSentBack", SqlDbType.VarChar).Value = EverSentBack;
                                    cmd.Parameters.Add("@HasException", SqlDbType.VarChar).Value = HasException;
                                    cmd.Parameters.Add("@WorkflowActionUrl", SqlDbType.VarChar).Value = WorkflowActionUrl;
                                    cmd.Parameters.Add("@OrgUnit1", SqlDbType.VarChar).Value = OrgUnit1;
                                    cmd.Parameters.Add("@OrgUnit2", SqlDbType.VarChar).Value = OrgUnit2;
                                    cmd.Parameters.Add("@OrgUnit3", SqlDbType.VarChar).Value = OrgUnit3;
                                    cmd.Parameters.Add("@OrgUnit4", SqlDbType.VarChar).Value = OrgUnit4;
                                    cmd.Parameters.Add("@OrgUnit5", SqlDbType.VarChar).Value = OrgUnit5;
                                    cmd.Parameters.Add("@OrgUnit6", SqlDbType.VarChar).Value = OrgUnit6;
                                    cmd.Parameters.Add("@Custom1", SqlDbType.VarChar).Value = Custom1;
                                    cmd.Parameters.Add("@Custom2", SqlDbType.VarChar).Value = Custom2;
                                    cmd.Parameters.Add("@Custom3", SqlDbType.VarChar).Value = Custom3;
                                    cmd.Parameters.Add("@Custom4", SqlDbType.VarChar).Value = Custom4;
                                    cmd.Parameters.Add("@Custom5", SqlDbType.VarChar).Value = Custom5;
                                    cmd.Parameters.Add("@Custom6", SqlDbType.VarChar).Value = Custom6;
                                    cmd.Parameters.Add("@Custom7", SqlDbType.VarChar).Value = Custom7;
                                    cmd.Parameters.Add("@Custom8", SqlDbType.VarChar).Value = Custom8;
                                    cmd.Parameters.Add("@Custom9", SqlDbType.VarChar).Value = Custom9;
                                    cmd.Parameters.Add("@Custom10", SqlDbType.VarChar).Value = Custom10;
                                    cmd.Parameters.Add("@Custom11", SqlDbType.VarChar).Value = Custom11;
                                    cmd.Parameters.Add("@Custom12", SqlDbType.VarChar).Value = Custom12;
                                    cmd.Parameters.Add("@Custom13", SqlDbType.VarChar).Value = Custom13;
                                    cmd.Parameters.Add("@Custom14", SqlDbType.VarChar).Value = Custom14;
                                    cmd.Parameters.Add("@Custom15", SqlDbType.VarChar).Value = Custom15;
                                    cmd.Parameters.Add("@Custom16", SqlDbType.VarChar).Value = Custom16;
                                    cmd.Parameters.Add("@Custom17", SqlDbType.VarChar).Value = Custom17;
                                    cmd.Parameters.Add("@Custom18", SqlDbType.VarChar).Value = Custom18;
                                    cmd.Parameters.Add("@Custom19", SqlDbType.VarChar).Value = Custom19;
                                    cmd.Parameters.Add("@Custom20", SqlDbType.VarChar).Value = Custom20;
                                  

                                    conn.Open();
                                    cmd.ExecuteNonQuery();
                                    log.Info("First set of 25 rows inserted in database");
                                    insertedRows++;
                                  if(insertedRows== expenseNodes.Count)
                                    {
                                        
                                        XmlNodeList NextPageNode = xmlDoc.SelectNodes("//Reports/NextPage");
                                        foreach (XmlNode NextPage in NextPageNode)
                                        {
                                            if (NextPage.InnerText != "")
                                            {
                                                int index = NextPage.InnerText.LastIndexOf('=');
                                                offset = NextPage.InnerText.Substring(index);
                                            }
                                        }
                                        goto INSETNEXTPAGEROWS;
                                    }
                                }
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in GetAllUsers:" + ex.Message);
                        return string.Empty;
                    }

                }
            }
            return "Successfully fetched all the reports";
        }
    }
    catch (Exception ex)
    {
        log.Error("Error in GetReports:" + ex.Message);
        return string.Empty;
    }
}
//to save the individual entries linked to the expense report
static async Task<string> GetandInsertReportEntries(string AccessToken, string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL,string connectionstring)
{
    log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    try
    {
        List<string> usersList = new List<string>();
        log.Info("In method GetandInsertReportEntries");
        // Make a request to the Concur API and get the XML response
        log.Info("Get all the users from Users table in database");
        using (var conn = new SqlConnection(connectionstring))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT_ALL_USERS", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                cmd.ExecuteNonQuery();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    usersList.Add(dt.Rows[i].ItemArray[0].ToString());
                }
            }
        }
        log.Info("Loop through all the users and match the user name with report entry");
        foreach (string userName in usersList)
        {
        
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AccessToken);
                //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
                //tokenResponse.EnsureSuccessStatusCode();

                var response = await client.GetAsync(baseURL + "api/v3.0/expense/entries?user=" + userName);
                response.EnsureSuccessStatusCode();
                string xmlData = await response.Content.ReadAsStringAsync();

                // Parse the XML data using XmlDocument
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                // Now you can navigate through the XML structure
                // For example, if there is an 'expenses' element, you can iterate through its children
                XmlNodeList expenseNodes = xmlDoc.SelectNodes("//Items/Entry");
                int insertedRows = 0;
                string offset = "";
            INSETNEXTPAGEROWS:
                if (offset != "")
                {
                    log.Info("Fetch next page's reports from the concur API offset:" + offset);
                    insertedRows = 0;
                    var responseNextPage = await client.GetAsync(baseURL + "api/v3.0/expense/entries?user=" + userName + "&offset" + offset);
                    responseNextPage.EnsureSuccessStatusCode();
                    string xmlDataNextPage = await responseNextPage.Content.ReadAsStringAsync();

                    // Parse the XML data using XmlDocument
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlDataNextPage);

                    // Now you can navigate through the XML structure
                    // For example, if there is an 'expenses' element, you can iterate through its children
                    expenseNodes = xmlDoc.SelectNodes("//Items/Entry");
                }
                foreach (XmlNode expenseNode in expenseNodes)
                {
                    string ReportOwnerID = expenseNode.SelectSingleNode("ReportOwnerID").InnerText;
                    //compare the username with report owner id.
                    //If it matches then insert the rpeort entry 

                    // Access specific elements within each expense
                    string ID = expenseNode.SelectSingleNode("ID").InnerText;
                    string URI = expenseNode.SelectSingleNode("URI").InnerText;
                    string ReportID = expenseNode.SelectSingleNode("ReportID").InnerText;

                    string ExpenseTypeCode = expenseNode.SelectSingleNode("ExpenseTypeCode").InnerText;
                    string ExpenseTypeName = expenseNode.SelectSingleNode("ExpenseTypeName").InnerText;
                    string SpendCategoryCode = expenseNode.SelectSingleNode("SpendCategoryCode").InnerText;
                    string SpendCategoryName = expenseNode.SelectSingleNode("SpendCategoryName").InnerText;
                    string PaymentTypeID = expenseNode.SelectSingleNode("PaymentTypeID").InnerText;
                    string PaymentTypeName = expenseNode.SelectSingleNode("PaymentTypeName").InnerText;
                    string TransactionDate = expenseNode.SelectSingleNode("TransactionDate").InnerText;
                    string TransactionCurrencyCode = expenseNode.SelectSingleNode("TransactionCurrencyCode").InnerText;
                    string TransactionAmount = expenseNode.SelectSingleNode("TransactionAmount").InnerText;
                    string ExchangeRate = expenseNode.SelectSingleNode("ExchangeRate").InnerText;
                    string PostedAmount = expenseNode.SelectSingleNode("PostedAmount").InnerText;
                    string ApprovedAmount = expenseNode.SelectSingleNode("ApprovedAmount").InnerText;
                    string VendorDescription = expenseNode.SelectSingleNode("VendorDescription").InnerText;
                    string VendorListItemID = expenseNode.SelectSingleNode("VendorListItemID").InnerText;
                    string VendorListItemName = expenseNode.SelectSingleNode("VendorListItemName").InnerText;
                    string LocationID = expenseNode.SelectSingleNode("LocationID").InnerText;
                    string LocationName = expenseNode.SelectSingleNode("LocationName").InnerText;
                    string LocationSubdivision = expenseNode.SelectSingleNode("LocationSubdivision").InnerText;
                    string LocationCountry = expenseNode.SelectSingleNode("LocationCountry").InnerText;
                    string Description = expenseNode.SelectSingleNode("Description").InnerText;
                    string IsPersonal = expenseNode.SelectSingleNode("IsPersonal").InnerText;
                    string IsBillable = expenseNode.SelectSingleNode("IsBillable").InnerText;
                    string IsPersonalCardCharge = expenseNode.SelectSingleNode("IsPersonalCardCharge").InnerText;
                    string HasImage = expenseNode.SelectSingleNode("HasImage").InnerText;
                    string IsImageRequired = expenseNode.SelectSingleNode("IsImageRequired").InnerText;
                    string ReceiptReceived = expenseNode.SelectSingleNode("ReceiptReceived").InnerText;
                    string TaxReceiptType = expenseNode.SelectSingleNode("TaxReceiptType").InnerText;
                    string ElectronicReceiptID = expenseNode.SelectSingleNode("ElectronicReceiptID").InnerText;
                    string CompanyCardTransactionID = expenseNode.SelectSingleNode("CompanyCardTransactionID").InnerText;
                    string TripID = expenseNode.SelectSingleNode("TripID").InnerText;
                    string HasItemizations = expenseNode.SelectSingleNode("HasItemizations").InnerText;
                    string AllocationType = expenseNode.SelectSingleNode("AllocationType").InnerText;
                    string HasAttendees = expenseNode.SelectSingleNode("HasAttendees").InnerText;
                    string HasVAT = expenseNode.SelectSingleNode("HasVAT").InnerText;
                    string HasAppliedCashAdvance = expenseNode.SelectSingleNode("HasAppliedCashAdvance").InnerText;
                    string HasComments = expenseNode.SelectSingleNode("HasComments").InnerText;
                    string HasExceptions = expenseNode.SelectSingleNode("HasExceptions").InnerText;
                    string IsPaidByExpensePay = expenseNode.SelectSingleNode("IsPaidByExpensePay").InnerText;
                    string EmployeeBankAccountID = expenseNode.SelectSingleNode("EmployeeBankAccountID").InnerText;
                    string Journey = expenseNode.SelectSingleNode("Journey").InnerText;
                    string LastModified = expenseNode.SelectSingleNode("LastModified").InnerText;
                    string FormID = expenseNode.SelectSingleNode("FormID").InnerText;
                    string OrgUnit1 = expenseNode.SelectSingleNode("OrgUnit1").InnerText;
                    string OrgUnit2 = expenseNode.SelectSingleNode("OrgUnit2").InnerText;
                    string OrgUnit3 = expenseNode.SelectSingleNode("OrgUnit3").InnerText;
                    string OrgUnit4 = expenseNode.SelectSingleNode("OrgUnit4").InnerText;
                    string OrgUnit5 = expenseNode.SelectSingleNode("OrgUnit5").InnerText;
                    string OrgUnit6 = expenseNode.SelectSingleNode("OrgUnit6").InnerText;
                    XmlNodeList Custom1 = xmlDoc.SelectNodes("//Items/Entry/Custom1");
                    string Custom1_Type = "";
                    string Custom1_Value = "";
                    string Custom1_Code = "";
                    string Custom1_ListItemID = "";
                    try
                    {
                       
                        foreach (XmlNode Custom1Node in Custom1)
                        {
                            if (Custom1Node.InnerText != "")
                            {
                                Custom1_Type = Custom1Node.SelectSingleNode("Type").InnerText;
                                Custom1_Value = Custom1Node.SelectSingleNode("Value").InnerText;
                                Custom1_Code = Custom1Node.SelectSingleNode("Code").InnerText;
                                Custom1_ListItemID = Custom1Node.SelectSingleNode("ListItemID").InnerText;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        log.Error("GetandInsertReportEntries :: Error while fetching custom1 data:" + ex.Message);
                    }
                    string Custom2 = "";

                    string Custom3 = "";


                    string Custom4 = "";


                    string Custom5 = "";


                    string Custom6 = "";


                    string Custom7 = "";


                    string Custom8 = "";


                    string Custom9 = "";

                    XmlNodeList Custom10 = xmlDoc.SelectNodes("//Items/Entry/Custom10");
                    string Custom10_Type = "";
                    string Custom10_Value = "";
                    string Custom10_Code = "";
                    string Custom10_ListItemID = "";
                    try { 
                    foreach (XmlNode Custom10Node in Custom10)
                    {
                            if (Custom10Node.InnerText != "")
                            {
                                Custom10_Type = Custom10Node.SelectSingleNode("Type").InnerText;
                                Custom10_Value = Custom10Node.SelectSingleNode("Value").InnerText;
                                Custom10_Code = Custom10Node.SelectSingleNode("Code").InnerText;
                                Custom10_ListItemID = Custom10Node.SelectSingleNode("ListItemID").InnerText;
                            }
                    }
                    }
                    catch (Exception ex)
                    {
                        log.Error("GetandInsertReportEntries :: Error while fetching custom10 data:" + ex.Message);
                    }
                    string Custom11 = expenseNode.SelectSingleNode("Custom11").InnerText;
                    string Custom12 = expenseNode.SelectSingleNode("Custom12").InnerText;
                    string Custom13 = expenseNode.SelectSingleNode("Custom13").InnerText;
                    string Custom14 = expenseNode.SelectSingleNode("Custom14").InnerText;
                    string Custom15 = expenseNode.SelectSingleNode("Custom15").InnerText;
                    string Custom16 = expenseNode.SelectSingleNode("Custom16").InnerText;
                    string Custom17 = expenseNode.SelectSingleNode("Custom17").InnerText;
                    string Custom18 = expenseNode.SelectSingleNode("Custom18").InnerText;
                    string Custom19 = expenseNode.SelectSingleNode("Custom19").InnerText;
                    string Custom20 = expenseNode.SelectSingleNode("Custom20").InnerText;
                    string Custom21 = expenseNode.SelectSingleNode("Custom21").InnerText;
                    string Custom22 = expenseNode.SelectSingleNode("Custom22").InnerText;
                    string Custom23 = expenseNode.SelectSingleNode("Custom23").InnerText;
                    string Custom24 = expenseNode.SelectSingleNode("Custom24").InnerText;
                    string Custom25 = expenseNode.SelectSingleNode("Custom25").InnerText;
                    string Custom26 = expenseNode.SelectSingleNode("Custom26").InnerText;
                    string Custom27 = expenseNode.SelectSingleNode("Custom27").InnerText;
                    string Custom28 = expenseNode.SelectSingleNode("Custom28").InnerText;
                    string Custom29 = expenseNode.SelectSingleNode("Custom29").InnerText;
                    string Custom30 = expenseNode.SelectSingleNode("Custom30").InnerText;
                    string Custom31 = expenseNode.SelectSingleNode("Custom31").InnerText;
                    string Custom32 = expenseNode.SelectSingleNode("Custom32").InnerText;
                    string Custom33 = expenseNode.SelectSingleNode("Custom33").InnerText;
                    string Custom34 = expenseNode.SelectSingleNode("Custom34").InnerText;
                    string Custom35 = expenseNode.SelectSingleNode("Custom35").InnerText;
                    string Custom36 = expenseNode.SelectSingleNode("Custom36").InnerText;
                    string Custom37 = expenseNode.SelectSingleNode("Custom37").InnerText;
                    string Custom38 = expenseNode.SelectSingleNode("Custom38").InnerText;
                    string Custom39 = expenseNode.SelectSingleNode("Custom39").InnerText;
                    string Custom40 = expenseNode.SelectSingleNode("Custom40").InnerText;

                    // Do something with the extracted data
                    try
                    {
                        log.Info("In method Insert Expense Reports");


                        using (var conn = new SqlConnection(connectionstring))
                        {
                            using (SqlCommand cmd = new SqlCommand("INSERT_EXPENSEENTRY", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@ID", SqlDbType.VarChar).Value = ID;
                                cmd.Parameters.Add("@URI", SqlDbType.VarChar).Value = URI;
                                cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = ReportID;
                                cmd.Parameters.Add("@ReportOwnerID", SqlDbType.VarChar).Value = ReportOwnerID;
                                cmd.Parameters.Add("@ExpenseTypeCode", SqlDbType.VarChar).Value = ExpenseTypeCode;
                                cmd.Parameters.Add("@ExpenseTypeNam", SqlDbType.VarChar).Value = ExpenseTypeName;
                                cmd.Parameters.Add("@SpendCategoryCode", SqlDbType.VarChar).Value = SpendCategoryCode;
                                cmd.Parameters.Add("@SpendCategoryName", SqlDbType.VarChar).Value = SpendCategoryName;
                                cmd.Parameters.Add("@PaymentTypeID", SqlDbType.VarChar).Value = PaymentTypeID;
                                cmd.Parameters.Add("@PaymentTypeName", SqlDbType.VarChar).Value = PaymentTypeName;
                                cmd.Parameters.Add("@TransactionDate", SqlDbType.VarChar).Value = TransactionDate;
                                cmd.Parameters.Add("@TransactionCurrencyCode", SqlDbType.VarChar).Value = TransactionCurrencyCode;
                                cmd.Parameters.Add("@TransactionAmount", SqlDbType.VarChar).Value = TransactionAmount;
                                cmd.Parameters.Add("@ExchangeRate", SqlDbType.VarChar).Value = ExchangeRate;

                                cmd.Parameters.Add("@PostedAmount", SqlDbType.VarChar).Value = PostedAmount;
                                cmd.Parameters.Add("@ApprovedAmount", SqlDbType.VarChar).Value = ApprovedAmount;
                                cmd.Parameters.Add("@VendorDescription", SqlDbType.VarChar).Value = VendorDescription;
                                cmd.Parameters.Add("@VendorListItemID", SqlDbType.VarChar).Value = VendorListItemID;
                                cmd.Parameters.Add("@VendorListItemName", SqlDbType.VarChar).Value = VendorListItemName;
                                cmd.Parameters.Add("@LocationID", SqlDbType.VarChar).Value = LocationID;
                                cmd.Parameters.Add("@LocationName", SqlDbType.VarChar).Value = LocationName;
                                cmd.Parameters.Add("@LocationSubdivision", SqlDbType.VarChar).Value = LocationSubdivision;
                                cmd.Parameters.Add("@LocationCountry", SqlDbType.VarChar).Value = LocationCountry;
                                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = Description;
                                cmd.Parameters.Add("@IsPersonal", SqlDbType.VarChar).Value = IsPersonal;
                                cmd.Parameters.Add("@IsBillable", SqlDbType.VarChar).Value = IsBillable;
                                cmd.Parameters.Add("@IsPersonalCardCharge", SqlDbType.VarChar).Value = IsPersonalCardCharge;
                                cmd.Parameters.Add("@HasImage", SqlDbType.VarChar).Value = HasImage;
                                cmd.Parameters.Add("@IsImageRequired", SqlDbType.VarChar).Value = IsImageRequired;
                                cmd.Parameters.Add("@ReceiptReceived", SqlDbType.VarChar).Value = ReceiptReceived;
                                cmd.Parameters.Add("@TaxReceiptType", SqlDbType.VarChar).Value = TaxReceiptType;
                                cmd.Parameters.Add("@ElectronicReceiptID", SqlDbType.VarChar).Value = ElectronicReceiptID;
                                cmd.Parameters.Add("@CompanyCardTransactionID", SqlDbType.VarChar).Value = CompanyCardTransactionID;
                                cmd.Parameters.Add("@TripID", SqlDbType.VarChar).Value = TripID;
                                cmd.Parameters.Add("@HasItemizations", SqlDbType.VarChar).Value = HasItemizations;
                                cmd.Parameters.Add("@AllocationType", SqlDbType.VarChar).Value = AllocationType;
                                cmd.Parameters.Add("@HasAttendees", SqlDbType.VarChar).Value = HasAttendees;
                                cmd.Parameters.Add("@HasVAT", SqlDbType.VarChar).Value = HasVAT;
                                cmd.Parameters.Add("@HasAppliedCashAdvance", SqlDbType.VarChar).Value = HasAppliedCashAdvance;
                                cmd.Parameters.Add("@HasComments", SqlDbType.VarChar).Value = HasComments;
                                cmd.Parameters.Add("@HasExceptions", SqlDbType.VarChar).Value = HasExceptions;
                                cmd.Parameters.Add("@IsPaidByExpensePay", SqlDbType.VarChar).Value = IsPaidByExpensePay;
                                cmd.Parameters.Add("@EmployeeBankAccountID", SqlDbType.VarChar).Value = EmployeeBankAccountID;
                                cmd.Parameters.Add("@Journey", SqlDbType.VarChar).Value = Journey;
                                cmd.Parameters.Add("@LastModified", SqlDbType.VarChar).Value = LastModified;
                                cmd.Parameters.Add("@FormID", SqlDbType.VarChar).Value = FormID;
                                cmd.Parameters.Add("@OrgUnit1", SqlDbType.VarChar).Value = OrgUnit1;
                                cmd.Parameters.Add("@OrgUnit2", SqlDbType.VarChar).Value = OrgUnit2;
                                cmd.Parameters.Add("@OrgUnit3", SqlDbType.VarChar).Value = OrgUnit3;
                                cmd.Parameters.Add("@OrgUnit4", SqlDbType.VarChar).Value = OrgUnit4;
                                cmd.Parameters.Add("@OrgUnit5", SqlDbType.VarChar).Value = OrgUnit5;
                                cmd.Parameters.Add("@OrgUnit6", SqlDbType.VarChar).Value = OrgUnit6;
                                cmd.Parameters.Add("@Custom1_Type", SqlDbType.VarChar).Value = Custom1_Type;
                                cmd.Parameters.Add("@Custom1_Value", SqlDbType.VarChar).Value = Custom1_Value;
                                cmd.Parameters.Add("@Custom1_Code", SqlDbType.VarChar).Value = Custom1_Code;
                                cmd.Parameters.Add("@Custom1_ListItemID", SqlDbType.VarChar).Value = Custom1_ListItemID;

                                cmd.Parameters.Add("@Custom2", SqlDbType.VarChar).Value = Custom2;


                                cmd.Parameters.Add("@Custom3", SqlDbType.VarChar).Value = Custom3;


                                cmd.Parameters.Add("@Custom4", SqlDbType.VarChar).Value = Custom4;


                                cmd.Parameters.Add("@Custom5", SqlDbType.VarChar).Value = Custom5;


                                cmd.Parameters.Add("@Custom6", SqlDbType.VarChar).Value = Custom6;


                                cmd.Parameters.Add("@Custom7", SqlDbType.VarChar).Value = Custom7;


                                cmd.Parameters.Add("@Custom8", SqlDbType.VarChar).Value = Custom8;


                                cmd.Parameters.Add("@Custom9", SqlDbType.VarChar).Value = Custom9;


                                cmd.Parameters.Add("@Custom10_Type", SqlDbType.VarChar).Value = Custom10_Type;
                                cmd.Parameters.Add("@Custom10_Value", SqlDbType.VarChar).Value = Custom10_Value;
                                cmd.Parameters.Add("@Custom10_Code", SqlDbType.VarChar).Value = Custom10_Code;
                                cmd.Parameters.Add("@Custom10_ListItemID", SqlDbType.VarChar).Value = Custom10_ListItemID;

                                cmd.Parameters.Add("@Custom11", SqlDbType.VarChar).Value = Custom11;


                                cmd.Parameters.Add("@Custom12", SqlDbType.VarChar).Value = Custom12;


                                cmd.Parameters.Add("@Custom13", SqlDbType.VarChar).Value = Custom13;


                                cmd.Parameters.Add("@Custom14", SqlDbType.VarChar).Value = Custom14;


                                cmd.Parameters.Add("@Custom15", SqlDbType.VarChar).Value = Custom15;


                                cmd.Parameters.Add("@Custom16", SqlDbType.VarChar).Value = Custom16;




                                cmd.Parameters.Add("@Custom17", SqlDbType.VarChar).Value = Custom17;


                                cmd.Parameters.Add("@Custom18", SqlDbType.VarChar).Value = Custom18;


                                cmd.Parameters.Add("@Custom19", SqlDbType.VarChar).Value = Custom19;


                                cmd.Parameters.Add("@Custom20", SqlDbType.VarChar).Value = Custom20;


                                cmd.Parameters.Add("@Custom21", SqlDbType.VarChar).Value = Custom21;


                                cmd.Parameters.Add("@Custom22", SqlDbType.VarChar).Value = @Custom22;


                                cmd.Parameters.Add("@Custom23", SqlDbType.VarChar).Value = Custom23;


                                cmd.Parameters.Add("@Custom24", SqlDbType.VarChar).Value = Custom24;


                                cmd.Parameters.Add("@Custom25", SqlDbType.VarChar).Value = Custom25;


                                cmd.Parameters.Add("@Custom26", SqlDbType.VarChar).Value = Custom26;


                                cmd.Parameters.Add("@Custom27", SqlDbType.VarChar).Value = Custom27;


                                cmd.Parameters.Add("@Custom28", SqlDbType.VarChar).Value = Custom28;


                                cmd.Parameters.Add("@Custom29", SqlDbType.VarChar).Value = Custom29;


                                cmd.Parameters.Add("@Custom30", SqlDbType.VarChar).Value = Custom30;


                                cmd.Parameters.Add("@Custom31", SqlDbType.VarChar).Value = Custom31;


                                cmd.Parameters.Add("@Custom32", SqlDbType.VarChar).Value = Custom32;


                                cmd.Parameters.Add("@Custom33", SqlDbType.VarChar).Value = Custom33;


                                cmd.Parameters.Add("@Custom34", SqlDbType.VarChar).Value = Custom34;


                                cmd.Parameters.Add("@Custom35", SqlDbType.VarChar).Value = Custom35;


                                cmd.Parameters.Add("@Custom36", SqlDbType.VarChar).Value = Custom36;


                                cmd.Parameters.Add("@Custom37", SqlDbType.VarChar).Value = Custom37;


                                cmd.Parameters.Add("@Custom38", SqlDbType.VarChar).Value = Custom38;


                                cmd.Parameters.Add("@Custom39", SqlDbType.VarChar).Value = Custom39;
                                cmd.Parameters.Add("@Custom40", SqlDbType.VarChar).Value = Custom40;
                                conn.Open();
                                cmd.ExecuteNonQuery();
                                log.Info("First set of 25 rows inserted in database");
                                insertedRows++;
                                if (insertedRows == expenseNodes.Count)
                                {

                                    XmlNodeList NextPageNode = xmlDoc.SelectNodes("//Entries/NextPage");
                                    foreach (XmlNode NextPage in NextPageNode)
                                    {
                                        if (NextPage.InnerText != "")
                                        {
                                            int index = NextPage.InnerText.LastIndexOf('=');
                                            offset = NextPage.InnerText.Substring(index);
                                            goto INSETNEXTPAGEROWS;
                                        }
                                       else
                                        {
                                            break;
                                        }
                                    }
                                    
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in GetandInsertReportEntries:" + ex.Message);
                        return string.Empty;
                    }


                }
                
            }
        }
        return "Report entry inserted successfully";
    }
    catch (Exception ex)
    {
        log.Error("Error in GetandInsertReportEntries:" + ex.Message);
        return string.Empty;
    }
}
