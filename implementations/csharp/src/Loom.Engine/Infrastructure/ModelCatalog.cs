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
            new ModelMetaData {ModelId = "gpt-5.5", ProviderName = "OpenAI", ContextWindow = 128000, CostLevel = 5, LatencyLevel = 4, QualityLevel = 5},
            new ModelMetaData {ModelId = "gpt-4.1-nano", ProviderName = "OpenAI", ContextWindow = 128000, CostLevel = 1, LatencyLevel = 1, QualityLevel = 2},
            new ModelMetaData {ModelId = "gemini-2.5-flash", ProviderName = "GoogleAI", ContextWindow = 128000, CostLevel = 1, LatencyLevel = 2, QualityLevel = 3}
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
        /// according to the requested ExecutionPriority, ranking on the relevant signal
        /// (CostLevel, LatencyLevel or QualityLevel).
        /// </summary>
        /// <param name="priority"> Chosen by the user </param>
        /// <returns> The best ModelMetaData for that priority </returns>
        /// <exception cref="Exception"> Thrown if the catalog is empty </exception>
        public ModelMetaData GetFallBack(ExecutionPriority priority)
        {
            if (_models.Count == 0)
                throw new Exception("Model catalog is empty: no fallback model available");

            switch (priority)
            {
                // Cheapest model.
                case ExecutionPriority.CostOptimized:
                    return _models.OrderBy(m => m.CostLevel).First();

                // Fastest model (lowest latency rank).
                case ExecutionPriority.LatencyOptimized:
                    return _models.OrderBy(m => m.LatencyLevel).First();

                // Most capable model (highest quality rank).
                case ExecutionPriority.QualityOptimized:
                    return _models.OrderByDescending(m => m.QualityLevel).First();

                // Balanced: a simple value score that rewards quality while penalising
                // cost and latency (quality is double-weighted). Highest score wins.
                case ExecutionPriority.Balanced:
                default:
                    return _models.OrderByDescending(m => 2 * m.QualityLevel - m.CostLevel - m.LatencyLevel).First();
            }
        }

    }
}