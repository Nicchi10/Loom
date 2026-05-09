Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json

Namespace Infrastructure
    Public Class ProviderHttpClient

        Private Shared ReadOnly _httpClient As New HttpClient()

        Public Async Function PostAsync(Of TResponse)(headers As Dictionary(Of String, String), url As String, payload As Dictionary(Of String, Object)) As Task(Of TResponse)

            _httpClient.DefaultRequestHeaders.Clear()
            For Each h In headers
                _httpClient.DefaultRequestHeaders.Add(h.Key, h.Value)
            Next
            Dim serializerSettings As New JsonSerializerSettings With {
                .NullValueHandling = NullValueHandling.Ignore
            }
            Dim jsonBody As String = JsonConvert.SerializeObject(payload, serializerSettings)
            Dim content As New StringContent(jsonBody, Encoding.UTF8, "application/json")

            Try
                Dim response As HttpResponseMessage = Await _httpClient.PostAsync(url, content)

                Dim responseBody As String = Await response.Content.ReadAsStringAsync()
                If Not response.IsSuccessStatusCode Then
                    Throw New Exception($"OpenAI error {response.StatusCode}: {responseBody}")
                End If


                Dim modelResponse As TResponse = JsonConvert.DeserializeObject(Of TResponse)(responseBody)
                Return modelResponse
            Catch ex As Exception
                Throw New Exception($"[ERROR] while network calling to {url}: {ex}")
                Return Nothing
            End Try

        End Function


    End Class
End Namespace

