using System;
using System.Collections.Generic;

namespace SharpPieces.Web
{

    /// <summary>
    /// Represents a pair of 2 objects of the same type.
    /// </summary>
    public class Pair<T>
    {

        // Fields

        private T first, second;


        // Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public Pair(T first, T second)
        {
            this.first = first;
            this.second = second;
        }


        // Properties
        
        /// <summary>
        /// Gets the first.
        /// </summary>
        public T First
        {
            get { return this.first; }
        }

        /// <summary>
        /// Gets the second.
        /// </summary>
        public T Second
        {
            get { return this.second; }
        }

    }

}
