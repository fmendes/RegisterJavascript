using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SharpPieces.Web.Controls
{
    public class UploadTriggerControlConverter : ControlIDConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadTriggerControlConverter"/> class.
        /// </summary>
        public UploadTriggerControlConverter()
        {
        }

        /// <summary>
        /// Returns a value indicating whether the control ID of the specified control is added to the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"></see> that is returned by the <see cref="M:System.Web.UI.WebControls.ControlIDConverter.GetStandardValues(System.ComponentModel.ITypeDescriptorContext)"></see> method.
        /// </summary>
        /// <param name="control">The control instance to test for inclusion in the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"></see>.</param>
        /// <returns>true in all cases.</returns>
        protected override bool FilterControl(Control control)
        {
            return control is Button || control is LinkButton || control is ImageButton;
        }
    }
}
