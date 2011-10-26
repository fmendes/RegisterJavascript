using System;
using System.ComponentModel;
using System.Web.UI;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web;
using SharpPieces.Web.Controls;
using System.Security.Permissions;
using SharpPieces.Web.Controls.Design;
using System.Drawing.Design;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace SharpPieces.Web.Controls
{
    /// <summary>
    /// Represents an image filtered by an ASP.NET Http Handler.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [Designer(typeof(DynamicImageDesigner)), Description("Image filtered by an Asp.Net Http Handler."), DefaultProperty("Source")]
    [ToolboxData("<{0}:DynamicImage runat=\"server\"></{0}:DynamicImage>"), PersistChildren(false), ParseChildren(true), Themeable(true)]
    public class DynamicImage : HttpHandledControl
    {

        // Fields

        protected KeyValuePair<Type, IDictionary<string, string>>? imageCreator = null;
        protected Dictionary<Type, IDictionary<string, string>> imageTransformations = null;

        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicImage"/> class.
        /// </summary>
        public DynamicImage()
            : base(HtmlTextWriterTag.Img)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandledControl"/> class.
        /// </summary>
        /// <param name="tag">An HTML tag.</param>
        public DynamicImage(string tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandledControl"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public DynamicImage(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Restores view-state information from a previous page request that was saved by the <see cref="M:System.Web.UI.Control.SaveViewState"></see> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object"></see> that represents the control state to be restored.</param>
        protected override void LoadViewState(object savedState)
        {
            if ((null != savedState) && (savedState is object[]) && (2 == ((object[])savedState).Length))
            {
                base.LoadViewState(((object[])savedState)[0]);
                this.Text.LoadViewState(((object[])savedState)[1]);
            }
        }

        /// <summary>
        /// Saves any server control view-state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current view state. If there is no view state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            return new object[2] { base.SaveViewState(), this.Text.SaveViewState() };
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            if (null == this.Page.Site)
            {
                writer.Write("Error");
                return;
            }

            // resolve the image url
            IUrlResolutionService service = (IUrlResolutionService)this.Page.Site.GetService(typeof(IUrlResolutionService));
            if ((null == service) || (null == this.Source))
            {
                writer.Write("Error");
                return;
            }

            // add the image
            if ((Unit.Empty != this.Width) || (Unit.Empty != this.Height))
            {
                if (Unit.Empty != this.Width)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
                }
                if (Unit.Empty != this.Height)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());
                }
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "70px");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Src, service.ResolveClientUrl(this.Source));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        /// <summary>
        /// Adds HTML attributes and styles that need to be rendered to the specified <see cref="T:System.Web.UI.HtmlTextWriterTag"></see>. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"></see> that represents the output stream to render HTML content on the client.</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            // add the basic attributes
            if (ResizeHandler.Server == this.ResizeHandler)
            {
                // prevent the webcontrol from rendering width and height
                Unit tempWidth = this.Width;
                Unit tempHeight = this.Height;
                this.Width = Unit.Empty;
                this.Height = Unit.Empty;

                base.AddAttributesToRender(writer);

                this.Width = tempWidth;
                this.Height = tempHeight;
            }
            else
            {
                base.AddAttributesToRender(writer);
            }

            // add src
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Util.AppRelativeToAbsolute(this.Src, true));

            // add alternate text
            if (!string.IsNullOrEmpty(this.AlternateText))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.AlternateText);
            }
        }

        /// <summary>
        /// Registers a custom image type creator.
        /// </summary>
        /// <typeparam name="T">The custom image creator type.</typeparam>
        /// <param name="parameters">The parameters to pass.</param>
        public void RegisterCustomCreatorType<T>(IDictionary<string, string> parameters)
            where T : IImageCreator, new()
        {
            this.imageCreator = new KeyValuePair<Type, IDictionary<string, string>>(typeof(T), parameters);
        }

        /// <summary>
        /// Registers a custom image type creator.
        /// </summary>
        /// <typeparam name="T">The custom image creator type.</typeparam>
        /// <param name="paramKey">The param key to pass.</param>
        /// <param name="paramValue">The param value to pass.</param>
        public void RegisterCustomCreatorType<T>(string paramKey, string paramValue)
            where T : IImageCreator, new()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (null != paramKey)
            {
                parameters.Add(paramKey, paramValue);
            }
            this.RegisterCustomCreatorType<T>(parameters);
        }

        /// <summary>
        /// Registers a custom image type transformation.
        /// </summary>
        /// <typeparam name="T">The custom image transformation type.</typeparam>
        /// <param name="parameters">The parameters to pass.</param>
        public void RegisterCustomTransformationType<T>(IDictionary<string, string> parameters)
            where T : IImageTransformation, new()
        {
            if (null == this.imageTransformations)
            {
                this.imageTransformations = new Dictionary<Type, IDictionary<string, string>>();
            }

            this.imageTransformations[typeof(T)] = parameters ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Registers a custom image type transformation.
        /// </summary>
        /// <typeparam name="T">The custom image transformation type.</typeparam>
        /// <param name="paramKey">The param key to pass.</param>
        /// <param name="paramValue">The param value to pass.</param>
        public void RegisterCustomTransformationType<T>(string paramKey, string paramValue)
            where T : IImageTransformation, new()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (null != paramKey)
            {
                parameters.Add(paramKey, paramValue);
            }

            this.RegisterCustomTransformationType<T>(parameters);
        }

        /// <summary>
        /// Unregisters any registered custom image creator types.
        /// </summary>
        public void UnregisterCustomCreatorTypes()
        {
            this.imageCreator = null;
        }
        

        // Properties

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        /// <value>The image URL.</value>
        [Description("The dynamic image URL."), Category("Creation"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), Bindable(true)]
        [Themeable(false), UrlProperty]
        public virtual string Source
        {
            get { return (string)this.ViewState["Source"] ?? string.Empty; }
            set { this.ViewState["Source"] = value; }
        }

        /// <summary>
        /// Gets or sets the alternate text.
        /// </summary>
        /// <value>The alternate text.</value>
        [Category("Appearance"), Description("The dynamic image alternate text."), DefaultValue(""), Localizable(true), Bindable(true)]
        [Themeable(false)]
        public virtual string AlternateText
        {
            get { return (string)this.ViewState["AlternateText"] ?? string.Empty; }
            set { this.ViewState["AlternateText"] = value; }
        }

        /// <summary>
        /// Gets or sets the client cache duration in minutes.
        /// Set 0 or less if no cache is required.
        /// </summary>
        /// <value>The client cache duration in minutes; set 0 or less if no cache is required.</value>
        [Description("The dynamic image client cache duration in minutes; set 0 or less if no cache is required."), Category("Transformation"), DefaultValue(0)]
        [Themeable(false)]
        public virtual int ClientCacheDuration
        {
            get { return (null != this.ViewState["ClientCacheDuration"]) ? (int)this.ViewState["ClientCacheDuration"] : 0; }
            set { this.ViewState["ClientCacheDuration"] = value; }
        }

        /// <summary>
        /// Gets or sets the server cache duration in minutes.
        /// Set 0 or less if no cache is required.
        /// </summary>
        /// <value>The server cache duration in minutes; set 0 or less if no cache is required.</value>
        [Description("The dynamic image server cache duration in minutes; set 0 or less if no cache is required."), Category("Transformation"), DefaultValue(0)]
        [Themeable(false)]
        public virtual int ServerCacheDuration
        {
            get { return (null != this.ViewState["ServerCacheDuration"]) ? (int)this.ViewState["ServerCacheDuration"] : 0; }
            set { this.ViewState["ServerCacheDuration"] = value; }
        }

        /// <summary>
        /// Gets or sets the image resize handler.
        /// </summary>
        /// <value>The image resize handler.</value>
        [Description("The dynamic image resize handler."), Category("Transformation"), DefaultValue(ResizeHandler.Client)]
        [Themeable(false)]
        public virtual ResizeHandler ResizeHandler
        {
            get { return (null != this.ViewState["ResizeHandler"]) ? (ResizeHandler)this.ViewState["ResizeHandler"] : ResizeHandler.Client; }
            set
            {
                if ((ResizeHandler.Server == value) && ((UnitType.Pixel != this.Width.Type) || (UnitType.Pixel != this.Height.Type)))
                {
                    throw new NotSupportedException("Server resizeing does not support Unit as measure.");
                }
                this.ViewState["ResizeHandler"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the image rotate flip type.
        /// </summary>
        /// <value>The image rotate flip type.</value>
        [Description("The dynamic image rotate-flip type."), Category("Transformation"), DefaultValue(RotateFlipType.RotateNoneFlipNone)]
        [Themeable(false)]
        public virtual RotateFlipType RotateFlip
        {
            get { return (null != this.ViewState["RotateFlip"]) ? (RotateFlipType)this.ViewState["RotateFlip"] : RotateFlipType.RotateNoneFlipNone; }
            set { this.ViewState["RotateFlip"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to draw in grayscale.
        /// </summary>
        /// <value><c>true</c> if the image should be draw in grayscale; otherwise, <c>false</c>.</value>
        [Bindable(true), Description("Indicates whether to draw in grayscale."), Category("Transformation"), DefaultValue(false)]
        [Themeable(false)]
        public virtual bool DrawGrayscale
        {
            get { return (null != this.ViewState["DrawGrayscale"]) ? (bool)this.ViewState["DrawGrayscale"] : false; }
            set { this.ViewState["DrawGrayscale"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to draw in sepia.
        /// </summary>
        /// <value><c>true</c> if the image should be draw in sepia; otherwise, <c>false</c>.</value>
        [Bindable(true), Description("Indicates whether to draw in sepia."), Category("Transformation"), DefaultValue(false)]
        [Themeable(false)]
        public virtual bool DrawSepia
        {
            get { return (null != this.ViewState["DrawSepia"]) ? (bool)this.ViewState["DrawSepia"] : false; }
            set { this.ViewState["DrawSepia"] = value; }
        }

        /// <summary>
        /// Gets or sets the image dynamic text.
        /// </summary>
        /// <value>The image dynamic text.</value>
        [Description("The dynamic image dynamic text."), Category("Transformation"), DefaultValue(""), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty), Themeable(false)]
        public virtual DynamicText Text
        {
            get
            {
                if (null == this.ViewState["Text"])
                {
                    DynamicText text = new DynamicText();
                    if (this.EnableViewState)
                    {
                        text.TrackViewState();
                    }
                    this.ViewState["Text"] = text;
                }
                return (DynamicText)this.ViewState["Text"];
            }
            set
            {
                if ((null != value) && this.EnableViewState)
                {
                    value.TrackViewState();
                }
                this.ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the dynamic image format type.
        /// </summary>
        /// <value>The dynamic image format type.</value>
        [Description("The dynamic image format type."), Category("Transformation"), DefaultValue(DynamicImageFormat.Original)]
        [Themeable(false)]
        public virtual DynamicImageFormat ImageFormat
        {
            get { return (null != this.ViewState["ImageFormat"]) ? (DynamicImageFormat)this.ViewState["ImageFormat"] : DynamicImageFormat.Original; }
            set { this.ViewState["ImageFormat"] = value; }
        }

        /// <summary>
        /// Gets the image real src that points to the http handler.
        /// It is used as a key for the server cache so shoud be overriden with caution.
        /// </summary>
        /// <value>The image src.</value>
        [Browsable(false)]
        public virtual string Src
        {
            get
            {
                double? serverWidth = null, serverHeight = null;
                if (ResizeHandler.Server == this.ResizeHandler)
                {
                    if (!this.Width.IsEmpty)
                    {
                        serverWidth = this.Width.Value;
                    }
                    if (!this.Height.IsEmpty)
                    {
                        serverHeight = this.Height.Value;
                    }
                }

                return string.Concat(
                    "~/",
                    this.HttpHandlerName,
                    "?",
                    DynamicImageProvider.ToQueryString(
                        this.Source,
                        serverWidth.HasValue ? (int?)(int)Math.Round(serverWidth.Value) : (int?)null,
                        serverHeight.HasValue ? (int?)(int)Math.Round(serverHeight.Value) : (int?)null,
                        this.ClientCacheDuration,
                        this.ServerCacheDuration,
                        this.RotateFlip,
                        this.DrawGrayscale,
                        this.DrawSepia,
                        this.Text,
                        this.ImageFormat,
                        this.imageCreator,
                        this.imageTransformations));
            }
        }

        /// <summary>
        /// Gets the http handler type.
        /// </summary>
        [Browsable(false)]
        public override Type HttpHandlerType
        {
            get { return typeof(DynamicImageProvider); }
        }

        /// <summary>
        /// Gets the http handler name.
        /// </summary>
        [Browsable(false)]
        public override string HttpHandlerName
        {
            get { return "di.axd"; }
        }

    }

    /// <summary>
    /// The image resize handler type.
    /// </summary>
    public enum ResizeHandler
    {
        /// <summary>
        /// The image is resized by the browser.
        /// </summary>
        Client,
        /// <summary>
        /// The image is resized by the server.
        /// </summary>
        Server
    }

    /// <summary>
    /// Represents a dynamic text structure.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("Text")]
    public class DynamicText : IStateManager, ICloneable
    {

        // Fields

        private bool tracks;
        private DirtyFlags dirtyFlags = DirtyFlags.None;
        private string value = null;
        private Font font = new Font("Arial", 10);
        private Color color = Color.Black;
        private StringAlignment hAlign = StringAlignment.Far;
        private StringAlignment vAlign = StringAlignment.Far;


        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicText"/> class.
        /// </summary>
        public DynamicText()
        {
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
        internal virtual void LoadViewState(object state)
        {
            if (null != state)
            {
                object[] s = state as object[];
                if (null != s[0])
                {
                    this.Value = (string)s[0];
                }
                if (null != s[1])
                {
                    this.Font = (Font)s[1];
                }
                if (null != s[2])
                {
                    this.Color = (Color)s[2];
                }
                if (null != s[3])
                {
                    this.HorizontalAlign = (StringAlignment)s[3];
                }
                if (null != s[4])
                {
                    this.VerticalAlign = (StringAlignment)s[4];
                }
            }
        }

        /// <summary>
        /// When implemented by a class, saves the changes to a server control's view state to an <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"></see> that contains the view state changes.
        /// </returns>
        internal virtual object SaveViewState()
        {
            object[] state = new object[5];

            state[0] = ((this.dirtyFlags & DirtyFlags.Value) == DirtyFlags.Value) ? this.Value : null;
            state[1] = ((this.dirtyFlags & DirtyFlags.Font) == DirtyFlags.Font) ? (object)this.Font : (object)null;
            state[2] = ((this.dirtyFlags & DirtyFlags.Color) == DirtyFlags.Color) ? (object)this.Color : (object)null;
            state[3] = ((this.dirtyFlags & DirtyFlags.HAlign) == DirtyFlags.HAlign) ? (object)this.HorizontalAlign : (object)null;
            state[4] = ((this.dirtyFlags & DirtyFlags.VAlign) == DirtyFlags.VAlign) ? (object)this.VerticalAlign : (object)null;

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
            return this.value;
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
            if ((null == obj) || (typeof(DynamicText) != obj.GetType()))
            {
                return false;
            }

            DynamicText text = (DynamicText)obj;

            return
                (this.Value == text.Value) &&
                (this.Color == text.Color) &&
                (this.Font.Equals(text.Font)) &&
                (this.HorizontalAlign == text.HorizontalAlign) &&
                (this.VerticalAlign == text.VerticalAlign);
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
        /// Gets or sets the dynamic text value.
        /// </summary>
        /// <value>The dynamic text value.</value>
        [Description("The dynamic text value."), DefaultValue(null), NotifyParentProperty(true)]
        public virtual string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.Value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the dynamic text font.
        /// </summary>
        /// <value>The dynamic text font.</value>
        [PersistenceMode(PersistenceMode.Attribute), DefaultValue(typeof(Font), "Arial;10.0f"), NotifyParentProperty(true)]
        public Font Font
        {
            get
            {
                if (null == this.font)
                {
                    this.font = new Font("Arial", 10);
                }
                return this.font;
            }
            set
            {
                this.font = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.Font;
                }
            }
        }

        /// <summary>
        /// Gets or sets the dynamic text font color.
        /// </summary>
        /// <value>The dynamic text font color.</value>
        [Description("The dynamic text font color."), DefaultValue(typeof(Color), "Black"), TypeConverter(typeof(WebColorConverter)), NotifyParentProperty(true)]
        public virtual Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.Color;
                }
            }
        }

        /// <summary>
        /// Gets or sets the dynamic text horizontal alignment.
        /// </summary>
        /// <value>The dynamic text horizontal alignment.</value>
        [Description("The dynamic text horizontal alignment."), DefaultValue(StringAlignment.Far), NotifyParentProperty(true)]
        public virtual StringAlignment HorizontalAlign
        {
            get { return this.hAlign; }
            set
            {
                this.hAlign = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.HAlign;
                }
            }
        }

        /// <summary>
        /// Gets or sets the dynamic text vertical alignment.
        /// </summary>
        /// <value>The dynamic text vertical alignment.</value>
        [Description("The dynamic text vertical alignment."), DefaultValue(StringAlignment.Far), NotifyParentProperty(true)]
        public virtual StringAlignment VerticalAlign
        {
            get { return this.vAlign; }
            set
            {
                this.vAlign = value;
                if ((this as IStateManager).IsTrackingViewState)
                {
                    this.dirtyFlags = this.dirtyFlags | DirtyFlags.VAlign;
                }
            }
        }


        // Nested Types

        private enum DirtyFlags
        {
            None = 0,
            Value = 1,
            Font = 2,
            Color = 4,
            HAlign = 8,
            VAlign = 16,
            All = Value + Font + Color + HAlign + VAlign
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

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// A shallow copy in this case.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// A shallow copy in this case.
        /// </returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

    }


    /// <summary>
    /// The dynamic image format output types.
    /// </summary>
    public enum DynamicImageFormat
    {
        /// <summary>
        /// The original image format.
        /// </summary>
        Original,
        /// <summary>
        /// The .bmp image format.
        /// </summary>
        Bmp,
        /// <summary>
        /// The .gif image format.
        /// </summary>
        Gif,
        /// <summary>
        /// The .jpeg image format.
        /// </summary>
        Jpeg,
        /// <summary>
        /// The .png image format.
        /// </summary>
        Png
    }

    /// <summary>
    /// The text container size type.
    /// </summary>
    public enum TextContainerSizeType
    {
        /// <summary>
        /// The specified width and height are used; in case they aren't specified, default values are used.
        /// </summary>
        Specified,
        /// <summary>
        /// The specified width and height aren't taken into account, the container size is streched to text.
        /// </summary>
        StrechToText
    }   
}
