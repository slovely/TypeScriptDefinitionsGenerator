using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
                configuration.ControllerPredicate = t =>
                {
                    // Dynamically check types, as might not be using AspNetCore (might be ServiceStack for example).  Note that namespace moved in core v3
                    var controllerBaseType = Type.GetType("Microsoft.AspNetCore.Mvc.Core.ControllerBase, Microsoft.AspNetCore.Mvc.Core")
                        ?? Type.GetType("Microsoft.AspNetCore.Mvc.ControllerBase, Microsoft.AspNetCore.Mvc.Core");
                    if (controllerBaseType == null) return false;
                    return controllerBaseType.IsAssignableFrom(t);
                };
                configuration.ActionsPredicate = m =>
                {
                    var iActionResultType = Type.GetType("Microsoft.AspNetCore.Mvc.IActionResult, Microsoft.AspNetCore.Mvc.Abstractions");
                    if (iActionResultType == null) return false;
                    var genericTaskType = typeof(Task<>);
                    var iActionResultTypeTask = genericTaskType.MakeGenericType(Type.GetType("Microsoft.AspNetCore.Mvc.IActionResult, Microsoft.AspNetCore.Mvc.Abstractions"));
                    var actionResultTypeTask = genericTaskType.MakeGenericType(Type.GetType("Microsoft.AspNetCore.Mvc.ActionResult, Microsoft.AspNetCore.Mvc.Core"));
                    var httpResponseMessageType = Type.GetType("System.Net.Http.HttpResponseMessage, System.Net.Http");
                    var httpResponseMessageTypeTask = genericTaskType.MakeGenericType(httpResponseMessageType);
                    
                    return m.IsPublic && !iActionResultType.IsAssignableFrom(m.ReturnType)
                                      && !iActionResultTypeTask.IsAssignableFrom(m.ReturnType)
                                      && !actionResultTypeTask.IsAssignableFrom(m.ReturnType)
                                      && !httpResponseMessageType.IsAssignableFrom(m.ReturnType)
                                      && !httpResponseMessageTypeTask.IsAssignableFrom(m.ReturnType);
                };
                configuration.SignalRGenerator = new SignalRGenerator();
                configuration.GetActionParameters = WebApiUrlGenerator.GetActionParameters;
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
    }
}