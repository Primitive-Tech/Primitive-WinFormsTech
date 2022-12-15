Imports System.Runtime.CompilerServices, Newtonsoft.Json, System.IO, System.Linq, System.Speech.Synthesis, System.Diagnostics, System.ComponentModel
Imports System.Drawing, NLua, Primitive_Server, System.Windows.Forms
Imports VB_Extension.Script, VB_Extension, BasicAudio.AudioPlayer, BasicAudio, WikiDotNet
Imports System.IO.Compression
Public Class VBLib
    Public int As Integer = 12
    Public Sub LoadAllAsync(ByRef controls As Control(), func As Func(Of Object, Boolean))

        Dim UIElements = From ctr As PictureBox In controls
                         Where ctr.GetType = New PictureBox().GetType AndAlso func(ctr)
                         Select ctr
        Dim imageList As New List(Of Image)
        '<= ----------- Load Images -------------
        For Each element In UIElements
            Dim newResc As Image
            Try
                newResc = Image.FromFile(element.Tag.ToString)
                imageList.Add(newResc)
            Catch
            End Try
        Next
        ' <= --------- Apply_Changes ------------
        For i As Integer = 0 To UIElements.Count
            UIElements(i).SizeMode = PictureBoxSizeMode.StretchImage
            UIElements(i).Image = imageList(i)
        Next
    End Sub
