[![Build status](https://ci.appveyor.com/api/projects/status/qee4nv3ta5ubyyik?svg=true)](https://ci.appveyor.com/project/slovely/typescriptdefinitionsgenerator/branch/master)
[![Nuget](https://img.shields.io/nuget/v/TypeScriptDefinitionsGenerator.svg)](https://www.nuget.org/packages/TypeScriptDefinitionsGenerator/)


# TypeScriptDefinitionsGenerator
This is a solution enabling TypeScript definitions of your server-side code to be generated quickly and automatically on each build.  Ensures that renaming a class/property on the server doesn't break your client-side code at runtime.  
Supports WebAPI and SignalR method return/parameter types automatically, or tell it where the classes you want to convert are and it'll do them.

### Features
 * Generates .d.ts files for all c# classes used by your WebAPI action methods or SignalR hubs (and any other classes in specified namespaces).
 * Generates interface definitions for SignalR hubs to ensure your client matches the server.
 * Creates a TypeScript class for each WebAPI controller and methods to call the server in a type-safe manner.  Calls to your server can then be made like this:
```
// Will call the MyWebApiController.GetPerson method.  The generated getPerson method
// has a typed 'number' parameter and the return type is a JQueryPromise<Person> object
// so in the callback typing `person.` will give the correct intellisense.
Api.myWebApiController.getPerson(3).done(person => alert(person.Name));
```
 * Ensures that your website compilation will fail if the client/server are out-of-sync - rather than getting runtime errors when the site is running.

### Setup
1 - Because we want the build to fail if something on the server doesn't match with something on the client, we need to have the server build first - this means that you
 WebAPI / SignalR hubs need to be moved into a separate assembly, which you then reference from your web project.  
 Normally this 'just works' as WebAPI/SignalR find controllers/hubs in any references assemblies.

2 - Install the TypeScriptDefinitionsGenerator nuget package to your web project.

3 - Update the `TsGenerator.props` file added by the nuget package.

4 - Include the generated files in your project and ensure `enums.ts` and `actions.ts` are output into your HTML (e.g. include in your BundleConfig).

5 - The default generation of WebAPI methods requires JQuery to be present, as it using `$.ajax` for calling the server.

6 - You must define a variable in your javascript called 'rootPath' that points to the website route, something like this:
```
        <script type="text/javascript">
            var rootPath = '@Url.Content("~/")';
        </script>
```

##### TsGenerator.props Options
 - TsGenInputAssembly [Required] - enter the assemblies containing your webapi controllers/signalr hubs/other models here.  The Path is relative to the WebSite Project Directory.  You must add at least one assembly.
 - TsGenOutputFolder [Required] - enter the path where the generated typescript files should go.  The path is relative to the WebSite Project Directory.
 - TsGenWebApiMethods [Optional, default true] - Enter false to switch off generation of WebAPI methods (no `actions.ts` will be created).
 - TsGenSuppressDefaultServiceCaller [Optional, default true] - Enter false to prevent `servicecaller.ts` being generated.
 - TsGenDebug [Optional, default false] - This is used to aid debugging the build.  Enter true to be prompted to attach a debugger when building.
 - TsNamespaces [Optional] - Enter a list of namespaces for classes you want to generate even if they aren't references from WebAPI / SignalR hubs.  Use '%%' as a wildcard (sorry, not very friendly - thank MSBuild!).
 - TsGenApiMethodStyle [Optional] - if using TsGenWebApiMethods option, this specifies the style of the client-side API calls, can be either 'Default' or 'Aurelia'. 

##### TsGenApiMethodStyle / TsGenWebApiMethods

Setting TsGenWebApiMethods to `true` will search your assemblies for WebAPI actions and generate TypeScript methods so you can call them in a type-safe way.  
The default format of the call will be like this:
```
    // Example c# Web API method
    [HttpGet]
    public Person LoadPerson(int id, string type) { .... }
    
    // Generated TS (output to actions.ts)
    //AUTOGEN START
    module Api {
        export class Person{
            public static search(id: number, name: string, ajaxOptions: JQueryAjaxSettings = null): JQueryPromise<Person> {
                return ServiceCaller.get("api/person/search/" + id + "?name=" + name, null, ajaxOptions);
        }
    }
    //AUTOGEN END
    
    // Now you can use this like this:
    Api.Person.loadPerson(42, "Dave").done(response => alert(response.Name));
```
By default, `ServiceCaller.ts` will also be output and will use JQuery AJAX methods to make the calls to the server.  It supports [HttpPost]/[HttpGet] attributes as well as 
understanding whether parameters are part of the URL (route parameters), request body ([FromBody]) or QueryString (everything else).  If you do not use JQuery
you can set TsGenSuppressDefaultServiceCaller=true, and provide your own implementation instead - although currently `actions.ts` will still return `JQueryPromise`
results.  When calling the methods, you can optionally override AJAX settings by supplying a standard JQueryAjaxSettings object.

By setting TsGenWebApiMethods=Aurelia, the generated methods will use the Aurelia `HttpClient` instead of JQuery.  The output will then be something like this:
```
    //AUTOGEN START
    import {autoinject} from "aurelia-dependency-injection";
    import {HttpClient, json} from "aurelia-fetch-client";
    
    @autoinject
    export class Person {
        constructor(private http: HttpClient) {
        }
            
        public search(id: number, name: string, ajaxOptions: RequestInit = null): PromiseLike<Person> {
            const options: RequestInit = { 
                method: "get", 
                body: null
            };
            if (ajaxOptions) Object.assign(options, ajaxOptions);
            return this.http.fetch("api/person/search/" + id + "?name=" + name, options)
              .then(response => (response && response.status!==204) ? response.json() : null);
        }
    }
    //AUTOGEN END
    
    // Now in your Aurelia models, you can inject this in like this:
    import Actions = require("../server/actions");
    
    @autoinject
    export class PersonViewModel {
        constructor(personCtrl: Actions.Person) {
            personCtrl.search(1, "Joe").then(result => alert(result.Name));
        }
    }
```

The generation currently only works with actions using the {controller}/{action}/{id} route format, or if the method has a custom `[RouteAttribute]`.  

##### Multiple Assemblies
When using multiple assemblies, to avoid a whole world of pain, ensure that all any dependencies shared between the assemblies are the same version (e.g. if you reference JSON.NET v9.0.0 in one project, make sure the other project references v9.0.0 as well).

### ASPNET CORE v2
Since v.1.0.57, very early support for ASP.Net Core (v2 only) is included.  The instructions for running it are the same as above, but a couple of additional steps are required:
- You need to add the `TsGenerator.props` file to the root of your web project manually
- The assemblies containing your WebAPI methods must have `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` added to the csproj to allow them to be loaded via reflection.  Looks like something can be done to avoid that with the DependencyContext stuff, but I haven't looked into it yet.

**NOTE**: Because there is no way to know if an action is MVC or WebAPI in Core, the generator assumes actions that return `IActionResult` are for returning MVC views and will be excluded.
Also, there is no support for API methods that don't have the route: `api/{controller}/{action}` which is obviously pretty limiting unfortunately.
 
### Thanks
Massive thanks go to [Lukas Kabrt](https://bitbucket.org/LukasKabrt/) for his wonderful [TypeLite](https://bitbucket.org/LukasKabrt/typelite/) library which does the bulk of the 
TypeScript generation.  I am pleased that I was able to contribute the first version of generics support to his project while making this project.

Also thanks to [Murat Girgin](https://github.com/muratg) whose work on SRTS provided inspiration for the SignalR hub generation (indeed an earlier version used a hacked version
of this library).
