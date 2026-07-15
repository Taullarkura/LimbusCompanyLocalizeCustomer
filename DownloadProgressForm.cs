using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LimbusCompanyLocalizeCustomer
{
    public partial class DownloadProgressForm : BaseForm
    {
        public void SetProgress(int percent,long total_read,long total)
        {
            CurrentStateLabel.Text = $"当前状态:下载中 ({total_read}/{ total} 字节)";
            DownloadLLCProgress.Value = percent/100f;
        }
        public void SetUnzip(int percent,long total_unziped,long total)
        {
            CurrentStateLabel.Text = $"当前状态:解压中 ({total_unziped}/{total} 字节)";
            DownloadLLCProgress.Value = percent / 100f;
        }
        public DownloadProgressForm()
        {
            InitializeComponent();
            
        }
    }
}
