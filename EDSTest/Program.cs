using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDSTest
{
    class Program
    {

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }
        public static async Task<bool> MainAsync(bool test = false)
        {
            // variables will be changed
            string apiVersion = "v1";
            string tenantId = "default";
            string namespaceId = "default";


           /* IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.test.json", optional: true);
            IConfiguration configuration = builder.Build();
            Console.WriteLine("Hello World!");
            */

             
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

                try
                {
                    // Step 1 

                    // create Timestamp property
                    SdsTypeProperty timestamp = new SdsTypeProperty
                    {
                        Id = "Timestamp",
                        Name = "Timestamp",
                        IsKey = true,
                        SdsType = new SdsType
                        {
                            Name = "DateTime",
                            SdsTypeCode = 16
                        }
                    };

                    // create Value property
                    SdsTypeProperty value = new SdsTypeProperty
                    {
                        Id = "Value",
                        Name = "Value",
                        IsKey = false,
                        SdsType = new SdsType
                        {
                            Name = "Double",
                            SdsTypeCode = 14
                        }
                    };

                    // create SineWave SdsType
                    SdsType sineWaveType = new SdsType
                    {
                        Id = "SineWave",
                        Name = "SineWave",
                        SdsTypeCode = 1,
                        Properties = new List<SdsTypeProperty>()
                        {
                            timestamp,
                            value
                        }
                    };

                    // send the http request to EDS to create type
                    StringContent type = new StringContent(JsonConvert.SerializeObject(sineWaveType));
                    HttpResponseMessage responseType =
                        await httpClient.PostAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{sineWaveType.Id}",
                            type);
                    
                    // step 2
                    // create sine wave stream                   
                    SdsStream sineWaveStream = new SdsStream
                    {
                        TypeId = sineWaveType.Id,
                        Id = "SineWave",
                        Name = "SineWave"
                    };

                    // send the http request to EDS to create stream
                    StringContent stream = new StringContent(JsonConvert.SerializeObject(sineWaveStream));
                    HttpResponseMessage responseStream =
                        await httpClient.PostAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}",
                            stream);

                    // step 3 - fill data with a sine wave of doubles ranging from -1 to 1
                    List<SineData> wave = new List<SineData>();
                    DateTime current = new DateTime();
                    current = DateTime.UtcNow;
                    
                    for (int i = 0; i < 100; i = i + 1)
                    {
                        SineData newEvent = new SineData(i);
                        newEvent.Timestamp = current.AddSeconds(i).ToString("o");
                        wave.Add(newEvent);
                    }

                    // send the http request to EDS to send events
                    StringContent data = new StringContent(JsonConvert.SerializeObject(wave));
                    HttpResponseMessage responseDataEgress =
                        await httpClient.PostAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}/Data",
                            data); 

                    // step 4 - read in the data from the stream
                    HttpResponseMessage responseDataIngress =
                        await httpClient.GetAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}/Data"); 


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw e;
                }


            }
            return true;
        }
    }
}
