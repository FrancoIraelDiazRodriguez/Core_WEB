namespace Core_Web.Utils
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class RequiresPermissionAttribute(string module, string action) : Attribute
    {

        public string Module { get; } = module;
        public string Action { get; } = action;
    }
}

