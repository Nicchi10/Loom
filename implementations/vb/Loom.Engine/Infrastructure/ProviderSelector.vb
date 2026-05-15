'Imports Loom.Core.Enums
'Imports Loom.Core.Interfaces
'Imports Loom.Core.Models

'Namespace Infrastructure
'    Public Class ProviderSelector

'        ' Providers list added
'        Private ReadOnly _adapters As New List(Of IProviderAdapter)

'        ''' <summary>
'        ''' Register an avaiable adapter
'        ''' </summary>
'        ''' <param name="adapter"></param>
'        Public Sub RegisterProvider(adapter As IProviderAdapter)
'            If Not _adapters.Any(Function(a) a.ProviderName = adapter.ProviderName) Then
'                _adapters.Add(adapter)
'            End If
'        End Sub

'        ''' <summary>
'        ''' Select best provider avaiable basing provided hints
'        ''' </summary>
'        ''' <param name="hints"></param>
'        ''' <returns> Best avaiable provider </returns>
'        Public Function GetBestProvider(hints As ExecutionHints) As IProviderAdapter

'            ' No registered adapters
'            If Not _adapters.Any() Then
'                Throw New InvalidOperationException("No registered providers")
'            End If

'            ' Explict favorite adapter
'            If Not String.IsNullOrWhiteSpace(hints.PreferredModel) Then
'                Dim preferred As IProviderAdapter = Nothing
'                For Each a In _adapters
'                    If a.ProviderName.IndexOf(hints.PreferredModel, StringComparison.OrdinalIgnoreCase) >= 0 Then
'                        preferred = a
'                        Exit For
'                    End If
'                Next
'                If preferred IsNot Nothing Then Return preferred
'            End If

'            ' Priority optimization 
'            Dim bestAdapter As IProviderAdapter = Nothing
'            Dim bestScore As Integer = Integer.MinValue

'            For Each a In _adapters
'                Dim score As Integer

'                Select Case hints.Priority
'                    Case ExecutionPriority.QualityOptimized
'                        If a.ProviderName.Contains("Pro") Then
'                            score = 10
'                        Else
'                            score = 5
'                        End If
'                    Case ExecutionPriority.LatencyOptimized, ExecutionPriority.CostOptimized
'                        If a.ProviderName.Contains("Flash") Or a.ProviderName.Contains("Mini") Then
'                            score = 10
'                        Else
'                            score = 5
'                        End If
'                    Case Else
'                        score = 1
'                End Select

'                If score > bestScore Then
'                    bestScore = score
'                    bestAdapter = a
'                End If
'            Next
'            Return bestAdapter
'        End Function

'    End Class
'End Namespace

