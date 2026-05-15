Imports Loom.Core.Interfaces

Namespace Models
    ''' <summary>
    ''' Implements IValidationResult
    ''' </summary>
    Public Class ValidationResult
        Implements Interfaces.IValidationResult

        Private ReadOnly _isValid As Boolean
        Private ReadOnly _errors As New List(Of String)

        Public ReadOnly Property IsValid As Boolean Implements IValidationResult.IsValid
            Get
                Return _isValid
            End Get
        End Property

        Public ReadOnly Property Errors As IEnumerable(Of String) Implements IValidationResult.Errors
            Get
                Return _errors
            End Get
        End Property

        ''' <summary>
        ''' Validator constructor
        ''' </summary>
        ''' <param name="isValid"> Boolean </param>
        ''' <param name="errors"> List of String </param>
        Public Sub New(isValid As Boolean, Optional errors As List(Of String) = Nothing)
            _isValid = isValid
            If errors IsNot Nothing Then _errors.AddRange(errors)
        End Sub

        ''' <summary>
        ''' Success
        ''' </summary>
        ''' <returns> No error (True) </returns>
        Public Shared Function Success() As ValidationResult
            Return New ValidationResult(True)
        End Function

        ''' <summary>
        ''' Failure
        ''' </summary>
        ''' <param name="errors"> List of String</param>
        ''' <returns> False + Errors </returns>
        Public Shared Function Failure(ParamArray errors As String()) As ValidationResult
            Return New ValidationResult(False, errors.ToList())
        End Function

    End Class
End Namespace