using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetMASS
{
  public class ConsoleToControl:TextWriter
    {
        private Control textbox;
        public ConsoleToControl(Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke(new Action(() => { textbox.Text += value; textbox.Refresh(); }));
            }
            else
            {
                textbox.Text += value; textbox.Refresh();
            }
        }

        public override void Write(string value)
        {
            if (textbox.InvokeRequired)
            {
                textbox.Invoke(new Action(() => { textbox.Text += value; ; textbox.Refresh(); }));
            }
            else
            {
                textbox.Text += value; textbox.Refresh();
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
