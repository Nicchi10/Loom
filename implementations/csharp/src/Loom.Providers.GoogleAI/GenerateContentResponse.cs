using System.Collections.Generic;

namespace Loom.Providers.GoogleAI
{
    /// <summary>
    /// Root response
    /// </summary>
    public class GenerateContentResponse
    {
        public List<Candidate> Candidates { get; set; }
        public PromptFeedback PromptFeedback { get; set; }
        public UsageMetadata UsageMetadata { get; set; }
        public string ModelVersion { get; set; }
        public string ResponseId { get; set; }
    }

    // --------------------------------------------------
    // CANDIDATE
    // --------------------------------------------------
    public class Candidate
    {
        public Content Content { get; set; }
        public string FinishReason { get; set; }
        public List<SafetyRating> SafetyRatings { get; set; }
        public CitationMetadata CitationMetadata { get; set; }
        public int TokenCount { get; set; }
        public List<GroundingAttribution> GroundingAttributions { get; set; }
        public double AvgLogprobs { get; set; }
        public LogprobsResult LogprobsResult { get; set; }
        public int Index { get; set; }
    }

    // --------------------------------------------------
    // CONTENT / PART
    // --------------------------------------------------
    public class Content
    {
        public string Role { get; set; } // "user" | "model"
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
        public Blob InlineData { get; set; }
        public FileData FileData { get; set; }
        public FunctionCall FunctionCall { get; set; }
        public FunctionResponse FunctionResponse { get; set; }
        public ExecutableCode ExecutableCode { get; set; }
        public CodeExecutionResult CodeExecutionResult { get; set; }
        public bool? Thought { get; set; }
        public string ThoughtSignature { get; set; }
    }

    public class Blob
    {
        public string MimeType { get; set; }
        public string Data { get; set; } // base64
    }

    public class FileData
    {
        public string MimeType { get; set; }
        public string FileUri { get; set; }
    }

    public class FunctionCall
    {
        public string Name { get; set; }
        public Dictionary<string, object> Args { get; set; }
    }

    public class FunctionResponse
    {
        public string Name { get; set; }
        public Dictionary<string, object> Response { get; set; }
    }

    public class ExecutableCode
    {
        public string Language { get; set; } // "PYTHON"
        public string Code { get; set; }
    }

    public class CodeExecutionResult
    {
        public string Outcome { get; set; }
        public string Output { get; set; }
    }

    // --------------------------------------------------
    // SAFETY
    // --------------------------------------------------
    public class SafetyRating
    {
        public string Category { get; set; }
        public string Probability { get; set; } // NEGLIGIBLE, LOW, MEDIUM, HIGH
        public float ProbabilityScore { get; set; }
        public string Severity { get; set; }
        public float SeverityScore { get; set; }
        public bool Blocked { get; set; }
    }

    public class PromptFeedback
    {
        public string BlockReason { get; set; } // SAFETY, OTHER, BLOCKLIST, PROHIBITED_CONTENT
        public List<SafetyRating> SafetyRatings { get; set; }
    }

    // --------------------------------------------------
    // CITATIONS
    // --------------------------------------------------
    public class CitationMetadata
    {
        public List<Citation> Citations { get; set; }
    }

    public class Citation
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Uri { get; set; }
        public string Title { get; set; }
        public string License { get; set; }
        public PublicationDate PublicationDate { get; set; }
    }

    public class PublicationDate
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    // --------------------------------------------------
    // GROUNDING (Google Search grounding)
    // --------------------------------------------------
    public class GroundingAttribution
    {
        public AttributionSourceId SourceId { get; set; }
        public Content Content { get; set; }
    }

    public class AttributionSourceId
    {
        public GroundingPassageId GroundingPassage { get; set; }
        public SemanticRetrieverChunk SemanticRetrieverChunk { get; set; }
    }

    public class GroundingPassageId
    {
        public string PassageId { get; set; }
        public int PartIndex { get; set; }
    }

    public class SemanticRetrieverChunk
    {
        public string Source { get; set; }
        public string Chunk { get; set; }
    }

    // --------------------------------------------------
    // LOGPROBS
    // --------------------------------------------------
    public class LogprobsResult
    {
        public List<TopCandidates> TopCandidates { get; set; }
        public List<LogprobsCandidate> ChosenCandidates { get; set; }
    }

    public class TopCandidates
    {
        public List<LogprobsCandidate> Candidates { get; set; }
    }

    public class LogprobsCandidate
    {
        public string Token { get; set; }
        public int TokenId { get; set; }
        public float LogProbability { get; set; }
    }

    // --------------------------------------------------
    // USAGE
    // --------------------------------------------------
    public class UsageMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CachedContentTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int ToolUsePromptTokenCount { get; set; }
        public int ThoughtsTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
        public List<ModalityTokenCount> PromptTokensDetails { get; set; }
        public List<ModalityTokenCount> CacheTokensDetails { get; set; }
        public List<ModalityTokenCount> CandidatesTokensDetails { get; set; }
        public List<ModalityTokenCount> ToolUsePromptTokensDetails { get; set; }
    }

    public class ModalityTokenCount
    {
        public string Modality { get; set; } // "TEXT", "IMAGE", "AUDIO", "VIDEO"
        public int TokenCount { get; set; }
    }
}
