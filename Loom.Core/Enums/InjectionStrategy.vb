
Namespace Enums
    ''' <summary>
    ''' Defines the mode how context is injected into the final prompt context of RAG
    ''' </summary>
    Public Enum InjectionStrategy
        ''' <summary>
        ''' Creates marked and split section (e.g. ### CONTEXT ###)
        ''' </summary>
        Sectioned = 0

        ''' <summary>
        ''' Provides XML structure section
        ''' </summary>
        SystemXML = 1

        ''' <summary>
        ''' Provides JSON structure section
        ''' </summary>
        SystemJSON = 2
    End Enum
End Namespace