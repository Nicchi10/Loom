using System.Collections.Generic;

namespace Loom.Providers.GoogleAI.Schemas
{
    public class GooglePropertySchema
    {
        public string type { get; set; }
        public List<string> @enum { get; set; }
        public string description { get; set; }
    }
}
