using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClutterFree
{
    public partial class Form1 : Form
    {
        public string searchPattern = "";
        public DataTable filesTable = new DataTable();
        public Form1()
        {
            InitializeComponent();

            filesTable.Columns.Add("FileName");
            filesTable.Columns.Add("FilePath");
            filesTable.Columns.Add("Size");
            filesTable.Columns.Add("DateAccessed");
        }

        private void ChooseFolderBtn_Click(object sender, EventArgs e)
        {
           if(FbDialog.ShowDialog() == DialogResult.OK)
            {
                FolderLabel.Text = FbDialog.SelectedPath;
                SearchFiles();
            }
        }

        private string SetSearchPattern()
        {
            if (FileOptionsCombo.SelectedText == "Videos")
            {
                return "*.mov,*.mp4";
            }
            else
            {
                return "*.*";
            }
        }

        private void SearchFiles()
        {
            filesTable.Clear();
            foreach (string s in Directory.GetFiles(FolderLabel.Text, SetSearchPattern(), System.IO.SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(s);
                long size = fi.Length;
                size = (size / 1024) / 1024;
                if (size > 2)
                {
                    DataRow dr = filesTable.NewRow();
                    dr["FileName"] = fi.Name;
                    dr["FilePath"] = fi.DirectoryName;

                    dr["Size"] = string.Format("{0} mb", size);
                    dr["DateAccessed"] = File.GetLastAccessTime(s).ToShortDateString();
                    filesTable.Rows.Add(dr);
                }
            }
            dataGridView1.DataSource = filesTable;
        }
    }
}
