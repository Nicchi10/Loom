namespace Loom.Providers.OpenAI.Schemas
{
    public class OpenAIToolSchema
    {
        public string type { get; set; } = "function";
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public OpenAIFunctionSchema parameters { get; set; } = new OpenAIFunctionSchema();
        public bool? strict { get; set; }
    }
}
