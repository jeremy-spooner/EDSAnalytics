using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Xunit;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace EDSFilterTest
{
    public class EDSFilterTest 
    {
        [Fact]
        public void Test1()
        {
            Assert.True(EDSTest.Program.MainAsync(true).Result);
            /*
            Console.WriteLine("Getting configuration from appsettings.json");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.test.json", optional: true);
            IConfiguration configuration = builder.Build();
            string port = configuration["EDSPort"];
            string tenantId = configuration["TenantId"];
            string namespaceId = configuration["NamespaceId"];
            string apiVersion = configuration["apiVersion"];
            // create type
            string type = @"{  ""Id"": ""EdsSample"",
                                ""Name"":""EdsSample"",
                                ""SdsTypeCode"": 1, 
                                ""Properties"":[
                                    {    ""Id"": ""Time"",
                                         ""Name"": ""Time"",
                                         ""IsKey"": true,
                                         ""SdsType"": {
                                            ""SdsTypeCode"": 16
                                         }
                                    },
                                    {    ""Id"":""Measurement"",
                                         ""Name"":""Measurement"",
                                         ""SdsType"": {
                                            ""SydsTypeCode"": 14
                                         }
                                    }
                            ]}";
            HttpClient httpClient = new HttpClient();
            httpClient.PostAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/EdsSample", new StringContent(JsonSerializer.Serialize(type)));
            // create stream 

            // send data 

            // delete stream

            // delete type
            Assert.True(true);
            */
        }
    }
}
