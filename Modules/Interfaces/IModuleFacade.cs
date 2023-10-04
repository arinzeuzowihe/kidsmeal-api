namespace KidsMealApi.Modules.Interfaces
{
    /// <summary>
    /// A facade to abstract all the service layer dependencies for a given module
    /// and it's endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IModuleFacade <T>
    {
        public ILogger<T>? ModuleLogger { get; set; }
    }
}