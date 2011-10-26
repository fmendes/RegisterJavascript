using System;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Drawing;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a WebControl improvement.
    /// </summary>
    public abstract class SharpControl : WebControl
    {

        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="WebControlPlus"/> class.
        /// </summary>
        public SharpControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebControlPlus"/> class.
        /// </summary>
        /// <param name="tag">An HTML tag.</param>
        public SharpControl(string tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebControlPlus"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public SharpControl(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"></see> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.DesignMode)
            {
                this.RenderDesignTime(writer);
            }
            else
            {
                base.Render(writer);
            }
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected virtual void RenderDesignTime(HtmlTextWriter writer)
        {
        }


        // Properties

        /// <summary>
        /// Gets or sets the background color of the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Drawing.Color"></see> that represents the background color of the control. The default is <see cref="F:System.Drawing.Color.Empty"></see>, which indicates that this property is not set.</returns>
        [Browsable(false)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        /// Gets or sets the border color of the Web control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Drawing.Color"></see> that represents the border color of the control. The default is <see cref="F:System.Drawing.Color.Empty"></see>, which indicates that this property is not set.</returns>
        [Browsable(false)]
        public override Color BorderColor
        {
            get { return base.BorderColor; }
            set { base.BorderColor = value; }
        }

        /// <summary>
        /// Gets or sets the border style of the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.UI.WebControls.BorderStyle"></see> enumeration values. The default is NotSet.</returns>
        [Browsable(false)]
        public override BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        /// <summary>
        /// Gets or sets the border width of the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.Unit"></see> that represents the border width of a Web server control. The default value is <see cref="F:System.Web.UI.WebControls.Unit.Empty"></see>, which indicates that this property is not set.</returns>
        /// <exception cref="T:System.ArgumentException">The specified border width is a negative value. </exception>
        [Browsable(false)]
        public override Unit BorderWidth
        {
            get { return base.BorderWidth; }
            set { base.BorderWidth = value; }
        }

        /// <summary>
        /// Gets the font properties associated with the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.FontInfo"></see> that represents the font properties of the Web server control.</returns>
        [Browsable(false)]
        public override FontInfo Font
        {
            get { return base.Font; }
        }

        /// <summary>
        /// Gets or sets the foreground color (typically the color of the text) of the Web server control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Drawing.Color"></see> that represents the foreground color of the control. The default is <see cref="F:System.Drawing.Color.Empty"></see>.</returns>
        [Browsable(false)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

    }

}
