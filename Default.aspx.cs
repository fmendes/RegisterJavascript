using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using XXXX;
using SharpPieces.Web.Controls;

namespace XXXX
{
	public partial class Default : System.Web.UI.Page
	{
	...


		protected void Page_Load(object sender, EventArgs e)
		{
			//// this doesn't work for individual panels, only for the page
			//this.MaintainScrollPositionOnPostBack = true;
			...

			// link JavaScript file to persist scroll position on panel
			ClientScript.RegisterClientScriptInclude(
				"CalendarDisplayDateValidation", ResolveClientUrl( "NPIR.js" ) );

			// add javascript to register the scroll position
			pnlDisplayPatientLookUp.Attributes.Add( "onscroll", "javascript:SetDivPosition()" );

			gvPatientLookUp.Attributes.Add("onkeydown", "javascript:getKey()");
			gvPatientInsurance.Attributes.Add("onkeydown", "javascript:getKey()");
			gvRespParty.Attributes.Add("onkeydown", "javascript:getKey()");
			pnlDisplayResults.Attributes.Add("onkeydown", "javascript:getKey()");
			pnlPatientInsurance.Attributes.Add("onkeydown", "javascript:getKey()");
			pnlPatientInfo.Attributes.Add("onkeydown", "javascript:getKey()");
			
			//gvPatientLookUp.Attributes.Add( "onkeydown", "javascript:handleKeyDownEvent(event)" );

...
		}
		...


	}
}
