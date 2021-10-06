using Hangfire;
using LandroidWorxApp.DataLayer;
using LandroidWorxApp.DataLayer.POCO;
using Mapster;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace LandroidWorxApp.BusinessLogic
{
    public class Manager : IManager
    {
        private readonly IConfiguration _configuration;
        private readonly RepoManager _repoManager;
        private readonly ILsClientWeb _lsClientWeb;
        private readonly TelemetryClient _telemetryClient;

        public Manager(IConfiguration configuration, string connectionStringName)
        {
            _configuration = configuration;
            _repoManager = new RepoManager(connectionStringName, configuration);
            _lsClientWeb = new LsClientWeb(configuration, connectionStringName);
            _telemetryClient = new TelemetryClient();
        }

        public void DeleteUserData(string username)
        {
            _repoManager.GenericOperations.DeleteByExpression<UserProduct>(x => x.Username == username);
            _repoManager.GenericOperations.DeleteByExpression<UserData>(x => x.Username == username);
            var plannings = _repoManager.GenericOperations.GetByExpression<TimePlanning>(x => x.Username == username);
            foreach (var item in plannings)
            {
                RecurringJob.RemoveIfExists(item.Id.ToString());
                RecurringJob.RemoveIfExists(string.Format("cbkStart_{0}", item.Id));
                RecurringJob.RemoveIfExists(string.Format("cbkEnd_{0}", item.Id));
            }
            _repoManager.GenericOperations.DeleteAll(plannings);
        }


        public GetTimePlanningsResponse GetTimePlannings(GetTimePlanningsRequest request)
        {
            var plannings = _repoManager.GenericOperations.GetByExpression<TimePlanning>(t => t.Username == request.Username && t.RobotSerialNumber == request.SerialNumber );
            return new GetTimePlanningsResponse() { Plannings = plannings.ConvertAll(c => c.Adapt<TimePlanning_BL>()) };
        }

        public SaveTimePlanningsResponse SaveTimePlanningsRequest(SaveTimePlanningsRequest request)
        {
            // Get old time plannings of user and remove all with recurring jobs 
            var oldPlannings = _repoManager.GenericOperations.GetByExpression<TimePlanning>(p => p.Username == request.Username && p.RobotSerialNumber == request.SerialNumber);
            oldPlannings.ForEach(p => { 
                RecurringJob.RemoveIfExists(p.Id.ToString());
                RecurringJob.RemoveIfExists(string.Format("cbkStart_{0}", p.Id));
                RecurringJob.RemoveIfExists(string.Format("cbkEnd_{0}", p.Id));
            });
            _repoManager.GenericOperations.DeleteAll(oldPlannings);

            var newPlannings = _repoManager.GenericOperations.SaveAll(request.Plannings.ConvertAll(c => c.Adapt<TimePlanning>()));

            if (newPlannings.Count() == 0)
                ResetTimeCommand(new ResetTimePlanCommandRequest()
                {
                    Username = request.Username,
                    WorkPercentage = request.WorkPercentage,
                    SerialNumber = request.SerialNumber,
                });

            newPlannings.ForEach(p =>
            {
                var timestart = p.TimeStart.Subtract(TimeSpan.FromMinutes(2));
                RecurringJob.AddOrUpdate<IManager>(p.Id.ToString(), (m) => m.SetTimeCommand(new SendTimePlanCommandRequest()
                {
                    Username = request.Username,
                    WorkPercentage = request.WorkPercentage,
                    SerialNumber = request.SerialNumber,
                    Planning = p.Adapt<TimePlanning_BL>()
                }), Cron.Weekly(p.DayOfWeek, timestart.Hours, timestart.Minutes), TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

                if (!string.IsNullOrEmpty(p.CallbackStart))
                {
                    var cbkStartTime = p.TimeStart.Add(TimeSpan.FromSeconds(p.CallbackStartDelaySeconds ?? 0));
                    RecurringJob.AddOrUpdate<IManager>(string.Format("cbkStart_{0}", p.Id), (m) => m.ExecuteCallback(p.CallbackStart),
                        Cron.Weekly(p.DayOfWeek, cbkStartTime.Hours, cbkStartTime.Minutes), TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
                }

                if (!string.IsNullOrEmpty(p.CallbackEnd))
                {
                    var cbkEndTime = p.TimeStart.Add(TimeSpan.FromMinutes(p.Duration)).Add(TimeSpan.FromSeconds(p.CallbackEndDelaySeconds ?? 0));
                    RecurringJob.AddOrUpdate<IManager>(string.Format("cbkEnd_{0}", p.Id), (m) => m.ExecuteCallback(p.CallbackEnd),
                        Cron.Weekly(p.DayOfWeek, cbkEndTime.Hours, cbkEndTime.Minutes), TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
                }

            });
            return new SaveTimePlanningsResponse()
            {
                PlanningsUpdated = newPlannings.ConvertAll(c => c.Adapt<TimePlanning_BL>())
            };
        }

        public async Task ResetTimeCommand(ResetTimePlanCommandRequest command)
        {
            UserData user = _repoManager.GenericOperations.GetSingleByExpression<UserData>(u => u.Username == command.Username);
            UserProduct product = _repoManager.GenericOperations.GetSingleByExpression<UserProduct>(p => p.SerialNumber == command.SerialNumber);

            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(user.X509Certificate2), (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var planCommand = string.Format("{{\"sc\":{{\"d\":[[{0}],[{1}],[{2}],[{3}],[{4}],[{5}],[{6}]],\"m\":1,\"p\":{7}}}}}",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                 "\"00:00\",0,0",
                command.WorkPercentage
                );

            _telemetryClient.TrackTrace("ResetplanCommand", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);


            await _lsClientWeb.PublishCommand(new LsClientWeb_PublishCommandRequest()
            {
                CertWX = certificate,
                Content = planCommand,
                CmdInPath = product.CmdInPath,
                CmdOutPath = product.CmdOutPath,
                Broker = user.Broker,
                Uuid = Guid.NewGuid().ToString(),
                Handler = (object sender, MqttMsgPublishedEventArgs e) =>
                {
                    if (!e.IsPublished)
                        _telemetryClient.TrackTrace("not published", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning, new Dictionary<string, string>() { { "planCommand", planCommand } });

                    ((MqttClient)sender).Disconnect();
                }
            });
           
        }

        public async Task SetTimeCommand(SendTimePlanCommandRequest command)
        {
            UserData user = _repoManager.GenericOperations.GetSingleByExpression<UserData>(u => u.Username == command.Username);
            UserProduct product = _repoManager.GenericOperations.GetSingleByExpression<UserProduct>(p => p.SerialNumber == command.SerialNumber);

            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(user.X509Certificate2), (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var zoneCommand = "{\"mzv\":[{0},{0},{0},{0},{0},{0},{0},{0},{0},{0}]}".Replace("{0}", command.Planning.Zone.ToString());
            var planCommand = string.Format("{{\"sc\":{{\"d\":[[{0}],[{1}],[{2}],[{3}],[{4}],[{5}],[{6}]],\"m\":1,\"p\":{7}}}}}",
                command.Planning.DayOfWeek == DayOfWeek.Sunday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Monday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Tuesday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Wednesday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Thursday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Friday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.Planning.DayOfWeek == DayOfWeek.Saturday ? string.Format("\"{0}:{1}\",{2},{3}", command.Planning.TimeStart.Hours, command.Planning.TimeStart.Minutes, command.Planning.Duration, command.Planning.CutEdge ? 1 : 0) : "\"00:00\",0,0",
                command.WorkPercentage
                );

            _telemetryClient.TrackTrace("ZoneCommand", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, new Dictionary<string, string>() { { "IdPlanning", command.Planning.Id.ToString() }, { "ZoneCommand", zoneCommand } });
            _telemetryClient.TrackTrace("planCommand", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, new Dictionary<string, string>() { { "IdPlanning", command.Planning.Id.ToString() }, { "PlanCommand", planCommand } });


            await _lsClientWeb.PublishCommand(new LsClientWeb_PublishCommandRequest()
            {
                CertWX = certificate,
                Content = planCommand,
                CmdInPath = product.CmdInPath,
                CmdOutPath = product.CmdOutPath,
                Broker = user.Broker,
                Uuid = Guid.NewGuid().ToString(),
                Handler = (object sender, MqttMsgPublishedEventArgs e) =>
                {
                    if (!e.IsPublished)
                        _telemetryClient.TrackTrace("not published", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning, new Dictionary<string, string>() { { "IdPlanning", command.Planning.Id.ToString() }, { "planCommand", planCommand } });

                    ((MqttClient)sender).Disconnect();
                }
            });

            await Task.Delay(10000);

            await _lsClientWeb.PublishCommand(new LsClientWeb_PublishCommandRequest()
            {
                CertWX = certificate,
                Content = zoneCommand,
                CmdInPath = product.CmdInPath,
                CmdOutPath = product.CmdOutPath,
                Broker = user.Broker,
                Uuid = Guid.NewGuid().ToString(),
                Handler = (object sender, MqttMsgPublishedEventArgs e) =>
                {
                    if (!e.IsPublished)
                        _telemetryClient.TrackTrace("not published", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning, new Dictionary<string, string>() { { "IdPlanning", command.Planning.Id.ToString() }, { "zoneCommand", zoneCommand } });

                    ((MqttClient)sender).Disconnect();
                }
            });
        }

        public async Task ExecuteCallback(string url)
        {
            HttpClient client = new HttpClient();

            var urlPart = url.Substring(url.IndexOf("://") + 3, url.Length - url.IndexOf("://") - 3);
            if (urlPart.IndexOf(":") > 0)
            {
                var username = urlPart.Split(':')[0];
                var password = urlPart.Split(':')[1].Split('@')[0];

                var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                url = url.Replace(string.Format("{0}:{1}@", username, password), "");
            }


            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return;
            else
                throw new Exception(response.ReasonPhrase);

        }
    }
}
