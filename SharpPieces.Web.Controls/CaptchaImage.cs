using System;
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a captacha image.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [Description("Captcha image generator."), DefaultProperty("Text")]
    [ToolboxData("<{0}:CaptchaImage runat=\"server\"></{0}:CaptchaImage>"), PersistChildren(false), ParseChildren(true), Themeable(true)]
    public class CaptchaImage : DynamicImage
    {

        // Fields

        private readonly static Random random = new Random(DateTime.Now.Millisecond);


        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptchaImage"/> class.
        /// </summary>
        public CaptchaImage()
            : base(HtmlTextWriterTag.Img)
        {
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SharpPieces.Web.Controls.Resources.DynamicImage.captcha.png"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        /// <summary>
        /// Adds HTML attributes and styles that need to be rendered to the specified <see cref="T:System.Web.UI.HtmlTextWriterTag"></see>. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"></see> that represents the output stream to render HTML content on the client.</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.ResizeHandler = ResizeHandler.Server;

            if (Unit.Empty == this.Width)
            {
                this.Width = 100;
            }

            if (Unit.Empty == this.Height)
            {
                this.Height = 30;
            }

            base.AddAttributesToRender(writer);
        }

        /// <summary>
        /// Resets the text.
        /// </summary>
        public void ResetText()
        {
            // store the encrypted text
            this.text.Value = this.GenerateText(this.TextLength, this.CharArray);
        }

        private string GenerateText(int length, string charArray)
        {
            if (string.IsNullOrEmpty(charArray))
            {
                return null;
            }

            // generate a new text
            string text = string.Empty;
            for (int i = 0; i < length; i++)
            {
                text += charArray[CaptchaImage.random.Next(charArray.Length)];
            }

            return text;
        }

        /// <summary>
        /// Determines whether the specified text is CAPTCHA valid.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="ignoreCase">If set to <c>true</c> case is ignored.</param>
        /// <returns>
        /// 	<c>true</c> if the specified text is CAPTCHA valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid(string text, bool caseSensitiveCheck)
        {
            return string.Equals(text, this.Text.Value, caseSensitiveCheck ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
        }

        // Properties

        private DynamicText text;

        /// <summary>
        /// Gets the captcha text.
        /// </summary>
        /// <value>The captcha text.</value>
        [Description("The captcha text."), Category("Behavior"), DefaultValue(""), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty), Themeable(false)]
        public override DynamicText Text
        {
            get
            {
                // check text
                if (null == this.text)
                {
                    this.text = new DynamicText();
                    this.text.HorizontalAlign = StringAlignment.Center;
                    this.text.VerticalAlign = StringAlignment.Center;

                    // kept in control state
                    this.text.TrackViewState();
                }

                // check text value
                if (string.IsNullOrEmpty(this.text.Value) && !this.DesignMode)
                {
                    this.ResetText();
                }

                return this.text;
            }
            set
            {
                this.text = value;
                if (null != this.text)
                {
                    // kept in control state
                    this.text.TrackViewState();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Drawing.Color"></see> that represents the background color of the control. The default is <see cref="F:System.Drawing.Color.Empty"></see>, which indicates that this property is not set.</returns>
        [Browsable(true), DefaultValue("White")]
        public override Color BackColor
        {
            get { return (null != this.ViewState["BackColor"]) ? (Color)this.ViewState["BackColor"] : Color.White; }
            set { this.ViewState["BackColor"] = value; }
        }

        /// <summary>
        /// Restores control-state information from a previous page request that was saved by the <see cref="M:System.Web.UI.Control.SaveControlState"></see> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object"></see> that represents the control state to be restored.</param>
        protected override void LoadControlState(object savedState)
        {
            base.LoadControlState(savedState);

            if (null != this.ViewState["Text"])
            {
                DynamicText decryptedText = (DynamicText)this.ViewState["Text"];
                decryptedText.Value = CaptchaImageProvider.DecryptText(decryptedText.Value);
                this.Text = decryptedText;
            }
        }

        /// <summary>
        /// Saves any server control state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current state. If there is no state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveControlState()
        {
            // the control state keeps the encrypted version
            DynamicText encryptedText = (DynamicText)this.Text.Clone();
            encryptedText.Value = CaptchaImageProvider.EncryptText(this.Text.Value);
            this.ViewState["Text"] = encryptedText;

            return base.SaveControlState();
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
        /// Gets or sets the captcha style.
        /// </summary>
        /// <value>The captcha style.</value>
        [Description("The captcha style."), Category("Appearance"), DefaultValue(CaptchaStyle.Confetti)]
        [Themeable(true)]
        public virtual CaptchaStyle DistortionStyle
        {
            get { return (null != this.ViewState["DistortionStyle"]) ? (CaptchaStyle)this.ViewState["DistortionStyle"] : CaptchaStyle.Confetti; }
            set { this.ViewState["DistortionStyle"] = value; }
        }

        /// <summary>
        /// Gets or sets the readness level.
        /// </summary>
        /// <value>The readness level.</value>
        [Description("The readness level."), Category("Behavior"), DefaultValue(ReadnessLevel.Normal)]
        [Themeable(false)]
        public virtual ReadnessLevel ReadnessLevel
        {
            get { return (null != this.ViewState["ReadnessLevel"]) ? (ReadnessLevel)this.ViewState["ReadnessLevel"] : ReadnessLevel.Normal; }
            set { this.ViewState["ReadnessLevel"] = value; }
        }

        /// <summary>
        /// Gets or sets the text length.
        /// The value has to be higher or equal to 3.
        /// </summary>
        /// <value>The text length.</value>
        [Description("The text length. The value has to be higher or equal to 3."), Category("Behavior"), DefaultValue(5)]
        [Themeable(false)]
        public virtual int TextLength
        {
            get { return (null != this.ViewState["TextLength"]) ? (int)this.ViewState["TextLength"] : 5; }
            set
            {
                if (3 > value)
                {
                    throw new ArgumentException("Length has to be higher or equal to 3.");
                }
                this.ViewState["TextLength"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the char array source.
        /// </summary>
        /// <value>The char array source.</value>
        [Description("The char array source."), Category("Behavior"), DefaultValue("ABCDEFHKLMNPRTVXYZ234789")]
        public virtual string CharArray
        {
            get { return (null != this.ViewState["CharArray"]) ? (string)this.ViewState["CharArray"] : "ABCDEFHKLMNPRTVXYZ234789"; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "ABCDEFHKLMNPRTVXYZ234789";
                }
                this.ViewState["CharArray"] = value;
            }
        }

        /// <summary>
        /// The http handler type.
        /// </summary>
        [Browsable(false)]
        public override Type HttpHandlerType
        {
            get { return typeof(CaptchaImageProvider); }
        }

        /// <summary>
        /// The http handler name.
        /// </summary>
        [Browsable(false)]
        public override string HttpHandlerName
        {
            get { return "ci.axd"; }
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
        /// Gets the captcha resize handler.
        /// </summary>
        /// <value>The button captcha handler.</value>
        [Browsable(false)]
        public new ResizeHandler ResizeHandler
        {
            get { return ResizeHandler.Server; }
        }

        /// <summary>
        /// Gets or sets the button rotate flip type.
        /// </summary>
        /// <value>The button rotate flip type.</value>
        [Browsable(false)]
        public override RotateFlipType RotateFlip
        {
            get { return base.RotateFlip; }
            set { base.RotateFlip = value; }
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
                // send the encrypted version
                DynamicText encryptedText = (DynamicText)this.Text.Clone();
                encryptedText.Value = CaptchaImageProvider.EncryptText(encryptedText.Value);

                return string.Concat(
                    "~/",
                    this.HttpHandlerName,
                    "?",
                    CaptchaImageProvider.ToQueryString(
                        (int)Math.Round(this.Width.Value),
                        (int)Math.Round(this.Height.Value),
                        this.ClientCacheDuration,
                        this.ServerCacheDuration,
                        this.RotateFlip,
                        this.DrawGrayscale,
                        this.DrawSepia,
                        encryptedText,
                        this.ImageFormat,
                        this.imageCreator,
                        this.imageTransformations,
                        this.SizeType,
                        this.DistortionStyle,
                        this.ReadnessLevel,
                        this.BackColor));
            }
        }

    }


    /// <summary>
    /// The captcha style.
    /// </summary>
    public enum CaptchaStyle : int
    {
        /// <summary>
        /// A confetti captcha style.
        /// </summary>
        Confetti = 0,
        /// <summary>
        /// A gradient captcha style.
        /// </summary>
        Gradient = 1,
        /// <summary>
        /// A holes captcha style.
        /// </summary>
        Holes = 2,
        /// <summary>
        /// A random captcha style.
        /// </summary>
        Random
    }

    /// <summary>
    /// The readness level.
    /// </summary>
    public enum ReadnessLevel
    {
        /// <summary>
        /// Normal readness level.
        /// </summary>
        Normal,
        /// <summary>
        /// Hard readness level.
        /// </summary>
        Hard,
        /// <summary>
        /// The highest readness level.
        /// </summary>
        AlmostImpossible
    }

}
