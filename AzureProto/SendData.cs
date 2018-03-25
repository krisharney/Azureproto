using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AzureProto;


namespace AzureProto
{
    public partial class SendData : Form
    {
        public SendData()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var file = new FolderBrowserDialog();
            //file.RootFolder = Environment.SpecialFolder.ApplicationData;
            file.ShowDialog(this);
            if (string.IsNullOrEmpty(file.SelectedPath)) { MessageBox.Show("Please select a directory"); return; }

            textBox1.Text = file.SelectedPath;

            Populate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(lstFiles.SelectedItem == null) { MessageBox.Show("Please select a data set");return; }
            if (Directory.Exists(lstFiles.SelectedItem.ToString()) == false) { MessageBox.Show("Please select a valid folder"); return; }
            PostIt();
        }

        private void Populate()
        {
            lstFiles.Items.Clear();
            if(Directory.Exists(textBox1.Text.Trim()) == false) { MessageBox.Show("Please select a valid folder"); return; }

            var files = Directory.EnumerateDirectories(textBox1.Text.Trim());
            foreach(var f in files)
            {
                lstFiles.Items.Add(f); 
            }
        }

        private void PostIt()
        {
            var dir = lstFiles.SelectedItem.ToString();
            var di = new DirectoryInfo(dir);
            var p = di.Name;

            using (var ctx = new BlobTestEntities1())
            {
                var d = ctx.Data.FirstOrDefault(x => x.Name == p && x.IsFolder == true);

                if (d == null)
                {
                    d = new Datum()
                    {
                        Name = p,
                        IsFolder = true,
                        FileName = di.Name,
                        TestGuid = Guid.NewGuid()
                    };

                    ctx.Data.Add(d);
                    ctx.SaveChanges();
                }

                var parent = d;
                WalkFiles(ctx, di, parent);

                foreach(var pd in di.GetDirectories())
                {
                    d = ctx.Data.FirstOrDefault(x => x.Name == pd.Name && x.IsFolder == true && parent.ParentId == x.ParentId);

                    if (d == null)
                    {
                        d = new Datum()
                        {
                            Name = pd.Name,
                            IsFolder = true,
                            FileName = pd.Name,
                            TestGuid = Guid.NewGuid(),
                            ParentId = parent.BlobTestId
                        };
                        ctx.Data.Add(d);
                        ctx.SaveChanges();
                    }
                    WalkFiles(ctx, pd, d);
                }
            }

            MessageBox.Show(this, "Data sent to cloud");
        }

        private void WalkFiles(BlobTestEntities1 ctx, DirectoryInfo di, Datum parent)
        {
            
            foreach (var f in di.GetFiles())
            {
                SaveData(ctx, parent, f);
            }
        }

        private Datum SaveData(BlobTestEntities1 ctx, Datum parent, FileInfo file)
        {

            var data = ctx.Data.FirstOrDefault(x => x.Name == file.Name && x.ParentId == parent.BlobTestId);
            if(data == null)
            {
                data = new Datum()
                {
                    Name = file.Name,
                    FileName = file.Name,
                    Data = File.ReadAllBytes(file.FullName),
                    TestGuid = Guid.NewGuid(),
                    ParentId = parent.BlobTestId
                };
                ctx.Data.Add(data);
            }
            else
            { data.Data = File.ReadAllBytes(file.FullName); }

            ctx.SaveChanges();
            return data;
        }
    }
}
