using System;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI;
using System.Drawing.Design;
using System.Web;
using System.Security.Permissions;

namespace SharpPieces.Web.Controls
{
    /// <summary>
    /// Extended DropDownList control.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("ExtendedItems")]
    public class ExtendedDropDownList : DropDownList
    {
        #region Private members
        private ExtendedListItemCollection _extendedItems;
        internal const string _optGroupAttributeKey = "optgroup";
        internal const string _separator = "#";
        bool _optGroupStarted = false;
        bool _selected = false; 
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the collection of grouped items in the list control.
        /// </summary>
        /// <value>The grouped items.</value>
        [Category("Default"), Description("The items in a grouped manner."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ExtendedListItemCollection ExtendedItems
        {
            get
            {
                // get a new collection if neccesary and 
                // hook the drop down list items collection
                if (null == this._extendedItems)
                {
                    this._extendedItems = new ExtendedListItemCollection(base.Items);
                }

                return this._extendedItems;
            }
        }

        /// <summary>
        /// Gets the collection of items in the list control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItemCollection"/> that represents the items within the list. The default is an empty list.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListItemCollection Items
        {
            get
            {
                // get a new collection if neccesary and 
                // hook the drop down list items collection
                if (null == this._extendedItems)
                {
                    this._extendedItems = new ExtendedListItemCollection(base.Items);
                }

                return this._extendedItems._wrappedCollection;
            }

        }

        /// <summary>
        /// Gets the selected item extended.
        /// </summary>
        /// <value>The selected item extended.</value>
        public ExtendedListItem SelectedItemExtended
        {
            get { return new ExtendedListItem(this.SelectedItem); }
        }

        public string SelectedGroup
        {
            get 
            {
                //get selected item
                ExtendedListItem item = this.SelectedItemExtended;

                if (item.GroupingType == ListItemGroupingType.New)
                    return item.GroupingText;
                else if (item.GroupingType == ListItemGroupingType.None)
                    return string.Empty;
                else
                {
                    // find index...
                    // go through prev items, to find group text
                    int tmpIdx = this.ExtendedItems.IndexOf(item) - 1;

                    while (tmpIdx >= 0)
                    {
                        if (this.ExtendedItems[tmpIdx].GroupingType == ListItemGroupingType.New)
                        {
                            return this.ExtendedItems[tmpIdx].GroupingText;
                        }

                        tmpIdx--;
                    }

                    //item was not found...
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Internal behaviour
        /// Saves the state of the view.
        /// </summary>
        protected override object SaveViewState()
        {
            // Create an object array with one element for the CheckBoxList's
            // ViewState contents, and one element for each ListItem in skmCheckBoxList
            object[] state = new object[this.Items.Count + 1];

            object baseState = base.SaveViewState();
            state[0] = baseState;

            // Now, see if we even need to save the view state
            bool itemHasAttributes = false;
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i].Attributes.Count > 0)
                {
                    itemHasAttributes = true;

                    // Create an array of the item's Attribute's keys and values
                    object[] attribKV = new object[this.Items[i].Attributes.Count * 2];
                    int k = 0;
                    foreach (string key in this.Items[i].Attributes.Keys)
                    {
                        attribKV[k++] = key;
                        attribKV[k++] = this.Items[i].Attributes[key];
                    }

                    state[i + 1] = attribKV;
                }
            }

            // return either baseState or state, depending on whether or not
            // any ListItems had attributes
            if (itemHasAttributes)
                return state;
            else
                return baseState;
        }

        /// <summary>
        /// Loads the state of the view.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;

            // see if savedState is an object or object array
            if (savedState is object[])
            {
                // we have an array of items with attributes
                object[] state = (object[])savedState;
                base.LoadViewState(state[0]);   // load the base state

                for (int i = 1; i < state.Length; i++)
                {
                    if (state[i] != null)
                    {
                        // Load back in the attributes
                        object[] attribKV = (object[])state[i];
                        for (int k = 0; k < attribKV.Length; k += 2)
                            this.Items[i - 1].Attributes.Add(attribKV[k].ToString(),
                                                           attribKV[k + 1].ToString());
                    }
                }
            }
            else
                // we have just the base state
                base.LoadViewState(savedState);
        }

        /// <summary>
        /// Renders the items in the <see cref="T:System.Web.UI.WebControls.ListControl"/> control.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream used to write content to a Web page.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.ExtendedItems.Count; i++)
                {
                    ExtendedListItem item = this.ExtendedItems[i];

                    if (item.GroupingType == ListItemGroupingType.New) //.Attributes[_optGroupAttributeKey] != null)
                    {
                        WriteOptionGroup(item, writer);

                        WriteOption(item, writer);
                    }
                    else if (item.GroupingType == ListItemGroupingType.Inherit)
                    {
                        WriteOption(item, writer);
                    }
                    else // ListItemGroupingType.None
                    {
                        if (_optGroupStarted)
                            writer.WriteEndTag("optgroup");

                        _optGroupStarted = false;

                        WriteOption(item, writer);
                    }
                }

                if (_optGroupStarted)
                    writer.WriteEndTag("optgroup");
            }
        }

        /// <summary>
        /// Writes the option group.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void WriteOptionGroup(ExtendedListItem item, HtmlTextWriter writer)
        {
            if (_optGroupStarted)
                writer.WriteEndTag("optgroup");

            writer.WriteBeginTag("optgroup");
            writer.WriteAttribute("label", item.GroupingText);
            if (!item.Enabled)
                writer.WriteAttribute("disabled", "disabled");
            if (!string.IsNullOrEmpty(item.GroupCssClass))
                writer.WriteAttribute("class", item.GroupCssClass);
            writer.Write('>');
            // writer.WriteLine();
            _optGroupStarted = true;
        }

        /// <summary>
        /// Writes the option.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void WriteOption(ExtendedListItem item, HtmlTextWriter writer)
        {
            writer.WriteBeginTag("option");
            if (item.Selected)
            {
                if (_selected)
                {
                    this.VerifyMultiSelect();
                }
                _selected = true;
                writer.WriteAttribute("selected", "selected");
            }

            if (!string.IsNullOrEmpty(item.CssClass))
                writer.WriteAttribute("class", item.CssClass);

            writer.WriteAttribute("value", item.Value, true);

            if (item.Attributes.Count > 0)
            {
                StateBag bag = new StateBag();
                foreach (string attrKey in item.Attributes.Keys)
                {
                    if (attrKey.IndexOf(ExtendedListItem._attrPrefix) == -1)
                        bag.Add(attrKey, item.Attributes[attrKey]);
                }

                System.Web.UI.AttributeCollection coll = new System.Web.UI.AttributeCollection(bag);

                coll.Render(writer);
            }

            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, item.Value);
            }

            writer.Write('>');
            HttpUtility.HtmlEncode(item.Text, writer);
            writer.WriteEndTag("option");
            writer.WriteLine();
        }
        #endregion 
    }

}
