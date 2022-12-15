using System.Collections.Generic; using System.Diagnostics; using System.Speech.Synthesis; using System.ComponentModel;using System.ComponentModel.Design;using System.Data;using System.Linq;using System.Windows.Forms;
using System.Windows.Forms.Design;using System.Windows.Forms.ComponentModel;using NLua;using System.Drawing.Design;using System.Configuration;using Primitive_Server; using VB_Extension;
namespace VCLua_Framework {
    [ToolboxItem(true),DesignTimeVisible(true)]
    public partial class Terminal : UserControl,IComponent,IDisposable,IContainerControl
    {
        private Lua? lua; private SpeechSynthesizer? tts; private VB_Extension.Script? script; private Server? server;
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string[] Headline = new string[2] { "LUA_Terminal[V1.1.2]\n\r", ">" }; private string[] InputContent = new string[] { "=>|" };public string? errorCode; private int z = 0;
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Terminal() { InitializeComponent(); } // <= --Add the controls to the user control.       
        private void Terminal_Load(object sender, EventArgs e){ }     
        [
        Category("Scripting-Rescources"),
        Description("Handles Scripting Functionalities.")
        ]
        //------------------------------------------------------------
        private string? MainScript = @" local sys =require ('os') local maths =require('math') function GO(args) response=args return true,response end " +
                                        "";
        private string cScript = " function CMD()\r\n    local console = io.popen(userCommand)\r\n    local resp = console:read(\"*a\")\r\n    console:close()\r\n  return true, resp\r\nend\r\r\r\nreturn CMD()";
        private string? ExtScript = " --------------------- MATHE ---------------------------------------- MATHE ---------------------------------------\r\nfunction sum(x,y)\r\n    if y == nil then \r\n        return x + x\r\n    else\r\n        return x + y \r\n    end\r\nend\r\nfunction diff(x,y)\r\n        if y == nil then \r\n            return x - x\r\n    else\r\n        return x - y \r\n    end\r\nend\r\nfunction multipl(x,y)\r\n    if y == nil then \r\n        return x * x\r\n    else\r\n        return x * y \r\n    end\r\nend\r\nfunction ratio(x,y)\r\n        if y == nil then \r\n            return x / x\r\n    else\r\n        return x / y \r\n    end\r\nend\r\n------------------------- Helpful --------------------------- Helpful -------------------------------------------------------------------------------------------------------\r\nfunction call(args)\r\n   return false, { x = 1},0 ,\"xxx\" ,args[1]\r\nend\r\nfunction formAccess(form, input,e,args)\r\n  this = form       -->Expect an object, but got a string!\r\n  return true, this,input,e\r\nend\r\nfunction cmdAccess(cmd)\r\n    local console = io.popen(cmd) --if the echo programm exit with success it will return 0.\r\n    local response = console:read(\"*a\")\r\n    console:close()\r\n  return true, response\r\nend\r\nfunction callCmd(command)\r\n cmdOutput,out,xy = os.execute(command)\r\n return true, cmdOutput, out,xy\r\nend\r\nfunction splitStr(str, startIndex,lastIndex)\r\n    last = -1 if lastIndex ~= nil then last = lastIndex end\r\n    if str ~= nil then return true,string.sub(str,startIndex,lastIndex) else return false end\r\nend\r\nfunction findStr(str,x)\r\n    if #str == 1 then x = \"%\"..x end\r\n    foundIndex, lastIndex= string.find(str, x) --(\"%\") für einzelne Buchstaben\r\n    i=foundIndex+1 --NextCut:Start_Index\r\n    if foundIndex ~= nil then return true, foundIndex-1, lastIndex-1 else return false, 0, 0 end\r\nend return true ";
        private string? LuaEnding = " return true,response,body ";
        //############################################################################################################################
        private bool _Editing_Mode = false; private bool _AudioOutput = false; private string myScript = ""; private string? user;
        [
        Category("Side Functions"),
        Description("Functionalities & More..")
        ]
        public bool Editing_Mode{
            get { return _Editing_Mode; }
            set { _Editing_Mode = value; commandLine.Multiline = value; Invalidate(); }
        }
        [Category("Scripting"),
        Description("VBLua_Bindings and Settings.")
        ]
        public string MyScript { 
            get => myScript; set => myScript = @value + LuaEnding; 
        }    
        public string? User { 
            get => user; set => user = value;
        }
        // ############################################################################################################################
        [
        Category("Console_Specifications"),
        Description("Handles Windows- and Scripting- and PrimitiveServer-CommandLine.")
        ]
        private bool useDefaultEnvironment=false;
        public bool AudioOutput{
            get { return _AudioOutput; } set { _AudioOutput = value; }
        }
        private string[]? Input {
            get { return commandLine.Text?.Substring(3)?.Split('_'); }
        }
        public string? Environment {
            get { return Input?.First(); }
        }
        [SettingsDescription("Uses your Variable instead of Typing in Console.")]
        public bool UseDefaultEnvironment{
            get { return useDefaultEnvironment; } set { useDefaultEnvironment = value; }
        }     
        //---------------------------- IN -/ Output -----------------------------------
        private object[]? Output = { false,""}; private string? StandartOutput { get => Output?[1].ToStr(); }
        protected bool Response;
        private string? StandartInput { get => Input?[1]; }
        protected string[]? Args { get => Input?[..2]; }
        // ################################################################################################################
        private bool PerformCommand() {
            var client = this;
            // ----------------------- Execution --------------------- Execution --------------------                              
            switch (Environment){
                case "CMD" when StandartInput != null:
                    {              
                        script = new(Properties.Resources.Cmd,false);         
                        script.MyNamespaces = new string[] { "Newtonsoft.Json", "System.Data" };                   
                        script.ScriptArgs = new (string, object)[] { ("namespace", Environment),("userCommand", "cmd"), ("commandStatement", StandartInput), ("client", client), ("this", this) };
                        Output = script.Start(false,false);
                        Response = (bool)Output.First(); if (Response) { errorCode = "Internal Script Failure"; }             
                        else { errorCode = (string)Output[1]; } Say();                     
                        return Response;       
                    }

                case "DB" when Args != null :
                    {
                        script = new("data/SchmandSQL", Args, false);
                        script.MyNamespaces = new string[] { "Newtonsoft.Json", "User", "Primitive_Server.Server" };
                        script.ScriptArgs = new (string,object)[] { ("namespace",Args[0].ToLower()), ("response", Args[1].ToLower()), ("client", client), ("this", this) };
                        Output = script.Start(false, true);
                        Response = (bool)Output.First(); if (Response) { errorCode = "Internal Script Failure"; }
                        else { errorCode = (string)Output[1]; } Say();
                        return Response;  //var scriptFunc = lua[Environment] as LuaFunction;var response = scriptFunc.Call(args);            
                    }

                case "SAY" when StandartInput!= null: { Output = new object[] { true, StandartInput}; Response = true; if (AudioOutput == false) { AudioOutput = true; Say(); AudioOutput = false; }
                        else { Say(); } return Response; }        
                case "" or null : { Output = new object[] { false, "" }; Response = false; return false; }
            }
            return Response;
        }
        //-------------------------------------------------------------------------------------------------------------
        private void resetInputLine() { commandLine.Lines = InputContent; commandLine.DeselectAll(); commandLine.SelectionStart = 3; }
        private void ControlInputLine(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter && Environment!=null) {
                PerformCommand();
                if (StandartOutput != null && StandartOutput!=""){
                    outputLine.Text += Environment + "::" + "\t" + StandartOutput; outputLine.Text += "\r\n" + ">";
                } else { outputLine.Text += "::Cant find Namespace " + Environment + "::" + "\t" ; outputLine.Text += "\r\n" + ">"; }
                resetInputLine();
                }
        }
        public void Writing(object sender, KeyPressEventArgs e){
            if (e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back ) {
                if (commandLine.SelectionStart <= 3 || commandLine.SelectedText.Contains("|") || commandLine.SelectedText.Contains("=") || commandLine.SelectedText.Contains(">")) {
                    resetInputLine(); } if (e.KeyChar == (char)Keys.Back && commandLine.SelectionStart <= 3) { e.Handled = true;  }
            }
            else if (commandLine.SelectedText.Contains("|") || commandLine.SelectedText.Contains("=") || commandLine.SelectedText.Contains(">")){
                resetInputLine();
            }
        }     
        private void Say() {
            if (AudioOutput && StandartOutput != null)
            {
                tts = new SpeechSynthesizer(); tts.SetOutputToDefaultAudioDevice();
                Prompt content = new Prompt(StandartOutput); tts.Rate = 2; tts.Volume = 100; // tts.SelectVoiceByHints(gender, speakerAge);              
                tts.SpeakAsync(content);
            }
        }
        // ################################################################################################################
        private Color color = Color.Teal;private Color hovercolor = Color.FromArgb(0, 0, 140);
        private Color clickcolor = Color.FromArgb(160, 180, 200); ContentAlignment align = ContentAlignment.TopCenter;
        private int textX = 6; private int textY = -20; Padding padding1 = new(1, 1, 1, 1); Padding padding2 = new(1, 0, 0, 1);
        [
        Category("ConsoleUI"),
        Description("Appearance of the Console.")
        ]
        public Size TerminalSize{
            get { return Size; } set { Size = value; Invalidate(); }
        }
        public ContentAlignment TextAlignment{get{return align; }set{ align = value;Invalidate();} }
        public Color BZBackColor{
            get { return color; } set { color = value; Invalidate(); }
        }
        public FlatStyle FlatStyle { get; private set; }    
        protected override void OnPaint(PaintEventArgs e){
            base.OnPaint(e); StringFormat style = new StringFormat();     
            style.Alignment = StringAlignment.Near; style.FormatFlags = new();
            style.FormatFlags = StringFormatFlags.LineLimit;style.FormatFlags = StringFormatFlags.NoClip;
            style.FormatFlags = StringFormatFlags.DisplayFormatControl; style.LineAlignment = StringAlignment.Near;
            e.Graphics.DrawString(Text,Font,new SolidBrush(ForeColor),ClientRectangle, style);
        }    
    }
}