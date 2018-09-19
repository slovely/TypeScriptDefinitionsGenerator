using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypeLite;
using TypeLite.TsModels;
using TypeScriptDefinitionsGenerator.Common;
using TypeScriptDefinitionsGenerator.Core.Extensions;
using TypeScriptDefinitionsGenerator.Core.SignalR;

namespace TypeScriptDefinitionsGenerator.Core
{
    class Program
    {
        private const string workingPath = "working";
        private static WebApiUrlGenerator _urlGenerator = new WebApiUrlGenerator();
        
        static void Main(string[] args)
        {
            Console.WriteLine("==============" + AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("TypeScriptGenerator.Core Stated: " + DateTime.Now.ToString("HH:mm:ss"));
            Console.WriteLine("CommandLine: " + string.Join(" ", args));

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Assemblies: ");
                foreach (var a in options.Assemblies) Console.WriteLine(" - " + a);
                Console.WriteLine("OutputPath: " + options.OutputFilePath);
                
                if (options.AttachDebugger)
                {
                    Debugger.Launch();
                    Debugger.Break();
                }

                var configuration = new GenerationConfiguration();
                configuration.ControllerPredicate = t => typeof(ControllerBase).IsAssignableFrom(t);
                configuration.ActionsPredicate = m => m.IsPublic && !typeof(IActionResult).IsAssignableFrom(m.ReturnType)
                                                                 && !typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType)
                                                                 && !typeof(Task<ActionResult>).IsAssignableFrom(m.ReturnType)
                                                                 && !typeof(HttpResponseMessage).IsAssignableFrom(m.ReturnType)
                                                                 && !typeof(Task<HttpResponseMessage>).IsAssignableFrom(m.ReturnType);
                configuration.SignalRGenerator = new SignalRGenerator();
                configuration.GetActionParameters = GetActionParameters;
                configuration.UrlGenerator = new WebApiUrlGenerator();
                var mainGenerator = new MainGenerator(options, configuration);
                mainGenerator.SetupWorkingFolder();
                try
                {
                    // TODO: Inspect the <assembly>.runtimeconfig.dev.json file to find places where packages can be loaded (see DependencyContext API?)
                    mainGenerator.GenerateTypeScriptContracts();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine("Reflection errors:");
                    foreach (var x in ex.LoaderExceptions)
                    {
                        Console.WriteLine(x.Message);
                    }
                    Console.WriteLine("***You might be able to fix this by adding: <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies> to your csproj file, until I figure out how to " +
                                      "use the deps.json / runtimeconfig.json files to load referenced assemblies automatically.");
                    throw;
                }
                mainGenerator.GenerateSignalrHubs();
                mainGenerator.GenerateServiceCallProxies();
            }
            else
            {
                Console.WriteLine("TypeScriptGenerator: Could not parse args: " + string.Join(" ", args));
            }
            Console.WriteLine("TypeScriptGenerator Finished: " + DateTime.Now.ToString("HH:mm:ss"));

        }

        private static List<ActionParameterInfo> GetActionParameters(MethodInfo action)
        {
            var result = new List<ActionParameterInfo>();
            var parameters = action.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                var param = new ActionParameterInfo();
                param.Name = parameterInfo.Name;
                param.Type = TypeConverter.GetTypeScriptName(parameterInfo.ParameterType);

                var bind = parameterInfo.GetCustomAttributes<BindAttribute>().FirstOrDefault();
                if (bind != null)
                {
                    param.Name = bind.Prefix ?? param.Name;
                }
                var fromBody = parameterInfo.GetCustomAttributes<FromBodyAttribute>().FirstOrDefault();
                // Parameters are from the URL unless specified by a [FromBody] attribute.
                param.FromUri = fromBody == null;

                //TODO: Support route parameters that are not 'id', might be hard as will need to parse routing setup
                if (parameterInfo.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    param.RouteProperty = true;
                }
                param.Name = param.Name.ToCamelCase();
                result.Add(param);
            }

            return result;
        }
    }
}