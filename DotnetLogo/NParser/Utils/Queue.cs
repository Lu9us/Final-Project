using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Utils
{
    /// <summary>
    /// .net queue was causing memory leaks this is its replacement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Queue<T>
    {


       public Queue()
        {
            array = new T[124];
        }


        private T[] array;
        
        public bool Empty()
        {
            return count < 1;

        }

        private int count;

        public int Count
        {
            get { return count; }
        }

        public void Enqueue(T data)
        {
            if (count >= array.Length)
            {
                int s = array.Length * 2;
                T[] newArray = new T[s];
                Array.Copy(array,newArray,array.Length);
                array = newArray;
            }

            array[count] = data;
            count++;

        }

        public T Dequeue()
        {
            if (Empty())
            {
                return default;
            }
            else
            {
                count--;
                T dat = array[count];
                array[count] = default;
                return dat;

            }

          
        }
    }
}


