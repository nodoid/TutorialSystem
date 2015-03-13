using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace form_from_xml
{
    public partial class mathsparser : Form
    {
        private bool hasbraces, hasdivide, haspower, hasroot, hasdecimal;

        public mathsparser()
        {
            hasbraces = hasdecimal = haspower = hasroot = hasdivide = true;
            InitializeComponent();
        }

        List<data> bigone = new List<data>();

        public class data
        {
            public string op;
            public ArrayList values = new ArrayList();

            public data(string o, ArrayList a)
            {
                this.op = o;
                this.values = a;
            }
        }


        public void analyseformula(string formula, bool s)
        {
            if (s == true)
                this.single.Checked = true;
            else
                this.multiple.Checked = true;

            this.formula.Text = formula;

            if (formula.Contains("+"))
                this.add.Checked = true;

            if (formula.Contains("-"))
                this.subtract.Checked = true;

            if (formula.Contains("*"))
                this.multiply.Checked = true;

            if (formula.Contains("/"))
            {
                this.divide.Checked = true;
                hasdivide = false;
            }

            if ((formula.Contains("(") && formula.Contains(")")) ||
                (formula.Contains("[") && formula.Contains("]")))
            {
                this.Brace.Checked = true;
                hasbraces = false;
            }

            if (formula.Contains("log") || formula.Contains("Log") ||
                formula.Contains("log10") || formula.Contains("Log10"))
                this.log.Checked = true;

            if (formula.Contains("ln"))
                this.ln.Checked = true;

            if (formula.Contains("E") || formula.Contains("Sum") || formula.Contains("sum"))
                this.sum.Checked = true;

            if (formula.Contains("SumFrom") || formula.Contains("sumfrom"))
                this.sumfrom.Checked = true;

            //if (Regex.Match("sumto", formula).Success)
            if (formula.Contains("SumTo") || formula.Contains("sumto"))
                this.sumto.Checked = true;

            //if (Regex.Match("sumbetween", formula).Success)
            if (formula.Contains("SumBetween") || formula.Contains("sumbetween"))
                this.sumbetween.Checked = true;

            if (formula.Contains(" -"))
                this.negative.Checked = true;

            if (formula.Contains("."))
            {
                this.dec.Checked = true;
                hasdecimal = false;
            }

            if (formula.Contains("sqrt"))
            {
                this.root.Checked = true;
                hasroot = false;
            }

            if (formula.Contains("^"))
                this.power.Checked = true;

            if (formula.Contains("pi"))
                this.pi.Checked = true;

            if (checkforerrors(formula) == true)
                return;
            else
                parseformula(formula);
        }

        private bool checkforerrors(string formula)
        {
            bool haserrors = false;

            int sl = formula.Length;
            string fc = formula;
            int sc = 0, so = 0;

            sc = fc.Length - fc.Replace("[", "").Length;
            fc = formula;
            so = fc.Length - fc.Replace("]", "").Length;

            try
            {
                int o = Regex.Matches("(", formula).Count;
                int c = Regex.Matches(")", formula).Count;    
            }
            catch (ArgumentException e)
            {
                if (e.Message.Contains("many )'s"))
                    errors.Text += "Mismatch on () braces - too many )s";
                if (e.Message.Contains("Not enough )'s"))
                    errors.Text += "Mismatch on () braces - too many (s";
                haserrors = true;
            }

            if (so != sc)
            {
                if (so > sc)
                    errors.Text += "Mismatch on [] braces - too many [s";
                else
                    errors.Text += "Mismatch on [] braces - too many ]s";
                haserrors = true;
            }

            return haserrors;
        }

        private void parseformula(string formula)
        {
            double finald = 0.0;
            
            bigone = findthelot(formula);
            //bodmas
            if (bigone[5].values.Count != 0)
            {
                for (int l = 0; l < bigone[5].values.Count; ++l)
                    finald += brackets(formula);
            }
            errors.Text += finald.ToString() + "\r\n";
        }

        private void mathsfunctions(ref double value, int stat, int option)
        { 
            switch (option)
            {
                case 0: 
                    value = Math.Sin(value);
                    break;
                case 1:
                    value = Math.Cos(value);
                    break;
                case 2:
                    value = Math.Tan(value);
                    break;
                case 3:
                    value = Math.Asin(value);
                    break;
                case 4:
                    value = Math.Acos(value);
                    break;
                case 5:
                    value = Math.Atan(value);
                    break;
                case 6:
                    value = Math.Exp(value);
                    break;
                case 7:
                    value = Math.Log10(value);
                    break;
                case 8:
                    value = Math.Pow(value, stat);
                    break;
                case 9:
                    value = Math.Log(value, stat);
                    break;
                case 10:
                    value = Math.Sqrt(value);
                    break;
                case 11:
                    value = Math.Sinh(value);
                    break;
                case 12:
                    value = Math.Cosh(value);
                    break;
                case 13:
                    value = Math.Tanh(value);
                    break;
                default:
                    break;
            }
        }

        private double simplestuff(double value, double doer, int opt)
        {
            switch (opt)
            {
                case 0:
                    value = value + doer;
                    break;
                case 1:
                    value = value - doer;
                    break;
                case 2:
                    value = value * doer;
                    break;
                case 3:
                    if (doer != 0)
                        value = value / doer;
                    break;
                default:
                    break;
            }
            return value;
        }

        private List<data> findthelot(string equation)
        {
            string[] searchfor =
                {"+", "-", "*", "/", "+", "(", "[", ")", "]", "^", "cos", "sin", "tan",
                    "asin", "acos", "atan", "sinh", "cosh", "tanh", "^", "sqrt", "log10", "ln", "sum", "sfrom", "sto", "pi"
                };
            foreach (var s in searchfor)
            {
                bigone.Add(new data(s, findoperators(s, equation)));
            }
            return bigone;
        }

        private double brackets(string formula)
        {
            double answer = 0.0;

            var formulae = new List<string>();
            var numbers = new ArrayList();
            var lengths = new ArrayList();

            for (var n = 0; n < bigone[5].values.Count; ++n)
            {
                // brackets
                int s = (int)bigone[5].values[n] + 1;
                int e = (int)bigone[7].values[n];
                formulae.Add(formula.Substring(s, e - s));
                // numbers
                
                var ops = new char[]
                {
                    Convert.ToChar("+"), Convert.ToChar("-"), Convert.ToChar("*"), Convert.ToChar("/"),
                    Convert.ToChar("^")
                };
                foreach (string t in formulae[n].Split(ops))
                {
                    numbers.Add(Convert.ToDouble(t));  
                    lengths.Add(t.Length);
                }
            }

            /*answer += analysis(9, -1, formulae, numbers, lengths);  power - not implemented yet */
            answer += analysis(3, 3, formulae, numbers, lengths); // divide
            answer += analysis(2, 2, formulae, numbers, lengths); // multiply
            answer += analysis(0, 0, formulae, numbers, lengths); // add
            answer += analysis(1, 1, formulae, numbers, lengths); // subtract

            return answer;
        }

        private double analysis(int param, int opt, List<string> formulae, ArrayList numbers, ArrayList lengths)
        {
            double val = 0.0;
            string[] oper = { "+", "-", "*", "/" };
            if (bigone[param].values.Count != 0) // yes
            {
                for (int m = 0; m < bigone[param].values.Count; ++m)
                {
                    try
                    {
                        string workon = formulae[m];
                        int len = workon.Length, st = 0;
                        for (int k = 0; k < ((ArrayList)lengths).Count; ++k)
                        {
                            if ((workon.Substring(st + (int)lengths[k], 1)) == oper[param])
                            {
                                val += simplestuff((double)numbers[k], (double)numbers[k + 1], opt);
                                st += (int)lengths[k] + 1 + (int)lengths[k + 1];
                                if (st + (int)lengths[k + 1] + 1 >= len)
                                    break;
                            }
                            else
                            {
                                st += (int)lengths[k];
                                if (st + (int)lengths[k + 1] + 1 >= len)
                                    break;
                            }
                        }
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        MessageBox.Show("Error", e.Message, MessageBoxButtons.OK);
                    }
                }
            }
            return val;
        }

        private ArrayList findoperators(string search, string line)
        {
            var find = new ArrayList();
            int c = 0, c1 = 0;
            for (int p = 0; p < line.Length; ++p)
            {
                c = line.IndexOf(search, p);
                if (c != c1 && c != -1)
                {
                    find.Add(c);
                    c1 = c;
                }
            }
            return find;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
