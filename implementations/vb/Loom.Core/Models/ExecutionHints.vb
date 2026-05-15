Namespace Models
    ''' <summary>
    ''' User preferences
    ''' </summary>
    Public Class ExecutionHints
        Public Property PreferredModel As String
        Public Property Priority As Enums.ExecutionPriority = Enums.ExecutionPriority.Balanced
        Public Property MaxRetryCount As Integer = 3 ' error scenario

    End Class
End Namespace