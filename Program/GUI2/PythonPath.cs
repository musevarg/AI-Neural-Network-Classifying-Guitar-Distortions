using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI2
{
    class PythonPath
    {
        public string path = "";

        public void firstLoad()
        {
            if (this.path.Equals(""))
            {
                MessageBox.Show("Please navigate to your python.exe file. \n It is usually in C:\\Program Files\\Python38, or navigate to your preferred virtual environment.", "Connect python.exe");
                OpenFileDialog file = new OpenFileDialog();
                file.InitialDirectory = @"C:\Program Files\";
                file.Filter = "Python.exe|*.exe";

                if (file.ShowDialog() == DialogResult.OK)
                {
                    if (!file.FileName.Contains("python.exe"))
                    {
                        MessageBox.Show("Wrong file selected. Please try again.");
                        firstLoad();
                    } else
                    {
                        this.path = file.FileName;
                    }
                }
                else
                {
                    firstLoad();
                }
            }
        }
    }
}
