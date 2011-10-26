using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SharpPieces.Web.Controls
{
    public class UploadEventArgs : EventArgs
    {
        private HttpPostedFile _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="FancyUploadEventArgs"/> class.
        /// </summary>
        public UploadEventArgs()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FancyUploadEventArgs"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        public UploadEventArgs(HttpPostedFile file)
        {
            this._file = file;
        }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public HttpPostedFile File
        {
            get { return this._file; }
            set { this._file = value; }
        }
    }
}
