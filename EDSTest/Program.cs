using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


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
            string port = "5590";


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
                    StringContent type = new StringContent(JsonSerializer.Serialize(sineWaveType));
                    HttpResponseMessage responseType =
                        await httpClient.PostAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{sineWaveType.Id}",
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
                    StringContent stream = new StringContent(JsonSerializer.Serialize(sineWaveStream));
                    HttpResponseMessage responseStream =
                        await httpClient.PostAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}",
                            stream);

                    // step 3 - fill data with a sine wave of doubles ranging from -1 to 1

                    List<SineData> wave = new List<SineData>();
                    DateTime current = new DateTime();
                    current = DateTime.UtcNow;
                    
                    // adds sample sine wave values and timestamps incremete
                    for (int i = 0; i < 100; i = i + 1)
                    {
                        SineData newEvent = new SineData(i);
                        newEvent.Timestamp = current.AddSeconds(i).ToString("o");
                        wave.Add(newEvent);
                    }
                    
                    // send the http request to EDS to send events
                    StringContent data = new StringContent(JsonSerializer.Serialize(wave));
                    HttpResponseMessage responseDataEgress = await httpClient.PostAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}/Data", data);
                    var returnData = new List<SineData>(); 

                    // step 4 - read in the data from the stream

                    var response = await httpClient.GetAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}/Data?startIndex={wave[0].Timestamp}&count=100");
                    var responseBody = await response.Content.ReadAsStreamAsync();
                    
                    // since the return values are in gzip, they must be decoded
                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        var destination = new MemoryStream();
                        using (var decompressor = (Stream)new GZipStream(responseBody, CompressionMode.Decompress, true))
                        {
                            decompressor.CopyToAsync(destination).Wait();
                        }

                        destination.Seek(0, SeekOrigin.Begin);
                        var requestContent = destination;
                        using (var sr = new StreamReader(requestContent))
                        {
                            returnData = await JsonSerializer.DeserializeAsync<List<SineData>>(requestContent);                  
                        }
                    }
                    else
                    {
                        // if count is changed to 1 then this statement will be reached because the response will not be in gzip format
                        Console.Write(response);
                    }

                    // step 5 - Write events to new stream with a deadfilter of -0.9 to 0.9 applied

                    // create new stream for data
                    SdsStream filteredSineWaveStream = new SdsStream
                    {
                        TypeId = sineWaveType.Id,
                        Id = "FilteredSineWave",
                        Name = "FilteredSineWave"
                    };

                    // send the http request to EDS to create stream
                    StringContent filteredStream = new StringContent(JsonSerializer.Serialize(filteredSineWaveStream));
                    HttpResponseMessage filteredResponseStream =
                        await httpClient.PostAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{filteredSineWaveStream.Id}", filteredStream);

                    // filters the data to only include values outside the range -0.9 to 0.9 
                    List<SineData> filteredWave = new List<SineData>();
                    int numberOfValidValues = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        // change this conditional to apply the type of filter you desire
                        if(returnData[i].Value > .9 || returnData[i].Value < -.9)
                        {
                            filteredWave.Add(returnData[i]);
                            numberOfValidValues++;
                        }
                    }

                    // send the filtered data to a new stream in EDS called FilteredSineWave
                    StringContent filteredData = new StringContent(JsonSerializer.Serialize(filteredWave));
                    HttpResponseMessage responseFilteredDataEgress =
                        await httpClient.PostAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{filteredSineWaveStream.Id}/Data", filteredData);

                    // step 6 - Delete Streams and Types

                    // delete SineWave Stream
                    HttpResponseMessage responseDeleteWaveStream =
                        await httpClient.DeleteAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{sineWaveStream.Id}");

                    // delete FilteredSineWave Stream
                    HttpResponseMessage responseDeleteFilteredWaveStream =
                        await httpClient.DeleteAsync($"http://localhost:5590/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Streams/{filteredSineWaveStream.Id}");

                    // delete SineWave Type
                    HttpResponseMessage responseDeleteType =
                        await httpClient.DeleteAsync($"http://localhost:{port}/api/{apiVersion}/Tenants/{tenantId}/Namespaces/{namespaceId}/Types/{sineWaveType.Id}");

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
