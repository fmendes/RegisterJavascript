using System;
using System.Web.UI.Design;
using System.Text;
using System.Web;


namespace SharpPieces.Web.Controls.Design
{

    /// <summary>
    /// Provides a LiveGrid control designer class for extending the its design-mode behavior.
    /// </summary>
    public class LiveGridDesigner : ControlDesigner
    {

        // methods

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the control at design time.
        /// </summary>
        /// <returns>
        /// The HTML markup used to represent the control at design time.
        /// </returns>
        public override string GetDesignTimeHtml()
        {
            int cellWidth = 80;
            int groupHeight = 35;
            int rowHeight = 20;
            int scrollWidth = 20;
            int scrollHeight = 20;
            int truncatesTextLength = 10;

            StringBuilder sbHTML = new StringBuilder();

            LiveGrid grid = this.ViewControl as LiveGrid;
           
            // bookmark
            if (grid.Bookmarking.AllowBookmarking && (BookmarkPosition.Top == grid.Bookmarking.BookmarkPosition))
            {
                sbHTML.AppendFormat("<span style=\"font-size:10px; font-weight:bold;\">{0}</span>", grid.Bookmarking.ToString());
            }
            
            // table
            if ((null != grid.Columns) && (0 < grid.Columns.Count))
            {
                // it's relevant only if there are columns

                int visibleRows = (grid.VisibleRows > 0) ? grid.VisibleRows : 5;

                sbHTML.AppendFormat(
                    "<div style=\"overflow:scroll; height:{0}px; width:{1}px; border: solid 1px {2};\">", 
                    // group row height + header height + rows height + scroll always visible, spacing is included
                    (grid.AllowGrouping ? 1 : 0) * (groupHeight + 1) + (1 + visibleRows) * (rowHeight + 1) + scrollHeight,
                    // columns width + scroll always visible, spacing is included
                    grid.Columns.Count * (cellWidth + 1) + scrollWidth,
                    !string.IsNullOrEmpty(grid.DataProviderPath) ? "#ffffff" : "red");
                sbHTML.Append("<table cellspacing=\"1\" cellpadding=\"0\" style=\"table-layout:fixed; border-width:0px; background-color:#c0c0c0; clear:left; float:left;\">");

                if (grid.AllowGrouping)
                {
                    // add groups
                    string currentGroup = null;
                    int inheritCount = 0;
                    sbHTML.Append("<tr style=\"background-color:#aaaaaa; color:#ffffff; font-weight:bold;\">");
                    foreach (LiveGridColumn column in grid.Columns)
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
                                        sbHTML.AppendFormat(
                                            "<td colspan=\"{0}\" style=\"width:{1}px; height:{2}px; white-space:nowrap; overflow:hidden;\">{3}</td>", 
                                            inheritCount, 
                                            inheritCount * (cellWidth + 1) - 1, 
                                            groupHeight,
                                            HttpUtility.HtmlEncode(this.GetTruncatedText(currentGroup, inheritCount * truncatesTextLength)));
                                    }

                                    currentGroup = string.Empty;
                                    inheritCount = 1;
                                    break;
                                }

                            case LiveGridColumn.ColumnGroupingType.New:
                                {
                                    if (0 < inheritCount)
                                    {
                                        sbHTML.AppendFormat(
                                            "<td colspan=\"{0}\" style=\"width:{1}px; height:{2}px; white-space:nowrap; overflow:hidden;\">{3}</td>",
                                            inheritCount,
                                            inheritCount * (cellWidth + 1) - 1,
                                            groupHeight,
                                            HttpUtility.HtmlEncode(this.GetTruncatedText(currentGroup, inheritCount * truncatesTextLength)));
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
                        sbHTML.AppendFormat(
                            "<td colspan=\"{0}\" style=\"width:{1}px; height:{2}px; white-space:nowrap; overflow:hidden;\">{3}</td>",
                            inheritCount,
                            inheritCount * (cellWidth + 1) - 1,
                            groupHeight,
                            HttpUtility.HtmlEncode(this.GetTruncatedText(currentGroup, inheritCount * truncatesTextLength)));
                    }

                    sbHTML.Append("</tr>");
                }                

                // add columns
                sbHTML.AppendFormat("<tr style=\"background-color:#aaaaaa; color:#ffffff; font-weight:bold;\">", rowHeight);
                foreach (LiveGridColumn column in grid.Columns)
                {
                    sbHTML.AppendFormat(
                        "<td style=\"text-decoration:{0}; width:{1}px; height:{2}px; white-space:nowrap; overflow:hidden;\">{3}</td>", 
                        column.Visible ? "none" : "line-through", 
                        cellWidth, 
                        rowHeight,
                        HttpUtility.HtmlEncode(this.GetTruncatedText(column.HeaderText, truncatesTextLength)));
                }
                sbHTML.Append("</tr>");

                // add rows                
                for (int i = 0; i < visibleRows; i++)
                {
                    if (i % 2 == 0)
                    {
                        sbHTML.Append("<tr style=\"background-color:#eeeeee;\">");
                    }
                    else
                    {
                        sbHTML.Append("<tr style=\"background-color:#fcfcfc;\">");
                    }

                    for (int j = 0; j < grid.Columns.Count; j++)
                    {
                        sbHTML.AppendFormat(
                            "<td style=\"text-decoration:{0}; width:{1}px; height:{2}px; white-space:nowrap; overflow:hidden;\">{3}</td>", 
                            grid.Columns[j].Visible ? "none" : "line-through", 
                            cellWidth, 
                            rowHeight,
                            HttpUtility.HtmlEncode(this.GetTruncatedText(grid.Columns[j].Mapping.ToString(), truncatesTextLength)));
                    }
                    sbHTML.Append("</tr>");
                }

                sbHTML.Append("</table>");
                sbHTML.Append("</div>");
            }
            else
            {
                sbHTML.Append("No columns.");

                sbHTML.Append("<span style=\"font-size:10px; clear:left; float:left;\">http://www.openrico.org/</span>");
                sbHTML.Append("<span style=\"font-size:10px; clear:left; float:left;\">http://www.dowdybrown.com/dbprod/</span>");
                sbHTML.Append("<span style=\"font-size:10px; clear:left; float:left;\">http://www.codeplex.com/sharppieces/</span>");
            }

            if (grid.Bookmarking.AllowBookmarking && (BookmarkPosition.Bottom == grid.Bookmarking.BookmarkPosition))
            {
                sbHTML.AppendFormat("<span style=\"font-size:10px; font-weight:bold; clear:left; float:left;\">{0}</span>", grid.Bookmarking.ToString());
            }

            return sbHTML.ToString();
        }

        private string GetTruncatedText(string text, int length)
        {
            if ((null == text) || (text.Length <= Math.Max(length, 2)))
            {
                return text;
            }
            return string.Concat(text.Remove(length - 3), "...");
        }

    }

}
