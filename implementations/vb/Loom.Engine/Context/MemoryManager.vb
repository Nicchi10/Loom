Imports Loom.Core.Enums
Imports Loom.Core.Models

Namespace Context
    ''' <summary>
    ''' Transforms the message into a summary paragraph, extracts key data, and updates the MemoryContext based on the MemoryMode
    ''' </summary>
    Public Class MemoryManager

        Private ReadOnly _invocation As LlmInvocation

        Public Sub New(invocation As LlmInvocation)
            _invocation = invocation
        End Sub

        ''' <summary>
        ''' Synchronize memory contents according to the defined strategy
        ''' </summary>
        ''' <param name="newData"> Optional: new facts extracted externally </param>
        Public Sub UpdateMemory(Optional newData As String = "")

            Dim flag As Integer = _invocation.Memory.Mode
            ' Dovremo anche gestire la parte del ConversationManager, per ora salva lo storico della chat...
            If flag = MemoryMode.None Then
                _invocation.Memory.Content = String.Empty
            ElseIf flag = MemoryMode.FullHistory Then
                _invocation.Memory.Content &= Environment.NewLine & newData
            ElseIf flag = MemoryMode.Summary Then
                _invocation.Memory.Content = Environment.NewLine & newData
            ElseIf flag = MemoryMode.Extractive Then
                Throw New NotImplementedException("Extractive memory to implement")
            ElseIf flag = MemoryMode.Hybrid Then
                Throw New NotImplementedException("Hybrid memory to implement")
            End If

        End Sub

        ''' <summary>
        ''' Check if the memory is expired
        ''' </summary>
        ''' <param name="timestampLastUpdate"> Last update </param>
        ''' <returns></returns>
        Public Function IsMemoryExpired(timestampLastUpdate As DateTime) As Boolean

            If Not _invocation.Memory.TTL.HasValue Then Return False

            Return (DateTime.Now - timestampLastUpdate) > _invocation.Memory.TTL.Value
        End Function

    End Class
End Namespace

