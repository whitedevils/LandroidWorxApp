using LandroidWorxApp.DataLayer.Operations;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandroidWorxApp.DataLayer
{
    public class RepoManager
    {
        public string ConnectionString { get; set; }
        public IConfiguration Configuration { get; set; }

        public RepoManager(string connectionString, IConfiguration configuration)
        {
            ConnectionString = connectionString;
            Configuration = configuration;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("nameConnectionString NULL or whitespace in " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private OperationBase _GenericOperation;
        public OperationBase GenericOperations
        {
            get
            {
                return _GenericOperation ?? (_GenericOperation = new OperationBase(this));
            }
        }       

    }
}
