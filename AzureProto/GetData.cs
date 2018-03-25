using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Data.Entity;
using System.IO;

namespace AzureProto
{
    public partial class GetData : Form
    {
        internal List<Datum> DataSource { get; set; }
        public GetData()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(DataSource == null)
            {
                using (var ctx = new BlobTestEntities1())
                {
                    DataSource = ctx.Data.Include(x => x.Data1)
                        .Where(x => x.ParentId == null).ToList();
                    RefreshTree();
                }
            }
            else { RefreshTree(); }
            
        }

        private void RefreshTree()
        {
            treeView1.Nodes.Clear();
            foreach(var data in DataSource)
            {
                AddNode(data);
            }
        }

        private void AddNode(Datum data, TreeNode pNode = null)
        {
            TreeNode node = new TreeNode(data.Name);
            
            if(pNode == null) { treeView1.Nodes.Add(node); }
            else { pNode.Nodes.Add(node); }
            foreach(var d in data.Data1)
            {
                AddNode(d, node);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var file = new FolderBrowserDialog();
            if (string.IsNullOrEmpty(textBox1.Text.Trim())) { file.RootFolder = Environment.SpecialFolder.ApplicationData; }
            else { file.SelectedPath = textBox1.Text.Trim(); }

            file.ShowDialog(this);
            if (string.IsNullOrEmpty(file.SelectedPath)) { MessageBox.Show(this, "Please select a directory"); return; }

            textBox1.Text = file.SelectedPath;
        }

        private void SaveData()
        {
            if(treeView1.SelectedNode == null) { MessageBox.Show(this, "Please select a data set to retrieve"); return; }
            var data = DataSource.FirstOrDefault(x => x.Name == treeView1.SelectedNode.Text);
            CreateFile(data, textBox1.Text.Trim());
            MessageBox.Show(this, "Data retrieved and saved");

        }

        private void CreateFile(Datum data, string filePath)
        {
            string dir = filePath;

            if (data.IsFolder)
            {
                dir = $"{filePath}\\{data.Name}";
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
            }
            else
            {
                string fp = string.Empty;
                if (checkBox1.Checked) { fp = $"{dir}\\{data.TestGuid.ToString()}"; }
                else { fp = $"{dir}\\{data.FileName}"; }
                File.WriteAllBytes(fp, data.Data);
            }

            foreach(var d in data.Data1)
            {
                CreateFile(d, dir);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim())) { MessageBox.Show(this, "Please select a directory"); return; }

            if(Directory.Exists(textBox1.Text.Trim()) == false) { MessageBox.Show(this, "Please select a valid directory"); }

            SaveData();
        }
    }
}
