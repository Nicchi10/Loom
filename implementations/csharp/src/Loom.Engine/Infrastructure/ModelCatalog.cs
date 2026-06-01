using Loom.Core.Enums;
using Loom.Core.Models;
using System;
using System.Collections.Generic;

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
        /// Fallback if user input model isn't found
        /// </summary>
        /// <param name="priority"> Choose by user </param>
        /// <returns> ModelMetaData object with user settings priority </returns>
        /// TODO: To be reviewed, only works if ExecutionPriority is 'CostOptimized', map the other options
        public ModelMetaData GetFallBack(ExecutionPriority priority)
        {
            ModelMetaData best = null;

            if (priority == ExecutionPriority.CostOptimized)
            {
                int lowestCost = int.MaxValue;

                foreach (var model in _models)
                {
                    if (model.CostLevel < lowestCost)
                    {
                        lowestCost = model.CostLevel;
                        best = model;
                    }
                }
            } else
            {
                int highestCost = int.MinValue;

                foreach (var model in _models)
                {
                    if (model.CostLevel > highestCost)
                    {
                        highestCost = model.CostLevel;
                        best = model;
                    }
                }
            }

            return best;
        }

    }
}