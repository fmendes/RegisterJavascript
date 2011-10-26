using System;
using System.Web.UI.Design;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.ComponentModel;

namespace SharpPieces.Web.Controls.Design
{

    /// <summary>
    /// Provides a DynamicImage control designer class for extending the its design-mode behavior.
    /// </summary>
    public class DynamicImageDesigner : ControlDesigner
    {

        // Methods

        /// <summary>
        /// Initializes the control designer and loads the specified component.
        /// </summary>
        /// <param name="component">The control being designed.</param>
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // register image http handler
            this.CheckHttpHandlerRegistration();
        }

        private void CheckHttpHandlerRegistration()
        {
            HttpHandledControl httpHandledControl = this.ViewControl as HttpHandledControl;
            if (null != httpHandledControl)
            {
                httpHandledControl.RegisterHandlerInConfiguration();
            }
        }

    }

}
