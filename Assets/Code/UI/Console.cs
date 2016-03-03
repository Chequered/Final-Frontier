using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using EndlessExpedition.Managers;
using System;

namespace EndlessExpedition
{
    public class ConsolePrintInfo
    {
        private LogType m_type;
        private string m_printContent;
        private string m_stackTrace;
        private bool m_command;
        private Color m_color;

        private int m_stackSize = 0;
        private int m_lines;

        public ConsolePrintInfo(LogType type = LogType.Log, string printContent = "", string stackTrace = "", bool command = false, Color color = default(Color))
        {
            m_type = type;
            m_printContent = printContent;
            m_stackTrace = stackTrace;
            m_command = command;
            m_color = color;

            m_lines = printContent.Split('\n').Length;
            if (m_lines == 1)
                m_lines = 0;

            if (m_stackTrace != "")
            {
                m_stackSize = stackTrace.Split('\n').Length;
            }
        }

        public LogType type
        {
            get
            {
                return m_type;
            }
        }
        public string printContent
        {
            get
            {
                return m_printContent;
            }
        }
        public string stackTrace
        {
            get
            {
                return m_stackTrace;
            }
        }
        public bool command
        {
            get
            {
                return m_command;
            }
        }
        
        public int stackSize
        {
            get
            {
                return m_stackSize;
            }
        }
        public Color color
        {
            get
            {
                return m_color;
            }
        }
        public int lineCount
        {
            get
            {
                return m_lines;
            }
        }
    }


    public class ConsoleCommand : Attribute{

        private bool m_hidden;
        private string m_description;

        public ConsoleCommand(string description = "", bool hidden = false)
        {
            m_hidden = hidden;
            m_description = description;
        }

        public bool hidden
        {
            get
            {
                return m_hidden;
            }
        }
        public string description
        {
            get
            {
                return m_description;
            }
        }
    }

    public class CMD : MonoBehaviour 
    {
        //Switches
        private static bool m_enabled;
        private static bool m_isTyping;
        private static bool m_usingBackspace = false;

        //Input
        private static string m_inputString;
        private static List<string> m_previousCommands = new List<string>();
        private static int m_previousCommandIndex = -1;

        //Graphical
        private static int m_fontSize;
        private static GUISkin m_skin;

        private static Texture2D m_windowBackgroundTexture;
        private static Texture2D m_inputFieldInactiveTexture;
        private static Texture2D m_inputFieldActiveTexture;

        private static Rect m_windowRect;
        private static Rect m_inputFieldRect;
        private static Rect m_titleRect;

        //Print commands
        private static List<ConsolePrintInfo> m_printCommands;
        private static List<MethodInfo> m_methods;
    
		public static void Init(GameObject consoleObject)
        {
			if(consoleObject.GetComponent<CMD>() == null)
				consoleObject.AddComponent<CMD>();

            m_fontSize = 12;

            m_skin = ScriptableObject.CreateInstance<GUISkin>();
            m_skin.GetStyle("Label").normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            m_skin.GetStyle("Label").alignment = TextAnchor.LowerLeft;
            m_skin.GetStyle("Label").wordWrap = true;
            m_skin.GetStyle("Label").fontSize = m_fontSize;

            float width = Screen.width / 100 * 75;
            float height = Screen.height / 100 * 75;
            float inputHeight = m_fontSize + 2; ;

            m_windowRect = new Rect(0, Screen.height - height - inputHeight, width, height);
            m_titleRect = new Rect(0, Screen.height - height - inputHeight * 2, width, inputHeight);
            m_inputFieldRect = new Rect(0, Screen.height - inputHeight, width, inputHeight);

            m_windowBackgroundTexture = new Texture2D(1, 1);
            m_windowBackgroundTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
            m_windowBackgroundTexture.Apply();
            
            m_inputFieldInactiveTexture = new Texture2D(1, 1);
            m_inputFieldInactiveTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 0.9f));
            m_inputFieldInactiveTexture.Apply();

