namespace LinbusCompanyLocalizeCustomer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SetLimbusCompanyPathDialog = new OpenFileDialog();
            LCIdtTree = new AntdUI.Tree();
            IdtInfoGroup = new GroupBox();
            IdtSkillTable = new AntdUI.Table();
            IdtNameInput = new AntdUI.Input();
            label1 = new AntdUI.Label();
            groupBox1 = new GroupBox();
            SaveCurrentPrjButton = new AntdUI.ButtonShadow();
            ApplyModifyToLocalFilesButton = new AntdUI.ButtonShadow();
            LCLCToolTip = new AntdUI.TooltipComponent();
            TopMenu = new AntdUI.Menu();
            SaveLCLCJsonDialog = new SaveFileDialog();
            OpenLCLCDJsonDialog = new OpenFileDialog();
            panel1 = new Panel();
            ExportToJsonFolderBrowserDialog = new FolderBrowserDialog();
            ExportToZipSaveFileDialog = new SaveFileDialog();
            IdtInfoGroup.SuspendLayout();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // SetLimbusCompanyPathDialog
            // 
            SetLimbusCompanyPathDialog.Filter = "边狱巴士公司|limbuscompany.exe";
            SetLimbusCompanyPathDialog.Title = "选择边狱巴士公司路径...";
            // 
            // LCIdtTree
            // 
            LCIdtTree.Location = new Point(25, 11);
            LCIdtTree.Name = "LCIdtTree";
            LCIdtTree.Size = new Size(426, 743);
            LCIdtTree.TabIndex = 0;
            LCIdtTree.Text = "tree1";
            LCIdtTree.SelectChanged += LCIdtTree_SelectChanged;
            // 
            // IdtInfoGroup
            // 
            IdtInfoGroup.Controls.Add(IdtSkillTable);
            IdtInfoGroup.Controls.Add(IdtNameInput);
            IdtInfoGroup.Controls.Add(label1);
            IdtInfoGroup.Location = new Point(489, 11);
            IdtInfoGroup.Name = "IdtInfoGroup";
            IdtInfoGroup.Size = new Size(1279, 638);
            IdtInfoGroup.TabIndex = 1;
            IdtInfoGroup.TabStop = false;
            IdtInfoGroup.Text = "信息修改";
            // 
            // IdtSkillTable
            // 
            IdtSkillTable.Bordered = true;
            IdtSkillTable.BorderRenderMode = AntdUI.TableBorderMode.High;
            IdtSkillTable.BorderWidth = 1.5F;
            IdtSkillTable.CellFocusedStyle = AntdUI.TableCellFocusedStyle.None;
            IdtSkillTable.CellImpactHeight = false;
            IdtSkillTable.EditMode = AntdUI.TEditMode.DoubleClick;
            IdtSkillTable.EmptyText = "请先选择一个人格";
            IdtSkillTable.Gap = 12;
            IdtSkillTable.Location = new Point(181, 225);
            IdtSkillTable.Name = "IdtSkillTable";
            IdtSkillTable.Radius = 3;
            IdtSkillTable.Size = new Size(860, 390);
            IdtSkillTable.TabIndex = 3;
            IdtSkillTable.Text = "table1";
            IdtSkillTable.CellClick += IdtSkillTable_CellClick;
            // 
            // IdtNameInput
            // 
            IdtNameInput.Location = new Point(168, 73);
            IdtNameInput.Multiline = true;
            IdtNameInput.Name = "IdtNameInput";
            IdtNameInput.Radius = 3;
            IdtNameInput.Size = new Size(282, 146);
            IdtNameInput.TabIndex = 1;
            IdtNameInput.Leave += IdtNameInput_Leave;
            // 
            // label1
            // 
            label1.Location = new Point(42, 84);
            label1.Name = "label1";
            label1.Size = new Size(150, 46);
            label1.TabIndex = 0;
            label1.Text = "人格名";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.TextMultiLine = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(SaveCurrentPrjButton);
            groupBox1.Controls.Add(ApplyModifyToLocalFilesButton);
            groupBox1.Location = new Point(503, 667);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1254, 164);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "功能";
            // 
            // SaveCurrentPrjButton
            // 
            SaveCurrentPrjButton.Location = new Point(6, 49);
            SaveCurrentPrjButton.Name = "SaveCurrentPrjButton";
            SaveCurrentPrjButton.Size = new Size(153, 65);
            SaveCurrentPrjButton.TabIndex = 7;
            SaveCurrentPrjButton.Text = "保存项目";
            LCLCToolTip.SetTip(SaveCurrentPrjButton, "保存项目文件\r\n快捷键:Ctrl+S");
            SaveCurrentPrjButton.Click += SaveCurrentPrjButton_Click;
            // 
            // ApplyModifyToLocalFilesButton
            // 
            ApplyModifyToLocalFilesButton.Location = new Point(179, 49);
            ApplyModifyToLocalFilesButton.Name = "ApplyModifyToLocalFilesButton";
            ApplyModifyToLocalFilesButton.Size = new Size(257, 65);
            ApplyModifyToLocalFilesButton.TabIndex = 6;
            ApplyModifyToLocalFilesButton.Text = "应用项目修改到文件";
            LCLCToolTip.SetTip(ApplyModifyToLocalFilesButton, "将当前项目文件对汉化的修改应用到本地汉化(即修改本地汉化文件)");
            ApplyModifyToLocalFilesButton.Click += ApplyModifyToLocalFilesButton_Click;
            // 
            // TopMenu
            // 
            TopMenu.BackActive = Color.WhiteSmoke;
            TopMenu.Location = new Point(20, 8);
            TopMenu.Name = "TopMenu";
            TopMenu.Size = new Size(1760, 58);
            TopMenu.TabIndex = 3;
            TopMenu.Text = "menu1";
            // 
            // SaveLCLCJsonDialog
            // 
            SaveLCLCJsonDialog.DefaultExt = "lclcp";
            SaveLCLCJsonDialog.FileName = "NewProject.lclcp";
            SaveLCLCJsonDialog.Filter = "汉化编辑器项目文件|*.lclcp";
            SaveLCLCJsonDialog.Title = "保存新的项目文件...";
            // 
            // OpenLCLCDJsonDialog
            // 
            OpenLCLCDJsonDialog.Filter = "汉化编辑器项目文件|*.lclcp";
            // 
            // panel1
            // 
            panel1.Controls.Add(LCIdtTree);
            panel1.Controls.Add(IdtInfoGroup);
            panel1.Controls.Add(groupBox1);
            panel1.Location = new Point(12, 122);
            panel1.Name = "panel1";
            panel1.Size = new Size(1778, 849);
            panel1.TabIndex = 4;
            // 
            // ExportToZipSaveFileDialog
            // 
            ExportToZipSaveFileDialog.DefaultExt = "zip";
            ExportToZipSaveFileDialog.Filter = "Zip压缩文件|*.zip";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1794, 974);
            Controls.Add(panel1);
            Controls.Add(TopMenu);
            KeyPreview = true;
            Name = "MainForm";
            Text = "边狱巴士公司汉化编辑器";
            Load += Form1_Load;
            KeyDown += MainForm_KeyDown;
            IdtInfoGroup.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private OpenFileDialog SetLimbusCompanyPathDialog;
        private AntdUI.Tree LCIdtTree;
        private GroupBox IdtInfoGroup;
        private AntdUI.Label label1;
        private AntdUI.Input IdtNameInput;
        private GroupBox groupBox1;
        private AntdUI.TooltipComponent LCLCToolTip;
        private AntdUI.Table IdtSkillTable;
        private AntdUI.Menu TopMenu;
        private SaveFileDialog SaveLCLCJsonDialog;
        private OpenFileDialog OpenLCLCDJsonDialog;
        private Panel panel1;
        private AntdUI.ButtonShadow ApplyModifyToLocalFilesButton;
        private AntdUI.ButtonShadow SaveCurrentPrjButton;
        private FolderBrowserDialog ExportToJsonFolderBrowserDialog;
        private SaveFileDialog ExportToZipSaveFileDialog;
    }
}
