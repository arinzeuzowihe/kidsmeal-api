using KidsMealApi.Modules.Interfaces;

namespace KidsMealApi.Modules.Extensions
{
    public static class ModuleExtensions
    {

        static readonly List<IModule> registeredModules = new List<IModule>();

        public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
        {
            var modules = DiscoveredModules();
            foreach(var module in modules)
            {
                module.RegisterModule(services, configuration);
                registeredModules.Add(module);
            }

            return services;
        }

        public static WebApplication MapEndpoints(this WebApplication app)
        {
            foreach(var module in registeredModules)
            {
                module.MapEndpoints(app);
            }

            return app;
        }

        public static IEnumerable<IModule> DiscoveredModules()
        {
            return typeof(IModule).Assembly
                .GetTypes()
                .Where(p => p.IsClass && p.IsAssignableTo(typeof(IModule)))
                .Select(Activator.CreateInstance)
                .Cast<IModule>();
        }
    }
}