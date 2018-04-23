using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Logger = GrapheneMod.Logging.Logging;
using GrapheneMod.dotNet;
using GrapheneMod.Extensions;

namespace GrapheneMod
{
    #region General Handles
    /// <summary>
    /// A function with no returning value or parameters.
    /// It is mostly used in events that don't pass any parameters and don't require a return.
    /// </summary>
    public delegate void VoidHandler();
    /// <summary>
    /// A function with no returning value containing an Exception parameter.
    /// It is mostly used in events that return if an error occurred.
    /// </summary>
    /// <param name="ex">The Exception object containing details of the error.</param>
    public delegate void ErrorHandler(Exception ex);
    #endregion

    #region Logging Handles
    /// <summary>
    /// A function with no returning value containing the "text" and "color" parameters.
    /// It is used when the program/game pushes a message to the logs.
    /// </summary>
    /// <param name="log">The string that was logged to the console/written to the log file</param>
    /// <param name="color">The color of the message that should be printed into the console. Note this is not logged into the log files!</param>
    public delegate void PushLogHandler(string text, ConsoleColor color, bool inConsole);
    /// <summary>
    /// A function with no returning value containing the "input" parameter.
    /// It is used when the program/game gets an input message from the console or a STDIN file.
    /// </summary>
    /// <param name="input">The input message gotten from the console or STDIN file.</param>
    public delegate void PullInputHandler(string input);
    #endregion

    /// <summary>
    /// The main class for the GrapheneMod mod loader. This class is used for loading/unloading/reloading the mod loader.
    /// It also contains functions for initialization of specific components such as the logging.
    /// </summary>
    public static class GrapheneMod
    {
        #region Graphene Information
        // Private data for GrapheneMod
        private static Assembly _GraphenAssembly = Assembly.GetExecutingAssembly();
        private static FileVersionInfo _GrapheneFileInfo = FileVersionInfo.GetVersionInfo(_GraphenAssembly.Location);

        // Public information about the file
        /// <summary>
        /// The name of the mod loader.
        /// </summary>
        public static string Name => _GrapheneFileInfo.ProductName;
        /// <summary>
        /// The name of the creator.
        /// </summary>
        public static string Creator => _GrapheneFileInfo.CompanyName;
        /// <summary>
        /// The version of the mod loader.
        /// </summary>
        public static string Version => _GrapheneFileInfo.ProductVersion;
        /// <summary>
        /// The description of the mod loader
        /// </summary>
        public static string Description => _GrapheneFileInfo.FileDescription;
        #endregion

        #region Graphene Paths
        /// <summary>
        /// The current working directory containing the mod loader.
        /// </summary>
        public static string CurrentDirectory => Path.GetDirectoryName(_GraphenAssembly.Location);
        /// <summary>
        /// The location of the directory containing all of the extensions.
        /// </summary>
        public static string ExtensionsDirectory => ExtensionInternal.ExtensionsDirectory;
        #endregion

        #region Graphene Events
        // Loading
        /// <summary>
        /// This event is called when the "Load" function inside Graphene is called.
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event VoidHandler OnGrapheneLoading;
        /// <summary>
        /// This event is called when Graphene has successfully loaded.
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event VoidHandler OnGrapheneLoaded;
        /// <summary>
        /// This event is called when Graphene fails to load due to an error.
        /// It returns the error(Exception).
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event ErrorHandler OnGrapheneLoadingError;

        // Unloading
        /// <summary>
        /// This event is called when the "Unload" function inside Graphene is called.
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event VoidHandler OnGrapheneUnloading;
        /// <summary>
        /// This event is called when Graphene has successfully unloaded.
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event VoidHandler OnGrapheneUnloaded;
        /// <summary>
        /// This event is called when Graphene fails to unload due to an error.
        /// It returns the error(Exception).
        /// Note: This does not include reloading Graphene!
        /// </summary>
        public static event ErrorHandler OnGrapheneUnloadingError;

        // Reloading
        /// <summary>
        /// This event is called when the "Reload" function inside Graphene is called.
        /// Note: This does not call Loading and Unloading events!
        /// </summary>
        public static event VoidHandler OnGrapheneReloading;
        /// <summary>
        /// This event is called when Graphene has successfully reloaded.
        /// Note: This does not call Loading and Unloading events!
        /// </summary>
        public static event VoidHandler OnGrapheneReloaded;
        /// <summary>
        /// This event is called when Graphene fails to reload due to an error.
        /// It returns the error(Exception).
        /// Note: This does not call Loading and Unloading events!
        /// </summary>
        public static event ErrorHandler OnGrapheneReloadingError;

        // Logging
        /// <summary>
        /// This event is called when Graphene sends a text message to the console or STDOUT file.
        /// It returns the text(String) and color(ConsoleColor).
        /// </summary>
        public static event PushLogHandler OnGrapheneLogOut;
        /// <summary>
        /// This event is called when Graphene gets a message from the console input or STDIN file.
        /// It returns the input(string).
        /// </summary>
        public static event PullInputHandler OnGrapheneLogIn;
        #endregion

        static GrapheneMod()
        {
            // Setup event chaining
            Logger.OnLog += delegate (string text, ConsoleColor color, bool inFile, bool inConsole)
            {
                OnGrapheneLogOut.CallTry(text, color, inConsole);
            };
        }

        #region Graphene Functions
        /// <summary>
        /// Call this function to start the loading process of GrapheneMod.
        /// This function returns if GrapheneMod is already loaded, so you don't have to worry about executing it multiple times.
        /// It will also call the Load events.
        /// </summary>
        public static void Load()
        {
            OnGrapheneLoading.CallTry();
            try
            {
                // Load the extensions
                foreach(string file in Directory.GetFiles(ExtensionsDirectory))
                    ExtensionInternal.RegisterAssemblyName(file);
                ExtensionInternal.LoadAssemblies();

                OnGrapheneLoaded.CallTry();
            }catch(Exception ex)
            {
                OnGrapheneLoadingError.CallTry(ex);
                Unload();
            }
        }

        /// <summary>
        /// Call this function to start the unloading process of GrapheneMod.
        /// This function returns if GrapheneMod isn't loaded, so you don't have to worry about executing it multiple times.
        /// This will also call the Unload events.
        /// </summary>
        public static void Unload()
        {
            OnGrapheneUnloading.CallTry();
            try
            {
                OnGrapheneUnloaded.CallTry();
            }catch(Exception ex)
            {
                OnGrapheneUnloadingError.CallTry(ex);
            }
        }

        /// <summary>
        /// Call this function to start the reloading process of GrapheneMod.
        /// This will also call the Reload events.
        /// Warning: This function should not be called mid-reload, as it can cause problems.
        /// </summary>
        public static void Reload()
        {
            OnGrapheneReloading.CallTry();
            try
            {
                OnGrapheneReloaded.CallTry();
            }catch(Exception ex)
            {
                OnGrapheneReloadingError.CallTry(ex);
            }
        }
        #endregion
    }
}
