namespace Models
{
    /// <summary>
    /// User preferences
    /// </summary>
    public class ExecutionHints
    {
        public string PreferredModel {  get; set; }
        public Enums.ExecutionPriority Priority { get; set; } = Enums.ExecutionPriority.Balanced;
        public int MaxRetryCount { get; set; } = 3;
    }
}
