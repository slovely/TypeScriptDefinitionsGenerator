using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace TypeScriptDefinitionsGenerator.BuildSupport
{
    /// <summary>
    /// Simple inline task to join an msbuild list into a comma-separated string.
    /// Used for converting namespaces / assemblies options to a form like that:
    ///  -n [ns1] [ns2]...[nsN] 
    /// </summary>
    public class CreateMulitpleParameterString : Task
    {
        public string ArgumentName { get; set; }

        public ITaskItem[] Values { get; set; }

        [Output]
        public string Parameter { get; set; }

        public override bool Execute()
        {
            Parameter = "";

            if (Values == null || !Values.Any()) return true;

            Parameter = ArgumentName + " ";
            Parameter += string.Join(",", Values.Where(n => !string.IsNullOrWhiteSpace(n.ItemSpec)).Select(n => n.ItemSpec));
            return true;
        }
    }
}
