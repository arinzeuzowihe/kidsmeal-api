namespace KidsMealApi.Modules.Interfaces
{
    /// <summary>
    /// Used to define all modules and allows for easy 
    /// discovery of new modules during the automatic module registration on startup
    /// </summary>
    public interface IModule
    {
         IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
         IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
    }
}