using System.Reflection;

namespace TypeScriptDefinitionsGenerator.Common.UrlGenerators
{
    public interface IUrlGenerator
    {
        string GetUrl(MethodInfo method);
   }
}