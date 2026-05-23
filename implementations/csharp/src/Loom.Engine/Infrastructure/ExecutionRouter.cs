using Loom.Core.Interfaces;
using Loom.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Loom.Engine.Infrastructure
{
    /// <summary>
    /// Records new adapter and routes user's input model to a specific provider
    /// </summary>
    public class ExecutionRouter
    {

        private readonly List<IProviderAdapter> _adapters = new List<IProviderAdapter>();
        private readonly ModelCatalog _catalog = new ModelCatalog();


        public void RegisterAdapter(IProviderAdapter adapter)
        {
            if (!_adapters.Any(a => a.ProviderName == adapter.ProviderName))
                _adapters.Add(adapter);
        }

        /// <summary>
        /// Selects the provider based on the entered model
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>Selected provider</returns>
        public IProviderAdapter Route(ILlmInvocation invocation)
        {
            var targetModelId = invocation.Hints.PreferredModel;
            ModelMetaData selectedModelInfo = null;

            if (!string.IsNullOrWhiteSpace(targetModelId))
                selectedModelInfo = _catalog.GetModelInfo(targetModelId);

            if (selectedModelInfo == null)
            {
                selectedModelInfo = _catalog.GetFallBack(invocation.Hints.Priority);
                invocation.Hints.PreferredModel = selectedModelInfo.ModelId;
            }

            var adapter = _adapters.FirstOrDefault(a => a.ProviderName == selectedModelInfo.ProviderName);

            if (adapter == null)
                throw new Exception($"No adapter installed for the provider: {selectedModelInfo.ProviderName}");

            return adapter;
        }

    }
}