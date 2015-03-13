using System;
using System.Windows.Forms;
using System.IO;

namespace form_from_xml
{
    public partial class Form1 : Form
    {
        public string place;

        public Form1()
        {
            string path = Environment.CurrentDirectory;
            string sep = Path.DirectorySeparatorChar.ToString();
            place = path + sep + "demo" + sep;
            InitializeComponent();
            try
            {
                textbox.LoadFile(place + "introtext.rtf");
            }
            catch (System.IO.FileNotFoundException)
            {
                errorhandling e = new errorhandling();
                e.throwFileNotFound(place + "introtext.rtf");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var xml = new xmlhandler();
            xml.loaddesign(1, place);
        }
    }
}
