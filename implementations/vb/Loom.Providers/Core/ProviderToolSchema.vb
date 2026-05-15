Namespace Core
    ''' <summary>
    ''' Schema
    ''' </summary>
    Public Class ProviderToolSchema
        Public Property type As String = "function"
        Public Property name As String
        Public Property description As String
        Public Property parameters As ProviderFunctionSchema
        Public Property strict As Boolean?
    End Class
    ''' <summary>
    ''' Parameters
    ''' </summary>
    Public Class ProviderFunctionSchema
        Public Property type As String = "object"
        Public Property properties As Dictionary(Of String, ProviderPropertySchema)
        Public Property required As List(Of String)
        Public Property additionalProperties As Boolean = False
    End Class
    ''' <summary>
    ''' Property
    ''' </summary>
    Public Class ProviderPropertySchema
        Public Property type As String
        Public Property [enum] As List(Of String)
        Public Property description As String

    End Class

End Namespace
