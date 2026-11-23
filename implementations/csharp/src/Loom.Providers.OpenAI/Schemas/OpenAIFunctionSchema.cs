using System.Collections.Generic;

namespace Loom.Providers.OpenAI.Schemas
{
    public class OpenAIFunctionSchema
    {
        public string type { get; set; } = "object";
        public Dictionary<string, OpenAIPropertySchema> properties { get; set; } = new Dictionary<string, OpenAIPropertySchema>();
        public List<string> required { get; set; } = new List<string>();
        public bool additionalProperties { get; set; } = false;
    }
}
