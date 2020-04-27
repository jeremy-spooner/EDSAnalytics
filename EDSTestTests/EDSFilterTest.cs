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
            Assert.True(EDSFilter.Program.MainAsync(true).Result);           
        }
    }
}
