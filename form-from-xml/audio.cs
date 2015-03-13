using System;
using System.Media;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using QuartzTypeLib;

namespace form_from_xml
{
    public class audio
    {

        private SoundPlayer player = new SoundPlayer();

        public void playaudio(string file)
        {
            var e = new errorhandling();
            player.SoundLocation = file;
            try
            {
                player.LoadAsync();
                player.Play();
            }
            catch (FileNotFoundException m)
            {
                e.throwFileNotFound(m.ToString());
                return;
            }
        }

        public void stopaudio()
        {
            player.Stop();
        }
    }

    public class mp3audio
    {
        public int audiolength;
        private ulong head;

        public int lengthofaudio(string filename)
        {
            return readheader(filename) == true ? audiolength : -1;
        }

        public bool readheader(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var fheader = new byte[4];
            int pos = 0;

            do
            {
                fs.Position = pos;
                fs.Read(fheader, 0, 4);
                pos++;
                loadheader(fheader);
            }
            while(!validheader() && (fs.Position != fs.Length));

            if (fs.Position != fs.Length)
            {
                getaudiolength(fs.Length);
                fs.Close();
                return true;
            }
            return false;
        }

        private void loadheader(byte[] c)
        {
            head = (ulong)(((c[0] & 255) << 24) | ((c[1] & 255) << 16) | ((c[2] & 255) << 8) | ((c[3] & 255)));
        }

        private bool validheader()
        {
            return (((fsync & 2047) == 2047) &&
            ((version & 3) != 1) &&
            ((layer & 3) != 0) &&
            ((bitrate & 15) != 0) &&
            ((bitrate & 15) != 15) &&
            ((freq & 3) != 3) &&
            ((emphasis & 3) != 2));
        }

        private int fsync
        {
            get { return (int)((head >> 21) & 2047); }
        }

        private int version
        {
            get { return (int)((head >> 19) & 3); }
        }

        private int layer
        {
            get { return (int)((head >> 17) & 3); }
        }

        private int bitrate
        {
            get { return (int)((head >> 12) & 15); }
        }

        private int freq
        {
            get { return (int)((head >> 10) & 3); }
        }

        private int emphasis
        {
            get { return (int)(head & 3); }
        }

        private void getaudiolength(long size)
        {
            int inkb = (int)((8 * size) / 1000);
            audiolength = inkb / bitrate;
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder returned, int len, IntPtr callback);

        public void playmp3(string filename)
        {
            var e = new errorhandling();
            string command = "";
            try
            {
                command = "open " + filename + " type mpegvideo alias MediaFile";
            }
            catch (System.IO.FileNotFoundException m)
            {
                e.throwFileNotFound(m.ToString());
                return;
            }
            mciSendString(command, null, 0, IntPtr.Zero);   
            command = "play MediaFile from 0";
            mciSendString(command, null, 0, IntPtr.Zero);
        }



        public void playmp3(string filename, int from, int to)
        {
            var e = new errorhandling();
            string command = "";
            try
            {
                command = "open \"" + filename + "\" type mpegvideo alias MediaFile";
                mciSendString(command, null, 0, IntPtr.Zero);
            }
            catch (System.IO.FileNotFoundException m)
            {
                e.throwFileNotFound(m.ToString());
                return;
            }

            command = "set MediaFile time format milliseconds";
            mciSendString(command, null, 0, IntPtr.Zero);
            
            command = "set MediaFile seek exactly on";
            mciSendString(command, null, 0, IntPtr.Zero);

            var returned = new StringBuilder(128);
            command = "status MediaFile length";
            mciSendString(command, returned, 128, IntPtr.Zero);

            var al = Convert.ToUInt64(returned.ToString());

            from *= 1000;
            to *= 1000;
            ulong t = Convert.ToUInt64(to);

            if (al > 0 && t < al)
            {
                command = "play MediaFile from " + from.ToString() + " to " + to.ToString();
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        public void stopmp3()
        {
            mciSendString("stop MediaFile", null, 0, IntPtr.Zero);
        }
    }

    public class video
    {
        private const int WM_APP = 0x800;
        private const int WM_GRAPHNOTIFY = WM_APP + 1;
        private const int EC_COMPLETE = 0x01;
        private const int WS_CHILD = 0x40000000;
        private const int WS_CLIPCHILDREN = 0x2000000;

        private IMediaControl mc = null;
        private IVideoWindow videoWindow = null;

        public void videoplayer(string filename, PictureBox pb)
        {
            var zerozero = new FilgraphManager();
            zerozero.RenderFile(filename);

            try
            {
                videoWindow = (IVideoWindow)zerozero;
                videoWindow.Owner = (int)pb.Handle;
                videoWindow.WindowStyle = WS_CHILD | WS_CLIPCHILDREN;
                videoWindow.SetWindowPosition(
                    pb.ClientRectangle.Left,
                    pb.ClientRectangle.Top,
                    pb.ClientRectangle.Width,
                    pb.ClientRectangle.Height);
            }
            catch
            {
                MessageBox.Show("Later");
            }
            mc = (IMediaControl)zerozero;
            mc.Run();
        }
    }
}
    


