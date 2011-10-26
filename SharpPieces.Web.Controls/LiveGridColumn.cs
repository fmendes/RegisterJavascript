using System;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Collections.Generic;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Represents a LiveGrid column.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("HeaderText")]
    public class LiveGridColumn
    {

        // fields

        private LiveGrid owner = null;
        private bool visible = true;
        private string headerText = null;
        private bool allowResizing = true;
        private bool allowSorting = false;
        private string cssClass = null;
        private LiveGridColumnMapping mapping = new LiveGridColumnMapping();
        private LiveGridColumnGrouping grouping = new LiveGridColumnGrouping();


        // methods

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.GetType().Name;
        }
        

        // properties
        
        /// <summary>
        /// Sets the LiveGrid owner.
        /// </summary>
        /// <value>The LiveGrid owner.</value>
        internal LiveGrid Owner
        {
            set { this.owner = value; }
        }

        /// <summary>
        /// Gets a value indicating whether a control is being used on a design surface.
        /// </summary>
        /// <value></value>
        /// <returns>true if the control is being used in a designer; otherwise, false.</returns>
        protected bool DesignMode
        {
            get { return (null != this.owner) && this.owner.DesignMode; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this column is visible.
        /// </summary>
        /// <value><c>True</c> if visible; otherwise, <c>false</c>.</value>
        [Category("Behavior"), Description("The column visibility."), DefaultValue(true), NotifyParentProperty(true)]
        public virtual bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }
                
        /// <summary>
        /// Gets or sets the column header.
        /// </summary>
        /// <value>The column header.</value>
        [Category("Appearance"), Description("The column header."), DefaultValue(null), NotifyParentProperty(true)]
        public virtual string HeaderText
        {
            get { return this.headerText; }
            set { this.headerText = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this column is resizable.
        /// </summary>
        /// <value><c>True</c> if resizable; otherwise, <c>false</c>.</value>
        [Category("Behavior"), Description("The column resize ability."), DefaultValue(true)]
        public virtual bool AllowResizing
        {
            get { return this.allowResizing; }
            set { this.allowResizing = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the column allows sort.
        /// If true the sort will automaticaly be bound field.
        /// </summary>
        /// <value><c>True</c> if the column allows sort; otherwise, <c>false</c>.</value>
        [Category("Behavior"), Description("The column sort ability."), DefaultValue(false)]
        public bool AllowSorting
        {
            get { return this.allowSorting; }
            set { this.allowSorting = value; }
        }
        
        /// <summary>
        /// Gets or sets the column CSS class.
        /// </summary>
        /// <value>The column CSS class.</value>
        [Category("Styles"), Description("The column css class."), DefaultValue(null)]
        public virtual string CssClass
        {
            get { return this.cssClass; }
            set { this.cssClass = value; }
        }
        
        /// <summary>
        /// Gets or sets the mapping information.
        /// </summary>
        /// <value>The mapping information.</value>
        [Category("Behavior"), Description("The column mapping information."), DefaultValue((string)null), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public LiveGridColumnMapping Mapping
        {
            get
            {
                if (null == this.mapping)
                {
                    this.mapping = new LiveGridColumnMapping();
                }
                return this.mapping;
            }
            set { this.mapping = value; }
        }
        
        /// <summary>
        /// Gets or sets the grouping information.
        /// </summary>
        /// <value>The grouping information.</value>
        [Category("Behavior"), Description("The column grouping information."), DefaultValue((string)null), MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public LiveGridColumnGrouping Grouping
        {
            get
            {
                if (null == this.grouping)
                {
                    this.grouping = new LiveGridColumnGrouping();
                }
                return this.grouping;
            }
            set { this.grouping = value; }
        }
        

        // nested types

        /// <summary>
        /// Defines the way a column is mapped.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("MappingType")]
        public class LiveGridColumnMapping
        {

            // fields

            private ColumnMappingType mappingType = ColumnMappingType.Field;
            private FieldColumnMapping fieldMapping = new FieldColumnMapping();
            private ExpressionColumnMapping expressionMapping = new ExpressionColumnMapping();


            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                switch (mappingType)
                {
                    case ColumnMappingType.Field:
                        return this.fieldMapping.ToString();

                    case ColumnMappingType.Expression:
                        return this.expressionMapping.ToString();

                    default:
                        return "(not supported)";
                }
            }


            // properties

            /// <summary>
            /// Gets or sets the mapping type.
            /// </summary>
            /// <value>The mapping type.</value>
            [Category("Default"), Description("The column mapping type. Choose between a simple Field mapping and a complex expression mapping."), DefaultValue(ColumnMappingType.Field)]
            public ColumnMappingType MappingType
            {
                get { return this.mappingType; }
                set { this.mappingType = value; }
            }
            
            /// <summary>
            /// Gets or sets the field mapping.
            /// </summary>
            /// <value>The field mapping.</value>
            [Category("Default"), Description("The field mapping is relevant just if the mapping type is field. Simply choose the field name to bind."), DefaultValue(null), MergableProperty(false)]
            [PersistenceMode(PersistenceMode.InnerProperty)]
            public FieldColumnMapping FieldMapping
            {
                get
                {
                    if (null == this.fieldMapping)
                    {
                        this.fieldMapping = new FieldColumnMapping();
                    }
                    return this.fieldMapping;
                }
                set { this.fieldMapping = value; }
            }
            
            /// <summary>
            /// Gets or sets the expression mapping.
            /// </summary>
            /// <value>The expression mapping.</value>
            [Category("Default"), Description("The field mapping is relevant just if the mapping type is expression. Add an expression like you do in string format and after add the field names to be placed into the expression template; be cautious on the field names order."), DefaultValue((string)null), MergableProperty(false)]
            [PersistenceMode(PersistenceMode.InnerProperty)]
            public ExpressionColumnMapping ExpressionMapping
            {
                get
                {
                    if (null == this.expressionMapping)
                    {
                        this.expressionMapping = new ExpressionColumnMapping();
                    }
                    return this.expressionMapping;
                }
                set { this.expressionMapping = value; }
            }

        }

        /// <summary>
        /// Represents a simple field mapping type.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("FieldName")]
        public class FieldColumnMapping
        {

            // fields

            private string fieldName;
            private string formatting;


            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                if (string.IsNullOrEmpty(this.fieldName))
                {
                    return "(not set)";
                }
                else
                {
                    return this.fieldName;
                }
            }


            // properties

            /// <summary>
            /// Gets or sets the name of the field to be bound.
            /// </summary>
            /// <value>The name of the field to be bound.</value>
            [Category("Default"), Description("The name of the field to be bound."), DefaultValue((string)null)]
            public string FieldName
            {
                get { return this.fieldName; }
                set { this.fieldName = value; }
            }

            /// <summary>
            /// Gets or sets the output formatting.
            /// </summary>
            /// <value>The output formatting.</value>
            [Category("Default"), Description("The output formatting."), DefaultValue((string)null)]
            public string Formatting
            {
                get { return this.formatting; }
                set { this.formatting = value; }
            }

        }

        /// <summary>
        /// Represents a complex mapping type through an expression template.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("Expression")]
        public class ExpressionColumnMapping
        {

            // fields

            private string expression;
            private string[] expressionFieldNames = new string[] { };
            private string sortFieldName;
            
            
            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                if (string.IsNullOrEmpty(this.expression))
                {
                    return "(not set)";
                }
                else
                {
                    try
                    {
                        return string.Format(this.expression, this.expressionFieldNames);
                    }
                    catch
                    {
                        return "(invalid)";
                    }
                }
            }


            // properties            

            /// <summary>
            /// Gets or sets the mapping expression.
            /// It has to be built just like a string format expression where the placeholders are actualy field names.
            /// Ex. Between {0} and {1} {2} orders were made, where StartDate, EndDate and OrderCount are specified as ExpressionFieldNames.
            /// </summary>
            /// <value>The mapping expression.</value>
            [Category("Default"), Description("The mapping expression; it has to be built just like a string format expression where the placeholders are actualy field names. Ex. Between {0} and {1} {2} orders were made, where StartDate, EndDate and OrderCount are specified as ExpressionFieldNames."), DefaultValue((string)null)]
            public string Expression
            {
                get { return this.expression; }
                set { this.expression = value; }
            }

            /// <summary>
            /// Gets or sets the expression field names.
            /// The order is important.
            /// </summary>
            /// <value>The expression field names.</value>
            [Category("Default"), Description("The expression field names. The order is important."), DefaultValue((string)null), TypeConverter(typeof(StringArrayConverter))]
            [PersistenceMode(PersistenceMode.Attribute)]
            public string[] ExpressionFieldNames
            {
                get
                {
                    if (null == this.expressionFieldNames)
                    {
                        this.expressionFieldNames = new string[] { };
                    }
                    return this.expressionFieldNames;
                }
                set { this.expressionFieldNames = value; }
            }

            /// <summary>
            /// Gets or sets the sort field name.
            /// </summary>
            /// <value>The sort field name.</value>
            [Category("Default"), Description("The sort field name."), DefaultValue((string)null)]
            public string SortFieldName
            {
                get { return this.sortFieldName; }
                set { this.sortFieldName = value; }
            }

        }

        /// <summary>
        /// Represents column grouping set of properties.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("GroupingType")]
        public class LiveGridColumnGrouping
        {

            // fields

            private ColumnGroupingType groupingType = ColumnGroupingType.None;
            private string groupText;


            // methods

            /// <summary>
            /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                switch (this.groupingType)
                {
                    case ColumnGroupingType.None:
                        return "(none)";

                    case ColumnGroupingType.Inherit:
                        return "(inherit)";

                    case ColumnGroupingType.New:
                        if (string.IsNullOrEmpty(this.groupText))
                        {
                            return "(not set)";
                        }
                        else
                        {
                            return this.groupText;
                        }

                    default:
                        return "(not supported)";
                }
            }


            // properties

            /// <summary>
            /// Gets or sets the grouping type.
            /// </summary>
            /// <value>The grouping type.</value>
            [Category("Default"), Description("The grouping type."), DefaultValue(ColumnGroupingType.None)]
            public ColumnGroupingType GroupingType
            {
                get { return this.groupingType; }
                set { this.groupingType = value; }
            }

            /// <summary>
            /// Gets or sets the group text.
            /// </summary>
            /// <value>The group text.</value>
            [Category("Default"), Description("The group text."), DefaultValue((string)null)]
            public string GroupText
            {
                get { return this.groupText; }
                set { this.groupText = value; }
            }

        }

        /// <summary>
        /// The column mapping type.
        /// </summary>
        public enum ColumnMappingType
        {
            /// <summary>
            /// Simple field mapping type.
            /// </summary>
            Field,
            /// <summary>
            /// Expression mapping type.
            /// </summary>
            Expression
        }

        /// <summary>
        /// Column grouping type.
        /// </summary>
        public enum ColumnGroupingType
        {
            /// <summary>
            /// Belongs to no group.
            /// </summary>
            None,
            /// <summary>
            /// The previous group is spaned over this group.
            /// </summary>
            Inherit,
            /// <summary>
            /// This is a new group.
            /// </summary>
            New
        }

    }


    /// <summary>
    /// Represents a collection of LiveGridColumn.
    /// </summary>
    public class LiveGridColumnCollection : List<LiveGridColumn>
    {

        // fields

        private bool isIndexed = false;


        // methods

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public new void Insert(int index, LiveGridColumn item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item.");
            }
            else
            {
                base.Insert(index, item);
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public new void Add(LiveGridColumn item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item.");
            }
            else
            {
                base.Add(item);
            }
        }


        // properties

        /// <summary>
        /// Gets or sets a value indicating whether this collection contains a index indicator.
        /// </summary>
        /// <value>
        /// 	<c>True</c> if this collection contains a index indicator; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexed
        {
            get { return this.isIndexed; }
            set { this.isIndexed = value; }
        }

    }
    
}
