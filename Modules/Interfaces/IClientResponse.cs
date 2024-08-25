using KidsMealApi.Modules.Enums;

namespace KidsMealApi.Modules.Interfaces
{
    public interface IClientResponse
    {
         public ClientResponseErrorCodes? ErrorCode { get; set; } 
         
    }
}