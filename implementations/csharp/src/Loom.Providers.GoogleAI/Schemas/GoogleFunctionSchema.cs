using System.Collections.Generic;

namespace Loom.Providers.GoogleAI.Schemas
{
    public class GoogleFunctionSchema
    {
        public string type { get; set; } = "object";
        public Dictionary<string, GooglePropertySchema> properties { get; set; }
        public List<string> required { get; set; }
    }
}
