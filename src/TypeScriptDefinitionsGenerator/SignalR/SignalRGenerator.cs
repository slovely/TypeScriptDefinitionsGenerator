﻿using System;
using System.Linq;
using System.Reflection;
using TypeLite;
using TypeScriptDefinitionsGenerator.Common.SignalR;
using TypeScriptDefinitionsGenerator.Extensions;

namespace TypeScriptDefinitionsGenerator.SignalR
{
    public class SignalRGenerator : BaseSignalRGenerator
    {
        private const string HUB_TYPE = "Microsoft.AspNet.SignalR.Hub";
        private const string IHUB_TYPE = "Microsoft.AspNet.SignalR.Hubs.IHub";
        public override bool IsHub(Type t)
        {
            return t.GetInterfaces().ToList().Exists(i => i != null && i.FullName?.Contains(IHUB_TYPE) == true);
        }


        public override string GenerateHubs(Assembly assembly, bool generateAsModules)
        {
            var hubs = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.FullName != null && t.BaseType.FullName.Contains(HUB_TYPE))
                .OrderBy(t => t.FullName)
                .ToList();

            if (!hubs.Any()) return "";

            var scriptBuilder = new ScriptBuilder("    ");
            // Output signalR style promise interface:
            scriptBuilder.AppendLine("interface ISignalRPromise<T> {");
            using (scriptBuilder.IncreaseIndentation())
            {
                scriptBuilder.AppendLineIndented("done(cb: (result: T) => any): ISignalRPromise<T>;");
                scriptBuilder.AppendLineIndented("error(cb: (error: any) => any): ISignalRPromise<T>;");

            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            hubs.ForEach(h => GenerateHubInterfaces(h, scriptBuilder));

            // Generate client connection interfaces
            scriptBuilder.AppendLineIndented("interface SignalR {");
            using (scriptBuilder.IncreaseIndentation())
            {
                hubs.ForEach(h => scriptBuilder.AppendLineIndented(h.Name.ToCamelCase() + ": I" + h.Name + "Proxy;"));
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            return scriptBuilder.ToString();
        }

        private void GenerateHubInterfaces(Type hubType, ScriptBuilder scriptBuilder)
        {
            if (!hubType.BaseType.FullName.Contains(HUB_TYPE)) throw new ArgumentException("The supplied type does not appear to be a SignalR hub.", "hubType");

            // Build the client interface
            scriptBuilder.AppendLineIndented(string.Format("interface I{0}Client {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                if (!hubType.BaseType.IsGenericType)
                {
                    scriptBuilder.AppendLineIndented("/* Client interface not generated as hub doesn't derive from Hub<T> */");
                }
                else
                {
                    GenerateMethods(scriptBuilder, hubType.BaseType.GetGenericArguments().First());
                }
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            // Build the interface containing the SERVER methods
            scriptBuilder.AppendLineIndented(string.Format("interface I{0} {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                GenerateMethods(scriptBuilder, hubType);
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            // Build the proxy class (represents the proxy generated by signalR).
            scriptBuilder.AppendLineIndented(string.Format("interface I{0}Proxy {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                scriptBuilder.AppendLineIndented("server: I" + hubType.Name + ";");
                scriptBuilder.AppendLineIndented("client: I" + hubType.Name + "Client;");
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();
        }

        private void GenerateMethods(ScriptBuilder scriptBuilder, Type type)
        {
            type.GetMethods()
                .Where(mi => mi.GetBaseDefinition().DeclaringType.Name == type.Name)
                .OrderBy(mi => mi.Name)
                .ToList()
                .ForEach(m => scriptBuilder.AppendLineIndented(GenerateMethodDeclaration(m)));
        }

        private string GenerateMethodDeclaration(MethodInfo methodInfo)
        {
            var result = methodInfo.Name.ToCamelCase() + "(";
            result += string.Join(", ", methodInfo.GetParameters().Select(param => param.Name + ": " + TypeConverter.GetTypeScriptName(param.ParameterType)));
            
            var returnTypeName = TypeConverter.GetTypeScriptName(methodInfo.ReturnType);
            returnTypeName = returnTypeName == "void" ? "void" : "ISignalRPromise<" + returnTypeName + ">";

            result += "): " + returnTypeName + ";";
            return result;
        }
    }
}