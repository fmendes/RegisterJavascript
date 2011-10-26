using System;
using System.ComponentModel;
using SharpPieces.Web.Controls.Design;
using System.Web.UI;
using System.Drawing;
using System.Drawing.Design;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// A .NET approach of Rico's livegrid.
    /// Check out the LiveGrid concept at http://www.openrico.org/ or http://www.dowdybrown.com/dbprod/ .
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [Designer(typeof(LiveGridDesigner)), Description("SharpPiece wrapper over Rico LiveGrid 2.0."), DefaultProperty("DataProviderPath")]
    [ToolboxBitmap(typeof(LiveGrid), "Resources.LiveGrid.Grid.bmp")]
    [ToolboxData("<{0}:LiveGrid runat=\"server\"></{0}:LiveGrid>"), ParseChildren(true, "Columns"), PersistChildren(false), Themeable(true)]
    public class LiveGrid : Control
    {

        // fields

        internal const string SCROLLPOS_SUFFIX = "_scrollPos";
        internal static readonly string BOOKMARK_CLASSNAME = "ricoBookmark";

        private string dataProviderPath = null;
        private LiveGridColumnCollection columns = new LiveGridColumnCollection();
        private bool allowResizing = true;
        private string resizeImageUrl = null;
        private byte? frozenColumnsCount = null;
        private bool allowGrouping = false;
        private bool isHeaderBeforeSort = true;
        private int visibleRows = -1;
        private int initialScrollTo = 0;
        private HighlightType highlight = HighlightType.None;
        private string loadingImageUrl = null;
        private string waitingForDataText = null;
        private bool useClientCache = false;
        private bool useCustomScripts = false;
        private bool useCustomStyles = false;
        private LiveGridSorting sorting = new LiveGridSorting();
        private LiveGridBookmarking bookmarking = new LiveGridBookmarking();


        // methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Page.IsPostBack)
            {
                int ofsset;
                if (int.TryParse(this.Page.Request.Form[this.ScrollPosClientId], out ofsset))
                {
                    this.InitialScrollTo = ofsset;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // add style support
            if (!this.useCustomStyles)
            {
                // declare rico styles
                this.InitializeBuiltInStyles();
            }

            // add script support
            if (!this.useCustomScripts)
            {
                // declare rico scripts
                this.InitializeBuiltInScripts();
            }

            // add column command support
            // should be an extra feature in EXTRA_COLUMN?
            // or it should be an extension to RICO? so it will behave like any other js file
            if (!this.Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "CommandFunction"))
            {
                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CommandFunction", LiveGridClientScripts.GetCommandRequestImpl("grid"), true);
            }

            // add scroll handler
            if (!this.Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "ScrollHandler"))
            {
                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ScrollHandler", LiveGridClientScripts.GetScrollHandlerImpl(this.ScrollPosClientId), true);
            }

            // register init script
            this.RegisterInitScript();
        }

        private void InitializeBuiltInStyles()
        {
            // declare rico style            
            this.Page.Header.Controls.Add(new LiteralControl(LiveGridClientScripts.GetCssLink(this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGrid.css"))));
        }

        private void InitializeBuiltInScripts()
        {
            // prototype.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.prototype.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.prototype.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.prototype.js"));
            }

            // rico.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.rico.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.rico.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.rico.js"));
            }

            // ricoBehaviors.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoBehaviors.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoBehaviors.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoBehaviors.js"));
            }

            // ricoCommon.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoCommon.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoCommon.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoCommon.js"));
            }

            // ricoGridCommon.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGridCommon.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGridCommon.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGridCommon.js"));
            }

            // ricoLiveGrid.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGrid.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGrid.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGrid.js"));
            }

            // ricoLiveGridAjax.js
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGridAjax.js"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGridAjax.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGridAjax.js"));
            }

            //// common.js
            //if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.common.js"))
            //{
            //    this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.common.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.common.js"));
            //}

            //// customHandlers.js
            //if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.customHandlers.js"))
            //{
            //    this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.customHandlers.js", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.customHandlers.js"));
            //}
        }

        private void RegisterInitScript()
        {
            //// ensure live grid client registration
            //if ((this.UseCustomScripts) && !this.Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "LoadModuleLiveGridAjax"))
            //{
            //    // load dinamicaly the scripts
            //    this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "LoadModuleLiveGridAjax", "Rico.loadModule('LiveGridAjax');");
            //}

            // register initialization script
            if (!this.Page.ClientScript.IsStartupScriptRegistered(this.GetType(), "OnLoad"))
            {
                List<string> bufferOptions = new List<string>();
                bufferOptions.Add(LiveGridClientScripts.GetOption("bufferTimeout", HttpContext.Current.Session.Timeout * 60 * 1000));
                if (this.UseCustomStyles && !string.IsNullOrEmpty(this.LoadingImageUrl))
                {
                    string url = this.LoadingImageUrl.StartsWith("~") ? VirtualPathUtility.ToAbsolute(this.LoadingImageUrl) : this.LoadingImageUrl;
                    //gridOptions.Add(LiveGridClientScripts.GetOption("busyImageUrl", string.Concat("'", url, "'")));
                    bufferOptions.Add(LiveGridClientScripts.GetOption("waitMsg", string.Format("\"<img src='{0}' alt='{1}' />\"", url, this.WaitingForDataText)));
                }
                else if (!string.IsNullOrEmpty(this.WaitingForDataText))
                {
                    bufferOptions.Add(LiveGridClientScripts.GetOption("waitMsg", string.Format("'{0}'", this.WaitingForDataText)));
                }

                List<string> columnSpecs = new List<string>();

                // handle index column
                if (this.IsIndexed)
                {
                    columnSpecs.Add(string.Concat(
                        "{",
                        LiveGridClientScripts.GetOption("noResize", "true"), ",",
                        LiveGridClientScripts.GetOption("canSort", "false"), ",",
                        LiveGridClientScripts.GetOption("ClassName", "'indexColumn'"),
                        "}"));
                }

                foreach (LiveGridColumn column in this.Columns)
                {
                    // set only non default column properties
                    List<string> columnSpec = new List<string>();
                    if (!column.AllowResizing)
                    {
                        columnSpec.Add(LiveGridClientScripts.GetOption("noResize", "true"));
                    }
                    if (!column.AllowSorting)
                    {
                        columnSpec.Add(LiveGridClientScripts.GetOption("canSort", "false"));
                    }
                    if (!column.Visible)
                    {
                        columnSpec.Add(LiveGridClientScripts.GetOption("visible", "false"));
                    }
                    if (!string.IsNullOrEmpty(column.CssClass))
                    {
                        columnSpec.Add(LiveGridClientScripts.GetOption("ClassName", string.Concat("'", column.CssClass, "'")));
                    }
                    columnSpecs.Add(string.Concat("{", string.Join(",", columnSpec.ToArray()), "}"));
                }

                List<string> gridOptions = new List<string>();
                gridOptions.Add(LiveGridClientScripts.GetOption("canSortDefault", this.Sorting.AllowSorting.ToString().ToLower()));
                gridOptions.Add(LiveGridClientScripts.GetOption("allowColResize", this.AllowResizing.ToString().ToLower()));
                if (this.UseCustomStyles && !string.IsNullOrEmpty(this.Sorting.SortAscendingImageUrl))
                {
                    string url = this.Sorting.SortAscendingImageUrl.StartsWith("~") ? VirtualPathUtility.ToAbsolute(this.Sorting.SortAscendingImageUrl) : this.Sorting.SortAscendingImageUrl;
                    gridOptions.Add(LiveGridClientScripts.GetOption("sortAscendImg", string.Concat("'", url, "'")));                    
                }
                else
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("sortAscendImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.sort_asc.gif"), "'")));
                }
                if (this.UseCustomStyles && !string.IsNullOrEmpty(this.Sorting.SortDescendingImageUrl))
                {
                    string url = this.Sorting.SortDescendingImageUrl.StartsWith("~") ? VirtualPathUtility.ToAbsolute(this.Sorting.SortDescendingImageUrl) : this.Sorting.SortDescendingImageUrl;
                    gridOptions.Add(LiveGridClientScripts.GetOption("sortDescendImg", string.Concat("'", url, "'")));                    
                }
                else
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("sortDescendImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.sort_desc.gif"), "'")));
                }
                if (this.UseCustomStyles && !string.IsNullOrEmpty(this.ResizeImageUrl))
                {
                    string url = this.ResizeImageUrl.StartsWith("~") ? VirtualPathUtility.ToAbsolute(this.ResizeImageUrl) : this.ResizeImageUrl;
                    gridOptions.Add(LiveGridClientScripts.GetOption("resizeBackground", string.Concat("'", url, "'")));
                }
                else 
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("resizeBackground", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.resize.gif"), "'")));
                }
                gridOptions.Add(LiveGridClientScripts.GetOption("filterImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.filtercol.gif"), "'")));
                gridOptions.Add(LiveGridClientScripts.GetOption("shadowImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow.png"), "'")));
                gridOptions.Add(LiveGridClientScripts.GetOption("shadowllImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow_ll.png"), "'")));
                gridOptions.Add(LiveGridClientScripts.GetOption("shadowurImg", string.Concat("'", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow_ur.png"), "'")));

                if (this.FrozenColumnsCount.HasValue)
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("frozenColumns", this.FrozenColumnsCount + (this.IsIndexed ? 1 : 0)));
                }
                if (this.allowGrouping)
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("headingRow", "1"));
                }
                if (!string.IsNullOrEmpty(this.Sorting.InitialSortExpression))
                {
                    int index = this.Columns.FindIndex(
                            delegate(LiveGridColumn loop)
                            {
                                switch (loop.Mapping.MappingType)
                                {
                                    case LiveGridColumn.ColumnMappingType.Field:
                                        {
                                            return string.Compare(this.Sorting.InitialSortExpression, loop.Mapping.FieldMapping.FieldName, true) == 0;
                                        }
                                    case LiveGridColumn.ColumnMappingType.Expression:
                                        {
                                            return string.Compare(this.Sorting.InitialSortExpression, loop.Mapping.ExpressionMapping.SortFieldName, true) == 0;
                                        }
                                    default:
                                        {
                                            return false;
                                        }
                                }                                
                            });
                    if (0 <= index)
                    {
                        gridOptions.Add(LiveGridClientScripts.GetOption("sortCol", index + (this.IsIndexed ? 1 : 0)));
                        gridOptions.Add(LiveGridClientScripts.GetOption("sortDir", (this.Sorting.InitialSortDirection == SortDirection.Descending) ? "'DESC'" : "'ASC'"));
                    }
                }
                if (this.IsHeaderBeforeSort)
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("hdrIconsFirst", "false"));
                }                              

                if (0 < this.InitialScrollTo)
                {
                    gridOptions.Add(LiveGridClientScripts.GetOption("offset", this.InitialScrollTo));
                }
                if (HighlightType.None != this.highlight)
                {
                    switch (this.highlight)
                    {
                        case HighlightType.CursorCell:
                            gridOptions.Add(LiveGridClientScripts.GetOption("highlightElem", "'cursorCell'"));
                            break;
                        case HighlightType.CursorRow:
                            gridOptions.Add(LiveGridClientScripts.GetOption("highlightElem", "'cursorRow'"));
                            break;
                        case HighlightType.MenuCell:
                            gridOptions.Add(LiveGridClientScripts.GetOption("highlightElem", "'menuCell'"));
                            break;
                        case HighlightType.MenuRow:
                            gridOptions.Add(LiveGridClientScripts.GetOption("highlightElem", "'menuRow'"));
                            break;
                        case HighlightType.Selection:
                            gridOptions.Add(LiveGridClientScripts.GetOption("highlightElem", "'selection'"));
                            break;
                    }
                }
                if (this.Bookmarking.AllowBookmarking)
                {
                    if (!string.IsNullOrEmpty(this.Bookmarking.BookmarkExpression) && 
                        this.Bookmarking.BookmarkExpression.Contains("{0}") && 
                        this.Bookmarking.BookmarkExpression.Contains("{1}") &
                        this.Bookmarking.BookmarkExpression.Contains("{2}"))
                    {
                        gridOptions.Add(LiveGridClientScripts.GetOption("bookmarkText", string.Format("'{0}'", this.Bookmarking.BookmarkExpression)));
                    }
                    if (!string.IsNullOrEmpty(this.Bookmarking.BookmarkTextNoRecords))
                    {
                        gridOptions.Add(LiveGridClientScripts.GetOption("bookmarkTextNoR", string.Format("'{0}'", this.Bookmarking.BookmarkTextNoRecords)));
                    }
                    if (!string.IsNullOrEmpty(this.Bookmarking.BookmarkLoadingText))
                    {
                        gridOptions.Add(LiveGridClientScripts.GetOption("bookmarkTextLoading", string.Format("'{0}'", this.Bookmarking.BookmarkLoadingText)));
                    }
                }

                gridOptions.Add(LiveGridClientScripts.GetOption("visibleRows", this.VisibleRows));
                gridOptions.Add(LiveGridClientScripts.GetOption("columnSpecs", string.Concat("[", string.Join(",", columnSpecs.ToArray()), "]")));

                gridOptions.Add(LiveGridClientScripts.GetOption("onscroll", LiveGridClientScripts.GetScrollHandler()));

                StringBuilder initializationScript = new StringBuilder();

                initializationScript.AppendLine("var grid;");
                initializationScript.AppendLine("var rico_buffer;");

                initializationScript.AppendLine("Rico.onLoad(function() {");

                initializationScript.AppendLine(LiveGridClientScripts.GetArrayDeclaration("buffer_options", bufferOptions.ToArray()));

                initializationScript.AppendLine(LiveGridClientScripts.GetNewBuffer("rico_buffer", this.DataProviderPath, "buffer_options"));

                initializationScript.AppendLine(LiveGridClientScripts.GetArrayDeclaration("grid_options", gridOptions.ToArray()));

                initializationScript.AppendLine(LiveGridClientScripts.GetNewLiveGrid("grid", this.ClientID, "rico_buffer", "grid_options"));

                // end function body
                initializationScript.AppendLine("});");

                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "OnLoad", initializationScript.ToString(), true);

            }
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"></see> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"></see> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // scroll handler
            this.RenderScrollHandler(writer);

            // bookmark
            if (BookmarkPosition.Top == this.bookmarking.BookmarkPosition)
            {
                this.RenderBookmark(writer);
            }

            // template table
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);

            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0px");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Thead);

            // add group columns if needed
            if (this.allowGrouping)
            {
                string currentGroup = null;
                int inheritCount = 0;

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                // handle index column in group context
                if (this.IsIndexed)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.RenderEndTag();
                }

                foreach (LiveGridColumn column in this.Columns)
                {
                    if (!column.Visible)
                    {
                        inheritCount++;
                        continue;
                    }

                    switch (column.Grouping.GroupingType)
                    {
                        case LiveGridColumn.ColumnGroupingType.None:
                            {
                                if (0 < inheritCount)
                                {
                                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, inheritCount.ToString());
                                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                    writer.Write(currentGroup ?? string.Empty);
                                    writer.RenderEndTag();
                                }

                                currentGroup = string.Empty;
                                inheritCount = 1;
                                break;
                            }

                        case LiveGridColumn.ColumnGroupingType.New:
                            {
                                if (0 < inheritCount)
                                {
                                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, inheritCount.ToString());
                                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                    writer.Write(currentGroup ?? string.Empty);
                                    writer.RenderEndTag();
                                }

                                currentGroup = column.Grouping.GroupText;
                                inheritCount = 1;
                                break;
                            }

                        case LiveGridColumn.ColumnGroupingType.Inherit:
                        default:
                            {
                                inheritCount++;
                                break;
                            }
                    }
                }

                if (0 < inheritCount)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, inheritCount.ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.Write(currentGroup ?? string.Empty);
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            // template table body
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // handle index column in column context
            if (this.IsIndexed)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                writer.RenderEndTag();
            }

            foreach (LiveGridColumn column in this.columns)
            {
                if (!string.IsNullOrEmpty(column.CssClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, column.CssClass);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                if (column.Visible)
                {
                    writer.Write(column.HeaderText ?? string.Empty);
                }
                writer.RenderEndTag();
            }
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();

            // bookmark
            if (BookmarkPosition.Bottom == this.bookmarking.BookmarkPosition)
            {
                this.RenderBookmark(writer);
            }

            // hide the grid settings for the handler
            if ((null != this.Page) && (null != this.ClientID))
            {
                this.Page.Session[LiveGridDataProvider.GetColumnsSessionKey(this.ClientID)] = this.columns;
            }
        }

        private void RenderScrollHandler(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ScrollPosClientId);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ScrollPosClientId);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.InitialScrollTo.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private void RenderBookmark(HtmlTextWriter writer)
        {
            if (this.bookmarking.AllowBookmarking && !string.IsNullOrEmpty(this.bookmarking.BookmarkExpression))
            {
                if (this.bookmarking.BookmarkExpression.Contains("{0}") && this.bookmarking.BookmarkExpression.Contains("{1}") & this.bookmarking.BookmarkExpression.Contains("{2}"))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, LiveGrid.BOOKMARK_CLASSNAME);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);

                    if (this.bookmarking.AllowBookmarkScroll)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format("javascript:{0}.pageUp();", "grid"));
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                        writer.Write("Prev. ");

                        // a
                        writer.RenderEndTag();

                        writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format("javascript:{0}.pageDown();", "grid"));
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                        writer.Write(" Next");

                        // a
                        writer.RenderEndTag();

                        writer.Write("&nbsp;");
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.BookmarkClientId);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);

                    // span
                    writer.RenderEndTag();

                    // div
                    writer.RenderEndTag();
                }
            }
        }

        
        // properties

        /// <summary>
        /// Gets or sets the path to the http handler which feeds the grid with data.
        /// </summary>
        /// <value>The path to the http handler which provides the XML data.</value>
        [Category("Data"), Description("The path to the http handler which feeds the grid with data."), DefaultValue((string)null), NotifyParentProperty(true)]
        public virtual string DataProviderPath
        {
            get { return this.dataProviderPath; }
            set { this.dataProviderPath = value; }
        }

        /// <summary>
        /// Gets or sets the grid columns.
        /// </summary>
        /// <value>The grid columns.</value>
        [Category("Default"), Description("The grid columns."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public LiveGridColumnCollection Columns
        {
            get
            {
                if (null == this.columns)
                {
                    this.columns = new LiveGridColumnCollection();
                }
                return this.columns;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should contain an index column.
        /// </summary>
        /// <value><c>True</c> if the grid should contain an index column; otherwise, <c>false</c>.</value>
        [Category("Default"), Description("Indicates whether the should contain an index column."), DefaultValue(false)]
        public virtual bool IsIndexed
        {
            get { return this.columns.IsIndexed; }
            set { this.columns.IsIndexed = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the grid supports resizeing.
        /// </summary>
        /// <value><c>true</c> if the grid supports resizeing; otherwise, <c>false</c>.</value>
        [Category("Behavior"), Description("Indicates whether the grid supports resizeing."), DefaultValue(true)]
        public virtual bool AllowResizing
        {
            get { return this.allowResizing; }
            set { this.allowResizing = value; }
        }

        /// <summary>
        /// Gets or sets the image URL for the column resizeing. 
        /// A default one will be provided when this one is null.
        /// </summary>
        /// <value>The image URL fot the column resizeing.</value>
        [Category("Appearance"), Description("The image URL for the column resizeing."), DefaultValue((string)null)]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
        public virtual string ResizeImageUrl
        {
            get { return this.resizeImageUrl; }
            set { this.resizeImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the first n frozen columns.
        /// </summary>
        /// <value>The first n frozen columns.</value>
        [Category("Behavior"), Description("The first n frozen columns."), DefaultValue((string)null)]
        public byte? FrozenColumnsCount
        {
            get { return this.frozenColumnsCount; }
            set { this.frozenColumnsCount = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the columns are using groups.
        /// </summary>
        /// <value><c>True</c> if the columns are using groups; otherwise, <c>false</c>.</value>
        [Category("Behavior"), Description("Indicates whether the columns support groups."), DefaultValue(false)]
        public virtual bool AllowGrouping
        {
            get { return this.allowGrouping; }
            set { this.allowGrouping = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether on the header text comes before the sort image or not.
        /// </summary>
        /// <value>
        /// 	<c>True</c> if the header text comes before the sort image; otherwise, <c>false</c>.
        /// </value>
        [Category("Appearance"), Description("Indicates whether on the header text comes before the sort image or not."), DefaultValue(true)]
        public virtual bool IsHeaderBeforeSort
        {
            get { return this.isHeaderBeforeSort; }
            set { this.isHeaderBeforeSort = value; }
        }

        /// <summary>
        /// Gets or sets the number of visible rows.
        /// Special values: -3 sizes to body, -2 to data, -1 to window.
        /// Default is 0.
        /// </summary>
        /// <value>The number visible rows.</value>
        [Category("Behavior"), Description("The number of visible rows.\r\nSpecial values: -3  forces the grid to fit to the body, -2 to the data, -1 to the window.\r\nDefault is -1."), DefaultValue(-1)]
        public virtual int VisibleRows
        {
            get { return this.visibleRows; }
            set { this.visibleRows = value; }
        }

        /// <summary>
        /// Gets or sets the index of the first visible row.
        /// </summary>
        /// <value>The index of the first visible row.</value>
        [Category("Behavior"), Description("The index of the first visible row."), DefaultValue(0)]
        public virtual int InitialScrollTo
        {
            get { return this.initialScrollTo; }
            set { this.initialScrollTo = value; }
        }

        /// <summary>
        /// Gets or sets the type of row highlight.
        /// </summary>
        /// <value>The type of row highlight.</value>
        [Category("Appearance"), Description("The type of row highlight."), DefaultValue(HighlightType.None)]
        public virtual HighlightType Highlight
        {
            get { return this.highlight; }
            set { this.highlight = value; }
        }

        /// <summary>
        /// Gets or sets the loading image URL. 
        /// A default text is provided when null.
        /// </summary>
        /// <value>The loading image URL.</value>
        [Category("Appearance"), Description("The loading image URL."), DefaultValue((string)null)]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
        public string LoadingImageUrl
        {
            get { return this.loadingImageUrl; }
            set { this.loadingImageUrl = value;  }
        }

        /// <summary>
        /// Gets or sets the 'waiting for data' text.
        /// </summary>
        /// <value>The 'waiting for data' text.</value>
        [Category("Appearance"), Description(""), DefaultValue("Waiting for data")]
        public string WaitingForDataText
        {
            get { return !string.IsNullOrEmpty(this.waitingForDataText) ? this.waitingForDataText : "Waiting for data"; }
            set { this.waitingForDataText = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the grid data should be cached on the client.
        /// Set enabled when performance is required rather then freshness.
        /// </summary>
        /// <value><c>true</c> if the grid data should be cached on the client; otherwise, <c>false</c>.</value>
        [Category("Performance"), Description("Indicates whether the grid data should be cached on client."), DefaultValue(false)]
        public virtual bool UseClientCache
        {
            get { return this.useClientCache; }
            set { this.useClientCache = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scripts embedded from the dll will be used or not.
        /// </summary>
        /// <value><c>True</c> if the scripts embedded from the dll; otherwise, <c>false</c>.</value>
        [Category("Performance"), Description("Indicates whether the js scripts, css are the dll embedded ones or not."), DefaultValue(false)]
        public virtual bool UseCustomScripts
        {
            get { return this.useCustomScripts; }
            set { this.useCustomScripts = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the css embedded from the dll will be used or not.
        /// </summary>
        /// <value><c>True</c> if the css embedded from the dll; otherwise, <c>false</c>.</value>
        [Category("Performance"), Description("Indicates whether the css are the dll embedded ones or not."), DefaultValue(false)]
        public virtual bool UseCustomStyles
        {
            get { return this.useCustomStyles; }
            set { this.useCustomStyles = value; }
        }

        /// <summary>
        /// Gets or sets the sorting customization.
        /// </summary>
        /// <value>The sorting customization.</value>
        [Category("Behavior"), Description("The sorting customization."), DefaultValue((string)null), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public LiveGridSorting Sorting
        {
            get
            {
                if (null == this.sorting)
                {
                    this.sorting = new LiveGridSorting();
                }
                return this.sorting;
            }
            set { this.sorting = value; }
        }

        /// <summary>
        /// Gets or sets the bookmarking customization.
        /// </summary>
        /// <value>The bookmarking customization.</value>
        [Category("Behavior"), Description("The bookmarking customization."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public LiveGridBookmarking Bookmarking
        {
            get
            {
                if (null == this.bookmarking)
                {
                    this.bookmarking = new LiveGridBookmarking();
                }
                return this.bookmarking;
            }
            set { this.bookmarking = value; }
        }

        /// <summary>
        /// Gets the name of the control tag. This property is used primarily by control developers.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the control tag.</returns>
        protected virtual string TagName
        {
            get { return Enum.Format(typeof(HtmlTextWriterTag), HtmlTextWriterTag.Table, "G").ToLower(); }
        }

        /// <summary>
        /// ViewState for LiveGrid is pointless.
        /// </summary>
        [Browsable(false)]
        public override bool EnableViewState
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Gets a value indicating whether a control is being used on a design surface.
        /// </summary>
        /// <value></value>
        /// <returns>true if the control is being used in a designer; otherwise, false.</returns>
        protected internal new bool DesignMode
        {
            get { return base.DesignMode; }
        }

        /// <summary>
        /// Gets the bookmark client id.
        /// </summary>
        /// <value>The bookmark client id.</value>
        protected virtual string BookmarkClientId
        {
            get { return string.Concat(this.ClientID, "_bookmark"); }
        }

        private string ScrollPosClientId
        {
            get { return string.Concat(this.ClientID, LiveGrid.SCROLLPOS_SUFFIX); }
        }


        // nested types

        /// <summary>
        /// Represent a set of sort related LiveGrid properties.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("AllowSorting")]
        public class LiveGridSorting
        {

            // fields

            private bool allowSorting = true;
            private string initialSortExpression = null;
            private SortDirection initialSortDirection = SortDirection.Ascending;
            private string sortAscendingImageUrl = null;
            private string sortDescendingImageUrl = null;


            //properties            

            /// <summary>
            /// Gets or sets a value indicating whether the grid supports sorting.
            /// </summary>
            /// <value><c>true</c> if the grid supports sorting; otherwise, <c>false</c>.</value>
            [Category("Behavior"), Description("Indicates whether the grid supports sorting."), DefaultValue(true)]
            public virtual bool AllowSorting
            {
                get { return this.allowSorting; }
                set { this.allowSorting = value; }
            }

            /// <summary>
            /// Gets or sets the image URL for the ascending sort. 
            /// A default one will be provided when this one is null.
            /// </summary>
            /// <value>The image URL for the ascending sort.</value>
            [Category("Appearance"), Description("The image URL for the ascending sort."), DefaultValue((string)null)]
            [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
            public string SortAscendingImageUrl
            {
                get { return this.sortAscendingImageUrl; }
                set { this.sortAscendingImageUrl = value; }
            }
            
            /// <summary>
            /// Gets or sets the image URL for the descending sort. 
            /// A default one will be provided when this one is null.
            /// </summary>
            /// <value>The image URL for the descending sort.</value>
            [Category("Appearance"), Description("The image URL for the descending sort."), DefaultValue(null)]
            [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty]
            public string SortDescendingImageUrl
            {
                get { return this.sortDescendingImageUrl; }
                set { this.sortDescendingImageUrl = value; }
            }

            /// <summary>
            /// Gets or sets the initial sort expression.
            /// </summary>
            /// <value>The index for the column that takes an initial sort.</value>
            [Category("Sorting"), Description("The initial sort expression."), DefaultValue((string)null)]
            public virtual string InitialSortExpression
            {
                get { return this.initialSortExpression; }
                set { this.initialSortExpression = value; }
            }
            
            /// <summary>
            /// Gets or sets the sort direction for the column that takes an initial sort.
            /// </summary>
            /// <value>The sort direction for the column that takes an initial sort.</value>
            [Category("Sorting"), Description("The sort direction for the column that takes an initial sort."), DefaultValue(SortDirection.Ascending)]
            public virtual SortDirection InitialSortDirection
            {
                get { return this.initialSortDirection; }
                set { this.initialSortDirection = value; }
            }


            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                if (this.allowSorting && !string.IsNullOrEmpty(this.initialSortExpression))
                {
                    return string.Format("{0} {1}", this.initialSortExpression, (SortDirection.Ascending == this.initialSortDirection) ? "ascending" : "descending");
                }
                else
                {
                    return null;
                }
            }

        }


        /// <summary>
        /// Represent a set of bookmark related LiveGrid properties.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("AllowBookmarking")]
        public class LiveGridBookmarking
        {

            // fields

            private bool allowBookmarking = false;
            private string bookmarkExpression = null;
            private string bookmarkTextNoRecords = null;
            private string bookmarkLoadingText = null;
            private BookmarkPosition bookmarkPosition = BookmarkPosition.Top;
            private bool allowBookmarkScroll = false;


            // properties            

            /// <summary>
            /// Gets or sets a value indicating whether the grid supports bookmarking.
            /// </summary>
            /// <value><c>True</c> if grid supports bookmarking; otherwise, <c>false</c>.</value>
            [Category("Behavior"), Description("Indicates whether the grid supports bookmarking."), DefaultValue(false), NotifyParentProperty(true)]
            public virtual bool AllowBookmarking
            {
                get { return this.allowBookmarking; }
                set { this.allowBookmarking = value; }
            }
            
            /// <summary>
            /// Gets or sets the bookmark text expression.
            /// The text simply is a unformatted expresion with 3 place holders
            /// where first is the first visible row index, second is the last 
            /// visible row index and third is the total no of rows.
            /// </summary>
            /// <value>The bookmark text expression.
            /// The text simply is a unformatted expresion with 3 place holders
            /// where first is the first visible row index, second is the last 
            /// visible row index and third is the total no of rows.</value>
            [Category("Bookmarking"), Description("The bookmark text expression. The text simply is a unformatted expresion with 3 place holders where first is the first visible row index, second is the last visible row index and third is the total no of rows."), DefaultValue("{0} - {1} / {2}"), NotifyParentProperty(true)]
            public virtual string BookmarkExpression
            {
                get { return !string.IsNullOrEmpty(this.bookmarkExpression) ? this.bookmarkExpression : "{0} - {1} / {2}"; }
                set { this.bookmarkExpression = value; }
            }
            
            /// <summary>
            /// Gets or sets the bookmark text for no records.
            /// </summary>
            /// <value>The bookmark text for no records.</value>
            [Category("Bookmarking"), Description("The bookmark text for no records."), DefaultValue("No records.")]
            public virtual string BookmarkTextNoRecords
            {
                get { return !string.IsNullOrEmpty(this.bookmarkTextNoRecords) ? this.bookmarkTextNoRecords : "No records."; }
                set { this.bookmarkTextNoRecords = value; }
            }

            /// <summary>
            /// Gets or sets the bookmark loading text.
            /// </summary>
            /// <value>The bookmark loading text.</value>
            [Category("Bookmarking"), Description("The bookmark loading text."), DefaultValue("Loading...")]
            public virtual string BookmarkLoadingText
            {
                get { return !string.IsNullOrEmpty(this.bookmarkLoadingText) ? this.bookmarkLoadingText : "Loading..."; }
                set { this.bookmarkLoadingText = value; }
            }
            
            /// <summary>
            /// Gets or sets the position of the bookmark relative to the grid.
            /// </summary>
            /// <value>The position of the bookmark relative to the grid.</value>
            [Category("Bookmarking"), Description("Indicates whether the bookmark will be placed on top or on bottom of the grid."), DefaultValue(BookmarkPosition.Top), NotifyParentProperty(true)]
            public virtual BookmarkPosition BookmarkPosition
            {
                get { return this.bookmarkPosition; }
                set { this.bookmarkPosition = value; }
            }
            
            /// <summary>
            /// Gets or sets a value indicating whether the grid supports scrolling.
            /// </summary>
            /// <value><c>True</c> whether the grid supports scrolling; otherwise, <c>false</c>.</value>
            [Category("Bookmarking"), Description("Indicates whether the grid supports scrolling."), DefaultValue(false)]
            public virtual bool AllowBookmarkScroll
            {
                get { return this.allowBookmarkScroll; }
                set { this.allowBookmarkScroll = value; }
            }


            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                if (this.allowBookmarking)
                {
                    if (this.IsVaid())
                    {
                        return string.Format(this.bookmarkExpression, 233, 256, 9);
                    }
                    else
                    {
                        return "(invalid)";
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            private bool IsVaid()
            {
                return !this.AllowBookmarking || (!string.IsNullOrEmpty(this.BookmarkExpression) && this.BookmarkExpression.Contains("{0}") && this.BookmarkExpression.Contains("{1}") && this.BookmarkExpression.Contains("{2}"));
            }

        }

    }


    /// <summary>
    /// The highlight type supported by LiveGrid.
    /// </summary>
    public enum HighlightType
    {
        None,
        CursorCell,
        CursorRow,
        MenuCell,
        MenuRow,
        Selection
    }


    /// <summary>
    /// The highlight type supported by LiveGrid.
    /// </summary>
    public enum BookmarkPosition
    {
        Top,
        Bottom
    }
    
}