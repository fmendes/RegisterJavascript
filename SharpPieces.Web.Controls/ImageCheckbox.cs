using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Specialized;
using SharpPieces.Web.Controls.Design;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a checkbox image control.
    /// </summary>
    [Themeable(true)]
    [DefaultProperty("Text"), DefaultEvent("CheckedChanged")]
    [ToolboxData("<{0}:ImageCheckBox runat=\"server\" ID=\"ImageCheckBox1\" Text=\"Enter text here\"></{0}:ImageCheckBox>")]
    [Description("Custom ASP NET Checkbox")]
    [Designer(typeof(ImageCheckBoxDesigner))]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ImageCheckBox : SharpControl, INamingContainer, IPostBackDataHandler, ICheckBoxControl
    {

        #region Private Variables
        string _text;
        string _uncheckedImageUrl = string.Empty;
        string _checkedImageUrl = string.Empty;
        bool _readOnly = false;

        Image _imgCheck;
        HiddenField _hidValue;
        HiddenField _hidCheckState;
        private string _onClientBeforeChange;
        private string _onClientAfterChange;

        private bool _autoPostBack = false;

        private static readonly object _EventCheckedChanged = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [Category("Data")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [Category("Data")]
        public string Value
        {
            get { return this._hidValue.Value; }
            set { this._hidValue.Value = value; }
        }

        /// <summary>
        /// Gets or sets the unchecked image URL.
        /// </summary>
        /// <value>The unchecked image URL.</value>
        [Category("Appearance")]
        public string UncheckedImageUrl
        {
            get { return _uncheckedImageUrl; }
            set { _uncheckedImageUrl = VirtualPathUtility.ToAbsolute(value); }
        }

        /// <summary>
        /// Gets or sets the checked image URL.
        /// </summary>
        /// <value>The checked image URL.</value>
        [Category("Appearance")]
        public string CheckedImageUrl
        {
            get { return _checkedImageUrl; }
            set { _checkedImageUrl = VirtualPathUtility.ToAbsolute(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox is read only.
        /// </summary>
        /// <value><c>true</c> if the checkbox is read only; otherwise, <c>false</c>.</value>
        [Category("Data")]
        public bool ReadOnly
        {
            get { return this._readOnly; }
            set { this._readOnly = value; }
        }
        
        /// <summary>
        /// Gets the name of the client object instance.
        /// </summary>
        /// <value>The name of the client object.</value>
        [Category("Client script")]
        private string ClientObjectName
        {
            get { return this.ClientID; }
        }

        /// <summary>
        /// Gets or sets the on client before change.
        /// </summary>
        /// <value>The on client before change.</value>
        [Category("Client script")]
        public string OnClientBeforeChange
        {
            get { return _onClientBeforeChange; }
            set { _onClientBeforeChange = value; }
        }

        /// <summary>
        /// Gets or sets the on client after change.
        /// </summary>
        /// <value>The on client after change.</value>
        [Category("Client script")]
        public string OnClientAfterChange
        {
            get { return _onClientAfterChange; }
            set { _onClientAfterChange = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control performa a postback on click.
        /// </summary>
        [Category("Behavior")]
        public bool AutoPostBack
        {
            get { return _autoPostBack; }
            set { _autoPostBack = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCheckBox"/> class.
        /// </summary>
        public ImageCheckBox()
        {
            _hidValue = new HiddenField();
            _hidValue.ID = "hidValue";

            _hidCheckState = new HiddenField();
            _hidCheckState.ID = "hidCheckState";

            _imgCheck = new Image();
            _imgCheck.ID = "imgCheck";

            this.Attributes["class"] = "checkbox";
        } 
        #endregion
        
        #region Internal behaviour
        /// <summary>
        /// Gets the init script.
        /// </summary>
        private string GetClientScriptInitCode()
        {
            StringBuilder sbInit = new StringBuilder();

            sbInit.AppendFormat("var {0} = new ImageCheckBox('{0}','{1}','{2}','{3}','{4}', {5}, {6}); ",
                    ClientObjectName,
                    this.UncheckedImageUrl,
                    this.CheckedImageUrl,
                    this._imgCheck.ClientID,
                    this._hidCheckState.ClientID,
                    string.IsNullOrEmpty(this._onClientBeforeChange) ? "null" : this._onClientBeforeChange,
                    string.IsNullOrEmpty(this._onClientAfterChange) ? "null" : this._onClientAfterChange
                    );

            return sbInit.ToString();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Controls.Add(_hidValue);
            this.Controls.Add(_hidCheckState);
            this.Controls.Add(_imgCheck);

            base.CreateChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //default images
            if (string.IsNullOrEmpty(this._uncheckedImageUrl))
                this._uncheckedImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.ImageCheckBox.unchecked.gif");

            if (string.IsNullOrEmpty(this._checkedImageUrl))
                this._checkedImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.ImageCheckBox.checked.gif");

            //add the initialization script
            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("checkbox"))
                this.Page.ClientScript.RegisterClientScriptInclude("checkbox", Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.ImageCheckBox.ImageCheckBox.js"));
            if (!this.Page.ClientScript.IsStartupScriptRegistered(this.ClientID))
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), ClientObjectName, this.GetClientScriptInitCode(), true);

            base.OnPreRender(e);

            if ((this.Page != null))
            {
                this.Page.RegisterRequiresPostBack(this);
            }
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"></see> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.DesignMode)
            {
                base.Render(writer);
                return;
            }

            //check/uncheck the control.
            if (this.Checked)
            {
                this._imgCheck.ImageUrl = CheckedImageUrl;
            }
            else
                this._imgCheck.ImageUrl = UncheckedImageUrl;

            this.Attributes.Add("onselectstart", "return false;");
            this.Attributes["href"] = "javascript:void(0);";
            this.Controls.Add(new LiteralControl(this._text));

            string onClickPostback = null;
            string keyPressPostback = null;

            if ((this.Page != null) && !this._readOnly && base.IsEnabled)
            {
                if (this._autoPostBack)
                {
                    PostBackOptions options = new PostBackOptions(this, string.Empty);

                    onClickPostback = Util.MergeScripts(new Pair<string>(string.Concat("if(!",ClientObjectName, ".ChangeState()) return false;"), this.Page.ClientScript.GetPostBackEventReference(options, true)));

                    keyPressPostback = Util.MergeScripts(new Pair<string>(string.Concat("if(!",ClientObjectName, ".CheckOnSpace(event)) return false;"), this.Page.ClientScript.GetPostBackEventReference(options, true)));
                }
                else
                {
                    onClickPostback = string.Concat(ClientObjectName, ".ChangeState();");
                    keyPressPostback = string.Concat(ClientObjectName, ".CheckOnSpace(event);");
                }

            }
            else if (this._readOnly)
                this.Attributes["class"] += " readonly";

            this.Attributes.Add("onClick", onClickPostback);
            this.Attributes.Add("onkeypress", keyPressPostback);

            this.Attributes["class"] += this.CssClass;

            writer.WriteBeginTag("a");
            string style = "";

            if (this.Width.Value > 0)
                style += "width: " + this.Width.ToString()+";";

            foreach (string attributeKey in this.Attributes.Keys)
            {
                if (!string.IsNullOrEmpty(this.Attributes[attributeKey]) && attributeKey != "style")
                    writer.WriteAttribute(attributeKey, this.Attributes[attributeKey]);
                else if (attributeKey == "style")
                    style += this.Attributes[attributeKey];
            }

            if(!string.IsNullOrEmpty(style))
                writer.WriteAttribute("style", style);
            
            foreach (Control ctl in this.Controls)
                ctl.RenderControl(writer);

            writer.WriteEndTag("a");
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            writer.Write(string.Format("<input type='checkbox'{1}>{0}<br />", this.Text, this.Checked ? "checked='checked'" : string.Empty));
        } 
        #endregion

        #region IPostBackDataHandler Members

        /// <summary>
        /// When implemented by a class, processes postback data for an ASP.NET server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>
        /// true if the server control's state changes as a result of the postback; otherwise, false.
        /// </returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return true;
        }

        /// <summary>
        /// When implemented by a class, signals the server control to notify the ASP.NET application that the state of the control has changed.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            this.OnCheckedChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="E:CheckedChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)base.Events[_EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region ICheckBoxControl Members

        /// <summary>
        /// Occurs when the value of the <see cref="P:System.Web.UI.ICheckBoxControl.Checked"></see> property changes between posts to the server.
        /// </summary>
        [Category("Action")]
        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(_EventCheckedChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(_EventCheckedChanged, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox is checked.
        /// </summary>
        [Category("Data")]
        public bool Checked
        {
            get { return this._hidCheckState.Value == "1"; }
            set { this._hidCheckState.Value = value ? "1" : "0"; }
        }

        #endregion
    }
}