            m_inputFieldActiveTexture = new Texture2D(1, 1);
            m_inputFieldActiveTexture.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f, 0.9f));
            m_inputFieldActiveTexture.Apply();

            m_printCommands = new List<ConsolePrintInfo>();

            m_enabled = false;

            Application.logMessageReceivedThreaded += OnLogMessageReceive;

            RefreshCommands();
        }

        private void OnDrawGizmos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Gizmos.DrawRay(ray.origin, ray.direction * 500f);
        }

        public static void RefreshCommands()
        {
            float startTime = Time.realtimeSinceStartup;
            m_methods = new List<MethodInfo>();

            Assembly assembly = Assembly.GetAssembly(typeof(Main));
            MethodInfo[] methods = assembly.GetTypes()
                  .SelectMany(t => t.GetMethods())
                  .Where(m => m.GetCustomAttributes(typeof(ConsoleCommand), false).Length > 0)
                  .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                if (!methods[i].IsStatic)
                    continue;
                if (methods[i].Name.Length > 3)
                {
                    if (methods[i].Name[0] == 'C' && methods[i].Name[1] == 'M' && methods[i].Name[2] == 'D')
                    {
                        m_methods.Add(methods[i]);
                    }
                }
            }
            Log("Refreshed Commands in " + (Time.realtimeSinceStartup - startTime) + "ms");
        }

        //Creating / Printing Commands
        private static void OnLogMessageReceive(string condition, string stackTrace, LogType type)
        {
            CreatePrintInfo(type, condition, stackTrace);
        }
        private static ConsolePrintInfo CreatePrintInfo(LogType type = LogType.Log, string printContent = "", string stackTrace = "", Color color = default(Color))
        {
            ConsolePrintInfo info = new ConsolePrintInfo(type, printContent, stackTrace, false, color);
            m_printCommands.Add(info);
            return info;
        }

        public static ConsolePrintInfo Log(object message, Color color = default(Color))
        {
            return CreatePrintInfo(LogType.Log, message.ToString(), "", color);
        }
        public static ConsolePrintInfo Warning(string warning)
        {
            return CreatePrintInfo(LogType.Warning, warning);
        }
        public static ConsolePrintInfo Error(string error)
        {
            ToggleConsole(true);
            return CreatePrintInfo(LogType.Error, error, Environment.StackTrace);
        }
        public static ConsolePrintInfo Error(Exception exception)
        {
            ToggleConsole(true);
            return CreatePrintInfo(LogType.Exception, exception.ToString(), exception.StackTrace);
        }

        private void Update()
        {
            if (m_enabled && m_isTyping)
            {
                if (Input.GetKey(KeyCode.Backspace))
                {
                    if (!m_usingBackspace)
                    {
                        if (m_inputString.Length > 0)
                            m_inputString = m_inputString.Remove(m_inputString.Length - 1);
                        m_usingBackspace = true;
                    }
                }
                else
                {
                    m_usingBackspace = false;
                    m_inputString += Input.inputString.Replace("`", "");
                }


                if (Input.GetKeyUp(KeyCode.Return))
                {
                    TryCommand();   
                }
            }

            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                ToggleConsole();
            }

            if (!m_enabled)
                return;


            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                if(m_previousCommandIndex - 1 >= 0)
                    m_previousCommandIndex--;
                if(m_previousCommands.Count > 0 && m_previousCommandIndex < m_previousCommands.Count)
                    if(m_previousCommands[m_previousCommandIndex] != null)
                        m_inputString = m_previousCommands[m_previousCommandIndex].Trim();
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                if (m_previousCommandIndex + 1 < m_previousCommands.Count)
                {
                    m_previousCommandIndex++;
                    if (m_previousCommands.Count > 0 && m_previousCommandIndex < m_previousCommands.Count)
                        if (m_previousCommands[m_previousCommandIndex] != null)
                            m_inputString = m_previousCommands[m_previousCommandIndex].Trim();
                }
                else
                    m_inputString = "";
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector2 switchedY = Input.mousePosition;
                switchedY.y = Screen.height - switchedY.y;
                if (m_inputFieldRect.Contains(switchedY))
                {
                    InputManager.isTypingInInputField = true;
                    m_isTyping = true;
                }
                else
                {
                    if (m_isTyping)
                        InputManager.isTypingInInputField = false;
                    m_isTyping = false;
                }
            }
        }

        private MethodInfo FindCommand(string commandName)
        {
            for (int i = 0; i < m_methods.Count; i++)
            {
                if (m_methods[i].Name.Trim() == commandName.Trim())
                {
                    return m_methods[i];
                }
            }
            return null;
        }

        private static void ToggleConsole(bool state)
        {
            m_enabled = state;
            InputManager.isTypingInInputField = m_enabled;
            m_isTyping = m_enabled;
            m_inputString = "";
        }
        private static void ToggleConsole()
        {
            m_enabled = !m_enabled;
            InputManager.isTypingInInputField = m_enabled;
            m_isTyping = m_enabled;
            m_inputString = "";
        }
        private void TryCommand()
        {
            string[] splitCommand = m_inputString.Split(' ');
            if(splitCommand.Length > 0)
            {
                string commandName = "CMD" + splitCommand[0];
                try
                {
                    //Find Command
                    MethodInfo command = FindCommand(commandName);
                    if (command != null)
                    {
                        Log("~ " + command, new Color32(115, 255, 115, 255));
                        if (splitCommand.Length > 1)
                        {
                            string[] parameters = new string[splitCommand.Length - 1];
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                parameters[i] = splitCommand[i + 1];
                            }

                            //Execute command
                            command.Invoke(this, parameters);
                        }else
                            command.Invoke(this, null);
                    }
                    else
                    {
                        //No command was found
                        Log("~ The command \"" + commandName + "\" could not be found.", new Color32(255, 115, 115, 255));
                    }
                    m_previousCommands.Add(m_inputString);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            m_inputString = "";
            m_previousCommandIndex = m_previousCommands.Count;
        }

        [ConsoleCommand("Hello!")]
        public static void CMDHelloWorld()
        {
            Log("Hello World!");
        }

        [ConsoleCommand("Completely clears the console")]
        public static void CMDClear()
        {
            m_printCommands.Clear();
        }

        [ConsoleCommand("List all non hidden commands")]
        public static void CMDListCommands()
        {
            for (int i = 0; i < m_methods.Count; i++)
            {
                object[] attributes = m_methods[i].GetCustomAttributes(true);
                bool hidden = false;
                string description = "";
                for (int a = 0; a < attributes.Length; a++)
                {
                    if(attributes[a].GetType() == typeof(ConsoleCommand))
                        if((attributes[a] as ConsoleCommand).hidden)
                        {
                            hidden = true;
                            break;
                        }
                        else
                        {
                            description = (attributes[a] as ConsoleCommand).description;
                        }
                }
                if (hidden)
                    continue;

                string commandName = m_methods[i].Name;
                commandName = commandName.Remove(0, 3);

                string final = "<b>" + commandName + "</b>";

                ParameterInfo[] parameters = m_methods[i].GetParameters();
                if(parameters.Length > 0)
                {
                    final += " (";
                    for (int p = 0; p < parameters.Length; p++)
                    {
                        final += parameters[p].Name;
                        if (p + 1 < parameters.Length)
                            final += ", ";
                    }
                    final += ")";
                }
                if(description != "")
                    final += "<size=11><color=#666699> " + description + "</color></size>";

                Log(final);
            }
        }

        [ConsoleCommand("List all non hidden commands without any markup or descriptions")]
        public static void CMDListCommandsRaw()
        {
            for (int i = 0; i < m_methods.Count; i++)
            {
                Log(m_methods[i].ToString());
            }
        }

        private void OnGUI()
        {
            if (!m_enabled)
                return;

            Color startColor = GUI.contentColor;

            GUI.depth = -9999;
            GUI.skin = m_skin;

            //Title
            GUI.DrawTexture(m_titleRect, m_inputFieldActiveTexture);
            Rect titleRect = m_titleRect;
            titleRect.x += titleRect.width / 2;
            GUI.Label(titleRect, "Console");

            GUI.DrawTexture(m_windowRect, m_windowBackgroundTexture);
            if(m_isTyping)
                GUI.DrawTexture(m_inputFieldRect, m_inputFieldActiveTexture);
            else
                GUI.DrawTexture(m_inputFieldRect, m_inputFieldInactiveTexture);

            GUI.contentColor = Color.black;
            GUI.Label(m_inputFieldRect, m_inputString);
            GUI.contentColor = startColor;

            float relativeY = 0;
            float allowedHeight = m_windowRect.height / m_fontSize;

            int lineNum = 0;
            for (int i = m_printCommands.Count; i-- > 0; )
            {
                if (m_printCommands[i].type == LogType.Error || m_printCommands[i].type == LogType.Exception)
                {
                    GUI.contentColor = new Color32(235, 115, 115, 255);
                    if (m_printCommands[i].stackTrace != "")
                    {
                        float stackHeight = m_printCommands[i].stackSize * m_fontSize + (m_fontSize + 2);
                        relativeY += stackHeight;

                        if (relativeY + lineNum * m_fontSize > m_windowRect.height)
                            break;

                        float y = (Screen.height - (lineNum * m_fontSize) - (m_fontSize + 2) * 2) - relativeY + m_fontSize / 2;
                        GUI.Label(new Rect(m_windowRect.x, y, m_windowRect.x, stackHeight), "[] " + m_printCommands[i].stackTrace);
                    }
                }

                m_skin.GetStyle("Label").fontStyle = FontStyle.Bold;
                if(m_printCommands[i].type == LogType.Log)
                {
                    if (m_printCommands[i].color != default(Color))
                        GUI.contentColor = m_printCommands[i].color;
                    GUI.Label(new Rect(m_windowRect.x, (Screen.height - (lineNum * m_fontSize) - (m_fontSize + 2) * 2) - relativeY, m_windowRect.x, m_fontSize), "- " + m_printCommands[i].printContent);
                }
                else if (m_printCommands[i].type == LogType.Error || m_printCommands[i].type == LogType.Exception)
                    GUI.Label(new Rect(m_windowRect.x, (Screen.height - (lineNum * m_fontSize) - (m_fontSize + 2) * 2) - relativeY, m_windowRect.x, m_fontSize), "{!!} " + m_printCommands[i].printContent);
                else if (m_printCommands[i].type == LogType.Warning)
                {
                    GUI.contentColor = new Color32(235, 235, 80, 255);
                    GUI.Label(new Rect(m_windowRect.x, (Screen.height - (lineNum * m_fontSize) - (m_fontSize + 2) * 2) - relativeY, m_windowRect.x, m_fontSize), "! " + m_printCommands[i].printContent);
                }
                else
                    GUI.Label(new Rect(m_windowRect.x, (Screen.height - (lineNum * m_fontSize) - (m_fontSize + 2) * 2) - relativeY, m_windowRect.x, m_fontSize), m_printCommands[i].printContent);
                m_skin.GetStyle("Label").fontStyle = FontStyle.Normal;

                if (relativeY + lineNum * m_fontSize > m_windowRect.height - m_fontSize - 2 - m_fontSize)
                    break;

                lineNum++;
                GUI.contentColor = startColor;

                relativeY += m_printCommands[i].lineCount * m_fontSize;
                if (m_printCommands[i].lineCount > 0)
                    relativeY += m_fontSize + 2;

            }
        }
    }
}

