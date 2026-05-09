Namespace Google

    ' Root response
    Public Class GenerateContentResponse
        Public Property Candidates As List(Of Candidate)
        Public Property PromptFeedback As PromptFeedback
        Public Property UsageMetadata As UsageMetadata
        Public Property ModelVersion As String
        Public Property ResponseId As String
    End Class

    ' --------------------------------------------------
    ' CANDIDATE
    ' --------------------------------------------------
    Public Class Candidate
        Public Property Content As Content
        Public Property FinishReason As String
        Public Property SafetyRatings As List(Of SafetyRating)
        Public Property CitationMetadata As CitationMetadata
        Public Property TokenCount As Integer
        Public Property GroundingAttributions As List(Of GroundingAttribution)
        Public Property AvgLogprobs As Double
        Public Property LogprobsResult As LogprobsResult
        Public Property Index As Integer
    End Class

    ' --------------------------------------------------
    ' CONTENT / PART
    ' --------------------------------------------------
    Public Class Content
        Public Property Role As String                         ' "user" | "model"
        Public Property Parts As List(Of Part)
    End Class

    Public Class Part
        Public Property Text As String
        Public Property InlineData As Blob
        Public Property FileData As FileData
        Public Property FunctionCall As FunctionCall
        Public Property FunctionResponse As FunctionResponse
        Public Property ExecutableCode As ExecutableCode
        Public Property CodeExecutionResult As CodeExecutionResult
        Public Property Thought As Boolean?
        Public Property ThoughtSignature As String
    End Class

    Public Class Blob
        Public Property MimeType As String
        Public Property Data As String                         ' base64
    End Class

    Public Class FileData
        Public Property MimeType As String
        Public Property FileUri As String
    End Class

    Public Class FunctionCall
        Public Property Name As String
        Public Property Args As Dictionary(Of String, Object)
    End Class

    Public Class FunctionResponse
        Public Property Name As String
        Public Property Response As Dictionary(Of String, Object)
    End Class

    Public Class ExecutableCode
        Public Property Language As String                     ' "PYTHON"
        Public Property Code As String
    End Class

    Public Class CodeExecutionResult
        Public Property Outcome As String
        Public Property Output As String
    End Class

    ' --------------------------------------------------
    ' SAFETY
    ' --------------------------------------------------
    Public Class SafetyRating
        Public Property Category As String
        Public Property Probability As String
        ' NEGLIGIBLE, LOW, MEDIUM, HIGH
        Public Property ProbabilityScore As Single
        Public Property Severity As String
        Public Property SeverityScore As Single
        Public Property Blocked As Boolean
    End Class

    Public Class PromptFeedback
        Public Property BlockReason As String
        ' SAFETY, OTHER, BLOCKLIST, PROHIBITED_CONTENT
        Public Property SafetyRatings As List(Of SafetyRating)
    End Class

    ' --------------------------------------------------
    ' CITATIONS
    ' --------------------------------------------------
    Public Class CitationMetadata
        Public Property Citations As List(Of Citation)
    End Class

    Public Class Citation
        Public Property StartIndex As Integer
        Public Property EndIndex As Integer
        Public Property Uri As String
        Public Property Title As String
        Public Property License As String
        Public Property PublicationDate As PublicationDate
    End Class

    Public Class PublicationDate
        Public Property Year As Integer
        Public Property Month As Integer
        Public Property Day As Integer
    End Class

    ' --------------------------------------------------
    ' GROUNDING (Google Search grounding)
    ' --------------------------------------------------
    Public Class GroundingAttribution
        Public Property SourceId As AttributionSourceId
        Public Property Content As Content
    End Class

    Public Class AttributionSourceId
        Public Property GroundingPassage As GroundingPassageId
        Public Property SemanticRetrieverChunk As SemanticRetrieverChunk
    End Class

    Public Class GroundingPassageId
        Public Property PassageId As String
        Public Property PartIndex As Integer
    End Class

    Public Class SemanticRetrieverChunk
        Public Property Source As String
        Public Property Chunk As String
    End Class

    ' --------------------------------------------------
    ' LOGPROBS
    ' --------------------------------------------------
    Public Class LogprobsResult
        Public Property TopCandidates As List(Of TopCandidates)
        Public Property ChosenCandidates As List(Of LogprobsCandidate)
    End Class

    Public Class TopCandidates
        Public Property Candidates As List(Of LogprobsCandidate)
    End Class

    Public Class LogprobsCandidate
        Public Property Token As String
        Public Property TokenId As Integer
        Public Property LogProbability As Single
    End Class

    ' --------------------------------------------------
    ' USAGE
    ' --------------------------------------------------
    Public Class UsageMetadata
        Public Property PromptTokenCount As Integer
        Public Property CachedContentTokenCount As Integer
        Public Property CandidatesTokenCount As Integer
        Public Property ToolUsePromptTokenCount As Integer
        Public Property ThoughtsTokenCount As Integer
        Public Property TotalTokenCount As Integer
        Public Property PromptTokensDetails As List(Of ModalityTokenCount)
        Public Property CacheTokensDetails As List(Of ModalityTokenCount)
        Public Property CandidatesTokensDetails As List(Of ModalityTokenCount)
        Public Property ToolUsePromptTokensDetails As List(Of ModalityTokenCount)
    End Class

    Public Class ModalityTokenCount
        Public Property Modality As String                     ' "TEXT", "IMAGE", "AUDIO", "VIDEO"
        Public Property TokenCount As Integer
    End Class

End Namespace
