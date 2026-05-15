Imports Loom.Core.Models
Imports Loom.Providers.Core

Namespace OpenAI
    ''' <summary>
    ''' Reconstructs raw data into the specific OpenAI format
    ''' </summary>
    Public Class OpenAIToolSchemaBuilder

        Public Function Build(def As ToolDefinition) As ProviderToolSchema
            ' Check parameters and required != Nothing
            Dim props As New Dictionary(Of String, ProviderPropertySchema)()

            If def.Parameters IsNot Nothing AndAlso def.Parameters.Count > 0 Then
                For Each p In def.Parameters
                    Dim propSchema As New ProviderPropertySchema()
                    propSchema.type = If(String.IsNullOrEmpty(p.Type), "string", p.Type)
                    propSchema.description = If(p.Description, "")

                    ' Enum added only if != Nothing
                    If p.Enums IsNot Nothing AndAlso p.Enums.Count > 0 Then
                        propSchema.[enum] = p.Enums
                    End If

                    props.Add(p.Name, propSchema)
                Next
            End If

            Dim requiredList As List(Of String) = If(def.Required IsNot Nothing, def.Required, New List(Of String)())

            Dim functionSchema As New ProviderFunctionSchema With {
                .type = "object",
                .properties = props,
                .required = requiredList,
                .additionalProperties = False
            }

            Return New ProviderToolSchema With {
                .name = def.ToolName,
                .description = def.Description,
                .parameters = functionSchema,
                .strict = def.Strictness.GetValueOrDefault(False)
            }
        End Function



    End Class

End Namespace