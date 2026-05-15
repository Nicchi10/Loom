Imports Loom.Core.Models
Imports Loom.Providers.Core

Namespace GoogleAI
    ''' <summary>
    ''' Reconstructs raw data into the specific GoogleAI format
    ''' </summary>
    Public Class GoogleAIToolSchemaBuilder
        Public Function Build(def As ToolDefinition) As GoogleFunctionDeclaration
            Dim props As New Dictionary(Of String, ProviderPropertySchema)()
            If def.Parameters IsNot Nothing AndAlso def.Parameters.Count > 0 Then
                For Each p In def.Parameters
                    Dim propSchema As New ProviderPropertySchema()
                    propSchema.type = If(String.IsNullOrEmpty(p.Type), "string", p.Type)
                    propSchema.description = If(p.Description, "")
                    If p.Enums IsNot Nothing AndAlso p.Enums.Count > 0 Then
                        propSchema.[enum] = p.Enums
                    End If
                    props.Add(p.Name, propSchema)
                Next
            End If

            Return New GoogleFunctionDeclaration With {
                .name = def.ToolName,
                .description = def.Description,
                .parameters = New GoogleFunctionSchema With {
                    .type = "object",
                    .properties = props,
                    .required = If(def.Required, New List(Of String)())
                }
            }
        End Function
    End Class

    Public Class GoogleFunctionDeclaration
        Public Property name As String
        Public Property description As String
        Public Property parameters As GoogleFunctionSchema
    End Class

    Public Class GoogleFunctionSchema
        Public Property type As String = "object"
        Public Property properties As Dictionary(Of String, ProviderPropertySchema)
        Public Property required As List(Of String)
    End Class
End Namespace
