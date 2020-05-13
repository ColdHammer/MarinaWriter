using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarinaWriter
{
    public partial class SizeForm : Form
    {
        public SizeForm()
        {
            InitializeComponent();
        }

        private void SizeForm_Load(object sender, EventArgs e)
        {
            ListViewItem litem = new ListViewItem("Strikeout");
            litem.Font = new Font(FontFamily.GenericMonospace, 8, FontStyle.Underline);
            styleList.Items.Add(litem);
            FontFamily[] fontFamilies = FontFamily.Families;
            for(int i = 0; i < fontFamilies.Length; i++)
            {
                ListViewItem item = new ListViewItem(fontFamilies[i].Name);
                item.Font = new Font(fontFamilies[i], 8, FontStyle.Regular);
                listView1.Items.Add(item);
            }
        }
        public void setFont(Font f)
        {
            label2.Font = f;
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        String text = "8";
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            char[] characters = textBox1.Text.ToCharArray();
            
            for (int i = 0; i < textBox1.Text.Length; i++)
            {
                char c = characters[i];
                if (48 > c || c >  57)
                {
                    textBox1.Text = text;
                    break;
                }
            }
            text = textBox1.Text;
            int size = 8;
            try
            {
                size = Int32.Parse(textBox1.Text);
            }
            catch (Exception) { }
            label2.Font = new Font(label2.Font.FontFamily, size > 0 ? size : 8, label2.Font.Style);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void styleList_ItemActivate(object sender, EventArgs e)
        {
            if(styleList.SelectedItems[0].Index == 0)
            {
                label2.Font = new Font(label2.Font.FontFamily, label2.Font.Size, label2.Font.Style ^ FontStyle.Underline);
            }else if(styleList.SelectedItems[0].Index == 1)
            {
                label2.Font = new Font(label2.Font.FontFamily, label2.Font.Size, label2.Font.Style ^ FontStyle.Italic);
            }
            else if(styleList.SelectedItems[0].Index == 2)
            {
                label2.Font = new Font(label2.Font.FontFamily, label2.Font.Size, label2.Font.Style ^ FontStyle.Bold);

            }
            else
            {
                label2.Font = new Font(label2.Font.FontFamily, label2.Font.Size, label2.Font.Style ^ FontStyle.Strikeout);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.SelectedItem.ToString();
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            FontFamily ff = new FontFamily(listView1.SelectedItems[0].Text);
            label2.Font = new Font(ff, label2.Font.Size, label2.Font.Style);
        }
        Font f;
        public Font getFont()
        {
            return f;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            f = label2.Font;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
