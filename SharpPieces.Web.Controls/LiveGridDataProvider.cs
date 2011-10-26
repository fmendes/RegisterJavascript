using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Web.UI;
using System.Xml.Serialization;
using System.Collections;
using System.Web.SessionState;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a base class for LiveGrid data content.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public abstract class LiveGridDataProvider : IHttpHandler, IRequiresSessionState
    {

        // events

        /// <summary>
        /// Occurs when a command is requested.
        /// </summary>
        public event CommandEventHandler CommandClick;

        /// <summary>
        /// Occurs when a row is bound.
        /// </summary>
        public event RowDataBoundEventHandler RowDataBound;


        // fields

        internal const string ID_KEY = "id";
        internal const string OFFSET_KEY = "offset";
        internal const string PAGESIZE_KEY = "page_size";
        internal const string ACTIONNAME_KEY = "actionname";
        internal const string ACTIONKEY_KEY = "actionkey";

        private HttpContext context = null;

        private string tableId = null;
        private string customCommandName = null;
        private string customCommandArgument = null;
        private string sortExpression = null;
        private SortDirection sortDirection = SortDirection.Ascending;
        private int offset = 0;
        private int pageSize = 0;
        private int totalCount = 0;
        private LiveGridColumnCollection columns;

        private IEnumerable data = null;


        // methods

        private void Init()
        {
            NameValueCollection queryString = this.Context.Request.QueryString;

            // the table id
            this.tableId = queryString[LiveGridDataProvider.ID_KEY];

            // get the columns
            this.columns = this.Context.Session[LiveGridDataProvider.GetColumnsSessionKey(this.tableId)] as LiveGridColumnCollection;

            // get the row index
            int.TryParse(queryString[LiveGridDataProvider.OFFSET_KEY], out this.offset);

            // get page size
            int.TryParse(queryString[LiveGridDataProvider.PAGESIZE_KEY], out this.pageSize);            

            // get sort
            Regex r = new Regex("^(s)(\\d+)$");
            foreach (string key in queryString.AllKeys)
            {
                if (r.IsMatch(key))
                {
                    LiveGridColumn column = this.columns[int.Parse(r.Match(key).Groups[2].Value) - (this.columns.IsIndexed ? 1 : 0)];
                    switch (column.Mapping.MappingType)
                    {
                        case LiveGridColumn.ColumnMappingType.Field:
                            {
                                this.sortExpression = column.Mapping.FieldMapping.FieldName;
                                this.sortDirection = (queryString[key].ToLower() != "asc") ? SortDirection.Descending : SortDirection.Ascending;
                                break;
                            }
                        case LiveGridColumn.ColumnMappingType.Expression:
                            {
                                this.sortExpression = column.Mapping.ExpressionMapping.SortFieldName;
                                this.sortDirection = (queryString[key].ToLower() != "asc") ? SortDirection.Descending : SortDirection.Ascending;
                                break;
                            }
                    }
                    break;
                }
            }

            // additional custom command name
            this.customCommandName = queryString[LiveGridDataProvider.ACTIONNAME_KEY];

            // additional custom command argument
            this.customCommandArgument = queryString[LiveGridDataProvider.ACTIONKEY_KEY];

        }

        private bool IsValidRequest()
        {
            NameValueCollection queryString = HttpContext.Current.Request.QueryString;

            return
                !string.IsNullOrEmpty(queryString[LiveGridDataProvider.ID_KEY]) &&
                !string.IsNullOrEmpty(queryString[LiveGridDataProvider.OFFSET_KEY]) &&
                !string.IsNullOrEmpty(queryString[LiveGridDataProvider.PAGESIZE_KEY]) &&
                (null != this.Context.Session[LiveGridDataProvider.GetColumnsSessionKey(queryString[LiveGridDataProvider.ID_KEY])] as LiveGridColumnCollection);
        }

        private void OnCommandClick(CommandEventArgs e)
        {
            if (null != this.CommandClick)
            {
                this.CommandClick(this, e);
            }
        }

        /// <summary>
        /// Occurs when a grid needs a data source.
        /// </summary>
        /// <param name="offset">The row offset.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalCount">The total count.</param>
        /// <returns>An IList, IEnumerable or IListSource.</returns>
        public abstract object NeedDataSource(int offset, int pageSize, out int totalCount);

        /// <summary>
        /// Occurs when a grid needs a data source.
        /// </summary>
        /// <param name="offset">The row offset.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalCount">The total count.</param>
        /// <param name="sortExpression">The sort expression.</param>
        /// <param name="sortDirection">The sort direction.</param>
        /// <returns>An IList, IEnumerable or IListSource.</returns>
        public abstract object NeedDataSource(int offset, int pageSize, out int totalCount, string sortExpression, SortDirection sortDirection);

        private void OnRowDataBound(RowDataBoundEventArgs e)
        {
            if (null != this.RowDataBound)
            {
                this.RowDataBound(this, e);
            }
        }

        private void Render()
        {
            // prepare response
            this.Context.Response.ClearContent();
            this.Context.Response.ContentType = "text/xml";
            this.Context.Response.Charset = "utf-8";

            XmlTextWriter writer = new XmlTextWriter(new MemoryStream(), System.Text.Encoding.GetEncoding("iso-8859-1"));
#if DEBUG
            writer.Formatting = Formatting.Indented;
#else
            writer.Formatting = Formatting.None;
#endif

            try
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("ajax-response");

                writer.WriteStartElement("response");
                writer.WriteAttributeString("type", "object");
                writer.WriteAttributeString("id", string.Concat(this.tableId, "_updater"));

                writer.WriteElementString("rowcount", this.totalCount.ToString());

                if (0 < this.pageSize)
                {
                    writer.WriteStartElement("rows");
                    writer.WriteAttributeString("update_ui", "true");
                    writer.WriteAttributeString("offset", this.offset.ToString());

                    // enumerate through rows
                    XmlDocument doc = new XmlDocument();
                    int index = 1;
                    foreach (object item in this.data)
                    {
                        // a row per tr element
                        XmlElement trElement = doc.CreateElement("tr");

                        // handle index column if necessary
                        if (this.columns.IsIndexed)
                        {
                            XmlElement tdElementIndex = doc.CreateElement("td");
                            tdElementIndex.InnerText = (this.offset + index).ToString();
                            trElement.AppendChild(tdElementIndex);
                        }

                        foreach (LiveGridColumn column in this.columns)
                        {
                            // a column per td element
                            XmlElement tdElement = doc.CreateElement("td");
                            switch (column.Mapping.MappingType)
                            {
                                case LiveGridColumn.ColumnMappingType.Field:
                                    {
                                        // simple field mapping
                                        if (!string.IsNullOrEmpty(column.Mapping.FieldMapping.FieldName))
                                        {
                                            tdElement.InnerText = (DataBinder.GetPropertyValue(item, column.Mapping.FieldMapping.FieldName, column.Mapping.FieldMapping.Formatting) ?? (object)string.Empty).ToString();
                                        }
                                        break;
                                    }

                                case LiveGridColumn.ColumnMappingType.Expression:
                                    {
                                        // expression mapping
                                        if ((null != column.Mapping.ExpressionMapping.Expression) && (null != column.Mapping.ExpressionMapping.ExpressionFieldNames))
                                        {
                                            object[] values = new object[column.Mapping.ExpressionMapping.ExpressionFieldNames.Length];
                                            for (int i = 0; i < column.Mapping.ExpressionMapping.ExpressionFieldNames.Length; i++)
                                            {
                                                if (null != column.Mapping.ExpressionMapping.ExpressionFieldNames[i])
                                                {
                                                    values[i] = DataBinder.GetPropertyValue(item, column.Mapping.ExpressionMapping.ExpressionFieldNames[i], null);
                                                }
                                                else
                                                {
                                                    values[i] = null;
                                                }
                                            }
                                            tdElement.InnerText = string.Format(column.Mapping.ExpressionMapping.Expression, values);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        throw new NotSupportedException("ColumnMappingType");
                                    }
                            }

                            // skip not visible columns
                            if (column.Visible)
                            {
                                trElement.AppendChild(tdElement);
                            }
                        }

                        // allow user interaction
                        this.OnRowDataBound(new RowDataBoundEventArgs(trElement));

                        // render the row
                        trElement.WriteTo(writer);

                        // increment index
                        index++;
                    }
                }
                else
                {
                    // has no point to render rows
                    writer.WriteStartElement("rows");
                    writer.WriteAttributeString("update_ui", "false");
                    writer.WriteAttributeString("offset", this.offset.ToString());
                }

                // rows
                writer.WriteEndElement();

                // response
                writer.WriteEndElement();

                // ajax-response
                writer.WriteEndElement();

                writer.Flush();

                // write the xml content to the response
                context.Response.BinaryWrite((writer.BaseStream as MemoryStream).ToArray());
            }
            finally
            {
                writer.Close();
            }

            // finalize response
            context.Response.End();
        }

        private void RenderError(string errorMessage)
        {
            // prepare response
            context.Response.ClearContent();
            context.Response.ContentType = "text/xml";
            context.Response.Charset = "utf-8";

            XmlTextWriter writer = new XmlTextWriter(new MemoryStream(), System.Text.Encoding.GetEncoding("iso-8859-1"));
#if DEBUG
            writer.Formatting = Formatting.Indented;
#else
            writer.Formatting = Formatting.None;
#endif

            try
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("ajax-response");

                writer.WriteStartElement("response");
                writer.WriteAttributeString("type", "object");
                writer.WriteAttributeString("id", this.tableId);

                writer.WriteStartElement("rows");
                writer.WriteAttributeString("update_ui", "false");
                writer.WriteEndElement();

                writer.WriteElementString("error", errorMessage ?? string.Empty);

                // response
                writer.WriteEndElement();

                // ajax-response
                writer.WriteEndElement();

                writer.Flush();

                // write the xml content to the response
                context.Response.BinaryWrite((writer.BaseStream as MemoryStream).ToArray());
            }
            finally
            {
                writer.Close();
            }

            // finalize response
            context.Response.End();
        }

        internal static string GetColumnsSessionKey(string tableId)
        {
            return string.Format("columns_{0}", tableId);
        }


        // properties

        /// <summary>
        /// Gets the current http context.
        /// </summary>
        /// <value>The current http context.</value>
        public HttpContext Context
        {
            get
            {
                if (null == this.context)
                {
                    this.context = HttpContext.Current;
                }
                return this.context;
            }
        }


        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;

            // make sure there isn't anything missing
            if (!this.IsValidRequest())
            {
                this.RenderError("Invalid request; check the query string params.");
            }

            // initialize from the current request
            this.Init();


            // handle any custom command
            if (!string.IsNullOrEmpty(customCommandName))
            {
                this.OnCommandClick(new CommandEventArgs(this.customCommandName, this.customCommandArgument));
            }

            // get the data
            object obj;
            if (string.IsNullOrEmpty(this.sortExpression))
            {
                obj = this.NeedDataSource(this.offset, this.pageSize, out this.totalCount);
            }
            else
            {
                obj = this.NeedDataSource(this.offset, this.pageSize, out this.totalCount, this.sortExpression, this.sortDirection);
            }

            // resolve data
            this.data = DataSourceHelper.GetResolvedDataSource(obj, null);

            if (null != this.data)
            {
                this.Render();
            }
            else
            {
                this.RenderError("Invalid data source; the data should be IList, IEnumerable or IListSource.");
            }
        }

        #endregion
    }


    /// <summary>
    /// Represents the method that handles RowDataBound.
    /// </summary>
    public delegate void RowDataBoundEventHandler(object sender, RowDataBoundEventArgs e);


    /// <summary>
    /// Provides data for the RowDataBound event.
    /// </summary>
    public class RowDataBoundEventArgs
    {

        // fields

        private XmlElement element = null;

        // methods

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataBoundEventArgs"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public RowDataBoundEventArgs(XmlElement element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }
            this.element = element;
        }

        // properties

        /// <summary>
        /// Gets the xml element.
        /// </summary>
        /// <value>The xml element.</value>
        public XmlElement Element
        {
            get { return this.element; }
        }

    }

}
