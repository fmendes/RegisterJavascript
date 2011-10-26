using System;
using System.Text;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Utility class for livegrid client scripting.
    /// </summary>
    public class LiveGridClientScripts
    {

        // fields

        internal static readonly string updateBookmarkHandler = "updateBookmark";

        // methods

        /// <summary>
        /// Gets the new declaration of a livegrid.
        /// </summary>
        /// <param name="name">The name of the livegrid object.</param>
        /// <param name="id">The id of the livegrid object.</param>
        /// <param name="buffer">The buffer param.</param>
        /// <param name="options">The options params.</param>
        /// <returns>The livegrid decaration sentence</returns>
        internal static string GetNewLiveGrid(string name, string id, string buffer, string options)
        {
            return string.Format("{0} = new Rico.LiveGrid('{1}', {2}, {3});", name, id, buffer, options);
        }

        /// <summary>
        /// Gets the new buffer.
        /// </summary>
        /// <param name="name">The name of the buffer object.</param>
        /// <param name="httpHandler">The HTTP handler in charge of the XML data rendering.</param>
        /// <param name="options">The options params.</param>
        /// <returns>The buffer decaration sentence.</returns>
        internal static string GetNewBuffer(string name, string httpHandler, string options)
        {
            return string.Format("{0} = new Rico.Buffer.AjaxSQL('{1}', {2});", name, httpHandler, options);
        }

        /// <summary>
        /// Gets an option declaration of a grid, buffer, etc.
        /// </summary>
        /// <param name="key">The option key.</param>
        /// <param name="value">The option value.</param>
        /// <returns>The option decaration sentence.</returns>
        internal static string GetOption(string key, object value)
        {
            return string.Concat(key, (string.IsNullOrEmpty(key) ? string.Empty : ":"), value);
        }

        /// <summary>
        /// Gets the array declaration of a javascript object.
        /// </summary>
        /// <param name="name">The name of the array object.</param>
        /// <param name="values">The values of the array object.</param>
        /// <returns>The array declaration sentence.</returns>
        internal static string GetArrayDeclaration(string name, string[] values)
        {
            StringBuilder sb = new StringBuilder("var ");
            sb.Append(name);
            sb.Append("= { ");
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append(values[i]);
                if (i != (values.Length - 1))
                {
                    sb.Append(",");
                }
            }
            sb.Append(" };");
            return sb.ToString();
        }

        internal static string GetCommandRequestImpl(string gridName)
        {
            return string.Format(
                "function __command(action, key)\r\n" +
                "{{\r\n" +
                "   var lastOffset={0}.buffer.lastOffset;\r\n" +
                "   {0}.buffer.clear();\r\n" +
                "   {0}.clearRows();\r\n" +
                "   {0}.buffer.lastOffset=lastOffset;\r\n" +
                "   {0}.buffer.options.requestParameters.push('actionname='+action);\r\n" +
                "   {0}.buffer.options.requestParameters.push('actionkey='+key);\r\n" +
                "   {0}.buffer.refresh();\r\n" +
                "   {0}.buffer.options.requestParameters.pop('actionname='+action);\r\n" +
                "   {0}.buffer.options.requestParameters.pop('actionkey='+key);\r\n" +
                "}}\r\n", gridName);
        }

        internal static string GetScrollHandlerImpl(string hiddenId)
        {
            return 
                @"function scrollHandler(sender, currentOffset)" + Environment.NewLine +
                @"{" + Environment.NewLine +
                @"   $(sender.tableId + '" + LiveGrid.SCROLLPOS_SUFFIX + "').value = currentOffset.toString();" + Environment.NewLine +
                @"}";
        }

        public static string GetCommandRequest(string actionName, string keyName)
        {
            return string.Format("javascript:__command({0}, {1});", actionName, keyName);
        }

        public static string GetScrollHandler()
        {
            return "scrollHandler";
        }

        internal static string GetCssLink(string filePath)
        {
            return string.Format("<link href='{0}' rel='stylesheet' type='text/css' />", filePath);
        }

    }

}
