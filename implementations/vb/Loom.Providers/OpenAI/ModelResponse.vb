Namespace OpenAI
    Public Class ModelResponse
        Public Property Id As String
        Public Property [Object] As String
        Public Property Created_at As Long
        Public Property Status As String
        Public Property [Error] As Object
        Public Property Incomplete_details As Object
        Public Property Instructions As String
        Public Property Max_output_tokens As Object
        Public Property Model As String
        Public Property Output As List(Of OutputItem)
        Public Property Parallel_tool_calls As Boolean
        Public Property Previous_response_id As String
        Public Property Reasoning As Reasoning
        Public Property Store As Boolean
        Public Property Temperature As Double
        Public Property Text As TextConfig
        Public Property Tool_choice As String
        Public Property Tools As List(Of Tool)
        Public Property Top_p As Double
        Public Property Truncation As String
        Public Property Usage As Usage
        Public Property User As Object
        Public Property Metadata As Dictionary(Of String, Object)
    End Class
    Public Class OutputItem
        Public Property Id As String
        Public Property Type As String
        Public Property Status As String
        Public Property Arguments As String
        Public Property Call_id As String
        Public Property Name As String
        Public Property Queries As List(Of String)
        Public Property Results As Object
        Public Property Role As String
        Public Property Content As List(Of MessageContent)
    End Class
    Public Class MessageContent
        Public Property Type As String
        Public Property Text As String
        Public Property Annotations As List(Of Annotation)
    End Class
    Public Class Annotation
        Public Property Type As String
        Public Property Index As Integer
        Public Property File_id As String
        Public Property Filename As String
    End Class
    Public Class Reasoning
        Public Property Effort As Object
        Public Property Summary As Object
    End Class
    Public Class TextConfig
        Public Property Format As FormatConfig
        Public Property Verbosity As String
    End Class
    Public Class FormatConfig
        Public Property Type As String
    End Class
    Public Class Tool
        Public Property Type As String
        Public Property Description As String
        Public Property Name As String
        Public Property Parameters As Dictionary(Of String, Object)
        Public Property Strict As Boolean
        Public Property Filters As Object
        Public Property Max_num_results As Integer?
        Public Property Ranking_options As Dictionary(Of String, Object)
        Public Property Vector_store_ids As List(Of String)
    End Class
    Public Class Usage
        Public Property Input_tokens As Integer
        Public Property Input_tokens_details As InputTokensDetails
        Public Property Output_tokens As Integer
        Public Property Output_tokens_details As OutputTokensDetails
        Public Property Total_tokens As Integer
    End Class
    Public Class InputTokensDetails
        Public Property Cached_tokens As Integer
    End Class
    Public Class OutputTokensDetails
        Public Property Reasoning_tokens As Integer
    End Class
End Namespace