End Class
'////////////////////// Extensions /////////////////////////////// Extensions //////////////////////////////// Extensions //////////////////////
Public Module Fnc
    '########################################## Custum Converters #########################################################
    <Extension()> '<= ------- NoneType_Array to String_Array
    Public Function ToText(array As Object()) As String()
        Return CType(array, String())
    End Function
    <Extension()> '<= Table_Object(INI File_Style)
    Public Function ToDataframe(ByRef CTR As List(Of Control), inputData As Object()) As Dictionary(Of Object, Object)
        Dim dataset = From data In CTR
                      Let groups As KeyValuePair(Of Control, Object()) =
                          KeyValuePair.Create(data, inputData.Intersect(data.Tag.GetType).ToArray)
                      Select groups
        Dim Table As Dictionary(Of Object, Object) = dataset
        Return Table
    End Function
    <Extension()> '<= Table_Object(INI File_Style)
    Public Function ToTable(ByRef inputStr As String(), Optional chunkBorder As String = vbCrLf) As Dictionary(Of String, String)
        Dim dataset = From data In inputStr
                      Let code As KeyValuePair(Of String, String) = KeyValuePair.Create(data.Split("=")(0), data.Split("=")(1))
                      Select code
        Dim Table As Dictionary(Of String, String) = dataset
        Return Table
    End Function
    <Extension()> '<= Dictionary_Object(INI File_Style)
    Public Function ToPairs(ByRef inputStr As String(), Optional chunkBorder As String = vbCrLf) As Dictionary(Of String, String)
        Dim dataset = From line In inputStr
                      Where line.Any() AndAlso line <> "" AndAlso line.Contains("#") = False
                      Let pair As KeyValuePair(Of String, String) = KeyValuePair.Create(line.Split("=")(0), line.Split("=")(1))
                      Select pair
        Dim Pairs As Dictionary(Of String, String) = dataset
        Return Pairs
    End Function
    '################################################ Scripting #######################################################
    Dim vblEngine As Lua
    Public vbl As Lua
    Dim syntax As New Syntax()
    '--------------------------- Do_Script ---------------------------------
    <Extension()>
    Public Function Execute(ByRef script As Script, Optional useOwnSyntax As Boolean = False, Optional localFile As Boolean = False) As Object()
        Dim output As Object() = {False, "ScriptObject not found°"}
        Dim statusResponse As Boolean
        If script.Check Then
            vblEngine = New Lua()
            ' <= ------------ Preloaders --------------- =>
            vblEngine.Preload()
            output = {False, "Error while Preloading"}
            If script.MyNamespaces.Any Then ' <= -- Inject_Classes
                vblEngine.LoadImports(script.MyNamespaces)
            End If
            If useOwnSyntax Then syntax.Syntaxing(script) ' <= -- Translate into_Lua
            ' <= -------------------------- Inject Variables --------------------------- =>
            If script.ScriptArgs.Any Then
                For Each arg In script.ScriptArgs
                    vblEngine(arg.Item1) = arg.Item2
                Next
            End If
            ' <= ---------------------------- Execution ------------------------------ =>
            Try
                Select Case localFile
                    Case False
                        output = vblEngine.DoString(script.Code.toStr)
                        statusResponse = output.First
                    Case True
                        output = vblEngine.DoFile(script.DataPath)
                        statusResponse = output.First
                End Select                                                                                             ' vblEngine.Globals
            Catch ex As Exception
                output = New Object() {False, "Error while Executing!"}
                statusResponse = False
            End Try
            script.Engine = vblEngine
        End If
        script.Output=output
        Return output
    End Function
    <Extension()> '<= ------- Load_Standart_Libs_into_LUA 
    Public Function Preload(ByRef lua As Lua) As Lua
        lua.LoadCLRPackage() '<= Preload.NET Namespaces
        lua.DoString($" import ('System.IO') import ('System.Windows.Forms')" +
                         "import ('System.Net') import ('System.Web') import ('System.Net.Http') ")
        Return lua
    End Function
    <Extension()> '<= ------- Load_.NET_Into_LUA 
    Public Function LoadImports(ByRef lua As Lua, importList As String()) As Lua
        If importList.Any Then
            Dim names = From importName In importList Select importName
            For Each import As String In names
                lua.DoString($" import('" + import + "') ")
            Next
        End If
        Return lua
    End Function
    <Extension()> '<= ------- Execution with Logs -------
    Public Function Log(ByRef script As Script, Optional localFile As Boolean = False) As Script
        Dim scriptResponse As Object() = script.Execute(localFile)
        File.WriteAllLines("Logs.txt", scriptResponse.ToText())
        Return script
    End Function
    '##########################################  Converters #########################################################
    <Extension()>
    Public Function Check(ByRef obj As Object) As Boolean
        If obj.GetType <> Nothing Then Return True Else Return False
    End Function
    '<? --------------------- Comparing(Of..) ---------------------- 
    <Extension()> ' <= Datatypes
    Public Function CheckType(ByRef obj As Object, type As Type) As Boolean
        If obj.GetType() Is GetType(Type) Then Return True Else Return False
    End Function
    <Extension()>' <= Datatypes
    Public Function CheckType(ByRef obj As Object(), type As Type) As Boolean
        If obj.GetType() Is GetType(Type) Then Return True Else Return False
    End Function
    <Extension()> '<= Explizit
    Public Function ToStr(array As Object) As String
        Return CType(array, String)
    End Function
    '< -------------- Maths ----------------- -------------- Maths ----------------- >
    <Extension()>
    Public Function ToInt(x As Double) As Integer '<= Implizit
        Return Convert.ToInt32(x)
    End Function
    <Extension()> '<= Implizit
    Public Function ToNumber(obj As Object) As Integer '<= Explizit
        Return CType(obj, Integer)
    End Function
    <Extension()>
    Public Function ToInt(x As Decimal) As Integer
        Return Convert.ToInt32(x)
    End Function
    <Extension()>
    Public Function ToInt(x As String) As Integer
        Return Convert.ToInt32(x)
    End Function
    <Extension()> '<= --------- Double
    Public Function ToDouble(x As Integer) As Double
        Return Convert.ToDouble(x)
    End Function
    <Extension()>
    Public Function ToDouble(x As Decimal) As Double
        Return Convert.ToDouble(x)
    End Function
    <Extension()> '<= -------- Decimal
    Public Function ToDec(x As Integer) As Decimal
        Return Convert.ToDecimal(x)
    End Function
    Public Function ToDec(x As Double) As Decimal
        Return Convert.ToDecimal(x)
    End Function
    <Extension()> '<= ------- Other
    Public Function ToBool(x As String) As Boolean
        Return Convert.ToBoolean(x)
    End Function
    '############################################## Utilities ###################################################
    Dim player As AudioPlayer
    Dim tts As SpeechSynthesizer
    <Extension()>
    Public Sub Mp3(ByRef audioFile As String, Optional path As String = "")
        player = New AudioPlayer(path)
        Try
            player.Filename = audioFile
            player.Play()
        Catch ex As Exception
        End Try
        player.Close()
    End Sub
    <Extension()>
    Public Sub Say(ByRef txt As String, Optional volume As Integer = 100)
        tts = New SpeechSynthesizer()
        tts.SetOutputToDefaultAudioDevice()
        tts.Rate = 2
        tts.Volume = volume
        Dim promt As New Prompt(txt)
        If txt.Check Then
            tts.Speak(promt)
            tts.Dispose()
        End If
    End Sub
    <Extension()>
    Public Async Function SayAsync(txt As String, Optional volume As Integer = 100) As Task(Of Action(Of String))
        tts = New SpeechSynthesizer()
        tts.SetOutputToDefaultAudioDevice()
        tts.Rate = 2
        tts.Volume = volume
        Dim promt As New Prompt(txt)
        Return Await SayAsync(txt)
    End Function
    <Extension()> '<= ------- ZIP_Files --------
    Public Sub Unzip(fileName As String, path As String)
        Using stream As FileStream = File.Open(fileName & ".zip", FileMode.Open)
            Try
                Using archive = New ZipArchive(stream)
                    archive.ExtractToDirectory(path)
                End Using
            Catch
                Msg("Failed Compression")
            End Try
        End Using
    End Sub
    <Extension()> 'Unzip
    Public Sub Zip(path As String, destination As String)
        Try
            ZipFile.CreateFromDirectory(path, destination)
        Catch
            Msg("Failed Compression")
        End Try
    End Sub
    <Extension()> '<= ------- Instant_MessageBox(StringOnly)
    Public Sub Msg(ByRef txt As String)
        If txt <> Nothing And txt <> "" Then
            MessageBox.Show(CType(txt, String))
        End If
    End Sub
    <Extension()> '<= ------- Instant_MessageBox()
    Public Sub Msg(ByRef txt As Object)
        If txt <> Nothing And txt <> "" Then
            MessageBox.Show(CType(txt, String))
        End If
    End Sub
    <Extension()> '<= ------- Instant_MessageBox(Array)
    Public Sub Msg(ByRef txt As String())
        If txt.Any Then
            Dim txtAsRichtext As String() = From line In txt.ToText
                                            Where line.GetType() Is GetType(String)
                                            Let lineAsText = line
                                            Select lineAsText
            If txtAsRichtext.Any Then
                MessageBox.Show(txtAsRichtext.ToString + vbCrLf + "Lines: " + txt.Count + ", " + txtAsRichtext.Count)
            End If
        End If
    End Sub
