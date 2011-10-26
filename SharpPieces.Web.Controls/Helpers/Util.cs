using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SharpPieces.Web
{

    /// <summary>
    /// Util methods.
    /// </summary>
    public sealed class Util
    {

        /// <summary>
        /// Turns to absolute an url relative to application.
        /// This method doesn't check the url format, it just tries to replace the app relative char.
        /// </summary>
        /// <param name="url">The url to transform.</param>
        /// <returns>An absolute url or the same one if not relative.</returns>
        public static string AppRelativeToAbsolute(string url, bool throwIfBadArgument)
        {
            if ((null == HttpContext.Current) || (null == HttpContext.Current.Request))
            {
                throw new NotSupportedException("Outside of current context bounds.");
            }
            if (null == url)
            {
                if (throwIfBadArgument)
                {
                    throw new ArgumentNullException("url");
                }
            }
            else if (!url.StartsWith("~"))
            {
                if (throwIfBadArgument)
                {
                    throw new ArgumentException("url is not a relative to application.");
                }
            }
            else
            {
                return string.Concat(HttpContext.Current.Request.ApplicationPath, url.Substring(1)).Replace("//", "/");
            }
            return url;
        }

        /// <summary>
        /// Merges a script pair.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        /// <returns>A merged script.</returns>
        public static string MergeScripts(Pair<string> scripts)
        {
            if (null == scripts)
            {
                return string.Empty;
            }

            string script = (null != scripts.First) ? scripts.First.Trim() : string.Empty;
            if (string.Empty != script)
            {
                if (!script.EndsWith(";"))
                {
                    script = script + ";";
                }
                string second = (null != scripts.Second) ? scripts.Second.Trim() : string.Empty;
                if (string.Empty != scripts.Second)
                {
                    script = script + (second.StartsWith("javascript:") ? second.Remove(0, "javascript:".Length) : second);
                }
            }
            else if (null != scripts.Second)
            {
                script = scripts.Second;
            }

            if (string.Empty != script)
            {
                return script.StartsWith("javascript:") ? script : "javascript:" + script;
            }

            return script;
        }

    }

}
