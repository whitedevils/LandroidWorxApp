using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandroidWorxApp.DataLayer.POCO
{
    [SugarTable("UsersData")]
    public class UserData
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Username { get; set; }
        public string Broker { get; set; }
        public string X509Certificate2 { get; set; }
    }
}
