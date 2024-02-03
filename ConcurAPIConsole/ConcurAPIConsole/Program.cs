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
string status = await GetAndInsertReports(refresh_token, username, password, clientId, clientSecret, baseURL);



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
static async Task<string> GetAndInsertReports(string refresh_token, string username, string password, string clientId, string clientSecret, string baseURL)
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

            var response = await client.GetAsync(baseURL + "api/v3.0/expense/reports?user=ALL");
            response.EnsureSuccessStatusCode();
            string xmlData = await response.Content.ReadAsStringAsync();

            // Parse the XML data using XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            // Now you can navigate through the XML structure
            // For example, if there is an 'expenses' element, you can iterate through its children
            XmlNodeList expenseNodes = xmlDoc.SelectNodes("//Items/Report");
            Console.WriteLine("GetReports:");
            log.Info("Fetch last inserted date from the Expenses table");
            string modifiedDateAfter = "";
            using (var conn = new SqlConnection("Data Source=VASP-LAPTOP13\\SQLEXPRESS01;Database=concur;Integrated Security=true;TrustServerCertificate=true;User Instance=False;"))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT_EXPENSES_LASTINSERTEDDATE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    modifiedDateAfter = cmd.ExecuteScalar().ToString();
                }
            }
            bool goAheadForInsertion = false;
            if (modifiedDateAfter == "" || modifiedDateAfter == null )
            {
                goAheadForInsertion = true;
            }
            else if (System.DateTime.Now.Date > System.DateTime.Parse(modifiedDateAfter))
            {
                goAheadForInsertion = true;
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
                            using (var conn = new SqlConnection("Data Source=VASP-LAPTOP13\\SQLEXPRESS01;Database=concur;Integrated Security=true;TrustServerCertificate=true;User Instance=False;"))
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
