using LandroidWorxApp.BusinessLogic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LandroidWorxApp.Tests
{
    public class CallbackTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
        }

        [Test]
        public void TestCallback()
        {
            IManager manager = new Manager(_configuration, "LandroidWorxAppData");
            manager.ExecuteCallback("http://roberto.gualandris@hotmail.it:*******@localhost:58267/api/Landroid/openSwitches?switchNames=Switch_1To2&switchNames=Switch_1To3");
        }
    }
}