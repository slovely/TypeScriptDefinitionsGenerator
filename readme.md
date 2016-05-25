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

##### TsGenerator.props Options
 - TsGenInputAssembly [Required] - enter the assembly containing your webapi controllers/signalr hubs/other models here.  The Path is relative to the WebSite Project Directory.
 - TsGenOutputFolder [Required] - enter the path where the generated typescript files should go.  The path is relative to the WebSite Project Directory.
 - TsGenWebApiMethods [Optional, default true] - Enter false to switch off generation of WebAPI methods (no `actions.ts` will be created).
 - TsGenUseDefaultServiceCaller [Optional, default true] - Enter false to prevent 'servicecaller'ts' being generated.
 - TsGenDebug [Optional, default false] - This is used to aid debugging the build.  Enter true to be prompted to attach a debugger when building.
 - TsNamespaces [Optional] - Enter a list of namespaces for classes you want to generate even if they aren't references from WebAPI / SignalR hubs.


### Thanks
Massive thanks go to [Lukas Kabrt](https://bitbucket.org/LukasKabrt/) for his wonderful [TypeLite](https://bitbucket.org/LukasKabrt/typelite/) library which does the bulk of the 
TypeScript generation.  I am pleased that I was able to contribute the first version of generics support to his project while making this project.