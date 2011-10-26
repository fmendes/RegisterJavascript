using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.UI;
using System.Configuration;
using System.Web.Configuration;
using System.Web.UI.Design;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a sharp piece control rendered by a specific http handler.
    /// </summary>
    public abstract class HttpHandledControl : SharpControl
    {

        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandledControl"/> class.
        /// </summary>
        public HttpHandledControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandledControl"/> class.
        /// </summary>
        /// <param name="tag">An HTML tag.</param>
        public HttpHandledControl(string tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandledControl"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public HttpHandledControl(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        /// <summary>
        /// Registers the Http Handler in configuration.
        /// </summary>
        public virtual void RegisterHandlerInConfiguration()
        {
            // supported only at design time 
            if (!this.DesignMode)
            {
                return;
            }

            // no point to add if http handler name or type is null
            if (string.IsNullOrEmpty(this.HttpHandlerName) || (null == this.HttpHandlerType))
            {
                return;
            }

            // get the Web application configuration
            IWebApplication webApplication = (IWebApplication)this.Site.GetService(typeof(IWebApplication));
            if (null != webApplication)
            {
                global::System.Configuration.Configuration webConfig = webApplication.OpenWebConfiguration(false);
                if (null == webConfig)
                {
                    throw new ConfigurationErrorsException("web.config file not found to register the http handler.");
                }

                // get the <system.web> section
                ConfigurationSectionGroup systemWeb = webConfig.GetSectionGroup("system.web");
                if (null == systemWeb)
                {
                    systemWeb = new ConfigurationSectionGroup();
                    webConfig.SectionGroups.Add("system.web", systemWeb);
                }

                // get the <httpHandlers> section
                HttpHandlersSection httpHandlersSection = (HttpHandlersSection)systemWeb.Sections.Get("httpHandlers");
                if (null == httpHandlersSection)
                {
                    httpHandlersSection = new HttpHandlersSection();
                    systemWeb.Sections.Add("httpHandlers", httpHandlersSection);
                }

                // add the image handler
                httpHandlersSection.Handlers.Add(new HttpHandlerAction(this.HttpHandlerName, this.HttpHandlerType.AssemblyQualifiedName, "*"));

                // save the new web config
                webConfig.Save();
            }
        }


        // Propeties        

        /// <summary>
        /// The http handler type.
        /// </summary>
        public abstract Type HttpHandlerType { get; }

        /// <summary>
        /// The http handler name.
        /// </summary>
        public abstract string HttpHandlerName { get; }

    }

}
