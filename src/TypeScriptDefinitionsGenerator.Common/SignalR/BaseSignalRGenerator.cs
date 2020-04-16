using System;
using System.Reflection;

namespace TypeScriptDefinitionsGenerator.Common.SignalR
{

    public abstract class BaseSignalRGenerator
    {
        public abstract string GenerateHubs(Assembly assembly, bool generateAsModules);
        public abstract bool IsHub(Type t);
    }
}