End Module
'################################################ JSON Loaders #######################################################
Public Module Json
    Public settings As New JsonSerializerSettings()
    ' <= -------- Deserialize ------------
    <Extension()>
    Public Function fromJSON(ByRef array As String) As Object()
        settings.Formatting = Formatting.Indented
        Return JsonConvert.DeserializeObject(array, settings)
    End Function
    <Extension()>
    Public Function fromJSON(ByRef array As String, Optional ByVal shuffle As Boolean = False) As List(Of Object)
        settings.Formatting = Formatting.Indented
        Return JsonConvert.DeserializeObject(array, settings)
    End Function
    ' <= -------- Serialize ------------
    <Extension()>
    Public Function toJSON(ByRef array As Object()) As String
        settings.Formatting = Formatting.Indented
        Return JsonConvert.SerializeObject(array, settings)
    End Function
    <Extension()>
    Public Function toJSON(ByRef list As List(Of Object)) As String
        settings.Formatting = Formatting.Indented
        Return JsonConvert.SerializeObject(list, settings)
    End Function
    <Extension()>
    Public Function toJSON(ByRef list As Object) As String
        settings.Formatting = Formatting.Indented
        Return JsonConvert.SerializeObject(list, settings)
    End Function
    '  Sub Search(x As String)
    '      Dim sr As WikiSearchQuery
    '      Using New Net.Http.HttpClient()
    '          wiki = New WikiSearcher
    '          Dim resp = wiki.Search(x)
    '          sr = resp.Query
    '      End Using
    '      sr.SearchResults.Msg()
    '  End Sub
End Module
