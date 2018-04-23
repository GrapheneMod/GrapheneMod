using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Logger = GrapheneMod.Logging.Logging;

namespace GrapheneMod.dotNet
{
    /// <summary>
    /// Collection of extensions for the Delegate class
    /// </summary>
    public static class Delegate_Extensions
    {
        /// <summary>
        /// Call all functions subscribed to the event.
        /// This also checks if there are any subscribed to prevent throwing an error.
        /// </summary>
        /// <param name="parameters">The parameters to pass to the event function.</param>
        public static void Call(this Delegate @delegate, params object[] parameters)
        {
            if (@delegate == null)
                return;
            Delegate[] invokeList = @delegate.GetInvocationList();

            if (invokeList.Length < 1)
                return;
            for (int i = 0; i < invokeList.Length; i++)
                invokeList[i].DynamicInvoke(parameters);
        }

        /// <summary>
        /// Call all functions subscribed to the event.
        /// This also checks if there are any subscribed to prevent throwing an error.
        /// This function uses try/catch on each delegate/function executed.
        /// </summary>
        /// <param name="parameters">The parameters to pass to the event function.</param>
        public static void CallTry(this Delegate @delegate, params object[] parameters)
        {
            if (@delegate == null)
                return;
            Delegate[] invokeList = @delegate.GetInvocationList();

            if (invokeList.Length < 1)
                return;
            foreach(Delegate @event in invokeList)
            {
                try
                {
                    @event.DynamicInvoke(parameters);
                }
                catch(Exception ex)
                {
                    Logger.LogError("Error executing method " + @event.Method.Name, ex);
                }
            }
        }
    }
}
