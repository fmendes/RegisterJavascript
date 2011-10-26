using System;
using System.Web;
using System.Security.Permissions;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Design;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a dynamicaly generated input gradient image.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [Description("Dynamicaly generated input gradient image."), DefaultProperty("Text"), DefaultEvent("Click")]
    [ToolboxData("<{0}:GradientButton runat=\"server\"></{0}:GradientButton>"), PersistChildren(false), ParseChildren(true), Themeable(true)]
    public class GradientButton : DynamicImage, IButtonControl, IPostBackEventHandler
    {

        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientButton"/> class.
        /// </summary>
        public GradientButton()
            : base(HtmlTextWriterTag.Input)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientButton"/> class.
        /// </summary>
        /// <param name="tag">An HTML tag.</param>
        public GradientButton(string tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientButton"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public GradientButton(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"></see> that represents an optional event argument to be passed to the event handler.</param>
        protected virtual void RaisePostBackEvent(string eventArgument)
        {
            if (this.CausesValidation)
            {
                this.Page.Validate(this.ValidationGroup);
            }

            this.OnClick(EventArgs.Empty);
            this.OnCommand(new CommandEventArgs(this.CommandName, this.CommandArgument));
        }

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        protected virtual void OnClick(EventArgs e)
        {
            if (null != this.Click)
            {
                this.Click(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Command"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.CommandEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCommand(CommandEventArgs e)
        {
            if (null != this.Command)
            {
                this.Command(this, e);
            }
            base.RaiseBubbleEvent(this, e);
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            //// add the image
            //if ((Unit.Empty != this.Width) || (Unit.Empty != this.Height))
            //{
            //    if (Unit.Empty != this.Width)
            //    {
            //        writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
            //    }
            //    if (Unit.Empty != this.Height)
            //    {
            //        writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());
            //    }
            //}
            //else
            //{
            //    writer.AddAttribute(HtmlTextWriterAttribute.Width, "70px");
            //}

            // temp
            writer.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.DynamicImage.gradient.png"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        /// <summary>
        /// Adds HTML attributes and styles that need to be rendered to the specified <see cref="T:System.Web.UI.HtmlTextWriterTag"></see>. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"></see> that represents the output stream to render HTML content on the client.</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            this.Source = null;

            base.ResizeHandler = ResizeHandler.Server;

            if (Unit.Empty == this.Width)
            {
                this.Width = 100;
            }

            if (Unit.Empty == this.Height)
            {
                this.Height = 30;
            }

            // add type
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");

            // add name
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);

            // handle the custom onclick
            string customOnClick = this.OnClientClick;
            if (!string.IsNullOrEmpty(this.Attributes["onclick"]))
            {
                customOnClick = (!string.IsNullOrEmpty(customOnClick)) ? Util.MergeScripts(new Pair<string>(customOnClick, this.Attributes["onclick"])) : this.Attributes["onclick"];
                this.Attributes.Remove("onclick");
            }

            // add image attributes
            base.AddAttributesToRender(writer);

            // add onclick handler
            if (this.Enabled && (null != this.Page))
            {
                PostBackOptions postBackOptions = new PostBackOptions(this, string.Empty);

                // prevent submit behavior
                postBackOptions.ClientSubmit = true;

                // handle any cross page post
                if (!string.IsNullOrEmpty(this.PostBackUrl))
                {
                    postBackOptions.ActionUrl = HttpUtility.UrlPathEncode(base.ResolveClientUrl(this.PostBackUrl));
                }

                // add validation handling
                if ((this.CausesValidation && (this.Page != null)) && (0 < this.Page.GetValidators(this.ValidationGroup).Count))
                {
                    postBackOptions.PerformValidation = true;
                    postBackOptions.ValidationGroup = this.ValidationGroup;
                }

                // add onclick javascript
                string postBackScript = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, false);
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Util.MergeScripts(new Pair<string>(customOnClick, postBackScript)));
            }
        }


        // Properties

        /// <summary>
        /// Gets or sets the dynamic image format type.
        /// </summary>
        /// <value>The dynamic image format type.</value>
        [Description("The dynamic image format type."), Category("Transformation"), DefaultValue(DynamicImageFormat.Png)]
        [Themeable(false)]
        public override DynamicImageFormat ImageFormat
        {
            get { return (null != this.ViewState["ImageFormat"]) ? (DynamicImageFormat)this.ViewState["ImageFormat"] : DynamicImageFormat.Png; }
            set { this.ViewState["ImageFormat"] = value; }
        }

        /// <summary>
        /// Gets or sets the button gradient background.
        /// </summary>
        /// <value>The button gradient background.</value>
        [Description("The button gradient background."), Category("Appearance"), DefaultValue(""), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty), Themeable(true)]
        public virtual GradientBackground GradientBackground
        {
            get
            {
                if (null == this.ViewState["GradientBackground"])
                {
                    GradientBackground gradientBackground = new GradientBackground();
                    if (this.EnableViewState)
                    {
                        gradientBackground.TrackViewState();
                    }
                    this.ViewState["GradientBackground"] = gradientBackground;
                }
                return (GradientBackground)this.ViewState["GradientBackground"];
            }
            set
            {
                if ((null != value) && this.EnableViewState)
                {
                    value.TrackViewState();
                }
                this.ViewState["GradientBackground"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image dynamic text.
        /// </summary>
        /// <value>The image dynamic text.</value>
        public override DynamicText Text
        {
            get
            {
                if (null == this.ViewState["Text"])
                {
                    DynamicText text = new DynamicText();
                    text.HorizontalAlign = StringAlignment.Center;
                    text.VerticalAlign = StringAlignment.Center;
                    if (this.EnableViewState)
                    {
                        text.TrackViewState();
                    }
                    this.ViewState["Text"] = text;
                }
                return (DynamicText)this.ViewState["Text"];
            }
            set { base.Text = value; }
        }

        /// <summary>
        /// Gets or sets the button size type.
        /// </summary>
        /// <value>The button size type.</value>
        [Description("The button size type."), Category("Appearance"), DefaultValue(TextContainerSizeType.Specified)]
        [Themeable(true)]
        public virtual TextContainerSizeType SizeType
        {
            get { return (null != this.ViewState["SizeType"]) ? (TextContainerSizeType)this.ViewState["SizeType"] : TextContainerSizeType.Specified; }
            set { this.ViewState["SizeType"] = value; }
        }

        /// <summary>
        /// The http handler type.
        /// </summary>
        [Browsable(false)]
        public override Type HttpHandlerType
        {
            get { return typeof(GradientButtonProvider); }
        }

        /// <summary>
        /// The http handler name.
        /// </summary>
        [Browsable(false)]
        public override string HttpHandlerName
        {
            get { return "gb.axd"; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to draw in grayscale.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the button should be draw in grayscale; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public override bool DrawGrayscale
        {
            get { return base.DrawGrayscale; }
            set { base.DrawGrayscale = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to draw in sepia.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the button should be draw in sepia; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public override bool DrawSepia
        {
            get { return base.DrawSepia; }
            set { base.DrawSepia = value; }
        }

        /// <summary>
        /// Gets the button resize handler.
        /// </summary>
        /// <value>The button resize handler.</value>
        [Browsable(false)]
        public new ResizeHandler ResizeHandler
        {
            get { return ResizeHandler.Server; }
        }

        /// <summary>
        /// Gets the image src.
        /// </summary>
        /// <value>The image src.</value>
        [Browsable(false)]
        public override string Src
        {
            get
            {
                return string.Concat(
                    "~/",
                    this.HttpHandlerName,
                    "?",
                    GradientButtonProvider.ToQueryString(
                        (int)Math.Round(this.Width.Value),
                        (int)Math.Round(this.Height.Value),
                        this.ClientCacheDuration,
                        this.ServerCacheDuration,
                        this.RotateFlip,
                        this.DrawGrayscale,
                        this.DrawSepia,
                        this.Text,
                        this.ImageFormat,
                        this.imageCreator,
                        this.imageTransformations,
                        this.GradientBackground,
                        this.SizeType));
            }
        }

        /// <summary>
        /// Gets or sets the client click script.
        /// </summary>
        /// <value>The client click script.</value>
        [Description("Client script for the onclick event."), Category("Behavior"), DefaultValue(""), Themeable(false)]
        public virtual string OnClientClick
        {
            get { return (null != this.ViewState["OnClientClick"]) ? (string)this.ViewState["OnClientClick"] : ""; }
            set { this.ViewState["OnClientClick"] = value; }
        }

        #region IButtonControl Members

        // Events

        /// <summary>
        /// Occurs when the button control is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Occurs when the button control is clicked.
        /// </summary>
        public event CommandEventHandler Command;


        // Properties

        /// <summary>
        /// Gets or sets a value indicating whether clicking the button causes page validation to occur.
        /// </summary>
        /// <value></value>
        /// <returns>true if clicking the button causes page validation to occur; otherwise, false.</returns>
        [Category("Behavior"), Description("Indicates whether clicking the button causes page validation to occur"), DefaultValue(true)]
        [Themeable(false)]
        public bool CausesValidation
        {
            get { return (null != this.ViewState["CausesValidation"]) ? (bool)this.ViewState["CausesValidation"] : true; }
            set { this.ViewState["CausesValidation"] = value; }
        }

        /// <summary>
        /// Gets or sets an optional argument that is propagated to the <see cref="E:System.Web.UI.WebControls.IButtonControl.Command"></see> event.
        /// </summary>
        /// <value></value>
        /// <returns>The argument that is propagated to the <see cref="E:System.Web.UI.WebControls.IButtonControl.Command"></see> event.</returns>
        [Themeable(false), Description("An optional argument that is propagated to the command event."), Category("Behavior"), Bindable(true), DefaultValue("")]
        public string CommandArgument
        {
            get { return (string)this.ViewState["CommandArgument"] ?? string.Empty; }
            set { this.ViewState["CommandArgument"] = value; }
        }

        /// <summary>
        /// Gets or sets the command name that is propagated to the <see cref="E:System.Web.UI.WebControls.IButtonControl.Command"></see> event.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the command that is propagated to the <see cref="E:System.Web.UI.WebControls.IButtonControl.Command"></see> event.</returns>
        [DefaultValue(""), Category("Behavior"), Description("The command name that is propagated to the command event.")]
        [Themeable(false)]
        public string CommandName
        {
            get { return (string)this.ViewState["CommandName"] ?? string.Empty; }
            set { this.ViewState["CommandName"] = value; }
        }

        /// <summary>
        /// Gets or sets the URL of the Web page to post to from the current page when the button control is clicked.
        /// </summary>
        /// <value></value>
        /// <returns>The URL of the Web page to post to from the current page when the button control is clicked.</returns>
        [Description("The URL of the Web page to post to from the current page when the button control is clicked."), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Category("Behavior")]
        [UrlProperty("*.aspx"), Themeable(false)]
        public virtual string PostBackUrl
        {
            get { return (string)this.ViewState["PostBackUrl"] ?? string.Empty; }
            set { this.ViewState["PostBackUrl"] = value; }
        }

        /// <summary>
        /// Gets or sets the text caption displayed for the button; in this case the dynamic text value.
        /// </summary>
        /// <value></value>
        /// <returns>The text caption displayed for the button; in this case the dynamic text value.</returns>
        string IButtonControl.Text
        {
            get { return this.Text.Value; }
            set { this.Text.Value = value; }
        }

        /// <summary>
        /// Gets or sets the name for the group of controls for which the button control causes validation when it posts back to the server.
        /// </summary>
        /// <value></value>
        /// <returns>The name for the group of controls for which the button control causes validation when it posts back to the server.</returns>
        [Category("Behavior"), DefaultValue(""), Description("The name for the group of controls for which the button control causes validation when it posts back to the server.")]
        [Themeable(false)]
        public string ValidationGroup
        {
            get { return (string)this.ViewState["ValidationGroup"] ?? string.Empty; }
            set { this.ViewState["ValidationGroup"] = value; }
        }

        #endregion

        #region IPostBackEventHandler Members

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"></see> that represents an optional event argument to be passed to the event handler.</param>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        #endregion
    }


    /// <summary>
    /// Represents a gradient background structure.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GradientBackground : IStateManager
    {

        // Fields

        private bool tracks;
        private DirtyFlags dirtyFlags = DirtyFlags.None;

        private Color borderColor = Color.Gray;
        private Color gradientStartColor = Color.Brown;
        private Color gradientEndColor = Color.White;
        private int roundCornerRadius = 6;
        private int borderWidth = 1;
        private GradientType type = GradientType.BackwardDiagonal;
        private Color innerBorderColor = Color.Gray;
        private int innerBorderWidth = 0;


        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientBackground"/> class.
        /// </summary>
        public GradientBackground()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientBackground"/> class.
        /// </summary>
        /// <param name="borderColor">The gradient border color.</param>
        /// <param name="gradientStartColor">The gradient start color.</param>
        /// <param name="gradientEndColor">The gradient end color.</param>
        /// <param name="roundCornerRadius">The gradient round corner radius.</param>
        /// <param name="borderWidth">The gradient border width.</param>
        /// <param name="type">The gradient type.</param>
        /// <param name="innerBorderColor">The gradient inner border color.</param>
        /// <param name="innerBorderWidth">The gradient inner border width.</param>
        public GradientBackground(Color borderColor, Color gradientStartColor, Color gradientEndColor,
            int roundCornerRadius, int borderWidth, GradientType type, Color innerBorderColor, int innerBorderWidth)
        {
            this.borderColor = borderColor;
            this.gradientStartColor = gradientStartColor;
            this.gradientEndColor = gradientEndColor;
            this.roundCornerRadius = roundCornerRadius;
            this.borderWidth = borderWidth;
            this.type = type;
            this.innerBorderColor = innerBorderColor;
            this.innerBorderWidth = innerBorderWidth;
        }

        /// <summary>
        /// When implemented by a class, gets a value indicating whether a server control is tracking its view state changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if a server control is tracking its view state changes; otherwise, false.</returns>
        internal bool IsTrackingViewState
        {
            get { return this.tracks; }
        }

        /// <summary>
        /// When implemented by a class, loads the server control's previously saved view state to the control.
        /// </summary>
        /// <param name="state">An <see cref="T:System.Object"></see> that contains the saved view state values for the control.</param>
        internal void LoadViewState(object state)
        {
            if (null != state)
            {
                object[] s = state as object[];
                if (null != s[0])
                {
                    this.BorderColor = (Color)s[0];
                }
                if (null != s[1])
                {
                    this.GradientStartColor = (Color)s[1];
                }
                if (null != s[2])
                {
                    this.GradientEndColor = (Color)s[2];
                }
                if (null != s[3])
                {
                    this.RoundCornerRadius = (int)s[3];
                }
                if (null != s[4])
                {
                    this.BorderWidth = (int)s[4];
                }
                if (null != s[5])
                {
                    this.Type = (GradientType)s[5];
                }
                if (null != s[6])
                {
                    this.InnerBorderColor = (Color)s[6];
                }
                if (null != s[7])
                {
                    this.InnerBorderWidth = (int)s[7];
                }
            }
        }

        /// <summary>
        /// When implemented by a class, saves the changes to a server control's view state to an <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"></see> that contains the view state changes.
        /// </returns>
        internal object SaveViewState()
        {
            object[] state = new object[8];

            state[0] = ((this.dirtyFlags & DirtyFlags.BorderColor) == DirtyFlags.BorderColor) ? (object)this.BorderColor : (object)null;
            state[1] = ((this.dirtyFlags & DirtyFlags.GradientStartColor) == DirtyFlags.GradientStartColor) ? (object)this.GradientStartColor : (object)null;
            state[2] = ((this.dirtyFlags & DirtyFlags.GradientEndColor) == DirtyFlags.GradientEndColor) ? (object)this.GradientEndColor : (object)null;
            state[3] = ((this.dirtyFlags & DirtyFlags.RoundCornerRadius) == DirtyFlags.RoundCornerRadius) ? (object)this.RoundCornerRadius : (object)null;
            state[4] = ((this.dirtyFlags & DirtyFlags.BorderWidth) == DirtyFlags.BorderWidth) ? (object)this.BorderWidth : (object)null;
            state[5] = ((this.dirtyFlags & DirtyFlags.Type) == DirtyFlags.Type) ? (object)this.Type : (object)null;
            state[6] = ((this.dirtyFlags & DirtyFlags.InnerBorderColor) == DirtyFlags.InnerBorderColor) ? (object)this.InnerBorderColor : (object)null;
            state[7] = ((this.dirtyFlags & DirtyFlags.InnerBorderWidth) == DirtyFlags.InnerBorderWidth) ? (object)this.InnerBorderWidth : (object)null;

            return state;
        }

        /// <summary>
        /// When implemented by a class, instructs the server control to track changes to its view state.
        /// </summary>
        internal void TrackViewState()
        {
            this.tracks = true;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if ((null == obj) || (typeof(GradientBackground) != obj.GetType()))
            {
                return false;
            }

            GradientBackground gradientBackground = (GradientBackground)obj;

            return
                (this.BorderColor == gradientBackground.BorderColor) &&
                (this.GradientStartColor == gradientBackground.GradientStartColor) &&
                (this.GradientEndColor == gradientBackground.GradientEndColor) &&
                (this.RoundCornerRadius == gradientBackground.RoundCornerRadius) &&
                (this.BorderWidth == gradientBackground.BorderWidth) &&
                (this.Type == gradientBackground.Type) &&
                (this.InnerBorderColor == gradientBackground.InnerBorderColor) &&
                (this.InnerBorderWidth == gradientBackground.InnerBorderWidth);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        // Properties

        /// <summary>
        /// Gets or sets the gradient border color.
        /// </summary>
        /// <value>The gradient border color.</value>
        [Description("The gradient border color."), DefaultValue(typeof(Color), "Gray"), TypeConverter(typeof(WebColorConverter)), NotifyParentProperty(true)]
        public virtual Color BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.BorderColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient start color.
        /// </summary>
        /// <value>The gradient start color.</value>
        [Description("The gradient start color."), DefaultValue(typeof(Color), "Brown"), TypeConverter(typeof(WebColorConverter)), NotifyParentProperty(true)]
        public virtual Color GradientStartColor
        {
            get { return this.gradientStartColor; }
            set
            {
                this.gradientStartColor = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.GradientStartColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient end color.
        /// </summary>
        /// <value>The gradient end color.</value>
        [Description("The gradient end color."), DefaultValue(typeof(Color), "White"), TypeConverter(typeof(WebColorConverter)), NotifyParentProperty(true)]
        public virtual Color GradientEndColor
        {
            get { return this.gradientEndColor; }
            set
            {
                this.gradientEndColor = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.GradientEndColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient round corner radius.
        /// </summary>
        /// <value>The gradient round corner radius.</value>
        [Description("The gradient round corner radius."), DefaultValue(6), NotifyParentProperty(true)]
        public virtual int RoundCornerRadius
        {
            get { return this.roundCornerRadius; }
            set
            {
                this.roundCornerRadius = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.RoundCornerRadius;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient border width.
        /// </summary>
        /// <value>The border width.</value>
        [Description("The gradient border width."), DefaultValue(1), NotifyParentProperty(true)]
        public virtual int BorderWidth
        {
            get { return this.borderWidth; }
            set
            {
                this.borderWidth = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.BorderWidth;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient type.
        /// </summary>
        /// <value>The gradient type.</value>
        [Description("The gradient type."), DefaultValue(GradientType.BackwardDiagonal), NotifyParentProperty(true)]
        public virtual GradientType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.Type;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient inner border color.
        /// </summary>
        /// <value>The gradient inner border color.</value>
        [Description("The gradient inner border color."), DefaultValue(typeof(Color), "Gray"), TypeConverter(typeof(WebColorConverter)), NotifyParentProperty(true)]
        public virtual Color InnerBorderColor
        {
            get { return this.innerBorderColor; }
            set
            {
                this.innerBorderColor = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.InnerBorderColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gradient inner border width.
        /// </summary>
        /// <value>The inner border width.</value>
        [Description("The gradient inner border width."), DefaultValue(0), NotifyParentProperty(true)]
        public virtual int InnerBorderWidth
        {
            get { return this.innerBorderWidth; }
            set
            {
                this.innerBorderWidth = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.InnerBorderWidth;
                }
            }
        }


        // Nested Types

        private enum DirtyFlags
        {
            None = 0,
            BorderColor = 1,
            GradientStartColor = 2,
            GradientEndColor = 4,
            RoundCornerRadius = 8,
            BorderWidth = 16,
            Type = 32,
            InnerBorderColor = 64,
            InnerBorderWidth = 128,
            All = BorderColor + GradientStartColor + GradientEndColor + RoundCornerRadius +
                BorderWidth + Type + InnerBorderColor + InnerBorderWidth
        }


        #region IStateManager Members

        /// <summary>
        /// When implemented by a class, gets a value indicating whether a server control is tracking its view state changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if a server control is tracking its view state changes; otherwise, false.</returns>
        bool IStateManager.IsTrackingViewState
        {
            get { return this.IsTrackingViewState; }
        }

        /// <summary>
        /// When implemented by a class, loads the server control's previously saved view state to the control.
        /// </summary>
        /// <param name="state">An <see cref="T:System.Object"></see> that contains the saved view state values for the control.</param>
        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        /// <summary>
        /// When implemented by a class, saves the changes to a server control's view state to an <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"></see> that contains the view state changes.
        /// </returns>
        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        /// <summary>
        /// When implemented by a class, instructs the server control to track changes to its view state.
        /// </summary>
        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        #endregion

    }


    /// <summary>
    /// The gradient type.
    /// </summary>
    public enum GradientType
    {
        /// <summary>
        /// Specifies a gradient from left to right.
        /// </summary>
        Horizontal,
        /// <summary>
        /// Specifies a gradient from top to bottom.
        /// </summary>
        Vertical,
        /// <summary>
        /// Specifies a gradient from upper left to lower right.
        /// </summary>
        ForwardDiagonal,
        /// <summary>
        /// Specifies a gradient from upper right to lower left.
        /// </summary>
        BackwardDiagonal,
        /// <summary>
        /// Specifies a gradient from center to margins.
        /// </summary>
        BlendingIn,
        /// <summary>
        /// Specifies a gradient from top to bottom with a sudden falloff.
        /// </summary>
        VerticalSuddenFalloff,
    }

}
