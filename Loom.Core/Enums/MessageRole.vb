Namespace Enums
    ''' <summary>
    ''' Defines entity role that have generated the message (System, User, Assistant, Tool)
    ''' </summary>
    Public Enum MessageRole
        ''' <summary>
        ''' Base instructions for the model
        ''' </summary>
        System = 0

        ''' <summary>
        ''' Explicit input provide by user
        ''' </summary>
        User = 1

        ''' <summary>
        ''' LLM response
        ''' </summary>
        Assistant = 2

        ''' <summary>
        ''' Function/tool output called by the model
        ''' </summary>
        Tool = 3
    End Enum
End Namespace


