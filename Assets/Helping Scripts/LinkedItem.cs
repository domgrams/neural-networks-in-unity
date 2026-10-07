using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace System.Collections.Generic
{
    /// <summary>
    /// a class that provides a few methods and properties to organize and use 2-dimensional chain of <see cref="LinkedItem{T}"/> properly
    /// </summary>
    /// <typeparam name="T">the contents of the individual <see cref="LinkedItem{T}"/></typeparam>
    public class LinkedChain2<T>
    {
        public int Coordinates1 { get => c1; }
        public int Coordinates2 { get => c2; }

        /// <summary>
        /// the 2D <see cref="LinkedItem{T}"/> at the start of the chain (coordinates [0,0])
        /// </summary>
        public LinkedItem<LinkedItem<T>> Start { get => start; } 

        /// <summary>
        /// the 2D <see cref="LinkedItem{T}"/> at a start of the selected 1D chain (coordinates [s,0])
        /// </summary>
        public LinkedItem<LinkedItem<T>> Selector1 { get => selector1; }

        /// <summary>
        /// the selected <see cref="LinkedItem{T}"/> in the 2-dimensional chain(coordinates [s,t])
        /// </summary>
        public LinkedItem<T> Selector2 { get => selector2; }

        /// <summary>
        /// whether the selector is not out of bounds, i.e. not Living if the selected <see cref="LinkedItem{T}"/> is null
        /// </summary>
        public bool Alive { get => selector2 != null; }

        public int Count1 { get => Start.Depth + 1; }
        public int Count2 { get => Selector1.Get().Depth + 1; }

        LinkedItem<LinkedItem<T>> start;
        LinkedItem<LinkedItem<T>> selector1;
        LinkedItem<T> selector2;

        private int c1;
        private int c2;
        

        /// <summary>
        /// initializes a new <see cref="LinkedChain2{T}"/> with the starting <see cref="LinkedItem{T}"/> given its contents (<paramref name="starting"/>)
        /// </summary>
        /// <param name="starting">the contents of the completely first <see cref="LinkedItem{T}"/> in the chain</param>
        public LinkedChain2(T starting)
        {
            start = new LinkedItem<LinkedItem<T>>(new LinkedItem<T>(starting));
            Reset();
        }

        /// <summary>
        /// selects and then gets or sets the given <see cref="LinkedItem{T}"/> by its coordinates
        /// </summary>
        /// <param name="index1">1st coordinate</param>
        /// <param name="index2">2nd coordinate</param>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> this[float index1, float index2]
        {
            get
            {
                if (c1 > index1 || c2 > index2)
                    Reset();

                while(c1 < index1)
                {
                    Next1();
                }
                while (c2 < index2)
                {
                    Next2();
                }
                return Selector2;
            }
            set
            {
                if (c1 > index1 || c2 > index2 - 1)
                    Reset();
                while (c1 < index1)
                {
                    Next1();
                }
                while (c2 < index2 - 1)
                {
                    Next2();
                }
                Selector2.Next = value;
                Next2();
            }
        }

        /// <summary>
        /// gets the contents of the selected <see cref="LinkedItem{T}"/>
        /// </summary>
        /// <returns>the contents of the <see cref="Selector2"/></returns>
        public T Get()
        {
            return Selector2.Contents;
        }

        /// <summary>
        /// selects the first <see cref="LinkedItem{T}"/>, i.e. <see cref="Selector2"/> = <see cref="Start"/>
        /// </summary>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> Reset()
        {
            selector2 = start.Contents;
            selector1 = start;
            c1 = 0;
            c2 = 0;
            return selector2;
        }

        /// <summary>
        /// selects the first <see cref="LinkedItem{T}"/> in the selected 1D chain, i.e. <see cref="Selector2"/> = <see cref="Selector1"/>
        /// </summary>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> ResetSelector2()
        {
            selector2 = selector1.Contents;
            c2 = 0;
            return selector2;
        }

        /// <summary>
        /// selects the next <see cref="LinkedItem{T}"/> in the chain, i.e. <see cref="Selector2"/> = <see cref="LinkedItem{T}.Next"/>
        /// <para>if the next selected is null, it selects the next 1D chain using <see cref="Selector1"/>, if that is also null, <see cref="Alive"/> turns false</para>
        /// </summary>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> Next2()
        {
            c2++;
            if (selector2.Next == null)
            {
                c1++;
                selector1 = selector1.Next;
                if (selector1 == null)
                {
                    selector2 = null;
                    return selector2;
                }
                return ResetSelector2();
            }
            selector2 = selector2.Next;
            return selector2;
        }

        /// <summary>
        /// selects the next 1D chain in the 2D chain, i.e. <see cref="Selector1"/> = <see cref="LinkedItem{T}.Next"/>, while selecting the first <see cref="LinkedItem{T}"/> in this 1D chain
        /// <para>if the next selected 1D chain is null, <see cref="Alive"/> turns false</para>
        /// </summary>
        /// <returns><see cref="Selector1"/></returns>
        public LinkedItem<LinkedItem<T>> Next1()
        {
            c1++;
            selector1 = selector1.Next;

            if (selector1 == null)
            {
                selector2 = null;
                return selector1;
            }

            ResetSelector2();
            return selector1;
        }

        /// <summary>
        /// selects the next <see cref="LinkedItem{T}"/> in the chain, i.e. <see cref="Selector2"/> = <see cref="LinkedItem{T}.Next"/>
        /// <para>if the next selected is null, it selects the next 1D chain using <see cref="Selector1"/>, if that is also null, it triggers <see cref="Reset"/></para>
        /// </summary>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> NextCycle()
        {
            if (selector2.Next == null && selector1.Next == null) return Reset();
            else return Next2();
        }


        /// <summary>
        /// appends <see cref="LinkedItem{T}"/> to the end of the selected 1D chain (like in <seealso cref="LinkedItem{T}.Append(T)"/>) given the <paramref name="contents"/> while selecting it
        /// </summary>
        /// <param name="contents">contents of the new <see cref="LinkedItem{T}"/></param>
        /// <returns><see cref="Selector2"/></returns>
        public LinkedItem<T> Append2(T item)
        {
            c2++;
            selector2 = selector2.Append(item);
            return selector2;
        }

        /// <summary>
        /// appends a new 1D chain <see cref="LinkedItem{T}"/> to the end of the <see cref="LinkedChain2{T}"/> (like in <seealso cref="LinkedItem{T}.Append(T)"/>) given the <paramref name="contents"/> while selecting it
        /// </summary>
        /// <param name="item">contents of the new <see cref="LinkedItem{T}"/> in the chain</param>
        /// <returns><see cref="Selector1"/></returns>
        public LinkedItem<LinkedItem<T>> Append1(T item)
        {
            c1++;
            selector1 = selector1.Append(new LinkedItem<T>(item));
            ResetSelector2();
            return selector1;
        }

    }


    /// <summary>
    /// a class that provides a few methods and properties to organize and use <see cref="LinkedItem{T}"/> properly, as a 1D chain
    /// </summary>
    /// <typeparam name="T">the contents of the individual <see cref="LinkedItem{T}"/></typeparam>
    public class LinkedChain<T>
    {
        /// <summary>
        /// the <see cref="LinkedItem{T}"/> at the start of the chain
        /// </summary>
        public LinkedItem<T> Start { get => start; }

        /// <summary>
        /// the selected <see cref="LinkedItem{T}"/> in the chain
        /// </summary>
        public LinkedItem<T> Selector { get => selector; }

        /// <summary>
        /// whether the the selected <see cref="LinkedItem{T}"/> is not out of bounds, i.e. is not null
        /// </summary>
        public bool Alive
        {
            get
            {
                return selector != null;
            }
        }

        LinkedItem<T> start;
        LinkedItem<T> selector;



        /// <summary>
        /// initializes a new <see cref="LinkedChain{T}"/> with the starting <see cref="LinkedItem{T}"/> given its contents (<paramref name="starting"/>)
        /// </summary>
        /// <param name="starting">the contents of the first <see cref="LinkedItem{T}"/> in the chain</param>
        public LinkedChain(T starting)
        {
            start = new LinkedItem<T>(starting);
            Reset();
        }

        /// <summary>
        /// gets the contents of the selected <see cref="LinkedItem{T}"/>
        /// </summary>
        /// <returns>the contents of the <see cref="Selector"/></returns>
        public T Get()
        {
            return Selector.Contents;
        }

        /// <summary>
        /// selects the first <see cref="LinkedItem{T}"/>, i.e. <see cref="Selector"/> = <see cref="Start"/>
        /// </summary>
        /// <returns><see cref="Selector"/></returns>
        public LinkedItem<T> Reset()
        {
            selector = start;
            return selector;
        }

        /// <summary>
        /// selects the next <see cref="LinkedItem{T}"/> in the chain, i.e. <see cref="Selector"/> = <see cref="LinkedItem{T}.Next"/>
        /// <para>if the next selected is null, <see cref="Alive"/> turns false</para>
        /// </summary>
        /// <returns><see cref="Selector"/></returns>
        public LinkedItem<T> Next()
        {
            selector = selector.Next;
            return selector;
        }

        /// <summary>
        /// selects the next <see cref="LinkedItem{T}"/> in the chain, i.e. <see cref="Selector"/> = <see cref="LinkedItem{T}.Next"/>
        /// <para> if the next selected is null, it executes the <see cref="Reset"/> of the chain</para>
        /// </summary>
        /// <returns><see cref="Selector"/></returns>
        public LinkedItem<T> NextCycle()
        {
            if (selector.Next == null) return Reset();
            else return Next();
        }

        /// <summary>
        /// appends <see cref="LinkedItem{T}"/> to the end of the chain (like in <seealso cref="LinkedItem{T}.Append(T)"/>) given the <paramref name="contents"/> while selecting it
        /// </summary>
        /// <param name="contents">contents of the new <see cref="LinkedItem{T}"/></param>
        /// <returns><see cref="Selector"/></returns>
        public LinkedItem<T> Append(T contents)
        {
            selector = selector.Append(contents);
            return selector;
        }
    }


    /// <summary>
    /// LinkedItem is a unit-based structure that is connected to a chain of units by referencing the next <see cref="Next"/> LinkedItem.
    /// <para>It has its data stored in type-<typeparamref name="T"/> contents <see cref="Contents"/>.</para>
    /// Visualization of acyclic form:
    /// <para> LI1 --> LI2 --> ... --> LI{N} --> null</para>
    /// Visualization of cyclic form:
    /// <para> LI1 --> LI2 --> ... --> LI{N} --> LI1 --> LI2 --> ...</para>
    /// 
    /// Formally, LinkedItem is a vertex of one-way directed path or cycle (in a graph theory), and it is referencing another vertex that is connected to it by an edge pointing towards the another vertex
    /// </summary>
    /// <typeparam name="T">Contents type</typeparam>
    public class LinkedItem<T>
    {
        /// <summary>
        /// gets or sets the <see cref="LinkedItem{T}"/> linked to this <see cref="LinkedItem{T}"/>
        /// <para>
        /// LI --> TLI --> LI --> LI
        /// </para> 
        /// <c>TLI.Next</c> .... ^
        /// </summary>
        public LinkedItem<T> Next { get { return next; } set { next = value; } }

        /// <summary>
        /// gets or sets the contents [type <typeparamref name="T"/>] of this <see cref="LinkedItem{T}"/>
        /// </summary>
        public T Contents { get { return contents; } set { contents = value; } }

        /// <summary>
        /// gets the number of <see cref="LinkedItem{T}"/> chained to this <see cref="LinkedItem{T}"/>
        /// <para>TLI --> LI --> LI --> LI</para>
        /// <code>TLI.Depth => 3</code>
        /// <para>if this <see cref="LinkedItem{T}"/> is chained to itself (i.e. cyclic form), it returns number of <see cref="LinkedItem{T}"/> in this cycle, times -1 to seperate it from acyclic forms.</para>
        /// <para>TLI --> LI --> LI --> TLI</para>
        /// <code>TLI.Depth => -2</code>
        /// </summary>
        public int Depth
        {
            get
            {
                int depth = 0;
                LinkedItem<T> item = Next;
                while(item != null && item != this)
                {
                    item = item.Next;
                    depth++;
                }
                if (item == this) return -depth; // is a cycle
                return depth; // is not a cycle
            }
        }


        private T contents;
        private LinkedItem<T> next;


        /// <summary>
        /// initializes new <see cref="LinkedItem{T}"/> with given <see cref="LinkedItem{T}.Contents"/>
        /// </summary>
        /// <param name="item">setting the contents of this <see cref="LinkedItem{T}"/></param>
        public LinkedItem(T item){
            contents = item;
            next = null;
        }

        /// <summary>
        /// gets the <see cref="LinkedItem{T}.Contents"/>
        /// </summary>
        /// <returns><see cref="Contents"/> [of type <typeparamref name="T"/>]</returns>
        public T Get()
        {
            return Contents;
        }

        /// <summary>
        /// sets the <see cref="Next"/> item as a new <see cref="LinkedItem{T}"/> with given <see cref="Contents"/>
        /// <para>TLI --> LI --> LI --> LI</para>
        /// <code>TLI.Add(contens)</code>
        /// <para>TLI --> <c>new LI(contents)</c></para>
        /// </summary>
        /// <param name="contents">setting the contents of the new <see cref="LinkedItem{T}"/></param>
        /// <returns>the new <see cref="LinkedItem{T}"/> that is the <see cref="Next"/> of this <see cref="LinkedItem{T}"/></returns>
        public LinkedItem<T> Add(T contents)
        {
            Next = new LinkedItem<T>(contents);
            return Next;
        }

        /// <summary>
        /// sets the <see cref="Next"/> item as a <see cref="LinkedItem{T}"/> <paramref name="newItem"/>
        /// <para>TLI --> LI --> LI --> LI</para>
        /// <code>TLI.Add(NLI)</code>
        /// <para>TLI --> NLI</para>
        /// </summary>
        /// <param name="newItem">the <see cref="Next"/> item to set</param>
        /// <returns><paramref name="newItem"/></returns>
        public LinkedItem<T> Add(LinkedItem<T> newItem)
        {
            Next = newItem;
            return Next;
        }

        /// <summary>
        /// sets the next <see cref="LinkedItem{T}"/> in the chain as a new <see cref="LinkedItem{T}"/> with given <see cref="Contents"/>
        /// <para>TLI --> LI --> LI --> LI</para>
        /// <code>TLI.Append(contents)</code>
        /// <para>TLI --> LI --> LI --> LI --> <c>new LI(contents)</c></para>
        /// when the form is cyclic (TLI --> LI --> ... --> TLI), it throws the <see cref="NotFiniteNumberException"/>
        /// </summary>
        /// <param name="contents">setting the contents of the new <see cref="LinkedItem{T}"/></param>
        /// <returns>the new <see cref="LinkedItem{T}"/></returns>
        public LinkedItem<T> Append(T contents)
        {
            return Append(new LinkedItem<T>(contents));
        }

        /// <summary>
        /// sets the next <see cref="LinkedItem{T}"/> in the chain as a new <see cref="LinkedItem{T}"/> with given <see cref="Contents"/>
        /// <para>TLI --> LI --> LI --> LI</para>
        /// <code>TLI.Append(NLI)</code>
        /// <para>TLI --> LI --> LI --> LI --> NLI</para>
        /// when the form is cyclic (TLI --> LI --> ... --> TLI), it throws the <see cref="NotFiniteNumberException"/>
        /// </summary>
        /// <param name="newItem">the <see cref="LinkedItem{T}"/> to add at the end of the chain</param>
        /// <returns><paramref name="newItem"/></returns>
        public LinkedItem<T> Append(LinkedItem<T> newItem)
        {
            LinkedItem<T> item = this;
            while (item.Next != null)
            {
                if (item.Next == this) throw new NotFiniteNumberException("Linked Item is not appendable: is a cycle"); // is a cycle
                item = item.Next;
            }
            item.Next = newItem;
            return item.Next;
        }

        /// <summary>
        /// gets the next <paramref name="index"/>th <see cref="LinkedItem{T}"/> in the chain after this <see cref="LinkedItem{T}"/>
        /// <para>TLI --> LI1 --> LI2 --> LI3</para>
        /// <code>TLI.GetNext(2) => LI2</code>
        /// </summary>
        /// <param name="index">number of <see cref="LinkedItem{T}"/> in the chain after this one</param>
        /// <returns>the next <paramref name="index"/>th <see cref="LinkedItem{T}"/> in the chain after this <see cref="LinkedItem{T}"/></returns>
        public LinkedItem<T> GetNext(int index = 1)
        {
            LinkedItem<T> item = this;
            for (int i = 0; i < index; i++)
            {
                item = item.Next;
            }
            return item;
        }

    }
}
