using System.Collections.Generic;

namespace Loom.Providers.OpenAI.Schemas
{
    public class OpenAIPropertySchema
    {
        public string type { get; set; } = "string";
        public List<string>? @enum { get; set; }
        public string description { get; set; } = "";
    }
}
