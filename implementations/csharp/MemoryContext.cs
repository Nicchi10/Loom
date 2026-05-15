using System;

namespace Models
{
    /// <summary>
    /// Memory state of single session
    /// </summary>
    public class MemoryContext
    {
        public Enums.MemoryMode Mode { get; set; } = Enums.MemoryMode.FullHistory;
        public string Content { get; set; }
        public TimeSpan? TTL { get; set; } // How long memory must persist
    }
}