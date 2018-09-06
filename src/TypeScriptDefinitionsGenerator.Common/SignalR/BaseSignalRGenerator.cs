using System.Reflection;

namespace TypeScriptDefinitionsGenerator.Common.SignalR
{

    public abstract class BaseSignalRGenerator
    {
        public abstract string IHUB_TYPE { get; }
        public abstract string GenerateHubs(Assembly assembly, bool generateAsModules);
    }
}