using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Wrapper over ListItem exposing the optgroup option.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter)), 
     ControlBuilder(typeof(ListItemControlBuilder)), 
     ParseChildren(true, "Text"), 
     AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class ExtendedListItem : IParserAccessor, IAttributeAccessor
    {
        #region Private members
        internal static string _attrPrefix = "extended_";
        internal ListItem _listItem; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        public ExtendedListItem() : this("") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public ExtendedListItem(string text) : this(text, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        public ExtendedListItem(string text, string value) : this(text, value, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public ExtendedListItem(string text, string value, bool enabled) : this(text, value, enabled, ListItemGroupingType.None, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="groupingType">Type of the grouping.</param>
        public ExtendedListItem(string text, string value, ListItemGroupingType groupingType) : this(text, value, true, groupingType, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptGroupListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="enabled">if set to <c>true</c> the item is enabled.</param>
        /// <param name="groupingType">The opt-grouping type.</param>
        /// <param name="groupingText">The opt-grouping text.</param>
        public ExtendedListItem(string text, string value, bool enabled, ListItemGroupingType groupingType, string groupingText)
        {
            this._listItem = new ListItem(text, value, enabled);
            this.GroupingType = groupingType;
            this.GroupingText = groupingText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptGroupListItem"/> class.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        internal ExtendedListItem(ListItem item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item");
            }

            this._listItem = item;
        } 
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets a collection of attribute name and value pairs for the <see cref="OptGroupListItem"/> that are not directly supported by the class.
        /// </summary>
        /// <value>A System.Web.UI.AttributeCollection that contains a collection of name and value pairs.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.AttributeCollection Attributes
        {
            get { return this._listItem.Attributes; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OptGroupListItem"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if the optgroup list item is enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [Category("Behaviour")]
        public bool Enabled
        {
            get { return this._listItem.Enabled; }
            set { this._listItem.Enabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OptGroupListItem"/> is selected.
        /// </summary>
        /// <value><c>true</c> if the item is selected; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        [Category("Behaviour")]
        public bool Selected
        {
            get { return this._listItem.Selected; }
            set { this._listItem.Selected = value; }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty), DefaultValue(""), Localizable(true)]
        public string Text
        {
            get { return this._listItem.Text; }
            set { this._listItem.Text = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [Localizable(true), DefaultValue("")]
        public string Value
        {
            get { return this._listItem.Value; }
            set { this._listItem.Value = value; }
        }

        /// <summary>
        /// Gets or sets the opt-grouping type.
        /// </summary>
        /// <value>The opt-grouping type.</value>
        [Browsable(true), Description("The grouping type."), Category("OptGrouping"), DefaultValue(ListItemGroupingType.None), NotifyParentProperty(true)]
        public ListItemGroupingType GroupingType
        {
            get
            {
                if (null == this._listItem.Attributes[_attrPrefix + "GroupingType"])
                {
                    this._listItem.Attributes[_attrPrefix + "GroupingType"] = ListItemGroupingType.None.ToString();
                }
                return (ListItemGroupingType)Enum.Parse(typeof(ListItemGroupingType), this._listItem.Attributes[_attrPrefix + "GroupingType"]);
            }
            set { this._listItem.Attributes[_attrPrefix + "GroupingType"] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the opt-grouping text.
        /// </summary>
        /// <value>The opt-grouping text.</value>
        [Description("The grouping text."), Category("OptGrouping"), DefaultValue(""), NotifyParentProperty(true)]
        public string GroupingText
        {
            get
            {
                if (null == this._listItem.Attributes[_attrPrefix + "GroupingText"])
                {
                    this._listItem.Attributes[_attrPrefix + "GroupingText"] = "";
                }
                return (string)this._listItem.Attributes[_attrPrefix + "GroupingText"];
            }
            set { this._listItem.Attributes[_attrPrefix + "GroupingText"] = value; }
        }

        /// <summary>
        /// Gets or sets the group CSS class.
        /// </summary>
        /// <value>The group CSS class.</value>
        [Description("The optgroup element css class."), Category("OptGrouping"), NotifyParentProperty(true), DefaultValue("")]
        public string GroupCssClass
        {
            get { return _listItem.Attributes[_attrPrefix + "GroupCssClass"]; }
            set { _listItem.Attributes[_attrPrefix + "GroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the css class.
        /// </summary>
        [Description("The option element css class."), Category("OptGrouping"), NotifyParentProperty(true), DefaultValue("")]
        public string CssClass
        {
            get { return _listItem.Attributes[_attrPrefix + "CssClass"]; }
            set { _listItem.Attributes[_attrPrefix + "CssClass"] = value; }
        }
        #endregion

        #region Internal behaviour
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="o">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the specified System.Object is equal to the current System.Object; otherwise, false.</returns>
        public override bool Equals(object o)
        {
            if ((null == o) || !(o is ExtendedListItem))
            {
                return false;
            }

            return this._listItem.Equals(o) && (this.GroupingType == ((ExtendedListItem)o).GroupingType) && (this.GroupingText == ((ExtendedListItem)o).GroupingText);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this._listItem.GetHashCode();
        }

        /// <summary>
        /// Renders the attributes.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal void RenderAttributes(HtmlTextWriter writer)
        {
            this._listItem.Attributes.AddAttributes(writer);
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        string IAttributeAccessor.GetAttribute(string name)
        {
            return this._listItem.Attributes[name];
        }

        /// <summary>
        /// Sets the attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this._listItem.Attributes[name] = value;
        }

        /// <summary>
        /// When implemented by an ASP.NET server control, notifies the server control that an element, either XML or HTML, was parsed.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> that was parsed.</param>
        void IParserAccessor.AddParsedSubObject(object obj)
        {
            ((IParserAccessor)this._listItem).AddParsedSubObject(obj);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            switch (this.GroupingType)
            {
                case ListItemGroupingType.None:
                    {
                        return !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]";
                    }
                case ListItemGroupingType.New:
                    {
                        return string.Concat(!string.IsNullOrEmpty(this.GroupingText) ? this.GroupingText : "[Group]", ".", !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]");
                    }
                case ListItemGroupingType.Inherit:
                    {
                        return string.Concat(" ", ".", !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]");
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        } 
        #endregion

        #region Nested Types
        private enum DirtyFlags
        {
            None = 0,
            GroupingType = 1,
            GroupingText = 2,
            All = GroupingType | GroupingText
        }
        #endregion
    }

    /// <summary>
    /// List item grouping type.
    /// </summary>
    public enum ListItemGroupingType
    {
        /// <summary>
        /// Belongs to no group.
        /// </summary>
        None,
        /// <summary>
        /// This is a new group.
        /// </summary>
        New,
        /// <summary>
        /// The previous group is spaned over this group.
        /// </summary>
        Inherit
    }

}
