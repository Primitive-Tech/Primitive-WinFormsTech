Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Tab
Imports Primitive_Server, System.IO, NLua, WikiDotNet
'################################################ Scripting/VBL '################################################ Scripting/VBL '################################################
Public Class Script
    Inherits VBLib
    Public Engine As Lua '<= Standart_User
    Dim Form As Form
    Protected server As Server '<= Standart_WebConnection Implemention(HTTP/FTP)
    Dim Script As Primitive_Server.Script '<= Standart_Script Implemention
    Public DataPath As String = ""
    Public Path As String = ""
    Public User As User '<= Standart_User

    Public Name, Method As String
    Public ScriptArgs As (String, Object)() = {("method", Method)}
    Public Args As String()
    Public Output As Object() = {}
    Public StatusResponse As Boolean
    Public CodeSource As String()
    Public Code As String
    Public MyNamespaces As String() = {"VBLib", "System.IO", "Newtonsoft.Json"}
    Sub New()
    End Sub
    Sub New(Code As String, useOwnSyntax As Boolean)
        Me.Name = Name
        Me.Code = Code
        Method = "GET"
        DataPath = Name & ".vbl"
        _UseOwnSyntax = useOwnSyntax
    End Sub
    '###################################### Initializer ScriptClass ##################################################
    Sub New(CodeSource As String(), Optional useOwnSyntax As Boolean = False, Optional name As String = "NewScript")
        Me.Name = name
        Me.CodeSource = CodeSource
        Method = "GET"
        DataPath = name & ".vbl"
        _UseOwnSyntax = useOwnSyntax
    End Sub
    '<= ------------- Konstruktor:SchmandSQL_Request
    Sub New(name As String, args As String(), Optional useOwnSyntax As Boolean = False, Optional method As String = "GET")
        Me.Name = name
        Me.Args = args
        DataPath = name & ".vbl"
        Path = name + ".vbl"
        Me.CodeSource = File.ReadAllLines(DataPath)
        Me.Code = File.ReadAllText(DataPath)
        Me.Method = method
        _UseOwnSyntax = useOwnSyntax
    End Sub
    '<= ------------- Konstruktor:FromString
    Sub New(name As String, method As String, Optional useOwnSyntax As Boolean = False)
        Me.Name = name
        DataPath = name & ".vbl"
        Me.CodeSource = File.ReadAllLines(DataPath)
        Me.Code = File.ReadAllText(DataPath)
        Me.Method = method
        _UseOwnSyntax = useOwnSyntax
    End Sub
    '<= ------------- Konstruktor:FromString(Local) 
    Sub New(name As String, Optional path As String = "data/", Optional useOwnSyntax As Boolean = False, Optional ID As String = "Anonymous")
        Me.Name = name
        DataPath = path & name & ".vbl"
        Me.CodeSource = File.ReadAllLines(DataPath)
        Me.Code = File.ReadAllText(DataPath)
        Me.Method = "GET"
        _UseOwnSyntax = useOwnSyntax
    End Sub
    '----------------------------------------------------------------------------------------------------
    Dim _UseOwnSyntax As Boolean = False
    Public Property UseOwnSyntax As Boolean
        Get
            Return _UseOwnSyntax
        End Get
        Set(value As Boolean)
            _UseOwnSyntax = value
        End Set
    End Property
    Function Start(Optional useOwnSyntax As Boolean = False, Optional localFile As Boolean = False) As Object()
        Return Me.Execute(useOwnSyntax, localFile)
    End Function
End Class

'################################ SyntaxConfiguration & Debug ######################################################
Public Class Syntax
    Sub New()
        Me.Syntaxfilepath = "Syntax.dict"
    End Sub
    Public oldCode, infos As String
    Dim Body = {}
    Public Syntaxfilepath As String
    Public Sub Syntaxing(ByRef script As Script)
        Dim codeToTranslate = script.CodeSource
        If codeToTranslate.Any Then
            For Each pair As KeyValuePair(Of String, String) In SyntaxDict
                For Each line In codeToTranslate
                    line.Replace(pair.Value, pair.Key)
                Next
            Next
            script.CodeSource = codeToTranslate
        End If
    End Sub
    Public Sub Schleife()
        Throw New NotImplementedException()
    End Sub
    ReadOnly Property SyntaxDict As Dictionary(Of String, String)
        Get
            Return File.ReadAllLines(Syntaxfilepath).ToPairs()
        End Get
    End Property
End Class