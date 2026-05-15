Namespace Models
    ''' <summary>
    ''' Select the best model for a provider
    ''' </summary>
    Public Class ModelMetaData
        Public Property ModelId As String
        Public Property ProviderName As String
        Public Property ContextWindow As Integer
        Public Property SupportsTools As Boolean
        Public Property CostLevel As Integer ' 1 (cheap) -> 5 (expensive)
        Public Property Capabilities As List(Of String) ' ('vision', 'audio'...)
    End Class
End Namespace