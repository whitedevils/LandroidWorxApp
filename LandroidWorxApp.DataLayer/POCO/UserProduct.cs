using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandroidWorxApp.DataLayer.POCO
{
    [SugarTable("UserProducts")]
    public class UserProduct
    {
        public string Username { get; set; }
        [SugarColumn(IsPrimaryKey = true)]
        public string SerialNumber { get; set; }
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public string CmdInPath { get; set; }
        public string CmdOutPath { get; set; }
    }
}
