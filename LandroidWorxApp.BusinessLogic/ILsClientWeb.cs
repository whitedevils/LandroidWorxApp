using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LandroidWorxApp.BusinessLogic
{
    public interface ILsClientWeb
    {
        Task<LsClientWeb_LoginResponse> Login(LsClientWeb_LoginRequest request);
        Task<LsClientWeb_GetProductsResponse> GetProducts(LsClientWeb_GetProductsRequest request);
        Task<LsClientWeb_GetProductStatusResponse> GetProductStatus(LsClientWeb_GetProductStatusRequest request);
        Task<LsClientWeb_PublishCommandResponse> PublishCommand(LsClientWeb_PublishCommandRequest request);
        Task<LsClientWeb_GetProductActivitiesResponse> GetProductActivity(LsClientWeb_GetProductActivitiesRequest request);
    }
}
