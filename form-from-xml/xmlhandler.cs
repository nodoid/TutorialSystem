using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace form_from_xml
{
	
    public class xmlhandler : Form
    {
	
        public void loaddesign(int m, string place)
        {
            FormList f;
            f = null;
            errorhandling e = new errorhandling();
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(FormList));
                TextReader r = new StreamReader(place + "designer-test.xml");
                f = (FormList)s.Deserialize(r);
                r.Close();
            }
            catch (System.IO.FileNotFoundException)
            {
                e.throwFileNotFound(place + "designer-test.xml");
            }
            catch (System.InvalidOperationException s)
            {
                e.throwSystemException(s.ToString());
            }
            var wf = new winformgen();
            wf.makeform(f, 1, place);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // xmlhandler
            // 
            this.ClientSize = new System.Drawing.Size(792, 316);
            this.Name = "xmlhandler";
            this.ResumeLayout(false);

        }
    }

    public class winformgen : Form
    {
        private bool playingmp3 = false;
        private bool playingwav = false;

        public void makeform(FormList f, int page, string place)
        {
            int realpage = page - 1;
            var form1 = new Form();
            var resources = new System.Resources.ResourceManager(typeof(Form1));
            var asm = typeof(Form).Assembly;
            var err = new errorhandling();
            //for (int k = 0; k < f.Forms[realpage].ElementNos; ++k)
            //{
            foreach (var fort in f.Forms[realpage].elements)
            {
                var tp = asm.GetType(fort.ElementType);
                var m = Activator.CreateInstance(tp);
                if (m is Control)
                {
                    var widget = (Control)m;
                    var fb = new Button();
                    var bb = new Button();
                    if (f.Forms[realpage].BackLink != 999 || f.Forms[realpage].ForwardLink != 999)
                    {
                        bool fbu = false, bbu = false;
                        if (f.Forms[realpage].ForwardLink != 999)
                        {
                            fb.Name = "forward";
                            fb.Enabled = true;
                            fb.Visible = fort.buttonafter;
                            fb.Location = new Point(f.Forms[realpage].WinWidth - 100, f.Forms[realpage].WinHeight - 75);
                            fb.Text = "Next >>";
                            fbu = true;
                        }
                        if (f.Forms[realpage].BackLink != 999)
                        {
                            bb.Name = "backward";
                            bb.Enabled = true;
                            bb.Visible = fort.buttonafter;
                            bb.Location = new Point(f.Forms[realpage].WinWidth - 200, f.Forms[realpage].WinHeight - 75);
                            bb.Text = "<< Back";
                            bbu = true;
                        }
                        fb.Height = bb.Height = 23;
                        fb.Width = bb.Width = 75;
                        if (fbu == true)
                        {
                            form1.Controls.Add(fb);
                            fb.Click += delegate(object s, EventArgs e)
                            {
                                ButtonClick(s, e, f, f.Forms[realpage].ForwardLink, form1, place);
                            };
                        }
                        if (bbu == true)
                        {
                            form1.Controls.Add(bb);
                            bb.Click += delegate(object s, EventArgs e)
                            {
                                ButtonClick(s, e, f, f.Forms[realpage].BackLink, form1, place);
                            };
                        }
                    }

                    if (fort.ElementType != "System.Windows.Forms.PictureBox" &&
                        fort.ElementType != "System.Windows.Forms.RichTextBox")
                    {
                        widget.Text = fort.Text;
                        widget.Location = new Point(fort.X, fort.Y);
                        widget.Height = fort.Height;
                        widget.Width = fort.Width;
                        widget.Name = fort.ElementName;
                        widget.TabIndex = fort.TabIndex;
                        form1.Controls.Add(widget);
                    }

                    if (fort.HasClick == true)
                    {
                        widget.Click += new System.EventHandler(this.widget_Click);
                    }

                    if (fort.HasQuestions == true)
                    {
                        if (fort.QAfter == true)
                        {
                            var z = new Timer
                            { 
                                Interval = fort.QAfterLen * 1000,
                                Enabled = true,
                            };
                            z.Tick += (object s, EventArgs e) => askquestions(s, e, f, realpage, form1, fb, bb, z);
                        }
                        else
                            askquestions(f, realpage, form1, fb, bb);
                    }

                    if (fort.HasWav == true || fort.HasMP3 == true)
                    {
                        if (fort.PlayLen != 0)
                        {
                            var playtimer = new Timer
                            {
                                Interval = fort.PlayLen * 1000,
                                Enabled = true
                            };
                            playtimer.Tick += (object s, EventArgs e) => playtimer_Tick(s, e, playtimer);                 
                        }

                        if (fort.HasWav == true)
                        {
                            var c = new audio();
                            playingwav = true;
                            c.playaudio(place + fort.AudioName);
                        }
                        else if (fort.AudioStart == 0)
                        {
                            var mp3 = new mp3audio();
                            playingmp3 = true;
                            mp3.playmp3(place + fort.AudioName);
                        }
                        else
                        {
                            var mp3 = new mp3audio();
                            playingmp3 = true;
                            mp3.playmp3(place + fort.AudioName, fort.AudioStart, fort.AudioEnd);
                        }
                    }

                    if (fort.ElementType == "System.Windows.Forms.PictureBox" && fort.WithExternal == true &&
                        fort.ExternalFile != "")
                    {
                        var b = new PictureBox
                        {
                            Location = new Point(fort.X, fort.Y),
                            Name = fort.ElementName,
                            Height = fort.Height,
                            Width = fort.Width,
                            SizeMode = PictureBoxSizeMode.StretchImage
                        };
                        if (fort.HasVideo == false)
                        {
                            try
                            {
                                b.Image = Image.FromFile(place + fort.ExternalFile);
                            }
                            catch (System.NotSupportedException)
                            {
                                err.throwFileNotFound(place + fort.ExternalFile);
                            }
                        }
                        else
                        {
                            var v = new video();
                            v.videoplayer(place + fort.ExternalFile, b);
                        }
                        b.TabIndex = fort.TabIndex;
                        form1.Controls.Add(b);
                    }
                    if (fort.ElementType == "System.Windows.Forms.RichTextBox" && fort.WithExternal == true &&
                        fort.ExternalFile != "")
                    {
                        var b = new RichTextBox
                        {
                            Location = new Point(fort.X, fort.Y),
                            Name = fort.ElementName,
                            Height = fort.Height,
                            Width = fort.Width,
                            ReadOnly = true
                        };
                        try
                        {
                            b.LoadFile(place + fort.ExternalFile);
                        }
                        catch (System.NotSupportedException)
                        {
                            err.throwFileNotFound(place + fort.ExternalFile);
                        }
                        b.TabIndex = fort.TabIndex;
                        form1.Controls.Add(b);
                    }
                }
                if (fort.hasmaths == true && fort.formula != "")
                {
                    var mp = new mathsparser();
                    mp.Show();
                    mp.analyseformula(fort.formula, fort.singlevalue);
                }
            }
            //realpage++;
            //}  
            form1.FormBorderStyle = FormBorderStyle.FixedDialog;
            form1.MaximizeBox = false;
            form1.MinimizeBox = false;
            form1.StartPosition = FormStartPosition.CenterScreen;
            form1.FormClosed += new FormClosedEventHandler(formclosed);
            form1.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            form1.Width = f.Forms[realpage].WinWidth;
            form1.Height = f.Forms[realpage].WinHeight;
            form1.Text = f.Forms[realpage].WinTitle;
            form1.Name = f.Forms[realpage].WinName;
            form1.Show();
        }

        private void askquestions(FormList f, int realpage, Form t, Button fb, Button bb)
        {
            int qt = -1;
            foreach (Qs quest in f.Forms[realpage].question)
            {
                if (quest.QA == true)
                    qt = quest.QI;
                var r = new RadioButton
                { 
                    Location = new Point(quest.QX, quest.QY),
                    Text = quest.QText,
                    TabIndex = quest.QI,
                    Width = quest.QLen,
                    Checked = false
                };
                r.Click += (object s, EventArgs e) => RadioSelect(s, e, qt, fb, bb);
                t.Controls.Add(r);
            }
        }

        private void askquestions(object o, EventArgs e, FormList f, int realpage, Form t, Button fb, Button bb, Timer p)
        {
            p.Interval = 1;
            askquestions(f, realpage, t, fb, bb);
        }

        private void widget_Click(object o, EventArgs e)
        {
            MessageBox.Show("Hello", "Hello", MessageBoxButtons.OK);
        }

        private void playtimer_Tick(object o, EventArgs e, Timer m)
        {
            formclosed(o, e);
        }

        private void formclosed(object o, EventArgs e)
        {
            if (playingwav == true || playingmp3 == true)
            {
                if (playingmp3 == true)
                {
                    var mp3 = new mp3audio();
                    playingmp3 = false;
                    mp3.stopmp3();
                }
                else
                {
                    var a = new audio();
                    playingwav = false;
                    a.stopaudio();
                }
            }
        }

        private void ButtonClick(object o, EventArgs e, FormList f, int page, Form f1, string place)
        {
            var m = (Button)o;

            formclosed(o, e);
		   
            if (m.Name == "forward")
            {
                f1.Dispose();
                makeform(f, /*f.Forms[page].ForwardLink*/page, place);
            }
            else
            {
                f1.Dispose();
                makeform(f, /*f.Forms[page].BackLink*/page, place);
            }
        }

        private void RadioSelect(object o, EventArgs e, int qa, Button fb, Button bb)
        {
            var c = (RadioButton)o;
            if (c.TabIndex == qa)
            {
                MessageBox.Show("Congratulations, you got it right", "You have answered the question", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Nahhhhhhhhhh, no banana this time", "You have answered the question", MessageBoxButtons.OK);
            }
            fb.Visible = true;
            bb.Visible = true;
        }
    }

    [Serializable]
    [XmlRoot("Forms")]
    public class FormList
    {
        private List<FormData> forms;

        /// <summary>
        /// Initializes a new instance of the <see cref="Forms"/> class.
        /// </summary>
        public FormList()
        {
            forms = new List<FormData>();
        }

        [XmlElement("Form")]
        public FormData[] Forms
        {
            get { return this.forms.ToArray(); }
            set { this.forms = new List<FormData>(value); }
        }
    }

    [Serializable]
    public class FormData
    {
        public List<Element> elements;
        public List<Qs> question;

        public FormData()
        {
            elements = new List<Element>();
            question = new List<Qs>();
        }

        public string WinName { get; set; }

        public int WinWidth { get; set; }

        public int WinHeight { get; set; }

        public string WinTitle { get; set; }

        public int BackLink { get; set; }

        public int ForwardLink { get; set; }

        public int PageNumber { get; set; }

        public int ElementNos { get; set; }

        [XmlElement("Element")]
        public Element[] Elements
        {
            get { return this.elements.ToArray(); }
            set { this.elements = new List<Element>(value); }
        }

        [XmlElement("Question")]
        public Qs[] questions
        {
            get { return this.question.ToArray(); }
            set { this.question = new List<Qs>(value); }
        }
    }

    [Serializable]
    public class Qs
    {
        [XmlAttribute("QName")]
        public string QName { get; set; }

        public int QX { get; set; }

        public int QY { get; set; }

        public int QLen { get; set; }

        public string QText { get; set; }

        public bool QA { get; set; }

        public int QI { get; set; }
    }


    [Serializable]
    public class Element
    {
        [XmlAttribute("Name")]
        public string ElementName { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Text { get; set; }

        public int TabIndex { get; set; }

        [XmlAttribute("Type")]
        public string ElementType { get; set; }

        [XmlAttribute("External")]
        public bool WithExternal { get; set; }

        public string ExternalFile { get; set; }

        [XmlAttribute("Events")]
        public bool HasClick { get; set; }

        public bool HasWritable { get; set; }

        public bool HasNumeric { get; set; }

        public bool HasRadio { get; set; }

        public bool HasCheck { get; set; }

        public bool HasRO { get; set; }

        public bool HasQuestions { get; set; }

        public bool HasWav { get; set; }

        public bool HasMP3 { get; set; }

        public bool HasVideo { get; set; }

        public string AudioName { get; set; }

        public int AudioStart { get; set; }

        public int AudioEnd { get; set; }

        public int PlayLen { get; set; }

        public bool QAfter { get; set; }

        public int QAfterLen { get; set; }

        public bool buttonafter { get; set; }

        public bool hasmaths { get; set; }

        [XmlAttribute("Maths")]
        public bool waste { get; set; }

        public string formula { get; set; }

        public bool singlevalue { get; set; }

        public bool rowvalue { get; set; }
    }
}
