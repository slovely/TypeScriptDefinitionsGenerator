using System;
using System.Collections.Generic;

namespace TypeScriptDefinitionsGenerator.Common.Models
{
    public class Controller
    {
        public Controller()
        {
            Actions = new List<Action>();
        }

        public Type ServerType { get; set; }
        public string TypeScriptName { get; set; }
        public IList<Action> Actions { get; set; }
    }

    public class Action
    {
        public Action()
        {
            ActionParameters = new List<ActionParameterInfo>();
        }

        public Type ServerType { get; set; }
        public string TypeScriptName { get; set; }
        public IEnumerable<string> Verbs { get; set; }
        public string Url { get; set; }
        public List<ActionParameterInfo> ActionParameters { get; set; }
        public string ReturnType { get; set; }
        public string DataParameterName { get; set; }
    }

    public class WebApiModel
    {
        public WebApiModel()
        {
            Controllers = new List<Controller>();
        }

        public IList<Controller> Controllers { get; set; }
    }
}