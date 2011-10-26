using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SharpPieces.Web.Controls.Base")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SharpPieces.Web.Controls.Base")]
[assembly: AssemblyCopyright("Copyright ©  2007")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("91300a7f-fe28-48f7-b491-da4c43f5e528")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]

// control tag prefix 
[assembly: TagPrefix("SharpPieces.Web.Controls", "piece")]

// DynamicImage embedded resources
[assembly: WebResource("SharpPieces.Web.Controls.Resources.DynamicImage.gradient.png", "image/png")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.DynamicImage.captcha.png", "image/png")]

// ImageCheckBox embedded resources
[assembly: WebResource("SharpPieces.Web.Controls.Resources.ImageCheckBox.ImageCheckBox.js", "text/javascript", PerformSubstitution = false)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.ImageCheckBox.checked.gif", "image/gif", PerformSubstitution = false)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.ImageCheckBox.unchecked.gif", "image/gif", PerformSubstitution = false)]

// LiveGrid embedded resources
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.prototype.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.rico.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoBehaviors.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoCommon.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGridCommon.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGrid.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoLiveGridAjax.js", "text/javascript", PerformSubstitution = true)]
//[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.common.js", "text/javascript", PerformSubstitution = true)]
//[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.customHandlers.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.ricoGrid.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.resize.gif", "image/gif")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.sort_asc.gif", "image/gif")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.sort_desc.gif", "image/gif")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.filtercol.gif", "image/gif")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow.png", "image/png")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow_ll.png", "image/png")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.LiveGrid.Rico.shadow_ur.png", "image/png")]

// Rating embedded resources
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Rating.Rating.js", "text/javascript", PerformSubstitution = true)]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Rating.ratingDemo.png", "image/png")]

// Uploadx embedded resources
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.delete.png", "image/gif")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.FancyUpload.js", "text/javascript")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.mootools.js", "text/javascript")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.Swiff.Base.js", "text/javascript")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.Swiff.Uploader.js", "text/javascript")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.Swiff.Uploader.swf", "application/x-shockwave-flash")]
[assembly: WebResource("SharpPieces.Web.Controls.Resources.Upload.Upload.css", "text/css")]