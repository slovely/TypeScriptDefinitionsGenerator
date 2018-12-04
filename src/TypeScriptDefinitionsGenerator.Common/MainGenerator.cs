using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TypeLite;
using TypeLite.TsModels;
using TypeScriptDefinitionsGenerator.Common.Extensions;
using TypeScriptDefinitionsGenerator.Common.UrlGenerators;

namespace TypeScriptDefinitionsGenerator.Common
{
    public class MainGenerator
    {
        private readonly Options _options;
        private readonly GenerationConfiguration _configuration;
        private readonly ServiceStackHelper _ssHelper = new ServiceStackHelper();

        public MainGenerator(Options options, GenerationConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        private const string workingPath = "working";
        
        public void SetupWorkingFolder()
        {
            // Create and empty working folder
            var workingDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, workingPath));
            workingDir.EnumerateFiles().ToList().ForEach(f => f.Delete());

            Directory.CreateDirectory(_options.OutputFilePath);
            foreach (var assembly in _options.Assemblies)
            {
                LoadReferencedAssemblies(assembly);
            }
        }

        private static void LoadReferencedAssemblies(string assembly)
        {
            var sourceAssemblyDirectory = Path.GetDirectoryName(assembly);

            foreach (var file in Directory.GetFiles(sourceAssemblyDirectory, "*.dll"))
            {
                try
                {
                    File.Copy(file, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, workingPath, new FileInfo(file).Name), true);
                }
                catch (IOException ex)
                {
                    if (!ex.Message.Contains("because it is being used by another process")) throw;
                }
            }
        }

        public void GenerateServiceCallProxies()
        {
            if (_options.GenerateWebApiActions)
            {
                switch (_options.ActionsStyle)
                {
                    case ActionsStyle.Default:
                        GenerateWebApiActions();
                        break;
                    case ActionsStyle.Aurelia:
                        GenerateAureliaWebApiActions();
                        break;
                    case ActionsStyle.Angular:
                        GenerateAngularActions();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (_options.GenerateServiceStackRequests)
            {
                switch (_options.ActionsStyle)
                {
                    case ActionsStyle.Default:
                        GenerateWebApiActions();
                        break;
                    case ActionsStyle.Aurelia:
                        GenerateAureliaWebApiActions();
                        break;
                    case ActionsStyle.Angular:
                        GenerateAngularActions();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        public void GenerateTypeScriptContracts()
        {
            var generator = new TypeScriptFluent()
                .WithConvertor<Guid>(c => "string");

            generator.WithMemberFormatter(i =>
            {
                var identifier = i.Name;
                if (_options.CamelCase)
                {
                    identifier = Char.ToLower(identifier[0]) + identifier.Substring(1);
                }
                if (_options.GenerateServiceStackRequests)
                {
                    if (!_ssHelper.IsPropertyRequired(i.MemberInfo))
                    {
                        identifier += "?";
                    }
                }
                return identifier;
            });

            foreach (var assemblyName in _options.Assemblies)
            {
                var fi = new FileInfo(assemblyName);
                // Load all input assemblies from the same location to ensure duplicates aren't generated (as the same type loaded from 
                // two different places will appear to be different, so both would otherwise be generated).
                var assembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, workingPath, fi.Name));
                Console.WriteLine("Loaded assembly: " + assemblyName);

                GenerateContractsForWebApiRequestResponseClasses(assembly, generator);

                if (_options.GenerateServiceStackRequests)
                {
                    GenerateContractsForServiceStackRequestResponseClasses(assembly, generator);
                }
                
                // Add all classes that are declared inside the specified namespace
                if (_options.Namespaces != null && _options.Namespaces.Any())
                {
                    var types = assembly.GetTypes()
                        .Where(t => IncludedNamespace(_options, t));
                    ProcessTypes(types, generator);
                }

                generator.AsConstEnums(false);
            }

            var tsEnumDefinitions = generator.Generate(TsGeneratorOutput.Enums);
            tsEnumDefinitions = tsEnumDefinitions.Replace("module ", "export module ");
            if (_options.UseStringEnums)
            {
                tsEnumDefinitions = Regex.Replace(tsEnumDefinitions, "\\b([a-zA-Z]*) = ([\\d]+)", "$1 = \"$1\"");
            }
            File.WriteAllText(Path.Combine(_options.OutputFilePath, "enums.ts"), tsEnumDefinitions);


            if (_options.GenerateAsModules)
            {
                //Generate interface definitions for all classes
                generator.WithMemberTypeFormatter((p, n) =>
                {
                    var asCollection = p.PropertyType as TsCollection;
                    var isCollection = asCollection != null;

                    if (isCollection)
                    {
                        var genericArguments = asCollection.ItemsType.Type.GetGenericArguments();
                        foreach (var arg in genericArguments)
                        {
                            // Really horrible hack... prefix enum generic parameters with 'Enum.'.  Makes things like Dictionary<string, AnEnum> work.
                            if (arg.IsEnum)
                            {
                                Console.WriteLine("***Replacing " + arg.FullName);
                                n = n.Replace(arg.FullName, "__Enums." + arg.FullName);
                            }
                        }

                        return (asCollection.ItemsType is TsEnum ? "__Enums." + n : n) + string.Concat(Enumerable.Repeat("[]", asCollection.Dimension));
                    }
                    return p.PropertyType is TsEnum ? "__Enums." + n : n;
                });
                var tsClassDefinitions = generator.Generate(TsGeneratorOutput.Properties | TsGeneratorOutput.Fields);
                tsClassDefinitions = "import * as __Enums from \"./enums\";\r\n\r\n" + tsClassDefinitions;
                tsClassDefinitions = tsClassDefinitions.Replace("declare module", "export module");
                tsClassDefinitions = tsClassDefinitions.Replace("interface", "export interface");
                tsClassDefinitions = Regex.Replace(tsClassDefinitions, @":\s*System\.Collections\.Generic\.KeyValuePair\<(?<k>[^\,]+),(?<v>[^\,]+)\>\[\];",
                    m => ": {[key: string]: " + m.Groups["v"].Value + "};",
                    RegexOptions.Multiline);
                File.WriteAllText(Path.Combine(_options.OutputFilePath, "classes.ts"), tsClassDefinitions);
            }
            else
            {
                var tsClassDefinitions = generator.Generate(TsGeneratorOutput.Properties | TsGeneratorOutput.Fields);
                tsClassDefinitions = Regex.Replace(tsClassDefinitions, @":\s*System\.Collections\.Generic\.KeyValuePair\<(?<k>[^\,]+),(?<v>[^\,]+)\>\[\];",
                    m => ": {[key: string]: " + m.Groups["v"].Value + "};",
                    RegexOptions.Multiline);
                File.WriteAllText(Path.Combine(_options.OutputFilePath, "classes.d.ts"), tsClassDefinitions);
            }
        }

        private void GenerateContractsForServiceStackRequestResponseClasses(Assembly assembly, TypeScriptFluent generator)
        {
            var requests = GetServiceStackRequestTypes(assembly);

            // Any requests that specify their return type, also generate them...
            var responseTypes = new List<Type>();
            requests.ForEach(req =>
            {
                var type = _ssHelper.GetResponseTypeForRequest(req);
                if (type != null)
                {
                    responseTypes.Add(type);
                }
            });
            
            ProcessTypes(requests, generator);
            ProcessTypes(responseTypes, generator);
        }

        private List<Type> GetServiceStackRequestTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => type.GetCustomAttributes().Any(attr => attr.GetType().FullName == "ServiceStack.RouteAttribute"))
                // ONLY INCLUDE TYPES WITH AN IRETURN<T> OR IRETURNVOID FOR NOW!
                .Where(type => _ssHelper.GetResponseTypeForRequest(type) != null || _ssHelper.ReturnsVoid(type))
                .OrderBy(t => t.Name)
                .ToList();
        }

        private void GenerateContractsForWebApiRequestResponseClasses(Assembly assembly, TypeScriptFluent generator)
        {
            // Get the WebAPI controllers...
            var controllers = assembly.GetTypes().Where(_configuration.ControllerPredicate);

            // Get the return types...
            var actions = controllers
                .SelectMany(c => c.GetMethods()
                    .Where(_configuration.ActionsPredicate)
                    .Where(m => m.DeclaringType == c)
                );
            ProcessMethods(actions, generator);

            var signalrHubs = assembly.GetTypes().Where(t => t.GetInterfaces().ToList().Exists(i => i != null && i.FullName?.Contains(_configuration.SignalRGenerator.IHUB_TYPE) == true));
            var methods = signalrHubs
                .SelectMany(h => h.GetMethods()
                    .Where(m => m.IsPublic)
                    .Where(m => m.DeclaringType == h || m.GetBaseDefinition()?.DeclaringType == h));
            ProcessMethods(methods, generator);

            var clientInterfaceTypes = signalrHubs.Where(t => t.BaseType.IsGenericType)
                .Select(t => t.BaseType.GetGenericArguments()[0]);
            var clientMethods = clientInterfaceTypes
                .SelectMany(h => h.GetMethods()
                    .Where(m => m.IsPublic)
                    .Where(m => m.DeclaringType == h));
            ProcessMethods(clientMethods, generator);
        }

        private static bool IncludedNamespace(Options options, Type t)
        {
            return options.Namespaces.Any(n => Regex.IsMatch((t.Namespace ?? ""), WildcardToRegex(n)));
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace(@"%", ".*") + "$";
        }

        private static void ProcessMethods(IEnumerable<MethodInfo> methods, TypeScriptFluent generator)
        {
            var returnTypes = methods.Select(m => m.ReturnType);
            ProcessTypes(returnTypes, generator);
            var inputTypes = methods.SelectMany(m => m.GetParameters()).Select(p => p.ParameterType);
            ProcessTypes(inputTypes, generator);
        }

        private static void ProcessTypes(IEnumerable<Type> types, TypeScriptFluent generator)
        {
            foreach (var clrType in types.Where(t => t != typeof (void)))
            {
                var clrTypeToUse = clrType;
                if (typeof (Task).IsAssignableFrom(clrTypeToUse))
                {
                    if (clrTypeToUse.IsGenericType)
                    {
                        clrTypeToUse = clrTypeToUse.GetGenericArguments()[0];
                    }
                    else continue; // Ignore non-generic Task as we can't know what type it will really be
                }
                if (clrTypeToUse.IsNullable())
                {
                    clrTypeToUse = clrTypeToUse.GetUnderlyingNullableType();
                }
                // Ignore compiler generated types
                if (Attribute.GetCustomAttribute(clrTypeToUse, typeof (CompilerGeneratedAttribute)) != null)
                {
                    continue;
                }

                Console.WriteLine("Processing Type: " + clrTypeToUse);
                if (clrTypeToUse == typeof(string) || clrTypeToUse.IsPrimitive || clrTypeToUse == typeof(object)) continue;

                if (clrTypeToUse.IsArray)
                {
                    ProcessTypes(new[] { clrTypeToUse.GetElementType() }, generator);
                }
                else if (clrTypeToUse.IsGenericType)
                {
                    ProcessTypes(clrTypeToUse.GetGenericArguments(), generator);
                    bool isEnumerable = typeof (IEnumerable).IsAssignableFrom(clrTypeToUse);
                    if (!isEnumerable)
                    {
                        generator.ModelBuilder.Add(clrTypeToUse);
                    }
                }
                else if (!typeof(IEnumerable).IsAssignableFrom(clrTypeToUse))
                {
                    generator.ModelBuilder.Add(clrTypeToUse);
                }
            }
        }

        public void GenerateSignalrHubs()
        {
            var allOutput = new StringBuilder();
            foreach (var assemblyName in _options.Assemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                allOutput.Append(_configuration.SignalRGenerator.GenerateHubs(assembly, _options.GenerateAsModules));
            }
            // Don't create the output if we don't have any hubs!
            if (allOutput.Length == 0) return;

            File.WriteAllText(Path.Combine(_options.OutputFilePath, "hubs.d.ts"), allOutput.ToString());
        }

        public void GenerateWebApiActions()
        {
            if (_options.GenerateServiceStackRequests)
            {
                throw new Exception("WebApi actions are not supported for ServiceStack APIs at the moment!  Please submit a Pull Request!");
            }

            var output = new StringBuilder("module Api {");
            //TODO: allow this is be configured
            output.Append(_interfaces);

            foreach (var assemblyName in _options.Assemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                var controllers = assembly.GetTypes().Where(_configuration.ControllerPredicate).OrderBy(t => t.Name);

                foreach (var controller in controllers)
                {
                    var actions = controller.GetMethods()
                        .Where(_configuration.ActionsPredicate)
                        .Where(m => m.DeclaringType == controller)
                        .OrderBy(m => m.Name);
                    if (!actions.Any()) continue;

                    var controllerName = controller.Name.Replace("Controller", "");
                    output.AppendFormat("\r\n  export class {0} {{\r\n", controllerName);

                    // TODO: WebAPI supports multiple actions with the same name but different parameters - this doesn't!
                    foreach (var action in actions)
                    {
                        if (NotAnAction(action)) continue;

                        var httpMethod = GetHttpMethod(action);
                        var returnType = TypeScriptDefinitionsGenerator.Common.TypeConverter.GetTypeScriptName(action.ReturnType);

                        var actionParameters = _configuration.GetActionParameters(action);
                        var dataParameter = actionParameters.FirstOrDefault(a => !a.FromUri && !a.RouteProperty);
                        var dataParameterName = dataParameter == null ? "null" : dataParameter.Name;
                        var url = _configuration.UrlGenerator.GetUrl(action);
                        // allow ajax options to be passed in to override defaults
                        output.AppendFormat("    public static {0}({1}): JQueryPromise<{2}> {{\r\n",
                            action.Name.ToCamelCase(), GetMethodParameters(actionParameters, "IExtendedAjaxSettings"), returnType);
                        output.AppendFormat("      return ServiceCaller.{0}({1}, {2}, ajaxOptions);\r\n",
                            httpMethod, url, dataParameterName);
                        output.AppendLine("    }");
                        output.AppendLine();
                    }

                    output.AppendLine("  }");
                }
            }

            output.Append("}");

            File.WriteAllText(Path.Combine(_options.OutputFilePath, "actions.ts"), output.ToString());

            if (!_options.SuppressDefaultServiceCaller)
            {
                // Write the default service caller
                using (var stream = typeof(MainGenerator).Assembly.GetManifestResourceStream(typeof(MainGenerator).Namespace + ".Resources.ServiceCaller.ts"))
                using (var reader = new StreamReader(stream))
                {
                    File.WriteAllText(Path.Combine(_options.OutputFilePath, "servicecaller.ts"), reader.ReadToEnd());
                }
            }
        }
        
        public void GenerateAureliaWebApiActions()
        {
            if (_options.GenerateServiceStackRequests)
            {
                throw new Exception("Aurelia actions are not supported for ServiceStack APIs at the moment!  Please submit a Pull Request!");
            }

            var output = new StringBuilder("import {autoinject} from \"aurelia-dependency-injection\";\r\n");
            output.AppendLine("import {HttpClient, json, RequestInit} from \"aurelia-fetch-client\";\r\n");
            var requiredImports = new HashSet<string>();

            //TODO: allow this is be configured
            output.Append(_interfaces);

            foreach (var assemblyName in _options.Assemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                var controllers = assembly.GetTypes().Where(_configuration.ControllerPredicate).OrderBy(t => t.Name);

                foreach (var controller in controllers)
                {
                    requiredImports.Add(controller.Namespace.Split('.')[0]);
                    var actions = controller.GetMethods()
                        .Where(_configuration.ActionsPredicate)
                        .Where(m => m.DeclaringType == controller)
                        .OrderBy(m => m.Name);

                    if (!actions.Any()) continue;
                    var controllerName = controller.Name.Replace("Controller", "");
                    output.AppendLine("  @autoinject");
                    output.AppendFormat("  export class {0} {{\r\n", controllerName);
                    output.AppendLine("    constructor(private http: HttpClient) {");
                    output.AppendLine("    }");

                    // TODO: WebAPI supports multiple actions with the same name but different parameters - this doesn't!
                    foreach (var action in actions)
                    {
                        if (NotAnAction(action)) continue;

                        var httpMethod = GetHttpMethod(action);
                        var actionName = GetActionName(action);
                        var returnType = TypeConverter.GetTypeScriptName(action.ReturnType);
                        if (returnType.Contains("."))
                        {
                            foreach (var s in returnType.GetTopLevelNamespaces())
                            {
                                requiredImports.Add(s);                                
                            }
                        }
                        
                        var actionParameters = _configuration.GetActionParameters(action);
                        actionParameters.ForEach(a =>
                        {
                            if (a.Type.Contains("."))
                            {
                                foreach (var s in a.Type.GetTopLevelNamespaces())
                                {
                                    requiredImports.Add(s);                                
                                }
                            }
                        });                      
                        var dataParameter = actionParameters.FirstOrDefault(a => !a.FromUri && !a.RouteProperty);
                        var dataParameterName = dataParameter == null ? "null" : dataParameter.Name;
                        var url = _configuration.UrlGenerator.GetUrl(action);
                        // allow ajax options to be passed in to override defaults
                        output.AppendFormat("    public {0}({1}): PromiseLike<{2}> {{\r\n",
                            actionName, GetMethodParameters(actionParameters, "RequestInit|undefined", true), returnType);
                        output.AppendFormat("      const options: RequestInit = {{ \r\n        method: \"{0}\", \r\n", httpMethod);
                        output.AppendFormat("        body: {0} ? json({0}) : undefined\r\n", dataParameterName);
                        output.AppendLine("      };");
                        output.AppendLine("      if (ajaxOptions) Object.assign(options, ajaxOptions);");
                        if (returnType == "string")
                        {
                            output.AppendFormat("      return this.http.fetch({0}, options)\r\n" +
                                                "        .then((response: Response) => (response && response.status!==204) ? response.text() : \"\");\r\n", url);
                        }
                        else
                        {
                            output.AppendFormat("      return this.http.fetch({0}, options)\r\n" +
                                                "        .then((response: Response) => (response && response.status!==204) ? response.json() : null);\r\n", url);
                        }
                        output.AppendLine("    }");
                        output.AppendLine();
                    }

                    output.AppendLine("  }");
                }
            }

            if (_options.GenerateAsModules)
            {
                var imports = new StringBuilder();
                imports.AppendLine("import Classes = require(\"./classes\");");
                foreach (var ns in requiredImports)
                {
                    imports.AppendFormat("import {0} = Classes.{0};\r\n", ns);
                }
                imports.AppendLine();
                output.Insert(0, imports.ToString());
            }
            File.WriteAllText(Path.Combine(_options.OutputFilePath, "actions.ts"), output.ToString());
        }


        private static string GetMethodParameters(List<ActionParameterInfo> actionParameters, string settingsType, bool useUndefinedForSettingsType = false, string optionsParameterName = "ajaxOptions")
        {
            var result = string.Join(", ", actionParameters.Select(a => a.Name + ": " + (a.ClrType?.IsEnum == true ? "Enums." : "") + a.Type));
            if (result != "") result += ", ";
            result += optionsParameterName + ": " + settingsType + (useUndefinedForSettingsType ? " = undefined" : " = null");
            return result;
        }

        private static string GetHttpMethod(MethodInfo action)
        {
            // TODO: Support other http methods
            if (action.CustomAttributes.Any(a => a.AttributeType.Name == "HttpPostAttribute")) return "post";
            return "get";
        }

        private static bool NotAnAction(MethodInfo action)
        {
            return action.CustomAttributes.Any(a => a.AttributeType.Name == "NonActionAttribute");
        }

        private static string GetActionName(MethodInfo action)
        {
            // TODO: Support ActionNameAttribute
            return action.Name.ToCamelCase();
        }

        private void GenerateAngularActions()
        {
            if (!_options.GenerateServiceStackRequests)
            {
                throw new Exception("Angular is only supported for ServiceStack APIs at the moment!  Please submit a Pull Request!");
            }

            var output = new StringBuilder("import {Injectable} from \"@angular/core\";");
            output.AppendLine();
            output.AppendLine("import {Observable, of} from \"rxjs\";");
            output.AppendLine("import {HttpClient, HttpErrorResponse, HttpHeaders, HttpParams} from \"@angular/common/http\";");
            output.AppendLine("import {ServiceService} from \"./../service.service\";");
            output.AppendLine("import {MessageService} from \"./../../core/components/messages/message.service\";");

            output.AppendLine();
            output.AppendLine(@"type NgHttpOptions = {
  headers?: HttpHeaders | {
      [header: string]: string | string[];
  };
  observe?: ""body"";
  params?: HttpParams | {
      [param: string]: string | string[];
  };
  reportProgress?: boolean;
  responseType?: ""json"";
  withCredentials?: boolean;
}");
            var requiredImports = new HashSet<string>();
            var allRequests = new List<string>();

            //TODO: allow this is be configured
            output.Append(_interfaces);

            foreach (var assemblyName in _options.Assemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                var requests = GetServiceStackRequestTypes(assembly);

                foreach (var request in requests)
                {
                    var requestTypeScriptName = TypeConverter.GetTypeScriptName(request);
                    var returnType = _ssHelper.GetResponseTypeForRequest(request);
                    var returnsVoid = _ssHelper.ReturnsVoid(request);

                    var returnTypeTypeScriptName = returnsVoid ? "void" : (returnType != null ? TypeConverter.GetTypeScriptName(returnType) : "any");
                    var routes = request.GetCustomAttributes().Where(attr => attr.GetType().FullName == "ServiceStack.RouteAttribute");
                    
                    if (!routes.Any()) continue;
                    output.AppendLine("  @Injectable({providedIn:\"root\"})");
                    // Replace the 'Request' at the end of the class name, so it doesn't clash with the generated interface in classes.ts - just to make life easier. 
                    output.AppendLine($"  export class {request.Name.ReplaceLastOccurrence("Request", "")} extends ServiceService {{");
                    output.AppendLine("    constructor(http: HttpClient, msg: MessageService) {");
                    output.AppendLine("        super(http, msg);");
                    output.AppendLine("    }");
                    allRequests.Add(request.Name);

                    var items = new List<ServiceStackRouteInfo>();
                    foreach (var route in routes)
                    {
                        var verbs = route.GetType().GetProperty("Verbs").GetValue(route) as string;
                        var path = route.GetType().GetProperty("Path").GetValue(route) as string;

                        foreach (var verb in verbs.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()))
                        {
                            if (verb.Equals("Options", StringComparison.OrdinalIgnoreCase)) continue;

                            output.AppendLine($"// {verb} - {path} - RETURNS: " + (returnType?.FullName ?? "(unknown)"));

                            var url = _ssHelper.GenerateUrlFromRoute(path, request, verb.Equals("get", StringComparison.OrdinalIgnoreCase), out var routeParameters);
                            items.Add(new ServiceStackRouteInfo(verb, path, url, routeParameters));
                        }

                    }
                    var gets = items.Where(i => i.Verb == "GET").ToList();
                    if (gets.Any())
                    {
                        var item = gets.MaxBy(i => i.RouteParameters.Count);
                        var actionParameters = _ssHelper.GetActionParameters(request, item);
                        output.AppendLine($"    {item.Verb.ToLower()}({GetMethodParameters(actionParameters, "NgHttpOptions", true, "httpOptions")}): Promise<{returnTypeTypeScriptName}> {{");
                        output.AppendLine($"      return super.makePromise(this.http.{item.Verb.ToLower()}<{returnTypeTypeScriptName}>({item.Url}, httpOptions));");
                        output.AppendLine("    }");
                    }
                    var deletes = items.Where(i => i.Verb == "DELETE").ToList();
                    if (deletes.Any())
                    {
                        var item = deletes.MaxBy(i => i.RouteParameters.Count);
                        var actionParameters = _ssHelper.GetActionParameters(request, item);
                        output.AppendLine($"    {item.Verb.ToLower()}({GetMethodParameters(actionParameters, "NgHttpOptions", true, "httpOptions")}): Promise<{returnTypeTypeScriptName}> {{");
                        output.AppendLine($"      return super.makePromise(this.http.{item.Verb.ToLower()}<{returnTypeTypeScriptName}>({item.Url}, httpOptions));");
                        output.AppendLine("    }");
                    }
                    var posts = items.Where(i => i.Verb == "POST").ToList();
                    if (posts.Any())
                    {
                        var item = posts.MinBy(i => i.RouteParameters.Count);
                        var actionParameters = _ssHelper.GetActionParameters(request, item);
                        actionParameters.Add(new ActionParameterInfo
                        {
                            Name = "body",
                            Type = TypeConverter.GetTypeScriptName(request)
                        });
                        actionParameters.ForEach(a =>
                        {
                            if (a.Type.Contains("."))
                            {
                                foreach (var s in a.Type.GetTopLevelNamespaces())
                                {
                                    requiredImports.Add(s);
                                }
                            }
                        }); 
                        output.AppendLine($"    {item.Verb.ToLower()}({GetMethodParameters(actionParameters, "NgHttpOptions", true, "httpOptions")}): Promise<{returnTypeTypeScriptName}> {{");
                        output.AppendLine($"      return super.makePromise(this.http.{item.Verb.ToLower()}<{returnTypeTypeScriptName}>({item.Url}, body, httpOptions));");
                        output.AppendLine("    }");
                    }
                    var puts = items.Where(i => i.Verb == "PUT").ToList();
                    if (puts.Any())
                    {
                        var item = puts.MinBy(i => i.RouteParameters.Count);
                        var actionParameters = _ssHelper.GetActionParameters(request, item);
                        actionParameters.Add(new ActionParameterInfo
                        {
                            Name = "body",
                            Type = TypeConverter.GetTypeScriptName(request)
                        });
                        actionParameters.ForEach(a =>
                        {
                            if (a.Type.Contains("."))
                            {
                                foreach (var s in a.Type.GetTopLevelNamespaces())
                                {
                                    requiredImports.Add(s);
                                }
                            }
                        }); 
                        output.AppendLine($"    {item.Verb.ToLower()}({GetMethodParameters(actionParameters, "NgHttpOptions", true, "httpOptions")}): Promise<{returnTypeTypeScriptName}> {{");
                        output.AppendLine($"      return super.makePromise(this.http.{item.Verb.ToLower()}<{returnTypeTypeScriptName}>({item.Url}, body, httpOptions));");
                        output.AppendLine("    }");
                    }

                    output.AppendLine("  }");
                }
            }

            if (_options.GenerateAsModules)
            {
                var imports = new StringBuilder();
                imports.AppendLine("import * as Classes from \"./classes\";");
                imports.AppendLine("import * as Enums from \"./enums\";");
                foreach (var ns in requiredImports)
                {
                    imports.AppendFormat("import {0} = Classes.{0};\r\n", ns);
                }
                imports.AppendLine();
                output.Insert(0, imports.ToString());
            }

            output.AppendLine("");
            output.AppendLine("export interface AllRequests {");
            foreach (var request in allRequests)
            {
                output.AppendLine($"  {request}: any;");
            }
            output.AppendLine("}");
            File.WriteAllText(Path.Combine(_options.OutputFilePath, "actions.ts"), output.ToString());
           
        }


        private static string _interfaces = @"
  export interface IDictionary<T> {
     [key: string]: T;
  }

";
    }
}