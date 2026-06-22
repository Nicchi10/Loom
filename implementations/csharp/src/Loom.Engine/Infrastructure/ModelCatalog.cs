using Loom.Core.Enums;
using Loom.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.Engine.Infrastructure
{
    /// <summary>
    /// Contains the list of all available models and the rules on how to get them
    /// </summary>
    public class ModelCatalog
    {

        // TODO(#1): make catalog extensible, providers declare their own models (SupportedModels),
        // ModelCatalog becomes a registry exposed via LoomClient.Models, see issue #1 (pending UML review)
        private readonly List<ModelMetaData> _models = new List<ModelMetaData>
        {
            new ModelMetaData {ModelId = "gpt-5.5", ProviderName = "OpenAI", ContextWindow = 128000, CostLevel = 5},
            new ModelMetaData {ModelId = "gpt-4.1-nano", ProviderName = "OpenAI", ContextWindow = 128000, CostLevel = 1},
            new ModelMetaData {ModelId = "gemini-2.5-flash", ProviderName = "GoogleAI", ContextWindow = 128000, CostLevel = 1}
        };

        /// <summary>
        /// Gets the model based on user input
        /// </summary>
        /// <param name="modelId"> e.g: "gpt-5.5" </param>
        /// <returns> ModelMetaData object </returns>
        public ModelMetaData GetModelInfo(string modelId)
        {
            foreach (var model in _models)
            {
                if (model.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase))
                    return model;
            }

            return null;
        }

        /// <summary>
        /// Fallback when the user did not pick a model: chooses one from the catalog
        /// according to the requested ExecutionPriority.
        /// </summary>
        /// <param name="priority"> Chosen by the user </param>
        /// <returns> The best ModelMetaData for that priority </returns>
        /// <exception cref="Exception"> Thrown if the catalog is empty </exception>
        /// <remarks>
        /// CostLevel is the only ranking signal the catalog has today, so latency and
        /// quality are approximated from it: the cheapest models are usually the small,
        /// fast ones, the most expensive are usually the largest, most capable ones.
        /// TODO(#1): add real LatencyLevel / QualityLevel to ModelMetaData and rank on those.
        /// </remarks>
        public ModelMetaData GetFallBack(ExecutionPriority priority)
        {
            if (_models.Count == 0)
                throw new Exception("Model catalog is empty: no fallback model available");

            var byCostAscending = _models.OrderBy(m => m.CostLevel).ToList();

            switch (priority)
            {
                // Cheapest model: lowest bill and, as a proxy, the lowest latency.
                case ExecutionPriority.CostOptimized:
                case ExecutionPriority.LatencyOptimized:
                    return byCostAscending.First();

                // Most expensive model: best proxy we have for "most capable".
                case ExecutionPriority.QualityOptimized:
                    return byCostAscending.Last();

                // Balanced: the positional middle of the cost-sorted list -- a rough
                // proxy, not a true cost midpoint. On even counts it biases to the pricier
                // side; with a richer catalog it lands between the cheap and pricey models.
                case ExecutionPriority.Balanced:
                default:
                    return byCostAscending[byCostAscending.Count / 2];
            }
        }

    }
}