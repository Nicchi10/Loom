Namespace Models
    ''' <summary>
    ''' Memory state of single session
    ''' </summary>
    Public Class MemoryContext
        Public Property Mode As Enums.MemoryMode = Enums.MemoryMode.FullHistory
        Public Property Content As String
        Public Property TTL As TimeSpan? ' How long memory must persist
    End Class
End Namespace