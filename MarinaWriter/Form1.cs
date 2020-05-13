using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;
using System.Drawing.Printing;

namespace MarinaWriter
{
    public partial class Form1 : Form, IMessageFilter
    {

        String filePath = null;
        Uri soundEffect;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_LBUTTONDOWN = 0x0201;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private HashSet<Control> controlsToMove = new HashSet<Control>();

        public Form1()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);

            controlsToMove.Add(this);
            controlsToMove.Add(translucentPanel3);
            //controlsToMove.Add(this.menuStrip1);//Add whatever controls here you want to move the form when it is clicked and dragged

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            _printDocument.BeginPrint += _printDocument_BeginPrint;
            _printDocument.PrintPage += _printDocument_PrintPage;
        }
        MediaPlayer backgroundSound = new MediaPlayer();
        String[] musicFiles;
        ToolStripItem[] musicTooltips;

        String[] soundEffectsFiles;
        ToolStripItem[] soundEffectsToolTips;

        String[] backgroundFiles;
        ToolStripItem[] backgroundToolTips;
        private void Form1_Load(object sender, EventArgs e)
        {
            customRichBox1.Font = new Font(customRichBox1.Font.FontFamily, 25, customRichBox1.Font.Style);
            if (Directory.Exists("Music"))
            {
                musicFiles = Directory.GetFiles(@"Music");
                musicTooltips = new ToolStripItem[musicFiles.Length];
                for (int i = 0; i < musicFiles.Length; i++)
                {
                    musicTooltips[i] = new ToolStripMenuItem();
                    musicTooltips[i].Text = musicFiles[i];
                    musicTooltips[i].Tag = i;
                    musicTooltips[i].Click += new System.EventHandler(this.MusicMenuStrip);
                    musicToolStripMenuItem.DropDownItems.Add(musicTooltips[i]);
                }
            }

            if (Directory.Exists("Sound Effects"))
            {
                soundEffectsFiles = Directory.GetFiles(@"Sound Effects");
                soundEffectsToolTips = new ToolStripItem[soundEffectsFiles.Length];
                for (int i = 0; i < soundEffectsFiles.Length; i++)
                {
                    soundEffectsToolTips[i] = new ToolStripMenuItem();
                    soundEffectsToolTips[i].Text = soundEffectsFiles[i];
                    soundEffectsToolTips[i].Tag = i;
                    soundEffectsToolTips[i].Click += new System.EventHandler(this.SoundMenuStrip);
                    keyboardSoundToolStripMenuItem.DropDownItems.Add(soundEffectsToolTips[i]);
                }
            }

            if (Directory.Exists("Backgrounds"))
            {
                backgroundFiles = Directory.GetFiles(@"Backgrounds");
                backgroundToolTips = new ToolStripItem[backgroundFiles.Length];
                for (int i = 0; i < backgroundFiles.Length; i++)
                {
                    backgroundToolTips[i] = new ToolStripMenuItem();
                    backgroundToolTips[i].Text = backgroundFiles[i];
                    backgroundToolTips[i].Tag = i;
                    backgroundToolTips[i].Click += new System.EventHandler(this.BackGroundStrip);
                    backgroundToolStripMenuItem.DropDownItems.Add(backgroundToolTips[i]);
                }
            }

            ToolStripItem[] FFToolTips = new ToolStripItem[System.Drawing.FontFamily.Families.Length];
            String[] Fonts = { "Calibri", "Blackadder ITC", "Bradley Hand ITC", "Brush Script MT", "Castellar", "Edwardian Script ITC", "Curlz MT", "Freestyle Script", "French Script MT", "Harrington", "Segoe Script" };
            for (int i = 0; i < Fonts.Length; i++)
            {
                ToolStripMenuItem FFToolTip = new ToolStripMenuItem();
                FFToolTip.Text = Fonts[i];
                FFToolTip.Font = new Font(new System.Drawing.FontFamily(Fonts[i]), FFToolTip.Font.Size, FFToolTip.Font.Style);
                FFToolTip.Click += new System.EventHandler(ChangeTheFont);

                fontToolStripMenuItem2.DropDownItems.Add(FFToolTip);

                FFToolTips[i] = new ToolStripMenuItem();
                FFToolTips[i].Text = Fonts[i];
                FFToolTips[i].Font = new Font(new System.Drawing.FontFamily(Fonts[i]), FFToolTips[i].Font.Size, FFToolTips[i].Font.Style);

                FFToolTips[i].Tag = i;
                FFToolTips[i].Click += new System.EventHandler(ChangeTheFont);
                fontToolStripMenuItem1.DropDownItems.Add(FFToolTips[i]);
            }


            findToolStripMenuItem_Click(sender, e);
            Form1_Resize(sender, e);

            switch (Properties.Settings.Default.LastBackground)
            {
                case "null":
                    this.BackgroundImage = Properties.Resources.Monasterio_Khor_Virap_Armenia;
                    break;
                default:
                    if (File.Exists(Properties.Settings.Default.LastBackground))
                    {
                        this.BackgroundImage = Image.FromFile(Properties.Settings.Default.LastBackground);

                        break;
                    }
                    goto case "null";
            }

            setDarkMode(Properties.Settings.Default.DarkMode);

            backgroundSound.MediaEnded += new EventHandler(this.MediaOnEnd);
            switch (Properties.Settings.Default.LastMusic)
            {
                case "null":
                    if (musicFiles.Length > 0)
                    {
                        backgroundSong = musicFiles[0];
                        MediaOnEnd(sender, e);
                        Properties.Settings.Default.LastMusic = musicFiles[0];
                        Properties.Settings.Default.Save();
                    }
                    break;
                case "paused":
                    if (musicFiles.Length > 0)
                    {
                        backgroundSong = musicFiles[0];
                        backgroundSound.Open(new Uri(backgroundSong, UriKind.Relative));
                        stopToolStripMenuItem.Text = "Resume";
                    }
                    break;
                default:
                    if (File.Exists(Properties.Settings.Default.LastMusic))
                    {
                        backgroundSong = Properties.Settings.Default.LastMusic;
                        MediaOnEnd(sender, e);
                        break;
                    }
                    goto case "null";
            }



            switch (Properties.Settings.Default.LastKeysound)
            {
                case "null":
                    if (soundEffectsFiles.Length > 0)
                    {
                        keyboardSound = soundEffectsFiles[0];
                        soundEffect = new Uri(soundEffectsFiles[0], UriKind.Relative);
                        Properties.Settings.Default.LastKeysound = soundEffectsFiles[0];
                        Properties.Settings.Default.Save();
                    }
                    break;
                case "muted":
                    soundEffect = null;
                    muteToolStripMenuItem.Text = "Unmute";
                    break;
                default:
                    if (File.Exists(Properties.Settings.Default.LastKeysound))
                    {
                        keyboardSound = Properties.Settings.Default.LastKeysound;
                        soundEffect = new Uri(Properties.Settings.Default.LastKeysound, UriKind.Relative);

                        break;
                    }
                    goto case "null";
            }

            textChanged = false;

        }

        private void ChangeTheFont(Object sender, EventArgs e)
        {
            setRichChangeFont(new System.Drawing.FontFamily(sender.ToString()));
        }

        String backgroundSong;

        private void MediaOnEnd(object sender, EventArgs e)
        {

            backgroundSound.Open(new Uri(backgroundSong, UriKind.Relative));
            backgroundSound.Play();
        }

        private void BackGroundStrip(object sender, EventArgs e)
        {
            changeBackground(sender.ToString());
        }

        private void changeBackground(String path)
        {
            this.BackgroundImage = Image.FromFile(path);
            Properties.Settings.Default.LastBackground = path;
            Properties.Settings.Default.Save();
        }

        private void MusicMenuStrip(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastMusic = sender.ToString();
            Properties.Settings.Default.Save();
            fadeout();
            backgroundSong = sender.ToString();
            backgroundSound.Open(new Uri(sender.ToString(), UriKind.Relative));
            fadeIn();
            stopToolStripMenuItem.Text = "Pause";
        }

        private void SoundMenuStrip(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastKeysound = sender.ToString();
            Properties.Settings.Default.Save();
            keyboardSound = sender.ToString();
            soundEffect = new Uri(sender.ToString(), UriKind.Relative);
            muteToolStripMenuItem.Text = "Mute";
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.Undo();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textChangedMessageBox(sender, e)) return;
            reset();
            textChanged = false;
        }

        private bool textChangedMessageBox(object sender, EventArgs e)
        {
            if (textChanged)
            {
                DialogResult result = MessageBox.Show("You have unsaved progress. Would you like to save and quit?", "Exit with unsaved progress", MessageBoxButtons.YesNoCancel);
                switch (result)
                {
                    case DialogResult.Yes:
                        saveToolStripMenuItem_Click(sender, e);
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        return true;
                }

            }
            return false;
        }

        private void reset()
        {
            filePath = null;
            customRichBox1.Text = "";
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFile.ShowDialog();
            if (result == DialogResult.Cancel) return;
            filePath = saveFile.FileName;
            saveToolStripMenuItem_Click(sender, e);
            textChanged = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                if (!File.Exists(filePath))
                    using (File.Create(filePath)) ;
                customRichBox1.SaveFile(filePath, RichTextBoxStreamType.RichText);
            }
            textChanged = false;
        }

        System.Drawing.FontFamily font = System.Drawing.FontFamily.GenericSansSerif;

        private void setFontFamily(System.Drawing.FontFamily ff)
        {
            if (customRichBox1.SelectionLength > 0)
            {
                customRichBox1.SelectionFont = new Font(ff, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style);
            }
            else
            {
                customRichBox1.Font = new Font(ff, customRichBox1.Font.Size, customRichBox1.SelectionFont.Style);
            }
        }

        private void setStyle(FontStyle style)
        {
            if (customRichBox1.SelectionLength > 0)
            {
                customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, customRichBox1.SelectionFont.Size, style);
            }
            else
            {
                customRichBox1.Font = new Font(customRichBox1.Font.FontFamily, customRichBox1.Font.Size, style);
            }
        }

        #region FontResize
        private void setSelectionSize(int size)
        {
            changeEntireFont(() =>
            {
                customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, size, customRichBox1.SelectionFont.Style);
            });
        }
        private void setSize(ref Font font, int size)
        {
            FontStyle style = new FontStyle();
            Font newFont = new Font(font.FontFamily, size, font.Style);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            setSelectionSize(8);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            setSelectionSize(10);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            setSelectionSize(12);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            setSelectionSize(14);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            setSelectionSize(16);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            setSelectionSize(20);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            setSelectionSize(50);
        }
        #endregion

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        #region Alignment

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectionAlignment = TextAlign.Left;
        }

        private void middleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectionAlignment = TextAlign.Center;
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectionAlignment = TextAlign.Right;
        }
        private void jUSTIFIEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectionAlignment = TextAlign.Justify;
        }
        #endregion

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN &&
                 controlsToMove.Contains(Control.FromHandle(m.HWnd)))
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                return true;
            }
            return false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textChangedMessageBox(sender, e)) return;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.Cancel) return;
            filePath = openFileDialog1.FileName;
            customRichBox1.LoadFile(filePath);
            customRichBox1.Focus();
            textChanged = false;
        }

        bool textChanged = false;
        int oldSize = 0;
        MediaPlayer simpleSound = new MediaPlayer();
        private void customRichBox1_TextChanged(object sender, EventArgs e)
        {
            textChanged = true;
            //if(Math.Abs(customRichBox1.Text.Length - oldSize) == 1)
            //{
            simpleSound.Open(soundEffect);
            simpleSound.Play();
            oldSize = customRichBox1.Text.Length;
            //}
        }

        private PrintDocument _printDocument = new PrintDocument();
        private int _checkPrint;


        private void _printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Print the content of RichTextBox. Store the last character printed.
            _checkPrint = customRichBox1.Print(_checkPrint, customRichBox1.TextLength, e);

            // Check for more pages
            e.HasMorePages = _checkPrint < customRichBox1.TextLength;
        }

        private void _printDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            _checkPrint = 0;
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == DialogResult.OK)
                _printDocument.Print();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectedText = "";
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectedText = Clipboard.GetData(DataFormats.Text).ToString();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                pasteToolStripMenuItem.Enabled = true;
            }
            else
            {
                pasteToolStripMenuItem.Enabled = false;
            }
            if (customRichBox1.SelectionLength > 0)
            {
                deleteToolStripMenuItem.Enabled = true;
                cutToolStripMenuItem.Enabled = true;
            }
            else
            {
                deleteToolStripMenuItem.Enabled = false;
                cutToolStripMenuItem.Enabled = false;
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, (Object)customRichBox1.SelectedText);
            customRichBox1.SelectedText = "";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, (Object)customRichBox1.SelectedText);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customRichBox1.SelectAll();
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (customRichBox1.SelectionLength > 0)
            {

                customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Bold);
            }
            else
            {
                customRichBox1.Font = new Font(customRichBox1.Font.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Bold);
            }
        }

        private void underlinedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (customRichBox1.SelectionLength > 0)
            {

                customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Underline);
            }
            else
            {
                customRichBox1.Font = new Font(customRichBox1.Font.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Underline);
            }
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeEntireFont(() => customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Italic));
        }
        const int w = 1000;
        bool paused = false;
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (paused && stopToolStripMenuItem.Text != "Pause")
            {
                paused = false;
                fadeIn();
            }
            if (WindowState == FormWindowState.Minimized && stopToolStripMenuItem.Text != "Pause")
            {
                fadeout();
                paused = true;
            }

            translucentPanel1.Width = w;
            int max_w = (this.Width - w) / 2;
            translucentPanel1.Location = new Point(max_w, translucentPanel1.Location.Y);
        }

        void fadeout()
        {
            while (0 < backgroundSound.Volume)
            {
                backgroundSound.Volume -= volumeStep;
                Thread.Sleep(10);
            }
            backgroundSound.Pause();
        }

        float volume = .5f;
        float volumeStep = .005f;
        void fadeIn()
        {
            backgroundSound.Play();
            while (volume > backgroundSound.Volume)
            {
                backgroundSound.Volume += volumeStep;
                Thread.Sleep(10);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        const String FIRST_NAME = "George";
        const String LAST_NAME = "Drougkas";
        const String USERNAME = "Coldhammer";
        const String EMAIL = "ctg.coldhammer@gmail.com";
        const String US_FORMAT = "First Name: {0}\nLast Name: {1}\nUsername: {2}\nE-mail: {3}";

        private void usToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format(US_FORMAT, FIRST_NAME, LAST_NAME, USERNAME, EMAIL), "About me");
        }

        const String ABOUT = "MarinaWriter is a program that was made for my girlfriend, Marina!\n\nBabe!, I love you! ❤❤❤";
        const String HEADLINE = "About MarinaWriter";

        private void marinaWriterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ABOUT, HEADLINE);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (stopToolStripMenuItem.Text == "Pause")
            {
                fadeout();
                stopToolStripMenuItem.Text = "Resume";
                Properties.Settings.Default.LastMusic = "paused";
                Properties.Settings.Default.Save();
            }
            else
            {

                fadeIn();
                stopToolStripMenuItem.Text = "Pause";
                Properties.Settings.Default.LastMusic = backgroundSong;
                Properties.Settings.Default.Save();
            }
        }
        string keyboardSound = null;
        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (muteToolStripMenuItem.Text == "Mute")
            {
                soundEffect = null;
                Properties.Settings.Default.LastKeysound = "muted";
                Properties.Settings.Default.Save();
                muteToolStripMenuItem.Text = "Unmute";
            }
            else
            {
                Properties.Settings.Default.LastKeysound = keyboardSound;
                Properties.Settings.Default.Save();
                soundEffect = new Uri(keyboardSound, UriKind.Relative);
                muteToolStripMenuItem.Text = "Mute";
            }
        }

        private const int cGrip = 16;
        private const int cCaption = 32;

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textChangedMessageBox(sender, e)) e.Cancel = true;
        }
        bool timer1works = false;
        private void scrollingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timer1works)
            {
                timer1.Stop();
                scrollingToolStripMenuItem.Text = "Slideshow";
                timer1works = false;
            }
            else
            {

                timer1.Start();
                scrollingToolStripMenuItem.Text = "Stop Slideshow";
                timer1works = true;
            }
        }
        int i = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            changeBackground(backgroundFiles[i]);
            if ((++i) == backgroundFiles.Length) i = 0;
        }

        public void setDarkMode(bool enabled)
        {
            System.Drawing.Color cl = enabled ? System.Drawing.Color.White : System.Drawing.Color.Black;
            Image img = enabled ? Properties.Resources.DarkMask : Properties.Resources.LightMask;
            customRichBox1.ForeColor = cl;
            this.menuStrip1.ForeColor = cl;
            label1.ForeColor = cl;
            label2.ForeColor = cl;


            translucentPanel3.BackgroundImage = img;
            darkmodeToolStripMenuItem.Checked = enabled;

            Properties.Settings.Default.DarkMode = enabled;
            Properties.Settings.Default.Save();
        }

        #region RichChangeColor
        public void setRichChangeColor(System.Drawing.Color c)
        {
            if (customRichBox1.SelectionLength > 0)
            {
                customRichBox1.SelectionColor = c;
            }
            else
            {
                customRichBox1.ForeColor = c;
            }
        }

        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.White);
        }

        private void blackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Black);
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Red);
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Blue);
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Green);
        }

        private void pinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Pink);
        }

        private void purpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setRichChangeColor(System.Drawing.Color.Purple);
        }

        private void customToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ColorForm cs = new ColorForm();
            cs.ShowDialog();
            if (cs.DialogResult == DialogResult.OK)
            {
                setRichChangeColor(cs.getColor());
            }
        }
        #endregion

        public void setRichChangeFont(System.Drawing.FontFamily ff)
        {
            changeEntireFont(() =>
            {
                customRichBox1.SelectionFont = new Font(ff, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style);
            });
        }

        private void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SizeForm sizeForm = new SizeForm();
            sizeForm.setFont(customRichBox1.Font);
            sizeForm.ShowDialog();
            if (sizeForm.DialogResult == DialogResult.OK)
            {
                customRichBox1.Font = sizeForm.getFont();
            }
        }

        public delegate void FontChange();
        public void changeEntireFont(FontChange font)
        {
            if (customRichBox1.SelectionFont != null)
            {
                font();
            }
            else
            {
                int constantSelectionStart = customRichBox1.SelectionStart;
                int constantSelectionLength = customRichBox1.SelectionLength;
                int constantSelectionEnd = constantSelectionStart + constantSelectionLength;

                int selectionStart = customRichBox1.SelectionStart;
                while (selectionStart < constantSelectionEnd)
                {
                    float size = 0;
                    FontStyle? style = null;
                    for (int i = selectionStart+1; i <= constantSelectionEnd; i++)
                    {
                        customRichBox1.SelectionStart = selectionStart;
                        customRichBox1.SelectionLength = i - selectionStart;
                        if (size == 0) size = customRichBox1.SelectionFont.Size;
                        if (style == null) style = customRichBox1.SelectionFont.Style;
                        if (customRichBox1.SelectionFont == null)
                        {
                            customRichBox1.SelectionStart = selectionStart;
                            customRichBox1.SelectionLength = i - selectionStart - 1;
                            font();
                            selectionStart = i-1;
                        }
                        else if (size != customRichBox1.SelectionFont.Size || style != customRichBox1.SelectionFont.Style)
                        {
                            customRichBox1.SelectionStart = selectionStart;
                            customRichBox1.SelectionLength = i - selectionStart - 1;
                            font();
                            selectionStart = i - 1;
                        }
                        else if (i == constantSelectionEnd)
                        {
                            customRichBox1.SelectionStart = selectionStart;
                            customRichBox1.SelectionLength = i - selectionStart;
                            font();
                            selectionStart = i;
                        }

                    }
                }
            }
        }

        private void strikeoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeEntireFont(()=>{
                customRichBox1.SelectionFont = new Font(customRichBox1.SelectionFont.FontFamily, customRichBox1.SelectionFont.Size, customRichBox1.SelectionFont.Style ^ FontStyle.Strikeout);
            });
        }

        #region SearchOnline
        public void searchOnline(string uri)
        {
            String processedKeyWord;
            processedKeyWord = customRichBox1.SelectedText.Length > 0? customRichBox1.SelectedText : customRichBox1.Text;
            System.Diagnostics.Process.Start(String.Format("{0}{1}", uri, processedKeyWord));
        }

        private void googleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchOnline("http://google.com/search?q=");
        }

        private void bingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchOnline("http://bing.com/search?q=");
        }

        private void duckduckgoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchOnline("http://duckduckgo.com/?q=");
        }

        private void wikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            searchOnline("https://en.wikipedia.org/wiki/Special:Search?search=");
        }
        #endregion

        /// <summary>
        /// Set the height of the translucentPanel1 to fit the translucentPanel2 under it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(translucentPanel2.Visible == true)
            {
                translucentPanel1.Size = new Size(translucentPanel1.Size.Width, translucentPanel1.Size.Height + (translucentPanel2.Height / 2));
            }
            else
            {
                translucentPanel1.Size = new Size(translucentPanel1.Size.Width, translucentPanel1.Size.Height - (translucentPanel2.Height / 2));
            }
            translucentPanel2.Visible = !translucentPanel2.Visible;
        }

        /// <summary>
        /// When you press enter then it launches the find functionality. 
        /// After that it focuses on the richtextbox so you can't repeat that function without refocusing on this textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {

                if (textBox1.Text.Length == 0) return;
                int selectionFrom = customRichBox1.SelectionStart + textBox1.Text.Length > customRichBox1.Text.Length - 1 ? 0 : customRichBox1.SelectionStart;

                if (customRichBox1.Find(textBox1.Text, selectionFrom + textBox1.Text.Length, RichTextBoxFinds.MatchCase) == -1)
                {
                    customRichBox1.Find(textBox1.Text, 0, RichTextBoxFinds.MatchCase);
                }

                customRichBox1.Focus();
            }
        }

        /// <summary>
        /// When the cursor is in the textbox2 text box, which is the replace textbox and you press enter then the it launches the replace functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                int selectionFrom = customRichBox1.SelectionStart + textBox1.Text.Length > customRichBox1.Text.Length - 1 ? 0 : customRichBox1.SelectionStart;
                if (customRichBox1.SelectionLength > 0)
                {
                    int found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.SelectionStart + customRichBox1.SelectionLength, RichTextBoxFinds.MatchCase ^ RichTextBoxFinds.NoHighlight);
                    customRichBox1.Focus();
                    if (found > -1)
                    {
                        customRichBox1.Text.Remove(found, textBox1.Text.Length);
                        customRichBox1.Text.Insert(found, textBox2.Text);
                    }
                }
                else
                {
                    int found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.SelectionStart + customRichBox1.SelectionLength, RichTextBoxFinds.MatchCase);
                    customRichBox1.Focus();
                    if (found > -1)
                    {
                        customRichBox1.SelectedText = textBox2.Text;
                    }
                }
            }
        }

        /// <summary>
        /// Find button functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) return;
            int selectionFrom =  customRichBox1.SelectionStart + textBox1.Text.Length > customRichBox1.Text.Length - 1? 0 : customRichBox1.SelectionStart;
            
            if(customRichBox1.Find(textBox1.Text, selectionFrom + textBox1.Text.Length, RichTextBoxFinds.MatchCase) == -1)
            {
                customRichBox1.Find(textBox1.Text, 0, RichTextBoxFinds.MatchCase);
            }

            customRichBox1.Focus();
        }

        /// <summary>
        /// Replace button functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int selectionStart = customRichBox1.SelectionStart;
            int selectionLength = customRichBox1.SelectionLength;
            if (customRichBox1.SelectionLength > 0)
            {

                int found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.SelectionStart + customRichBox1.SelectionLength, RichTextBoxFinds.MatchCase ^ RichTextBoxFinds.NoHighlight);
                customRichBox1.Focus();
                if (found > -1)
                {
                    
                    customRichBox1.Text = customRichBox1.Text.Remove(found, textBox1.Text.Length);
                    customRichBox1.Text = customRichBox1.Text.Insert(found, textBox2.Text);
                }
                customRichBox1.Select(selectionStart, selectionLength + textBox2.Text.Length - textBox1.Text.Length);
            }
            else
            {
                int found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.Text.Length-1, RichTextBoxFinds.MatchCase);
                customRichBox1.Focus();
                if (found > -1)
                {
                    customRichBox1.SelectedText = textBox2.Text;
                }
            }
        }

        /// <summary>
        /// Replace all button functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            int selectionStart = customRichBox1.SelectionStart;
            int selectionLength = customRichBox1.SelectionLength;
            int count = 0;
            if (customRichBox1.SelectionLength > 0)
            {
                int found;
                while ((found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.SelectionStart + customRichBox1.SelectionLength, RichTextBoxFinds.MatchCase ^ RichTextBoxFinds.NoHighlight)) > -1)
                {
                    count++;
                    if (found > -1)
                    {

                        customRichBox1.Text = customRichBox1.Text.Remove(found, textBox1.Text.Length);
                        customRichBox1.Text = customRichBox1.Text.Insert(found, textBox2.Text);
                    }
                    customRichBox1.Select(selectionStart, selectionLength + textBox2.Text.Length - textBox1.Text.Length);
                    selectionStart = customRichBox1.SelectionStart;
                    selectionLength = customRichBox1.SelectionLength;
                }
                customRichBox1.Focus();
            }
            else
            {
                int found;
                while ((found = customRichBox1.Find(textBox1.Text, customRichBox1.SelectionStart, customRichBox1.Text.Length, RichTextBoxFinds.MatchCase)) > -1)
                {
                    count++;
                    if (found > -1)
                    {
                        customRichBox1.SelectedText = textBox2.Text;
                    }
                }
                customRichBox1.Focus();

            }
            MessageBox.Show(String.Format("There where {0} instance(s) replaced!", count), "Replacing instances");
        }


        #region Highlight
        private void setHightLight(System.Drawing.Color color)
        {
            //if (customRichBox1.SelectionLength <= 0) return;
            if (color == null)
            {
                customRichBox1.SelectionBackColor = System.Drawing.Color.Transparent;
            }
            else
            {
                customRichBox1.SelectionBackColor = color;
            }


        }
        

        private void blackToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0, 0, 0));
        }

        private void redToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0xff, 0, 0));
        }

        private void blueToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0, 0, 0xff));
        }

        private void greenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0, 0xff, 0));
        }

        private void purpleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0x94, 0x00, 0xD3));
        }

        private void customToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ColorForm colorForm = new ColorForm();
            colorForm.ShowDialog();
            if(colorForm.DialogResult == DialogResult.OK)
            {
                setHightLight(colorForm.getColor());
            }
        }
        #endregion

        private void whiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            setHightLight(System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff));
        }

        private void darkmodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(darkmodeToolStripMenuItem.Checked == false)
            {
                setDarkMode(true);
            }
            else
            {
                setDarkMode(false);
            }
        }


    }
}
