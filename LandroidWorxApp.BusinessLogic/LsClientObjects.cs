using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LandroidWorxApp.BusinessLogic
{
    #region Structs
    /*
    User auth {"id":000,"name":"...","email":"...","created_at":"...","updated_at":"...","city":null,"address":null,"zipcode":null,
                "country_id":276,"phone":null,"birth_date":null,"sex":null,"newsletter_subscription":null,"user_type":"customer",
                "api_token":"...","token_expiration":"...", "mqtt_client_id":"android-..."}
    */
    [DataContract]
    public struct WorxUser
    {
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "email")]
        public string Email;
        [DataMember(Name = "api_token")]
        public string ApiToken;
        [DataMember(Name = "mqtt_client_id")]
        public string MqttClientId;
        [DataMember(Name = "mqtt_endpoint")]
        public string MqttEndpoint;
    }
    [DataContract]
    public struct LsOAuth
    {
        [DataMember(Name = "access_token")]
        public string Token;
        [DataMember(Name = "expires_in")]
        public int Expires;
        [DataMember(Name = "token_type")]
        public string Type;
    }
    [DataContract]
    public struct LsUserMe
    {
        [DataMember(Name = "mqtt_endpoint")] public string Endpoint;
    }
    [DataContract]
    public struct LsCertificate
    {
        [DataMember(Name = "pkcs12")]
        public string Pkcs12;
    }
    [DataContract]
    public struct LsMqttTopic
    {
        [DataMember(Name = "command_in")]
        public string CmdIn;
        [DataMember(Name = "command_out")]
        public string CmdOut;
    }
    [DataContract]
    public struct LsProductItem
    {
        [DataMember(Name = "serial_number")]
        public string SerialNo;
        [DataMember(Name = "mac_address")]
        public string MacAdr;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "firmware_auto_upgrade")]
        public bool AutoUpgd;
        [DataMember(Name = "mqtt_topics")]
        public LsMqttTopic Topic;
        [DataMember(Name = "pin_code")]
        public string Pin;
    }
    [DataContract]
    public struct LsMqtt
    {
        [DataMember(Name = "cfg")]
        public Config Cfg;
        [DataMember(Name = "dat")]
        public Data Dat;
    }

    [DataContract]
    public struct LsJson
    {
        [DataMember(Name = "email")] public string Email;
        [DataMember(Name = "pass")] public string Password;
        [DataMember(Name = "uuid")] public string Uuid;
        [DataMember(Name = "name")] public string Name;
        [DataMember(Name = "broker")] public string Broker;
        [DataMember(Name = "mac")] public string MacAdr;
        [DataMember(Name = "board")] public string Board;
        [DataMember(Name = "blade")] public int Blade;
        [DataMember(Name = "top")] public bool Top;
        [DataMember(Name = "x")] public int X;
        [DataMember(Name = "y")] public int Y;
        [DataMember(Name = "w")] public int W;
        [DataMember(Name = "h")] public int H;
        [DataMember(Name = "plugins")] public List<string> Plugins;

        public bool Equals(LsJson lsj)
        {
            bool b;

            b = Email == lsj.Email && Password == lsj.Password && Uuid == lsj.Uuid && Name == lsj.Name && Broker == lsj.Broker && MacAdr == lsj.MacAdr;
            b = b && Top == lsj.Top && X == lsj.X && Y == lsj.Y && W == lsj.W && H == lsj.H && Blade == lsj.Blade;
            if (b && Plugins != null && lsj.Plugins != null)
            {
                b = Plugins.Count == lsj.Plugins.Count;
                for (int i = 0; b && i < Plugins.Count; i++) b = b && Plugins[i] == lsj.Plugins[i];
            }
            return b;
        }
    }

    [DataContract]
    public struct LsEstimatedTime
    {
        [DataMember(Name = "beg")] public float Beg;
        [DataMember(Name = "end")] public float End;
        [DataMember(Name = "vpm")] public float VoltPerMin;
    }

    [DataContract]
    public struct LsEstimatedTimes
    {
        [DataMember(Name = "home_0")] public LsEstimatedTime HomeOff;
        [DataMember(Name = "home_1")] public LsEstimatedTime HomeOn;
        [DataMember(Name = "mowing")] public LsEstimatedTime Mowing;
    }

    #region Activity-Log
    /*
    {
      "_id":"5d65fcd8241fa136e0551d1f",
      "timestamp":"2019-08-28 04:02:31",
      "product_item_id":12061,
      "payload":{
        "cfg":{"dt":"28/08/2019","tm":"06:02:23","mzv":[0,0,0,0,0,0,0,0,0,0],"mz":[0,0,0,0]},
        "dat":{"le":0,"ls":0,"fw":3.51,"lz":0,"lk":0,"bt":{"c":0,"m":1}}
      }
    }
    */
    [DataContract]
    public struct ActivityConfig
    {
        [DataMember(Name = "dt")] public string Date;
        [DataMember(Name = "tm")] public string Time;
        [DataMember(Name = "mz")] public int[] MultiZones; // [0-3] start point in meters
        [DataMember(Name = "mzv")] public int[] MultiZonePercs; // [0-9] ring list of start indizes
    }
    [DataContract]
    public struct ActivityBattery
    {
        [DataMember(Name = "c")] public ChargeCoge Charging;
        [DataMember(Name = "m")] public int Miss;
    }
    [DataContract]
    public struct ActivityData
    {
        [DataMember(Name = "le")] public ErrorCode LastError;
        [DataMember(Name = "ls")] public StatusCode LastState;
        [DataMember(Name = "fw")] public double Firmware;
        [DataMember(Name = "lz")] public int LastZone;
        [DataMember(Name = "lk")] public int Lock;
        [DataMember(Name = "bt")] public ActivityBattery Battery;
    }
    [DataContract]
    public struct ActivityPayload
    {
        [DataMember(Name = "cfg")] public ActivityConfig Cfg;
        [DataMember(Name = "dat")] public ActivityData Dat;
    }
    [DataContract]
    public struct Activity
    {
        [DataMember(Name = "_id")] public string ActId;
        [DataMember(Name = "timestamp")] public string Stamp;
        [DataMember(Name = "product_item_id")] public string MowId;
        [DataMember(Name = "payload")] public ActivityPayload Payload;
    }
    #endregion
    #endregion
}
