﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace NParser.Performance
{
    internal enum TrackingState
    {
        CallLevel,
        OpLevel,
        StackLevel,
        LineLevel,
        All,
        None
        
    }

    public static class PeformanceTracker
   {
       private static readonly Dictionary<string, Stopwatch> watchList;
       private static Thread writerThread;
       private static Utils.Queue<String> writeQueue;
       private static int waitTime = 50;
       private static string filename = "Performance" + DateTime.Now.ToString("ddMMyyyyHHmmss")+".csv";
        private static TrackingState trackingState;
       private static object queueLock = new object();

        static PeformanceTracker()
        {
            watchList = new Dictionary<string, Stopwatch>();
            writeQueue = new Utils.Queue<string>();
            writerThread = new Thread(Write);
            writerThread.IsBackground = true;
           
            trackingState = TrackingState.None;
           
#if OPTRACK
                   trackingState = TrackingState.OpLevel;     
#endif
#if CALLTRACK

            trackingState = TrackingState.CallLevel;
#endif
#if STACKTRACK

            trackingState = TrackingState.StackLevel;
#endif
#if LINETRACK
          trackingState = TrackingState.LineLevel;
#endif
#if ALLTRACK
            trackingState = TrackingState.All;
#endif
            if (trackingState != TrackingState.None)
            {
                writerThread.Start();
            }
        }

       public static void StartStopWatch(string name)
       {
           if (!watchList.ContainsKey(name))
           {
               watchList.Add(name, new Stopwatch());
               watchList[name].Start();
           }
           else
           {

           }
       }
        public static int StopWithoutWrite(string name)
        {
            if (watchList.ContainsKey(name))
            {
                watchList[name].Stop();
                int data = watchList[name].Elapsed.Milliseconds;
                watchList.Remove(name);
                return data;
            }
            return -1;
        }
       public static void Stop(string name)
       {
           if (watchList.ContainsKey(name))
           {
               watchList[name].Stop();
               string data = name +"," + watchList[name].Elapsed.TotalMilliseconds.ToString() +
               Environment.NewLine;
               watchList.Remove(name);
              
                   writeQueue.Enqueue(data);
                
               
           }
           else
           {
               
              
           }

       }

       private static void Write()
       {
           File.Create(filename).Close();
           File.AppendAllText(filename,trackingState.ToString()+ Environment.NewLine);
           string data = null;
            while (true)
           {

               if (writeQueue.Count>0)
               {
                   
                       while (writeQueue.Count > 0)
                       {
                           
                            data = writeQueue.Dequeue();


                           File.AppendAllText(filename, data);

                           data = null;

                       
                   }
               }
               else
               {
                   Thread.Sleep(waitTime);

               }
           }

       }


   }
}

