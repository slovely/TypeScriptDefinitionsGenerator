using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using CommandLine;
using TypeLite;
using TypeScriptDefinitionsGenerator.Common;
using TypeScriptDefinitionsGenerator.Extensions;
using TypeScriptDefinitionsGenerator.SignalR;

namespace TypeScriptDefinitionsGenerator
{
    internal class Program
    {
        private const string workingPath = "working";
        private static WebApiUrlGenerator _urlGenerator = new WebApiUrlGenerator();
        
        private static void Main(string[] args)
        {
            Console.WriteLine("==============" + AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("TypeScriptGenerator DOTNET4.5 Stated: " + DateTime.Now.ToString("HH:mm:ss"));
            Console.WriteLine("CommandLine: " + string.Join(" ", args));

            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
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
                configuration.ControllerPredicate = t => typeof(ApiController).IsAssignableFrom(t);
                configuration.ActionsPredicate = m => m.IsPublic;
                configuration.SignalRGenerator = new SignalRGenerator();
                configuration.GetActionParameters = GetActionParameters;
                configuration.UrlGenerator = new WebApiUrlGenerator();
                var mainGenerator = new MainGenerator(options, configuration);
                mainGenerator.SetupWorkingFolder();
                mainGenerator.GenerateTypeScriptContracts();
                mainGenerator.GenerateSignalrHubs();
                if (options.GenerateWebApiActions)
                {
                    switch (options.ActionsStyle)
                    {
                        case ActionsStyle.Default:
                            mainGenerator.GenerateWebApiActions();
                            break;
                        case ActionsStyle.Aurelia:
                            mainGenerator.GenerateAureliWebApiActions();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
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

                var fromUri = parameterInfo.GetCustomAttributes<FromUriAttribute>().FirstOrDefault();
                if (fromUri != null)
                {
                    param.Name = fromUri.Name ?? param.Name;
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
