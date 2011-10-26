using System;
using System.Web.UI.Design.WebControls;
using System.ComponentModel;
using System.Web.UI.Design;

namespace SharpPieces.Web.Controls.Design
{
    /// <summary>
    /// Provides a ImageCheckBox control designer class for extending the its design-mode behavior.
    /// </summary>
    public class ImageCheckBoxDesigner : ControlDesigner
    {
        /// <summary>
        /// Gets a value indicating whether the control can be resized in the design-time environment.
        /// </summary>
        /// <value></value>
        /// <returns>true, if the control can be resized; otherwise, false.</returns>
        public override bool AllowResize
        {
            get { return false; }
        }
    }
}
