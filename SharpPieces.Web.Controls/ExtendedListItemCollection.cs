using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web;
using System.Security.Permissions;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace SharpPieces.Web.Controls
{

    /// <summary>
    /// Wrapper over ListItemCollection containing OptGroupListItems.
    /// </summary>
    public sealed class ExtendedListItemCollection : IList, ICollection, IEnumerable, IStateManager
    {
        #region Private members
        internal readonly ListItemCollection _wrappedCollection; 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItemCollection"/> class.
        /// </summary>
        /// <param name="wrappedCollection">The wrapped collection.</param>
        public ExtendedListItemCollection(ListItemCollection wrappedCollection)
        {
            if (null == wrappedCollection)
            {
                throw new ArgumentNullException("wrappedCollection");
            }
            this._wrappedCollection = wrappedCollection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItemCollection"/> class.
        /// </summary>
        public ExtendedListItemCollection()
        {
            this._wrappedCollection = new ListItemCollection();
        } 
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(string item)
        {
            this._wrappedCollection.Add(item);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(ExtendedListItem item)
        {
            this._wrappedCollection.Add(this.GetSafeWrappedItem(item));
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(ExtendedListItem[] items)
        {
            if (null != items)
            {
                ListItem[] listItems = new ListItem[items.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    listItems[i] = items[i]._listItem;
                }
                this._wrappedCollection.AddRange(listItems);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception>
        public void Clear()
        {
            this._wrappedCollection.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ExtendedListItem item)
        {
            return this._wrappedCollection.Contains(this.GetSafeWrappedItem(item));
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.-or- <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception>
        /// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception>
        public void CopyTo(Array array, int index)
        {
            List<ExtendedListItem> list = this.ToList();
            Array.Copy(list.ToArray(), 0, array, index, list.Count);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return this.ToList().GetEnumerator();
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int IndexOf(ExtendedListItem item)
        {
            return this._wrappedCollection.IndexOf(this.GetSafeWrappedItem(item));
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, string item)
        {
            this._wrappedCollection.Insert(index, item);
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, ExtendedListItem item)
        {
            this._wrappedCollection.Insert(index, this.GetSafeWrappedItem(item));
        } 
        #endregion

        #region Internal behaviour
        /// <summary>
        /// When implemented by a class, loads the server control's previously saved view state to the control.
        /// </summary>
        /// <param name="state">An <see cref="T:System.Object"/> that contains the saved view state values for the control.</param>
        internal void LoadViewState(object state)
        {
            if (null != state)
            {
                object[] s = (object[])state;

                // first obj is the wrapped state

                ((IStateManager)this._wrappedCollection).LoadViewState(s[0]);

                // second is a list of grouping types
                IList<ListItemGroupingType> types = s[1] as IList<ListItemGroupingType>;

                // third is a list of grouping texts
                IList<string> texts = s[2] as IList<string>;

                // restore grouping type and text
                if ((null != types) && (types.Count == this._wrappedCollection.Count) && (null != texts) && (texts.Count == this._wrappedCollection.Count))
                {
                    for (int i = 0; i < this._wrappedCollection.Count; i++)
                    {
                        this[i].GroupingType = types[i];
                        this[i].GroupingText = texts[i];
                    }
                }
            }
        }

        /// <summary>
        /// When implemented by a class, saves the changes to a server control's view state to an <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"/> that contains the view state changes.
        /// </returns>
        internal object SaveViewState()
        {
            object[] state = new object[3];

            // first obj is the wrapped state
            object wrappedViewState = this.SaveViewState();

            IList<ExtendedListItem> items = this.ToList();

            // second is a list of grouping types
            IList<ListItemGroupingType> listItemGroupingTypes = new List<ListItemGroupingType>();
            foreach (ExtendedListItem item in items)
            {
                listItemGroupingTypes.Add(item.GroupingType);
            }

            // third is a list of grouping texts
            IList<string> listItemGroupingTexts = new List<string>();
            foreach (ExtendedListItem item in items)
            {
                listItemGroupingTexts.Add(item.GroupingText);
            }

            state[0] = wrappedViewState;
            state[1] = listItemGroupingTypes;
            state[2] = listItemGroupingTexts;

            return state;
        }

        /// <summary>
        /// When implemented by a class, instructs the server control to track changes to its view state.
        /// </summary>
        internal void TrackViewState()
        {
            ((IStateManager)this._wrappedCollection).TrackViewState();
        }


        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Remove(string item)
        {
            this._wrappedCollection.Remove(item);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Remove(ExtendedListItem item)
        {
            this._wrappedCollection.Remove(this.GetSafeWrappedItem(item));
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.IList"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void RemoveAt(int index)
        {
            this._wrappedCollection.RemoveAt(index);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        int IList.Add(object item)
        {
            return ((IList)this._wrappedCollection).Add((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        bool IList.Contains(object item)
        {
            return ((IList)this._wrappedCollection).Contains((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        int IList.IndexOf(object item)
        {
            return ((IList)this._wrappedCollection).IndexOf((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        void IList.Insert(int index, object item)
        {
            ((IList)this._wrappedCollection).Insert(index, (object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void IList.Remove(object item)
        {
            ((IList)this).Remove((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }

        /// <summary>
        /// When implemented by a class, loads the server control's previously saved view state to the control.
        /// </summary>
        /// <param name="state">An <see cref="T:System.Object"/> that contains the saved view state values for the control.</param>
        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        /// <summary>
        /// When implemented by a class, saves the changes to a server control's view state to an <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"/> that contains the view state changes.
        /// </returns>
        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        /// <summary>
        /// When implemented by a class, instructs the server control to track changes to its view state.
        /// </summary>
        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        /// <summary>
        /// Toes the list.
        /// </summary>
        /// <returns></returns>
        private List<ExtendedListItem> ToList()
        {
            List<ExtendedListItem> list = new List<ExtendedListItem>();
            foreach (ListItem wrappedItem in this._wrappedCollection)
            {
                list.Add(new ExtendedListItem(wrappedItem));
            }
            return list;
        } 
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
        public int Capacity
        {
            get { return this._wrappedCollection.Capacity; }
            set { }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.</returns>
        public int Count
        {
            get { return this._wrappedCollection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return this._wrappedCollection.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return this._wrappedCollection.IsSynchronized; }
        }

        /// <summary>
        /// Gets the <see cref="SharpPieces.Web.Controls.ExtendedListItem"/> at the specified index.
        /// </summary>
        /// <value></value>
        public ExtendedListItem this[int index]
        {
            get { return new ExtendedListItem(this._wrappedCollection[index]); }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
        public object SyncRoot
        {
            get { return this._wrappedCollection.SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
        bool IList.IsFixedSize
        {
            get { return ((IList)this._wrappedCollection).IsFixedSize; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
        object IList.this[int index]
        {
            get { return ((IList)this._wrappedCollection)[index]; }
            set { }
        }

        /// <summary>
        /// When implemented by a class, gets a value indicating whether a server control is tracking its view state changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if a server control is tracking its view state changes; otherwise, false.</returns>
        bool IStateManager.IsTrackingViewState
        {
            get { return ((IStateManager)this._wrappedCollection).IsTrackingViewState; }
        }

        /// <summary>
        /// Gets the safe wrapped item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private ListItem GetSafeWrappedItem(ExtendedListItem item)
        {
            return (null != item) ? item._listItem : null;
        } 
	#endregion
    }
}
