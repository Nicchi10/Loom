using Loom.Core.Models;
using Loom.Providers.GoogleAI.Schemas;
using System.Collections.Generic;

namespace Loom.Providers.GoogleAI
{
    /// <summary>
    /// Reconstructs raw data into the specific GoogleAI format
    /// </summary>
    public class GoogleAIToolSchemaBuilder
    {
        public GoogleFunctionDeclaration Build(ToolDefinition def)
        {
            // Check parameters and required != null
            var props = new Dictionary<string, GooglePropertySchema>();

            if (def.Parameters != null && def.Parameters.Count > 0)
            {
                foreach (var param in def.Parameters)
                {
                    var propSchema = new GooglePropertySchema();
                    propSchema.type = string.IsNullOrEmpty(param.Type) ? "string" : param.Type;
                    propSchema.description = param.Description ?? "";

                    // Enum added only if != null
                    if (param.Enums != null && param.Enums.Count > 0)
                        propSchema.@enum = param.Enums;

                    props.Add(param.Name, propSchema);
                }
            }

            var requiredList = def.Required ?? new List<string>();

            // No additionalProperties / strict: Gemini doesn't support them
            var functionSchema = new GoogleFunctionSchema
            {
                type = "object",
                properties = props,
                required = requiredList
            };

            return new GoogleFunctionDeclaration
            {
                name = def.ToolName,
                description = def.Description,
                parameters = functionSchema
            };
        }
    }
}
