using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetMASS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += AllUnhandledExceptions;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.Run(new Form1());
        }

        private static void AllUnhandledExceptions(object sender, UnhandledExceptionEventArgs e)
            {
                var ex = (Exception)e.ExceptionObject;
                File.WriteAllText("FATAL.txt","FATAL ERROR: "+ex.Message);
                Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException(ex));
            }
        }
}
