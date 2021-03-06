﻿using System.Collections.Generic;
using CommandLine;

namespace TypeScriptDefinitionsGenerator.Common
{
    public class Options
    {
        public Options()
        {
        }

        [OptionList('a', "assemblies", Required = true, HelpText = "The DLL(s) containing the items to generate.", Separator = ',')]
        public List<string> Assemblies { get; set; }
        [Option('o', "output", Required = true, HelpText = "The folder to which the output will be written.")]
        public string OutputFilePath { get; set; }
        [Option('t', "templateFolder", Required = false, HelpText = "[OPTIONAL] The folder containing custom Handlebars templates.")]
        public string TemplateFolder { get; set; }
        [OptionList('n', "namespaces", HelpText = "All classes in this namespace will be converted.", Required = false, Separator = ',')]
        public List<string> Namespaces { get; set; }
        [Option("webapiactions", HelpText = "Indicates that methods should be generated for WebAPI actions")]
        public bool GenerateWebApiActions { get; set; }
        [Option("servicestack", HelpText = "Indicates that methods should be generated for ServiceStack requests")]
        public bool GenerateServiceStackRequests { get; set; }
        [Option("actionsstyle", HelpText = "Indicates that the style of action methods generated")]
        public ActionsStyle ActionsStyle { get; set; }
        [Option("debugger", HelpText = "Will prompt to attach the debugger")]
        public bool AttachDebugger { get; set; }
        [Option("suppressdefaultservicecaller", HelpText = "Don't use the default service caller (that uses JQuery ajax methods)", DefaultValue = false)]
        public bool SuppressDefaultServiceCaller { get; set; }
        [Option("generateasmodules", HelpText = "Generates classes/enums/actions using exported modules", DefaultValue = false)]
        public bool GenerateAsModules { get; set; }
        [Option("stringenums", HelpText = "Generates string enums", DefaultValue = false)]
        public bool UseStringEnums { get; set; }
        [Option("camelcase", HelpText = "Generates property names using camel case", DefaultValue = false)]
        public bool CamelCase { get; set; }
        [Option("actionsOutputFileName", HelpText = "Set the output filename for the actions file.  Default: actions.ts", DefaultValue = "actions.ts")]
        public string ActionsOutputFileName { get; set; }
        [Option("hubsOutputFileName", HelpText = "Set the output filename for the hubs file.  Default: hubs.d.ts", DefaultValue = "hubs.d.ts")]
        public string HubsOutputFileName { get; set; }
        [Option("wrapclasses", HelpText = "Optionally wrap the generated classes.ts in a module.  Default: do not wrap", DefaultValue = "")]
        public string WrapClassesInModule { get; set; }
        [Option("wrapenums", HelpText = "Optionally wrap the generated enums.ts in a module.  Default: do not wrap", DefaultValue = "")]
        public string WrapEnumsInModule { get; set; }
        [Option("actionsExplicitOptIn", HelpText = "Whether API methods require opt-in attribute, or all are generated", DefaultValue = false)]
        public bool ActionsExplicitOptIn { get; set; }
        [Option("supportMomentJs", HelpText = "Whether generated code supports moment js dates", DefaultValue = false)]
        public bool SupportMomentJs { get; set; }
    }

    public enum ActionsStyle
    {
        Default,
        Aurelia,
        Angular,
    }
}