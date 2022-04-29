using System.Collections.Generic;
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

            if (Values == null || Values.Length == 0) return true;

            // Removed LINQ statements here to work around an odd bug that causes the 
            // build to fail when run from VS2022 (but fine from Rider / Command Line - go figure!)
            Parameter = ArgumentName + " ";
            var valuesToJoin = new List<string>();
            foreach (var value in Values)
            {
                if (string.IsNullOrWhiteSpace(value.ItemSpec)) continue;
                valuesToJoin.Add(value.ItemSpec);
            }
            Parameter += string.Join(",", valuesToJoin);
            return true;
        }
    }
}
