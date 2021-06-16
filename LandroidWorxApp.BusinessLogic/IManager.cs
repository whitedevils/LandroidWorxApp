using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LandroidWorxApp.BusinessLogic
{
    public interface IManager
    {
        Task ExecuteCallback(string url);
        GetTimePlanningsResponse GetTimePlannings(GetTimePlanningsRequest request);
        Task ResetTimeCommand(ResetTimePlanCommandRequest command);
        SaveTimePlanningsResponse SaveTimePlanningsRequest(SaveTimePlanningsRequest request);
        Task SetTimeCommand(SendTimePlanCommandRequest time);
    }
}
