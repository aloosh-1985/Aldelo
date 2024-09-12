using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Aldelo.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


namespace Aldelo
{
    public class Helper
    {

        #region Aldelo
        public void AddHeaderParameters(ref RestRequest request)
        {
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("ISV-ID", "D-240814-0001");
            request.AddHeader("ISV-Key", "4ece60be-6324-4429-b572-d90dda48bd9c");
            request.AddHeader("App-Key", "8f4bf998-ff1e-496f-aaa5-f5bc64a46c9b");
            request.AddHeader("App-Version", "1.1.1.1");
            request.AddHeader("Store-Sub-ID", "3771-3C12");
            request.AddHeader("Store-App-Token", "bfc0b824-e45f-43c6-bc10-0e0c2bbb575b");
        }

        public void GetOrders()
        {

            var client = new RestClient("https://sandbox.aldelo.io/orderList/20240911?status=open&tablenumber=a1&begintime=0800&endtime=2000");
            var request = new RestRequest(Method.GET);
            AddHeaderParameters(ref request);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }

        public void GetCustomerEmails()
        {
            //*********************************************************************************************8
            var client = new RestClient("https://sandbox.aldelo.io/emailList");
            var request = new RestRequest(Method.GET);
            AddHeaderParameters(ref request);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            ///************
            //    var responseContent = response.Content.ReadAsStringAsync().Result;

            //    // Parse the JSON response
            //    JObject jsonObject = JObject.Parse(responseContent);

            //    // Iterate over the properties of the JObject
            //    foreach (var property in jsonObject.Properties())
            //    {
            //        // Deserialize the JSON into a Customer object
            //        Customer customer = JsonConvert.DeserializeObject<Customer>(property.Value.ToString());
            //        customers.Add(customer);
            //    }
            //}

            //return customers;
        }
        #endregion

        #region doordash
        public string GetJWTToken()
        {
            // Credentials provided from https://developer.doordash.com/portal/integration/drive/credentials
            // TODO: Replace placeholders with credential values
            var accessKey = new Dictionary<string, string>{
             {"developer_id", "a2df675c-1a61-4f7f-af9b-2c5c341269a6"}, // TODO: Update value with Developer ID
             {"key_id", "888c6346-cad5-4ea7-b627-1ee6207a862b"}, // TODO: Update value with Key ID
             {"signing_secret", "Vd7vRGdwuDFQp8o7cfyVK93KKJLansp3zIK14taMwV8"} // TODO: Update value with Signing Secret
           };
            // Signing Secret is Base64Encoded when generated on the Credentials page, need to decode to use
            var decodedSecret = Base64UrlEncoder.DecodeBytes(accessKey["signing_secret"]);
            var securityKey = new SymmetricSecurityKey(decodedSecret);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(credentials);

            // DoorDash header used to identify DoorDash JWT version
            header["dd-ver"] = "DD-JWT-V1";

            var payload = new JwtPayload(
                issuer: accessKey["developer_id"],
                audience: "doordash",
                claims: new List<Claim> { new Claim("kid", accessKey["key_id"]) },
                notBefore: null,
                expires: System.DateTime.UtcNow.AddMinutes(30),
                issuedAt: System.DateTime.UtcNow);

            var securityToken = new JwtSecurityToken(header, payload);
            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return token;
        }
        public async void  MakeNewDelivery(string deliverId,string token="")
        {
            if(string.IsNullOrEmpty(token))
            token = GetJWTToken();
            // Create data needed to create a new delivery  
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(new
            {
                external_delivery_id = deliverId,
                pickup_address = "901 Market Street 6th Floor San Francisco, CA 94103",
                pickup_business_name = "Wells Fargo SF Downtown",
                pickup_phone_number = "+16505555555",
                pickup_instructions = "Enter gate code 1234 on the callbox.",
                pickup_reference_tag = "Order number 61",
                dropoff_address = "901 Market Street 6th Floor San Francisco, CA 94103",
                dropoff_business_name = "Wells Fargo SF Downtown",
                dropoff_phone_number = "+16505555555",
                dropoff_instructions = "Enter gate code 1234 on the callbox."
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Note: In a production system don't create a new HttpClient per request
            // see https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines for guidance 
             HttpClient client = new  HttpClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync("https://openapi.doordash.com/drive/v2/deliveries", content);

            var status = result.StatusCode;
            var resultString = await result.Content.ReadAsStringAsync();

            Console.WriteLine("");
            Console.WriteLine("Result Status: " + status);
            Console.WriteLine("Result Response: " + resultString);
        }

        public async void RefundOrder(string orderId,string token)
        {
            if (string.IsNullOrEmpty(token))
                token = GetJWTToken();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(new
            {
                refund_reason = "cancelled_order"
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Note: In a production system don't create a new HttpClient per request
            // see https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines for guidance 
             HttpClient clientR = new HttpClient();

            clientR.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var result = await clientR.PostAsync("https://openapi.doordash.com/drive/v2/deliveries/" + orderId+"/refunds", content);

            var status = result.StatusCode;
            var resultString = await result.Content.ReadAsStringAsync();
        }

        #endregion
    }
}