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

namespace Aldelo
{
    public class Helper
    {
        public List<Customer> GetCustomerEmails()
        {

            List<Customer> customers = new List<Customer>();
            var client = new RestClient();
            var request = (HttpWebRequest)WebRequest.Create("https://sandbox.aldelo.io/emailList");

            request.Method = "GET";            
            request.Headers.Add("ISV-ID", "D-240814-0001");
            request.Headers.Add("ISV-Key", "4ece60be-6324-4429-b572-d90dda48bd9c");
            request.Headers.Add("App-Key", "8f4bf998-ff1e-496f-aaa5-f5bc64a46c9b");
            request.Headers.Add("App-Version", "1.1.1.1");
            request.Headers.Add("Store-Sub-ID", "3771-3C12");
            request.Headers.Add("Store-App-Token", "bfc0b824-e45f-43c6-bc10-0e0c2bbb575b");

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();

                    // Parse the JSON response
                    JObject jsonObject = JObject.Parse(responseContent);

                    // Iterate over the properties of the JObject
                    foreach (var property in jsonObject.Properties())
                    {
                        // Deserialize the JSON into a Customer object
                        Customer customer = JsonConvert.DeserializeObject<Customer>(property.Value.ToString());
                        customers.Add(customer);
                    }
                }
            }

            return customers;
        }
    }
}