using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrapheneMod.Logging
{
    #region Log Handlers
    /// <summary>
    /// A function with no returning value containing the text, color, inFile and inConsole parameters.
    /// It is mostly used in events for handling log messages.
    /// </summary>
    /// <param name="text">The text message of the log to be written into the log file or displayed in the console.</param>
    /// <param name="color">The color of the log message to be displayed in the console.</param>
    /// <param name="inFile">Will the log message be written into the log file.</param>
    /// <param name="inConsole">Will the log message be written into the console.</param>
    public delegate void LogHandler(string text, ConsoleColor color, bool inFile, bool inConsole);
    #endregion

    /// <summary>
    /// The class containing logging events and functions. Made for easier logging and debugging.
    /// </summary>
    public static class Logging
    {
        #region Log Information Properties
        private static string _Current_Path => Directory.GetCurrentDirectory() + "//GrapheneMod.log";
        private static string _Previous_Path => Directory.GetCurrentDirectory() + "//GrapheneMod.old.log";
        #endregion

        #region Log Variables
        private static ConsoleColor _Previous_Color = ConsoleColor.White;
        #endregion

        #region Log Events
        /// <summary>
        /// This event is called when a log message is pushed to the console/log file.
        /// It returns the text(String), the color of the log(ConsoleColor), if the log will be written to a file(Bool), if the log will be displayed in the console(Bool)
        /// </summary>
        public static event LogHandler OnLog;
        #endregion

        static Logging()
        {
            // Setup the log files
            if (File.Exists(_Previous_Path))
                File.Delete(_Previous_Path);
            if (File.Exists(_Current_Path))
                File.Move(_Current_Path, _Previous_Path);
            if (!File.Exists(_Current_Path))
                File.Create(_Current_Path).Close();
        }

        #region Log Functions
        // Console functions
        private static void WriteLine(string text, ConsoleColor color)
        {
            // Save and set the console color
            _Previous_Color = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (Console.CursorLeft != 0)
                ClearLine();
            Console.WriteLine(text);

            // Reset the console color
            Console.ForegroundColor = _Previous_Color;
            _Previous_Color = ConsoleColor.White;
        }
        private static void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.CursorTop--;
            Console.CursorLeft = 0;
        }

        // Private functions
        private static void Log(object log, ConsoleColor color, bool inFile, bool inConsole, Assembly assembly, string prefix)
        {
            string assemblyName = assembly.GetName().Name;
            switch (assemblyName.ToLower())
            {
                case "assembly-csharp":
                    assemblyName = "Game";
                    break;
                case "unityengine":
                    assemblyName = "Unity";
                    break;
                case "graphenemod":
                    assemblyName = "Graphene";
                    break;
            }
            string message = "[" + prefix + "] " + assemblyName + " >> " + log.ToString();

            if (inFile)
                File.AppendAllText(_Current_Path, message + Environment.NewLine);
            if (inConsole)
                WriteLine(message, color);
            OnLog(message, color, inFile, inConsole);
        }

        // Public functions
        /// <summary>
        /// Call this function to create a log entry in the specified location.
        /// You can also determine the console color of the log if it is printed into the console.
        /// This calls the OnLog event.
        /// </summary>
        /// <param name="log">The log object to be converted to a string and displayed.</param>
        /// <param name="inConsole">Should the log be shown in the console.</param>
        /// <param name="inFile">Should the log be appended to the log file.</param>
        /// <param name="color">The color of the log to be displayed in the console</param>
        public static void Log(object log, bool inConsole = true, bool inFile = true, ConsoleColor color = ConsoleColor.White) =>
            Log(log, color, inFile, inConsole, Assembly.GetCallingAssembly(), "LOG");
        /// <summary>
        /// Call this function to create an important log entry.
        /// This calls the OnLog event.
        /// </summary>
        /// <param name="log">The log object to be converted to a string and displayed.</param>
        public static void LogImportant(object log) =>
            Log(log, ConsoleColor.Cyan, true, true, Assembly.GetCallingAssembly(), "IMPORTANT");
        /// <summary>
        /// Call this function to create a warning log entry.
        /// This calls the OnLog event.
        /// </summary>
        /// <param name="log">The log object to be converted to a string and displayed.</param>
        public static void LogWarning(object log) =>
            Log(log, ConsoleColor.DarkYellow, true, true, Assembly.GetCallingAssembly(), "WARNING");
        /// <summary>
        /// Call this function to create an error log entry.
        /// This calls the OnLog event.
        /// </summary>
        /// <param name="log">The log object to be converted to a string and displayed.</param>
        /// <param name="error">The exception to be displayed/written into the log files.</param>
        /// <param name="showExceptionInConsole">Should the exception be displayed to the console.</param>
        public static void LogError(object log, Exception error = null, bool showExceptionInConsole = true)
        {
            Log(log, ConsoleColor.Red, true, true, Assembly.GetCallingAssembly(), "ERROR");
            if (error != null)
                Log(error, ConsoleColor.DarkRed, true, showExceptionInConsole, Assembly.GetCallingAssembly(), "EXCEPTION");
        }
        #endregion
    }
}
