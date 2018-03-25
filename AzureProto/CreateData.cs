using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using AzureProto;

namespace AzureProto
{
    public partial class CreateData : Form
    {
        private string Path { get; set; }
        public CreateData()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var file = new FolderBrowserDialog();
            file.RootFolder = Environment.SpecialFolder.ApplicationData;
            file.ShowDialog(this);
            if(string.IsNullOrEmpty(file.SelectedPath)) { MessageBox.Show(this, "Please select a directory"); return; }

            textBox1.Text = file.SelectedPath;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()) || string.IsNullOrEmpty(txtName.Text.Trim())) { MessageBox.Show("Both Path and Name are required"); return; }
            BuildTest();
        }

        private void BuildTest()
        {
            if(Directory.Exists(textBox1.Text.Trim()) == false) { Directory.CreateDirectory(textBox1.Text.Trim()); }
            CreateContent(txtName.Text.Trim(), -1);
            MessageBox.Show(this, "Test data created");
        }

        private void CreateContent(string parent, int depth)
        {
            string val;
            string fn;
            int max = Convert.ToInt32(numDepth.Value);
            int mc = Convert.ToInt32(numChildren.Value);
            if (depth == -1) { val = $"Parent content"; fn = "Parentfile.txt"; CreateFile($"{textBox1.Text.Trim()}\\{parent}\\{fn}", val); }
            depth++;
            for(int i = depth; i < max; i++)
            {
                string dir = $"{textBox1.Text.Trim()}\\{parent}\\{i}";
                val = $"Child Content {i} of Parent{parent}";
                fn = $"{dir}\\Child{i}_parent{parent}.txt";
                CreateFile(fn, val);

                for (int c = 0; c< mc; c++)
                {
                    val = $"Child Content {c} of Parent{i}";
                    fn = $"{dir}\\Child{c}_parent{i}.txt";
                    CreateFile(fn, val);
                }
                CreateContent(parent, i);

            }
        }

        private void CreateFile(string fileName, string content)
        {
            var fp = new FileInfo(fileName);
            if(Directory.Exists(fp.DirectoryName) == false){ fp.Directory.Create(); }

            File.WriteAllText(fileName, content);
        }
    }
}
