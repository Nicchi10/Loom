using System.Collections.Generic;

namespace Loom.Providers.OpenAI.Schemas
{
    public class OpenAIFunctionSchema
    {
        public string type { get; set; } = "object";
        public Dictionary<string, OpenAIPropertySchema> properties { get; set; }
        public List<string> required { get; set; }
        public bool additionalProperties { get; set; } = false;
    }
}
