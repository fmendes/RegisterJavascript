using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Web;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{
    /// <summary>
    /// Represents a rating input control.
    /// </summary>
    [Themeable(true)]
    [ToolboxData("<{0}:Rating runat=\"server\" ID=\"Rating1\"></{0}:Rating>")]
    [Description("Custom ASP NET Rating control")]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class Rating : SharpControl, INamingContainer
    {

        #region Private Members
        Panel mainContainer;
        HiddenField hidValue;
        Panel pnlImageContainer;
        Panel pnlTextContainer;
        string[] messageList = new string[] { };
        float itemHeight = 23;

        int itemCount = 5;
        bool allowMultipleChanges = false;

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the item count.
        /// </summary>
        /// <value>The item count.</value>
        [Category("Display")]
        public int ItemCount
        {
            get { return this.itemCount; }
            set { this.itemCount = value; }
        }

        /// <summary>
        /// Gets or sets the current rating.
        /// </summary>
        /// <value>The current rating.</value>
        [Category("Behaviour")]
        public int CurrentRating
        {
            get 
            {
                int rating;
                if(!int.TryParse(this.hidValue.Value, out rating))
                    rating = 0;

                return rating;
            }
            set 
            {
                this.hidValue.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the width of the rating.
        /// </summary>
        /// <value>The width of the rating.</value>
        [Category("Display")]
        public float ItemHeight
        {
            get { return this.itemHeight; }
            set { this.itemHeight = value; }
        }

        /// <summary>
        /// Gets the message list.
        /// </summary>
        [Category("Display")]
        [DefaultValue((string)null), TypeConverter(typeof(StringArrayConverter))]
        [PersistenceMode(PersistenceMode.Attribute)]
        public string[] MessageList
        {
            get { return this.messageList; }
            set { this.messageList = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow multiple changes on the control.
        /// </summary>
        [Category("Behaviour")]
        public bool AllowMultipleChanges
        {
            get { return allowMultipleChanges; }
            set { allowMultipleChanges = value; }
        }

        #endregion

        /// <summary>
        /// Gets the name of the client.
        /// </summary>
        /// <value>The name of the client.</value>
        private string ClientName
        {
            get { return this.mainContainer.ClientID; }
        }

        public Rating()
        {
            this.mainContainer = new Panel();
            this.mainContainer.ID = "mainContainer";

            hidValue = new HiddenField();
            hidValue.ID = "hidValue";

            this.pnlImageContainer = new Panel();
            this.pnlImageContainer.ID = "pnlImageContainer";
            this.pnlImageContainer.CssClass = "ImageContainer";

            this.pnlTextContainer = new Panel();
            this.pnlTextContainer.ID = "pnlTextContainer";
            this.pnlTextContainer.CssClass = "TextContainer";

            this.PreRender += new EventHandler(Rating_PreRender);
        }

        void Rating_PreRender(object sender, EventArgs e)
        {
            if (this.CssClass.Length == 0)
                this.mainContainer.CssClass = "RatingContainer";
            else
                this.mainContainer.CssClass = this.CssClass;


            this.pnlTextContainer.Controls.Add(new LiteralControl("Please choose a rating!"));
            if(!Page.ClientScript.IsClientScriptIncludeRegistered("rating"))
                this.Page.ClientScript.RegisterClientScriptInclude("rating", Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.Rating.Rating.js"));
            StringBuilder sbMessageList = new StringBuilder("[");

            foreach (string message in this.MessageList)
            {
                sbMessageList.AppendFormat("{0}'{1}'", sbMessageList.Length>1 ? ",":"", message);
            }

            sbMessageList.Append("]");

            string clientInit = string.Format("var {0} = new Rating('{0}','{1}','{2}','{3}',{4}, {5}, {6}); ", this.ClientName, this.pnlImageContainer.ClientID, this.pnlTextContainer.ClientID, this.hidValue.ClientID, this.AllowMultipleChanges.ToString().ToLower(), sbMessageList, this.itemHeight);

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientName, clientInit, true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.DesignMode)
            {
                base.Render(writer);
                return;
            }

            //skip rendering the control
            this.mainContainer.RenderControl(writer);
        }

        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.Rating.ratingDemo.png"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        protected override void CreateChildControls()
        {
            
            this.Controls.Add(mainContainer);
            this.mainContainer.Controls.Add(hidValue);
            this.mainContainer.Controls.Add(pnlImageContainer);
            this.mainContainer.Controls.Add(pnlTextContainer);

            //generate items
            for (int itemIndex = 1; itemIndex <= this.ItemCount; itemIndex++)
            {
                Panel pnl = new Panel();
                pnl.ID = string.Concat("item", itemIndex);
                pnl.CssClass = "item";
                this.pnlImageContainer.Controls.Add(pnl);
                
                pnl.Attributes.Add("onmouseover", string.Format("{0}.ChangeRating({1},false)", this.ClientName, itemIndex));
                pnl.Attributes.Add("onmouseout", string.Format("{0}.RestoreRating()", this.ClientName));
                pnl.Attributes.Add("onclick", string.Format("{0}.ChangeRating({1},true)", this.ClientName, itemIndex));

                if (this.messageList != null && this.messageList.Length > itemIndex)
                    pnl.ToolTip = this.messageList[itemIndex];
            }

            base.CreateChildControls();
        }
    }
}



// alex (some can be stupid):

// conceptual ideas:
// 1. inherit from a custom control? why: done
//  - hide a lot of unusefull design time props
//  - some common features like integrated validation (for input controls)
//  - ...
// 2. when designing a new control use low level html rendering or using asp.net controls? depends on control... whichever is easier

// functional ideas:
// 3. maybe when interacting with onmousemove a mechanisn can be used to know what level is set. //done
// 4. more or less then 5 stars. //I added the example
// 5. overall status (see codeproject rating control). // v2 maybe
// 6. each image to have tooltip indicating the rating. //done, same as 1

// design time features:
// 7. custom pictures support.
// 8. design time render support.
// 9. better category for properties.

// code:
// 10. comments.
// 11. a certain code organization.
// 12. some errors (ex. [Description("Custom ASP NET Checkbox")] for rating control).

//TODO: fucking designer :(