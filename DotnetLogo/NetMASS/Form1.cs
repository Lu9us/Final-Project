using NParser.Runtime;
using NParser.Runtime.DataStructs;
using NParser.Types;
using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetMASS
{
    public partial class Form1 : Form
    {
        private ExecutionEngine es = new ExecutionEngine();
        bool scriptVerified = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtScript.ScrollBars = ScrollBars.Vertical;
            txtConsole.ScrollBars = ScrollBars.Vertical;
            pbSim.Paint += AgentDraw;
            Console.SetOut(new ConsoleToControl(txtConsole));

        }

        private Bitmap changeColour(Bitmap src, Color c)
        {
            Color actualC;
            Bitmap b = new Bitmap(src.Width, src.Height);
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    actualC = src.GetPixel(i, j);
                    if (actualC.A > 150)
                    {
                        b.SetPixel(i, j, c);
                    }
                    else
                    {
                        b.SetPixel(i, j, actualC);
                    }
                }

            }


            return b;
        }


        private void AgentDraw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Image img = Image.FromFile("Images/Tile.png");

            int i = 0;
            int j = 0;
           

            foreach (Patch p in es.sys.patches)
            {
                Bitmap b = changeColour(new Bitmap(img), Color.FromName(((NSString)p.properties.GetProperty("p-color")).val.Replace('\"',' ').Trim()));
                g.DrawImage(b, new PointF(p.x*img.Width, p.y*img.Height));
            }
            img = Image.FromFile("Images/Agent.png");
            foreach (Agent a in es.sys.agents.Values)
            {
                Bitmap b = changeColour(new Bitmap(img), Color.FromName(((NSString)a.properties.GetProperty("color")).val.Replace('\"', ' ').Trim()));
                g.DrawImage(b, new PointF(a.x*img.Width , a.y * img.Height));

            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Multiselect = false;
            if (d.ShowDialog() == DialogResult.OK)
            {
                var v = File.ReadAllLines(d.FileName);
                foreach(string s in v)
                {
                    txtScript.AppendText(s);
                       txtScript.AppendText(Environment.NewLine);
                    
                 }
            }
            scriptVerified = false;

        }

        private void btbVerify_Click(object sender, EventArgs e)
        {


            if (!string.IsNullOrEmpty(txtScript.Text)||!string.IsNullOrWhiteSpace(txtScript.Text))
            {
                txtConsole.Clear();
                es = new ExecutionEngine();
                es.Load(txtScript.Text.Split(Environment.NewLine.ToCharArray()[0]));
                scriptVerified = true;
                pbSim.Refresh();
            }

        }

        private void btbExec_Click(object sender, EventArgs e)
        {
            ParseTree t = new ParseTree(txtInput.Text);
            es.ExecuteTree(t);
            pbSim.Refresh();
        }
    }
}
