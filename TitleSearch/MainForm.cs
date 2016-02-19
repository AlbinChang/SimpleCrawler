using ProgressBarForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TitleSearch
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Dictionary<string, string> titleURL = new Dictionary<string, string>();

        private async void MainForm_Load(object sender, EventArgs e)
        {
            #region 进度提示窗口设置
            ProgressForm progressForm = new ProgressForm();
            progressForm.Size = new Size(600, 80);
            progressForm.StartPosition = FormStartPosition.Manual;
            progressForm.Location = new Point(this.DesktopLocation.X + this.Width / 2 - progressForm.Size.Width / 2, this.DesktopLocation.Y + this.Height / 2 - progressForm.Size.Height / 2);
            progressForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            progressForm.progress.Maximum = 100;
            progressForm.progress.Minimum = 0;
            progressForm.progress.Value = 0;
            progressForm.Show(this);
            this.Enabled = false;
            #endregion

            using (StreamReader reader = new StreamReader("TitleUrl.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string title = await reader.ReadLineAsync();
                    string url = await reader.ReadLineAsync();

                    try
                    {
                        titleURL.Add(title, url);
                    }
                    catch (Exception)
                    {
                        // 异常:
                        //   T:System.ArgumentNullException:
                        //     key 为 null。
                        //
                        //   T:System.ArgumentException:
                        //     System.Collections.Generic.Dictionary`2 中已存在具有相同键的元素。
                    }

                    //更新进度条
                    if (progressForm.progress.Value < 90)
                    {
                        progressForm.progress.Value++;
                        progressForm.Text = "正在加载离线数据：" + progressForm.progress.Value + "%";                        
                    }

                }
            }

            progressForm.progress.Value = 100;
            progressForm.Text = "任务进度：" + progressForm.progress.Value + "%";
            progressForm.Close();
            this.Enabled = true;

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            string key = txtKeyBox.Text;

            foreach (var item in titleURL)
            {
                if (item.Key.Contains(key))
                {
                    dataGridView.Rows.Add(item.Key, item.Value);
                }
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (IsANonHeaderLinkCell(e))
            {
                MoveToLinked(e);
            }
        }

        private bool IsANonHeaderLinkCell(DataGridViewCellEventArgs cellEvent)
        {
            if (dataGridView.Columns[cellEvent.ColumnIndex] is  DataGridViewLinkColumn &&
                cellEvent.RowIndex != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void MoveToLinked(DataGridViewCellEventArgs e)
        {
            string url = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            Process.Start(url);

        }
    }
}
