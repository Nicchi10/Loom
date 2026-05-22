using Loom.Core.Enums;
using Loom.Core.Models;
using System;


namespace Loom.Engine.Context
{

    /// <summary>
    /// Transforms the message into a summary paragraph, extracts key data, and updates the MemoryContext based on the MemoryMode
    /// </summary>
    public class MemoryManager
    {

        private readonly LlmInvocation _invocation;

        public MemoryManager(LlmInvocation invocation)
        {
            _invocation = invocation;
        }

        /// <summary>
        /// Synchronize memory contents according to the defined strategy
        /// </summary>
        /// <param name="newData">Optional: New facts extracted externally</param>
        public void UpdateMemory(string newData = "")
        {
            MemoryMode flag = _invocation.Memory.Mode;

            switch (flag)
            {
                case MemoryMode.None:
                    _invocation.Memory.Content = "";
                    break;

                case MemoryMode.FullHistory:
                    _invocation.Memory.Content += Environment.NewLine + newData;
                    break;

                case MemoryMode.Summary:
                    _invocation.Memory.Content = Environment.NewLine + newData;
                    break;

                case MemoryMode.Extractive:
                    throw new NotImplementedException("Extractive memory to implements");

                case MemoryMode.Hybrid:
                    throw new NotImplementedException("Hybrid memory to implements");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="timestampLastUpdate"></param>
        /// <returns></returns>
        public bool IsMemoryExpired(DateTime timestampLastUpdate)
        {

            if (!_invocation.Memory.TTL.HasValue) return false;

            return (DateTime.Now - timestampLastUpdate) > _invocation.Memory.TTL.Value;

        }



    }
}
