Namespace Interfaces
    ''' <summary>
    ''' Represents the formal outcome of the invocation
    ''' </summary>

    Public Interface IValidationResult
        ''' <summary>
        ''' Indicates if the invocation is ready to be sent
        ''' </summary>
        ReadOnly Property IsValid As Boolean

        ''' <summary>
        ''' Error message list
        ''' </summary>
        ReadOnly Property Errors As IEnumerable(Of String)
    End Interface
End Namespace


