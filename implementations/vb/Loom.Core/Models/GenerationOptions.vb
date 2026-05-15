Namespace Models
    ''' <summary>
    ''' Technical parameters
    ''' </summary>
    Public Class GenerationOptions
        Public Property Temperature As Single?
        Public Property TopK As Single? ' token filter
        Public Property MaxTokens As Integer? ' token output limits
        Public Property StopSequences As List(Of String)
        Public Property JsonMode As Boolean = False

    End Class
End Namespace