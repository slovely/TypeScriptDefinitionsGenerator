using System.Collections.Generic;
using CommandLine;

namespace TypeScriptDefinitionsGenerator.Core
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
        [OptionList('n', "namespaces", HelpText = "All classes in this namespace will be converted.", Required = false, Separator = ',')]
        public List<string> Namespaces { get; set; }
        [Option("webapiactions", HelpText = "Indicates that methods should be generated for WebAPI actions")]
        public bool GenerateWebApiActions { get; set; }
        [Option("actionsstyle", HelpText = "Indicates that the style of action methods generated")]
        public ActionsStyle ActionsStyle { get; set; }
        [Option("debugger", HelpText = "Will prompt to attach the debugger")]
        public bool AttachDebugger { get; set; }
        [Option("suppressdefaultservicecaller", HelpText = "Don't use the default service caller (that uses JQuery ajax methods)", DefaultValue = false)]
        public bool SuppressDefaultServiceCaller { get; set; }
    }

    public enum ActionsStyle
    {
        Default,
        Aurelia,
    }
}