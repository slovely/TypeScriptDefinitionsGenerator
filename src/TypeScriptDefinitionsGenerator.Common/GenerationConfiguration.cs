using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using TypeScriptDefinitionsGenerator.Common.SignalR;
using TypeScriptDefinitionsGenerator.Common.UrlGenerators;

namespace TypeScriptDefinitionsGenerator.Common
{
    public class GenerationConfiguration
    {
        public Func<Type, bool> ControllerPredicate { get; set; }
        public Func<MethodInfo, bool> ActionsPredicate { get; set; }
        public BaseSignalRGenerator SignalRGenerator { get; set; }
        public IUrlGenerator UrlGenerator { get; set; }
        public Func<MethodInfo, List<ActionParameterInfo>> GetActionParameters { get; set; }
    }
}