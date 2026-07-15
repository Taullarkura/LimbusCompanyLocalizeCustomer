namespace LimbusCompanyLocalizeCustomer
{
    partial class DownloadProgressForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DownloadLLCProgress = new AntdUI.Progress();
            CurrentStateLabel = new AntdUI.Label();
            SuspendLayout();
            // 
            // DownloadLLCProgress
            // 
            DownloadLLCProgress.Fill = Color.Aqua;
            DownloadLLCProgress.LoadingFull = true;
            DownloadLLCProgress.Location = new Point(73, 174);
            DownloadLLCProgress.Name = "DownloadLLCProgress";
            DownloadLLCProgress.Radius = 1;
            DownloadLLCProgress.Size = new Size(808, 57);
            DownloadLLCProgress.TabIndex = 0;
            DownloadLLCProgress.Text = "progress1";
            // 
            // CurrentStateLabel
            // 
            CurrentStateLabel.Location = new Point(213, 104);
            CurrentStateLabel.Name = "CurrentStateLabel";
            CurrentStateLabel.Size = new Size(475, 46);
            CurrentStateLabel.TabIndex = 1;
            CurrentStateLabel.Text = "当前状态:";
            CurrentStateLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DownloadProgressForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(987, 258);
            Controls.Add(CurrentStateLabel);
            Controls.Add(DownloadLLCProgress);
            Name = "DownloadProgressForm";
            Text = "正在下载零协汉化...";
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Progress DownloadLLCProgress;
        private AntdUI.Label CurrentStateLabel;
    }
}