using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Web;
using System.Drawing.Design;
using System.Web.UI.Design;
using System.Collections;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultEvent("FileReceived")]
    public class Upload : SharpControl
    {
        #region Private Fields

        private FileUpload uploadControl;
		private string _url;
		private string _swf;
		private bool _multiple;
		private bool _queued;
		private string _types;
		private int _limitSize;
		private int _limitFiles;
		private bool _instantStart;
		private bool _allowDuplicates;
		private int _optionFxDuration;
		private string _uploadTrigger;
		private string _queueList;
		private string _onComplete;
		private string _onError;
		private string _onCancel;
		private string _onAllComplete;
		private string _browseText;
		private string _removeImageUrl;
		private string _removeButtonCssClass;
		private string _fileNameCssClass;
		private string _fileSizeCssClass;

        #endregion

        #region Public Delegates

        public delegate void FileReceivedEventHandler(object sender, UploadEventArgs e);

        #endregion

        #region Events

        /// <summary>
        /// Fires when a file has been successfully uploaded to the server.
        /// </summary>
        [Category("Action"), Description("Fires when a file has been successfully uploaded to the server.")]
        public event FileReceivedEventHandler FileReceived;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadExtender"/> class.
        /// </summary>
        public Upload()
        {
			_url = null;
			_browseText = "Select Files";
			_removeButtonCssClass = "input-delete";
			_fileNameCssClass = "queue-file";
			_fileSizeCssClass = "queue-size";
			_multiple = true;
			_queueList = "photoupload-queue";
			//_limitSize = 4096000;
			_instantStart = true;
			_allowDuplicates = false;
			_optionFxDuration = 250;
            _types = "{'All files (*.*)': '*.*'}";

            this.uploadControl = new FileUpload();
            this.uploadControl.ID = "fileUpload";
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.Controls.Add(this.uploadControl);
            base.OnInit(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (!this.Page.IsPostBack)
            {
                HttpPostedFile file = this.Page.Request.Files["Filedata"];
                if (file != null)
                {
                    OnFileReceived(new UploadEventArgs(file));
                }
            }

            this.Page.Header.Controls.Add(new LiteralControl(string.Format("<link href='{0}' rel='stylesheet' type='text/css' />", GetResourceUrl("Upload.css"))));

            base.OnLoad(e);
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            RegisterClientScriptInclude(GetResourceUrl("mootools.js"));
            RegisterClientScriptInclude(GetResourceUrl("Swiff.Base.js"));
            RegisterClientScriptInclude(GetResourceUrl("Swiff.Uploader.js"));
            RegisterClientScriptInclude(GetResourceUrl("FancyUpload.js"));

            HtmlGenericControl si = new HtmlGenericControl();
            si.TagName = "script";
            si.Attributes.Add("type", "text/javascript");


            StringBuilder script = new StringBuilder();


            script.Append("window.addEvent('load', function() {\n");
            script.Append("var upload = new FancyUpload($('" + this.uploadControl.ClientID + "'), {\n");

            if (string.IsNullOrEmpty(_url))
            {
                _url = this.Page.Request.CurrentExecutionFilePath;
            }

            script.Append("url: '" + _url + "',\n");

            _swf = GetResourceUrl("Swiff.Uploader.swf");
            script.Append("swf: '" + _swf + "',\n");

            if (!string.IsNullOrEmpty(_uploadTrigger))
            {
                Control trigger = this.Parent.FindControl(_uploadTrigger);
                if (trigger != null)
                {
                    script.Append("uploadTrigger: $('" + trigger.ClientID + "'),\n");
                }
            }
            else
            {
                script.Append("instantStart: " + "true" + ",\n");
            }

            if (_instantStart && !string.IsNullOrEmpty(_uploadTrigger))
            {
                script.Append("instantStart: " + _instantStart.ToString().ToLower() + ",\n");
            }

            if (_multiple)
            {
                script.Append("multiple: " + _multiple.ToString().ToLower() + ",\n");
            }

            if (_queued)
            {
                script.Append("queued: " + _queued.ToString().ToLower() + ",\n");
            }

            // Types

            if (_limitSize > 0)
            {
                script.Append("limitSize: " + _limitSize.ToString() + ",\n");
            }

            if (_limitFiles > 0)
            {
                script.Append("limitFiles: " + _limitFiles.ToString() + ",\n");
            }

            // CreateReplecement

            if (_allowDuplicates)
            {
                script.Append("allowDuplicates: " + _allowDuplicates.ToString().ToLower() + ",\n");
            }

            if (_optionFxDuration > 0)
            {
                script.Append("optionFxDuration: " + _optionFxDuration + ",\n");
            }

            if (!string.IsNullOrEmpty(_queueList))
            {
                script.Append("queueList: '" + _queueList + "',\n");
            }

            if (!string.IsNullOrEmpty(_onComplete))
            {
                script.Append("onComplete: " + _onComplete + ",\n");
            }

            if (!string.IsNullOrEmpty(_onError))
            {
                script.Append("onError: " + _onError + ",\n");
            }

            if (!string.IsNullOrEmpty(_onCancel))
            {
                script.Append("onCancel: " + _onCancel + ",\n");
            }

            if (!string.IsNullOrEmpty(_onAllComplete))
            {
                script.Append("onAllComplete: " + _onAllComplete + ",\n");
            }

            if (!string.IsNullOrEmpty(_browseText))
            {
                script.Append("browseText: '" + _browseText + "',\n");
            }

            if (!string.IsNullOrEmpty(_removeImageUrl))
            {
                script.Append("removeImageUrl: '" + VirtualPathUtility.ToAbsolute(_removeImageUrl) + "',\n");
            }
            else
            {
                script.Append("removeImageUrl: '" + GetResourceUrl("delete.png") + "',\n");
            }

            if (!string.IsNullOrEmpty(_removeButtonCssClass))
            {
                script.Append("removeButtonCssClass: '" + _removeButtonCssClass + "',\n");
            }

            if (!string.IsNullOrEmpty(_fileNameCssClass))
            {
                script.Append("fileNameCssClass: '" + _fileNameCssClass + "',\n");
            }

            if (!string.IsNullOrEmpty(_fileSizeCssClass))
            {
                script.Append("fileSizeCssClass: '" + _fileSizeCssClass + "',");
            }

            if (!string.IsNullOrEmpty(_types))
            {
                script.Append("types: " + _types + ",");
            }



            script.Remove(script.Length - 1, 1);


            script.Append("\n\n});");
            script.Append("\n});\n");

            si.InnerText = script.ToString();
            this.Page.Header.Controls.Add(si);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Raises the <see cref="E:FileReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="FancyUpload.FancyUploadEventArgs"/> instance containing the event data.</param>
        protected virtual void OnFileReceived(UploadEventArgs e)
        {
            if (FileReceived != null)
            {
                FileReceived(this, e);
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Gets the resource URL.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public string GetResourceUrl(string fileName)
        {
            return Page.ClientScript.GetWebResourceUrl(typeof(SharpPieces.Web.Controls.Upload), "SharpPieces.Web.Controls.Resources.Upload." + fileName);
        }

        /// <summary>
        /// Registers the client script include.
        /// </summary>
        /// <param name="src">The SRC.</param>
        void RegisterClientScriptInclude(string src)
        {
            HtmlGenericControl si = new HtmlGenericControl();
            si.TagName = "script";
            si.Attributes.Add("type", "text/javascript");
            si.Attributes.Add("src", src);
            this.Page.Header.Controls.Add(si);
            this.Page.Header.Controls.Add(new LiteralControl("\n"));
        }

        /// <summary>
        /// Renders the control at design time.
        /// </summary>
        /// <param name="writer">The html writer.</param>
        protected override void RenderDesignTime(HtmlTextWriter writer)
        {
            writer.Write(string.Format("<input type=\"file\" size=\"30\">"));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the control which triggers the upload.
        /// </summary>
        /// <value>The trigger.</value>
        [DefaultValue(""),
        IDReferenceProperty,
        TypeConverter(typeof(UploadTriggerControlConverter)),
        Themeable(false),
        Category("Behavior"),
        Description("The control which triggers the upload. InstantStart should also be enabled in order for this property to be used.")]
        public string Trigger
        {
			get { return _uploadTrigger; }
			set { _uploadTrigger = value; }
        }

        /// <summary>
        /// Gets or sets the browse text.
        /// </summary>
        /// <value>The browse text.</value>
        [DefaultValue("Select Files"),
        Category("Interface"),
        Description("The text to be displayed as the browse button title.")]
        public string BrowseText
        {
			get { return _browseText; }
			set { _browseText = value; }
        }


        /// <summary>
        /// Gets or sets the maximum size of the file.
        /// </summary>
        /// <value>The maximum size of the file.</value>
		[DefaultValue(4096),
        Category("Behavior"),
        Description("The maximum size of a file allowed to be uploaded.")]
		public int MaximumFileSize
		{
			get { return _limitSize; }
			set { _limitSize = value; }
		}

        /// <summary>
        /// Gets or sets the queue list.
        /// </summary>
        /// <value>The queue list.</value>
        [Category("Interface"),
        Description("The queue list.")]
        public string QueueList
        {
			get { return _queueList; }
			set { _queueList = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether multiple selection is enabled or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if multiple selection is enabled; otherwise, <c>false</c>.
        /// </value>
		[Category("Behavior"), 
		DefaultValue(true),
        Description("Allow multiple files to be selected with the file browser.")]
		public bool EnableMultipleSelection
		{
			get { return _multiple; }
			set { _multiple = value; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether the upload will be triggered automatically after the files have been chosen.
        /// </summary>
        /// <value><c>true</c> if instanrt start is enabled; otherwise, <c>false</c>.</value>
		[Category("Behavior"),
		DefaultValue(true),
        Description("Enables the upload to be triggered automatically after the files have been selected.")]
		public bool InstantStart
		{
			get { return _instantStart; }
			set { _instantStart = value; }
		}

        /// <summary>
        /// Gets or sets the number of files allowed.
        /// </summary>
        /// <value>The number of files allowed.</value>
		[Category("Behavior"),
        DefaultValue(0),
        Description("Limits the maximum number of files allowed to be uploaded. Set to 0 for unlimited.")]
		public int LimitFiles
		{
			get { return _limitFiles; }
			set { _limitFiles = value; }
		}

        /// <summary>
        /// Gets or sets the size of the limit.
        /// </summary>
        /// <value>The size of the limit.</value>
		[Category("Behavior"),
        DefaultValue(0),
       Description("Limits the maximum file size allowed to be uploaded. Set to 0 for unlimited.")]
		public int LimitSize
		{
			get { return _limitSize; }
			set { _limitSize = value; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether duplicate file names are allowed in the queue.
        /// </summary>
        /// <value><c>true</c> if duplicate file names are allowed in the queue; otherwise, <c>false</c>.</value>
		[Category("Behavior"),
		DefaultValue(false),
        Description("Enables the option of having files with the same name in the queue.")]
		public bool AllowDuplicates
		{
			get { return _allowDuplicates; }
			set { _allowDuplicates = value; }
		}

        /// <summary>
        /// Gets or sets the duration of the FX duration.
        /// </summary>
        /// <value>The duration of the fade effect.</value>
		[Category("Behavior"),
		DefaultValue(250),
        Description("The duration in milliseconds of the FX used to display each file within the queue.")]
		public int OptionFxDuration
		{
			get { return _optionFxDuration; }
			set { _optionFxDuration = value; }
		}

        /// <summary>
        /// Gets or sets the client action to be executed on complete.
        /// </summary>
        /// <value>The client action to be executed on complete.</value>
        [Category("Client Methods"),
        DefaultValue(""),
        Description("The client method to be called when a file upload is complete.")]
		public string OnComplete
		{
			get { return _onComplete; }
			set { _onComplete = value; }
		}

        /// <summary>
        /// Gets or sets the client action to be executed on error.
        /// </summary>
        /// <value>The on error.</value>
        [Category("Client Methods"),
        DefaultValue(""),
        Description("The client method to be called when a file upload error occured.")]
		public string OnError
		{
			get { return _onError; }
			set { _onError = value; }
		}

        /// <summary>
        /// Gets or sets the client action to be executed on cancel.
        /// </summary>
        /// <value>The on cancel.</value>
        [Category("Client Methods"),
        DefaultValue(""),
        Description("The client method to be called when a file upload has been canceled.")]
		public string OnCancel
		{
			get { return _onCancel; }
			set { _onCancel = value; }
		}

        /// <summary>
        /// Gets or sets the on client action to be executed when all files have been uploaded.
        /// </summary>
        /// <value>The client action to be executed when all files have been uploaded.</value>
        [Category("Client Methods"),
        DefaultValue(""),
        Description("The client method to be called when all uploads have been completed.")]
		public string OnAllComplete
		{
			get { return _onAllComplete; }
			set { _onAllComplete = value; }
		}

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>The types.</value>
        [Category("Behavior"),
        DefaultValue("{'All files (*.*)': '*.*'}"),
        Description("Filter used to limit the allowed file extensions.")]
        public string Types
        {
            get { return _types; }
            set { _types = value; }
        }

        #region Design

        /// <summary>
        /// Gets or sets the file name CSS class.
        /// </summary>
        /// <value>The file name CSS class.</value>
        [Category("Appearance"),
        DefaultValue("queue-file"),
        Description("The style class for the file name label.")]
        public string FileNameCssClass
        {
			get { return _fileNameCssClass; }
			set { _fileNameCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the file size CSS class.
        /// </summary>
        /// <value>The file size CSS class.</value>
        [Category("Appearance"),
        DefaultValue("queue-size"),
        Description("The style class for the file size label.")]
        public string FileSizeCssClass
        {
			get { return _fileSizeCssClass; }
			set { _fileSizeCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the remove button CSS class.
        /// </summary>
        /// <value>The remove button CSS class.</value>
        [Category("Appearance"),
        DefaultValue("input-delete"),
        Description("The style class for the remove button.")]
        public string RemoveButtonCssClass
        {
			get { return _removeButtonCssClass; }
			set { _removeButtonCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the remove image URL.
        /// </summary>
        /// <value>The remove image URL.</value>
        [EditorAttribute(typeof(ImageUrlEditor), typeof(UITypeEditor)), Category("Appearance")]
        public string RemoveImageUrl
        {
			get 
			{
				return _removeImageUrl;
			}
            set 
			{
				_removeImageUrl = value;
			}
        }

        #endregion

        #endregion

    }
}
