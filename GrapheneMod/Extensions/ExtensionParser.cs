using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GrapheneMod.Extensions
{
    #region Extension Parser Handlers
    /// <summary>
    /// A function with no returning value with an extension parameter the same type as the extension.
    /// It is most commonly uses to indicate status changes of extensions.
    /// </summary>
    /// <typeparam name="TExt">The type of extension.</typeparam>
    /// <param name="extension">The extension instance.</param>
    public delegate void ExtensionStatusHandler<TExt>(TExt extension) where TExt : IExtension;
    #endregion

    #region Extension Interfaces
    /// <summary>
    /// Apply this interface to your class to make it a custom extension for the ExtensonParser.
    /// This does not contain anything, it is just there for a more optimized loading.
    /// </summary>
    public interface IExtension { }
    #endregion

    /// <summary>
    /// The extension parser parses Types into extensions and manages how each extension type should react.
    /// </summary>
    /// <typeparam name="TExt">The type of extension.</typeparam>
    public abstract class ExtensionParser<TExt> where TExt : IExtension
    {
        #region Properties
        /// <summary>
        /// The type of the extension to check for.
        /// Returned as the Type type.
        /// </summary>
        public Type Type => typeof(TExt);
        #endregion

        #region Extension Properties
        /// <summary>
        /// The array of extensions that are loaded by the parser.
        /// </summary>
        public abstract TExt[] Extensions { get; }
        #endregion

        #region Extension Events
        // Status updates of extension
        /// <summary>
        /// This event is called when an extension is loaded.
        /// </summary>
        public event ExtensionStatusHandler<TExt> OnExtensionLoad;
        /// <summary>
        /// This event is called when an extension is unloaded.
        /// </summary>
        public event ExtensionStatusHandler<TExt> OnExtensionUnload;
        /// <summary>
        /// This event is called when an extension is reloaded.
        /// </summary>
        public event ExtensionStatusHandler<TExt> OnExtensionReload;
        #endregion

        #region Loading/Unloading Functions
        /// <summary>
        /// Call this function to load an extension.
        /// This uses the Type as the extension parameter.
        /// </summary>
        /// <param name="extension">The Type of the extension class.</param>
        public abstract void Load(Type extension);
        /// <summary>
        /// Call this function to load an extension.
        /// This uses the instance as the extension parameter.
        /// </summary>
        /// <param name="extension">The instance of the extension class.</param>
        public virtual void Load(TExt extension) =>
            Load(extension.GetType());

        /// <summary>
        /// Call this function to unload an extension.
        /// This uses the Type as the extension parameter.
        /// </summary>
        /// <param name="extension">The Type of the extension class.</param>
        public abstract void Unload(Type extension);
        /// <summary>
        /// Call this function to unload an extension.
        /// This uses the instance as the extension parameter.
        /// </summary>
        /// <param name="extension">The instance of the extension class.</param>
        public virtual void Unload(TExt extension) =>
            Unload(extension.GetType());

        /// <summary>
        /// Call this function to reload an extension.
        /// This uses the Type as the extension parameter.
        /// </summary>
        /// <param name="extension">The Type of the extension class.</param>
        public virtual void Reload(Type extension)
        {
            Unload(extension);
            Reload(extension);
        }
        /// <summary>
        /// Call this function to reload an extension.
        /// This uses the instance as the extension parameter.
        /// </summary>
        /// <param name="extension">The instance of the extension class.</param>
        public virtual void Reload(TExt extension) =>
            Reload(extension.GetType());
        #endregion
    }
}
