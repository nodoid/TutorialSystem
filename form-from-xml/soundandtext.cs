using System;
using System.Collections.Generic;
using System.Text;

namespace form_from_xml
{
    class soundandtext
    {
    }

    class NumberBox : TextBox
    {
        public NumberBox()
        {
            this.CausesValidation = true;
            this.Validating += new CancelEventHandler(TextBox_Validation);
        }

        private void TextBox_Validation(object sender, CancelEventArgs e)
        {
            try
            {
                int value = System.Int32.Parse(this.Text);
            }
            catch (System.Exception)
            {
                e.Cancel = true;
            }
        }
    }


}
