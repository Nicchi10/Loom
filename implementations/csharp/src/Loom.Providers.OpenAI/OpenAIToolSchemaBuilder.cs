using Loom.Core.Models;
using Loom.Providers.OpenAI.Schemas;
using System.Collections.Generic;

namespace Loom.Providers.OpenAI
{
    /// <summary>
    /// Reconstructs raw data into the specific OpenAI format
    /// </summary>
    public class OpenAIToolSchemaBuilder
    {
        public OpenAIToolSchema Build(ToolDefinition def)
        {
            // Check parameters and required != null
            var props = new Dictionary<string, OpenAIPropertySchema>();

            if (def.Parameters != null && def.Parameters.Count > 0)
            {
                foreach (var param in def.Parameters)
                {
                    var propSchema = new OpenAIPropertySchema();
                    propSchema.type = string.IsNullOrEmpty(param.Type) ? "string" : param.Type;
                    propSchema.description = param.Description ?? "";

                    // Enum added only if != null
                    if (param.Enums != null && param.Enums.Count > 0)
                        propSchema.@enum = param.Enums;

                    props.Add(param.Name, propSchema);
                }
            }

            var requiredList = def.Required ?? new List<string>();

            var functionSchema = new OpenAIFunctionSchema
            {
                type = "object",
                properties = props,
                required = requiredList,
                additionalProperties = false
            };

            return new OpenAIToolSchema
            {
                name = def.ToolName,
                description = def.Description,
                parameters = functionSchema,
                strict = def.Strictness.GetValueOrDefault(false)
            };
        }
    }
}
