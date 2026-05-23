using System.Collections.Generic;

namespace Loom.Providers.OpenAI
{
    public class ModelResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created_at { get; set; }
        public string Status { get; set; }
        public object Error { get; set; }
        public object Incomplete_details { get; set; }
        public string Instructions { get; set; }
        public object Max_output_tokens { get; set; }
        public string Model { get; set; }
        public List<OutputItem> Output { get; set; }
        public bool Parallel_tool_calls { get; set; }
        public string Previous_response_id { get; set; }
        public Reasoning Reasoning { get; set; }
        public bool Store { get; set; }
        public double Temperature { get; set; }
        public TextConfig Text { get; set; }
        public string Tool_choice { get; set; }
        public List<Tool> Tools { get; set; }
        public double Top_p { get; set; }
        public string Truncation { get; set; }
        public Usage Usage { get; set; }
        public object User { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class OutputItem
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Arguments { get; set; }
        public string Call_id { get; set; }
        public string Name { get; set; }
        public List<string> Queries { get; set; }
        public object Results { get; set; }
        public string Role { get; set; }
        public List<MessageContent> Content { get; set; }
    }

    public class MessageContent
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public List<Annotation> Annotations { get; set; }
    }

    public class Annotation
    {
        public string Type { get; set; }
        public int Index { get; set; }
        public string File_id { get; set; }
        public string Filename { get; set; }
    }

    public class Reasoning
    {
        public object Effort { get; set; }
        public object Summary { get; set; }
    }

    public class TextConfig
    {
        public FormatConfig Format { get; set; }
        public string Verbosity { get; set; }
    }

    public class FormatConfig
    {
        public string Type { get; set; }
    }

    public class Tool
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool Strict { get; set; }
        public object Filters { get; set; }
        public int? Max_num_results { get; set; }
        public Dictionary<string, object> Ranking_options { get; set; }
        public List<string> Vector_store_ids { get; set; }
    }

    public class Usage
    {
        public int Input_tokens { get; set; }
        public InputTokensDetails Input_tokens_details { get; set; }
        public int Output_tokens { get; set; }
        public OutputTokensDetails Output_tokens_details { get; set; }
        public int Total_tokens { get; set; }
    }

    public class InputTokensDetails
    {
        public int Cached_tokens { get; set; }
    }

    public class OutputTokensDetails
    {
        public int Reasoning_tokens { get; set; }
    }
}
