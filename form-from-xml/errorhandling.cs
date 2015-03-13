using System.Windows.Forms;

namespace form_from_xml
{
    class errorhandling
    {
        public void throwFileNotFound(string text)
        {
            MessageBox.Show("Unable to find " + text, "File not found", MessageBoxButtons.OK);
        }

        public void throwSystemException(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButtons.OK);
        }
    }
}
