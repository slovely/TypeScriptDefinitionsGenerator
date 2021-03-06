﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeLite;
using TypeScriptDefinitionsGenerator.Common;
using TypeScriptDefinitionsGenerator.Common.SignalR;
using TypeScriptDefinitionsGenerator.Core.Extensions;

namespace TypeScriptDefinitionsGenerator.Core.SignalR
{
    public class SignalRGenerator : BaseSignalRGenerator
    {
        private readonly Options _options;

        public SignalRGenerator(Options options)
        {
            _options = options;
        }

        private const string HUB_TYPE = "Microsoft.AspNetCore.SignalR.Hub";
        public override bool IsHub(Type t)
        {
            return GetAllBaseTypes(t).Select(x => x.FullName).Contains("Microsoft.AspNetCore.SignalR.Hub");
        }

        public override string GenerateHubs(Assembly assembly, bool generateAsModules)
        {
            var hubs = assembly.GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.FullName != null && t.BaseType.FullName.Contains(HUB_TYPE))
                .OrderBy(t => t.FullName)
                .ToList();

            if (!hubs.Any())
            {
                Console.WriteLine("No SignalR hubs found");
                return "";
            }
            Console.WriteLine(hubs.Count + " SignalR hubs found");
            var requiredClassImports = new HashSet<string>();
            foreach (var hub in hubs)
            {
                requiredClassImports.Add(hub.FullName.GetTopLevelNamespaces()[0]);
            }

            var scriptBuilder = new ScriptBuilder("    ");

            scriptBuilder.AppendLine();

            hubs.ForEach(h => GenerateHubInterfaces(h, scriptBuilder, generateAsModules, requiredClassImports));

            // Generate client connection interfaces
            if (generateAsModules) scriptBuilder.Append("export ");
            scriptBuilder.AppendLineIndented("interface SignalR {");
            using (scriptBuilder.IncreaseIndentation())
            {
                hubs.ForEach(h => scriptBuilder.AppendLineIndented(h.Name.ToCamelCase() + ": I" + h.Name + "Proxy;"));
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            var output = scriptBuilder.ToString();
            // Output imports if required
            // TODO: this isn't going to work if SignalR hubs are in more than one assembly.
            if (generateAsModules)
            {
                var imports =
                    string.IsNullOrWhiteSpace(_options.WrapClassesInModule)
                        ? "import * as Classes from \"./classes\";\r\n"
                        : "import {" + _options.WrapClassesInModule + " as Classes} from \"./classes\";\r\n";
                imports +=
                    string.IsNullOrWhiteSpace(_options.WrapEnumsInModule)
                        ? "import * as __Enums from \"./enums\";\r\n"
                        : "import {" + _options.WrapEnumsInModule + "  as __Enums} from \"./enums\";\r\n";
                foreach (var ns in requiredClassImports)
                {
                    imports += string.Format("import {0} = Classes.{0};\r\n", ns);
                }
                imports += "\r\n";
                output = imports + output;
            }

            return output;
        }

        private List<Type> GetAllBaseTypes(Type type)
        {
            var result = new List<Type>();
            while (type != null)
            {
                result.Add(type);
                type = type.BaseType;
            }
            return result;
        }

        private void GenerateHubInterfaces(Type hubType, ScriptBuilder scriptBuilder, bool generateAsModules, HashSet<string> requiredImports)
        {
            if (!hubType.BaseType.FullName.Contains(HUB_TYPE)) throw new ArgumentException("The supplied type does not appear to be a SignalR hub.", "hubType");

            // Build the client interface
            if (generateAsModules) scriptBuilder.AppendIndented("export ");
            scriptBuilder.AppendLineIndented(string.Format("interface I{0}Client {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                if (!hubType.BaseType.IsGenericType)
                {
                    scriptBuilder.AppendLineIndented("/* Client interface not generated as hub doesn't derive from Hub<T> */");
                }
                else
                {
                    GenerateMethods(scriptBuilder, requiredImports, hubType.BaseType.GetGenericArguments().First());
                }
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            // Build the interface containing the SERVER methods
            if (generateAsModules) scriptBuilder.AppendIndented("export ");
            scriptBuilder.AppendLineIndented(string.Format("interface I{0} {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                GenerateMethods(scriptBuilder, requiredImports, hubType);
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();

            // Build the proxy class (represents the proxy generated by signalR).
            if (generateAsModules) scriptBuilder.AppendIndented("export ");
            scriptBuilder.AppendLineIndented(string.Format("interface I{0}Proxy {{", hubType.Name));
            using (scriptBuilder.IncreaseIndentation())
            {
                scriptBuilder.AppendLineIndented("server: I" + hubType.Name + ";");
                scriptBuilder.AppendLineIndented("client: I" + hubType.Name + "Client;");
            }
            scriptBuilder.AppendLineIndented("}");
            scriptBuilder.AppendLine();
        }

        private void GenerateMethods(ScriptBuilder scriptBuilder, HashSet<string> requiredImports, Type type)
        {
            type.GetMethods()
                .Where(mi => mi.GetBaseDefinition().DeclaringType.Name == type.Name)
                .OrderBy(mi => mi.Name)
                .ToList()
                .ForEach(m => scriptBuilder.AppendLineIndented(GenerateMethodDeclaration(requiredImports, m)));
        }

        private string GenerateMethodDeclaration(HashSet<string> requiredImports, MethodInfo methodInfo)
        {
            var result = methodInfo.Name.ToCamelCase() + "(";
            result += string.Join(", ", methodInfo.GetParameters().Select(param =>
            {
                var isEnum = param.ParameterType.IsEnum || (param.ParameterType.IsNullable() && param.ParameterType.GetEnumUnderlyingType().IsEnum);
                var typeScriptName = TypeConverter.GetTypeScriptName(param.ParameterType);
                if (!isEnum)
                {
                    foreach (var ns in typeScriptName.GetTopLevelNamespaces())
                    {
                        requiredImports.Add(ns);
                    }
                }
                return param.Name + ": " + (isEnum ? "__Enums." : "") + typeScriptName;
            }));

            {
                var returnTypeName = TypeConverter.GetTypeScriptName(methodInfo.ReturnType);
                var isEnum = methodInfo.ReturnType.IsEnum || (methodInfo.ReturnType.IsNullable() && methodInfo.ReturnType.GetEnumUnderlyingType().IsEnum);
                returnTypeName = returnTypeName == "void" ? "void" : "Promise<" + (isEnum ? "__Enums." : "") + returnTypeName + ">";
                if (!isEnum)
                {
                    foreach (var ns in returnTypeName.GetTopLevelNamespaces())
                    {
                        requiredImports.Add(ns);
                    }
                }

                result += "): " + returnTypeName + ";";
            }
            return result;
        }
    }
}