namespace TypeScriptDefinitionsGenerator.Common
{
    public class ActionParameterInfo
    {
        public string Name { get; set; }
        public bool FromUri { get; set; }
        public bool RouteProperty { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
    }
}