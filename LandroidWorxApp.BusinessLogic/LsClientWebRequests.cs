using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static uPLibrary.Networking.M2Mqtt.MqttClient;

namespace LandroidWorxApp.BusinessLogic
{
    public class LsClientWeb_Base
    {
        public string BearerToken { get; set; }
    }

    public class LsClientWeb_LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GrantType { get; set; }
        public int ClientId { get { return 1; } }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
    public class LsClientWeb_LoginResponse
    {
        public string BearerToken { get; set; }
        public string BrokerUrl { get; set; }
        public X509Certificate2 CertWX { get; set; }
    }

    public class LsClientWeb_GetProductsRequest : LsClientWeb_Base
    {
        public string Username { get; set; }
    }
    public class LsClientWeb_GetProductsResponse
    {
        public List<LsProductItem> Products { get; set; }
    }

    public class LsClientWeb_GetProductStatusRequest : LsClientWeb_Base
    {
        public string SerianNumber { get; set; }
    }

    public class LsClientWeb_GetProductStatusResponse
    {
        public LsMqtt Status { get; set; }
    }

    public class LsClientWeb_GetProductActivitiesRequest : LsClientWeb_Base
    {
        public string SerianNumber { get; set; }
    }

    public class LsClientWeb_GetProductActivitiesResponse
    {
        public List<Activity> Activities { get; set; }
    }

    public class LsClientWeb_PublishCommandRequest : LsClientWeb_Base
    {
        public string Broker { get; set; }
        public string Uuid { get; set; }
        public string CmdInPath { get; set; }
        public string CmdOutPath { get; set; }
        public string Content { get; set; }
        public MqttMsgPublishedEventHandler Handler { get; set; }
        public X509Certificate2 CertWX { get; set; }
    }
    public class LsClientWeb_PublishCommandResponse
    {
        public int MessageId { get; set; }
    }
}
