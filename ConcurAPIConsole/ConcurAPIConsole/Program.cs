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
   
// Replace these with your Concur API credentials
// Concur API credentials
string clientId = "3ab2303c-c877-4ea2-9e9c-07c34bf71a30";
string clientSecret = "7ab4f0ec-7b3f-438f-b2ed-a9d936d5e74c";
string baseURL = "https://us2.api.concursolutions.com/";
string username = "1c2dfa4b-99da-45fe-b66c-c5e2ad0b1f9b";
string password = "at-222jvgk28ra24apg3wqg0t33duh3fn";
string refresh_token = "2n75e8k2x8m54qmr2gvculv3wnkugn";
string companyId = "56713";
// Display the number of command line arguments.
Console.WriteLine(args.Length);
// Step 1: Get Authorization token from Cocur
//string AccessToken = await GetAccessTokenAsync(refresh_token, username, password, clientId, clientSecret, baseURL);

// Step 2: Get Users data using authorization token
//string status = await GetAllUsers(refresh_token, username, password, clientId, clientSecret, baseURL);

// Step 3: Get Expense Reports
//string statusReport = await GetReports(refresh_token, username, password, clientId, clientSecret, baseURL);

// Step 4: Get Expense Reports Entries
//string statusReportEntry = await GetReportEntries(refresh_token, username, password, clientId, clientSecret, baseURL);
string status = await InsertUsers();



    //Store token with expiration and such in a table called concur.authenticationTokens
    static async Task<string> GetAccessTokenAsync(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL)
{
    log4net.Config.BasicConfigurator.Configure();
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
    try
    {
        log.Info("In method GetAccessTokenAsync");
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
    static async Task<string> GetAllUsers(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL)
{
    log4net.Config.BasicConfigurator.Configure();
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
    try
    {
        log.Info("In method GetAllUsers");

        string AccessToken = await GetAccessTokenAsync(refresh_token, username, password, clientId, clientSecret, baseURL);
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
            Console.WriteLine("GetAllUsers:");
            Console.WriteLine("Successfully fetched all the users");
            using (var conn = new SqlConnection("Server=70.35.201.113;Database=Energy_Recovery_Dev;persist security info=True;user id=VASPLogin;password=vasp@22$;multipleactiveresultsets=True"))
            {
                var cmd = new SqlCommand("insert into Users values (@bar)", conn);
                cmd.Parameters.AddWithValue("@bar", 17);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return "Successfully fetched all the users";
        }
    }
    catch (Exception ex)
    {
        log.Error("Error in GetAllUsers:" + ex.Message);
        return string.Empty;
    }
}
static async Task<string> InsertUsers()
{
    log4net.Config.BasicConfigurator.Configure();
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
    try
    {
        log.Info("In method InsertUsers");
        using (var conn = new SqlConnection("Data Source=VASP-LAPTOP13\\SQLEXPRESS01;Database=concur;Integrated Security=true;TrustServerCertificate=true;User Instance=False;"))
        {
            var cmd = new SqlCommand("insert into Sample values (@ID)", conn);
            cmd.Parameters.AddWithValue("@ID", 1);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        return "Successfully Inserted all the users";
        
    }
    catch (Exception ex)
    {
        log.Error("Error in GetAllUsers:" + ex.Message);
        return string.Empty;
    }
}
//all approved Expense reports since the last run time
static async Task<string> GetReports(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL)
{
    log4net.Config.BasicConfigurator.Configure();
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
    try
    {
        log.Info("In method GetReports");
        // Make a request to the Concur API and get the XML response

        string AccessToken = await GetAccessTokenAsync(refresh_token, username, password, clientId, clientSecret, baseURL);
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", AccessToken);
            //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
            //tokenResponse.EnsureSuccessStatusCode();

            var response = await client.GetAsync(baseURL + "api/v3.0/expense/reports?limit=15&user=ALL");
            response.EnsureSuccessStatusCode();
            string xmlData = await response.Content.ReadAsStringAsync();

            // Parse the XML data using XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            // Now you can navigate through the XML structure
            // For example, if there is an 'expenses' element, you can iterate through its children
            XmlNodeList expenseNodes = xmlDoc.SelectNodes("//Items/Report");
            Console.WriteLine("GetReports:");
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
                // Do something with the extracted data

                Console.WriteLine($"ID: {ID}, Name: {Name}");
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
    static async Task<string> GetReportEntries(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL)
{
    log4net.Config.BasicConfigurator.Configure();
    log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
    try
    {
        log.Info("In method GetReportEntries");
        // Make a request to the Concur API and get the XML response

        string AccessToken = await GetAccessTokenAsync(refresh_token, username, password, clientId, clientSecret, baseURL);
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", AccessToken);
            //var tokenResponse = await client.PostAsync(tokenEndpoint, tokenRequest);
            //tokenResponse.EnsureSuccessStatusCode();

            var response = await client.GetAsync(baseURL + "api/v3.0/expense/entries");
            response.EnsureSuccessStatusCode();
            string xmlData = await response.Content.ReadAsStringAsync();

            // Parse the XML data using XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            // Now you can navigate through the XML structure
            // For example, if there is an 'expenses' element, you can iterate through its children
            XmlNodeList expenseNodes = xmlDoc.SelectNodes("//Items/Entry");
            Console.WriteLine("GetReportEntries:");
            foreach (XmlNode expenseNode in expenseNodes)
            {
                // Access specific elements within each expense
                string ID = expenseNode.SelectSingleNode("ID").InnerText;
                string ReportID = expenseNode.SelectSingleNode("ReportID").InnerText;
                // Do something with the extracted data

                Console.WriteLine($"ID: {ID}, ReportID: {ReportID}");
            }
            return "Successfully fetched all the reprt entries";
        }
    }
    catch (Exception ex)
    {
        log.Error("Error in GetReportEntries:" + ex.Message);
        return string.Empty;
    }
}
