using AntdUI;
using LinbusCompanyLocalizeCustomer.JsonClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace LinbusCompanyLocalizeCustomer
{

    public partial class MainForm : AntdUI.BaseForm
    {
        public static string[] SKILL_FILES = {
            "Skills",
            "Skills_personality-01",
            "Skills_personality-02",
            "Skills_personality-03",
            "Skills_personality-04",
            "Skills_personality-05",
            "Skills_personality-06",
            "Skills_personality-07",
            "Skills_personality-08",
            "Skills_personality-09",
            "Skills_personality-10",
            "Skills_personality-11",
            "Skills_personality-12",
        };
        public static string App_path = AppDomain.CurrentDomain.BaseDirectory;
        public static string LimbusCompanyPath = "";
        public static string LLCPath = "";
        public static IdtList IdtList = new IdtList();
        public static Dictionary<int, string> SinnerNameConverter = new Dictionary<int, string>()
        {
            {1,"李箱" },
            {2,"浮士德" },
            {3,"堂吉诃德" },
            {4,"良秀" },
            {5,"默尔索" },
            {6,"鸿璐" },
            {7,"希斯克利夫" },
            {8,"以实玛利" },
            {9,"罗佳" },
            {10,"辛克莱" },
            {11,"奥提斯" },
            {12,"格里高尔" }
        };
        public static List<SkillInfo> SkillInfos = new List<SkillInfo>();
        public static Dictionary<TreeItem, List<SkillInfo>> IdtSkillBinding = [];
        private BindingSource SkillBindingSource = new BindingSource();
        private Notification.Config HintProjectNotfiication;

        public static string CurrentPrjFilePath = "";
        public static LCLCFileJson? CurrentPrjFileJson;
        public MainForm()
        {
            InitializeComponent();
        }

        private void ApplyProjectJsonToUI()
        {
            try
            {
                if (CurrentPrjFileJson == null)
                    return;

                // 1) 应用技能名覆盖：将 CurrentPrjFileJson.SkillInfoList 的名称应用到 SkillInfos
                if (CurrentPrjFileJson.SkillInfoList != null)
                {
                    foreach (var custom in CurrentPrjFileJson.SkillInfoList)
                    {
                        if (string.IsNullOrEmpty(custom.Skill_id))
                            continue;
                        var target = SkillInfos.Find(s => s.Skill_id == custom.Skill_id);
                        if (target != null)
                        {
                            target.Skill_name = custom.Skill_name ?? target.Skill_name;
                        }
                    }
                }

                // 2) 更新 IdtSkillBinding 中的引用，使表格显示新的技能名
                var keys = IdtSkillBinding.Keys.ToList();
                foreach (var key in keys)
                {
                    var list = IdtSkillBinding[key];
                    var newList = new List<SkillInfo>();
                    foreach (var si in list)
                    {
                        var updated = SkillInfos.Find(s => s.Skill_id == si.Skill_id);
                        if (updated != null)
                            newList.Add(updated);
                        else
                            newList.Add(si);
                    }
                    IdtSkillBinding[key] = newList;
                }

                // 3) 应用人格名覆盖：将 CurrentPrjFileJson.IdtNameList 中的 title 应用到树节点
                if (CurrentPrjFileJson.IdtNameList != null)
                {
                    foreach (var custom in CurrentPrjFileJson.IdtNameList)
                    {
                        try
                        {
                            // 使用 TreeItem.Tag 中的 id 进行匹配（兼容 string/int）
                            var target = FindTreeItemById(custom.id);
                            if (target != null && !string.IsNullOrEmpty(custom.title))
                            {
                                target.Text = custom.title;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 4) 刷新当前选中项的技能表视图
                var selected_item = LCIdtTree.SelectItem;
                if (selected_item != null && IdtSkillBinding.ContainsKey(selected_item))
                {
                    SkillBindingSource.DataSource = IdtSkillBinding[selected_item];
                    IdtSkillTable.Refresh();
                }
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "应用项目数据到界面时出错");
                Logger.Error(ex.ToString());
            }
        }

        // 在整个树中查找 Tag 等于给定 id 的节点
        private TreeItem FindTreeItemById(int id)
        {
            try
            {
                foreach (var parent in LCIdtTree.Items)
                {
                    if (parent == null) continue;
                    if (parent.Tag is int pt && pt == id) return parent;
                    if (parent.Sub == null) continue;
                    foreach (var child in parent.Sub)
                    {
                        if (child == null) continue;
                        if (child.Tag is int ct && ct == id) return child;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            return null;
        }


        private void IdtNameInput_Leave(object? sender, EventArgs e)
        {
            try
            {
                var selected = LCIdtTree.SelectItem;
                if (selected == null)
                    return;
                var newTitle = IdtNameInput.Text?.Trim() ?? string.Empty;

                // 更新界面节点文本
                selected.Text = newTitle;

                // 使用 selected.Tag（int）作为人格 id，将自定义 title 保存到项目文件
                if (CurrentPrjFileJson == null)
                    CurrentPrjFileJson = new LCLCFileJson();

                // Tag 保证为 int
                int personaId = (int)selected.Tag;

                // 用父节点的文本作为罪人名
                TreeItem parent = null;
                foreach (var p in LCIdtTree.Items)
                {
                    if (p.Sub != null && p.Sub.Contains(selected))
                    {
                        parent = p;
                        break;
                    }
                }
                var sinnerName = parent?.Text ?? string.Empty;

                var exist = CurrentPrjFileJson.IdtNameList.Find(x => x.id == personaId);
                if (exist != null)
                {
                    exist.title = newTitle;
                    exist.name = sinnerName;
                    exist.nameWithTitle = sinnerName;
                }
                else
                {
                    var info = new IdtInfo()
                    {
                        id = personaId,
                        title = newTitle,
                        name = sinnerName,
                        nameWithTitle = sinnerName,
                        desc = ""
                    };
                    CurrentPrjFileJson.IdtNameList.Add(info);
                }
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "同步人格名时出错");
                Logger.Error(ex.ToString());
            }
        }
        public void ExceptionNoticeHandle(Exception ex, string AddonInfo = "")
        {
            if (AddonInfo != "")
                new Notification.Config(new Target(this), "错误", AddonInfo + "\r\n" + ex.ToString(), TType.Error, TAlignFrom.BR).SetShowInWindow().open();
            else
                new Notification.Config(new Target(this), "错误", ex.ToString(), TType.Error, TAlignFrom.BR).SetShowInWindow().open();
        }

        protected override void OnCreateControl()
        {
            //获取巴士路径
            try
            {

                var lcp = Path.Combine(App_path, "LCP.txt");
                if (File.Exists(lcp))
                {
                    if (File.Exists(Path.Combine(lcp, "limbuscompany.exe")))
                        LimbusCompanyPath = File.ReadAllText(lcp);
                    else
                    {
                        LimbusCompanyPath = SteamGameLocator.FindGamePath(1973530);
                        if (LimbusCompanyPath == null)
                        {
                            var dr = MessageBox.Show("自动获取边狱巴士公司路径失败，您是否更改了游戏安装位置？请手动配置", "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (dr == DialogResult.Yes)
                            {
                                var dr2 = SetLimbusCompanyPathDialog.ShowDialog();
                                if (dr2 == DialogResult.OK)
                                {
                                    var LC_exe_path = Path.GetDirectoryName(SetLimbusCompanyPathDialog.FileName);
                                    //MessageBox.Show(LC_exe_path);
                                    if (Directory.Exists(LC_exe_path))
                                    {
                                        LimbusCompanyPath = LC_exe_path;
                                        File.WriteAllText(lcp, LimbusCompanyPath);
                                    }
                                }
                                else
                                {
                                    this.Close();
                                    Application.Exit();
                                }
                            }
                            else
                            {
                                this.Close();
                                Application.Exit();
                            }
                        }
                        else
                        {
                            File.WriteAllText(lcp, LimbusCompanyPath);
                        }
                    }
                }
                else
                {
                    LimbusCompanyPath = SteamGameLocator.FindGamePath(1973530);
                    if (LimbusCompanyPath == null)
                    {
                        var dr = MessageBox.Show("自动获取边狱巴士公司路径失败，您是否更改了游戏安装位置？请手动配置", "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                        {
                            var dr2 = SetLimbusCompanyPathDialog.ShowDialog();
                            if (dr2 == DialogResult.OK)
                            {
                                var LC_exe_path = Path.GetDirectoryName(SetLimbusCompanyPathDialog.FileName);
                                //MessageBox.Show(LC_exe_path);
                                if (Directory.Exists(LC_exe_path))
                                {
                                    LimbusCompanyPath = LC_exe_path;
                                    File.WriteAllText(lcp, LimbusCompanyPath);
                                }
                            }
                            else
                            {
                                this.Close();
                                Application.Exit();
                            }
                        }
                        else
                        {
                            this.Close();
                            Application.Exit();
                        }
                    }
                    else
                    {
                        File.WriteAllText(lcp, LimbusCompanyPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex);
            }
            //设置LLC路径
            LLCPath = Path.Combine(LimbusCompanyPath, "LimbusCompany_Data\\Lang\\LLC_zh-CN");
            HintProjectNotfiication = new Notification.Config(new Target(this), "提示", "请打开或新建一个项目文件", TType.Info, TAlignFrom.Top)
                .SetShowInWindow()
                .SetAutoClose(0)
                .SetID("hintproject");
            //初始化人格信息
            LoadLCText();
            //初始化技能表格绑定数据
            InitSkillTable();
            //初始化菜单
            InitMenu();
            //设置信息界面为不可用状态
            IdtInfoGroup.Enabled = false;
            panel1.Enabled = false;
            HintProjectNotfiication.open();

            base.OnCreateControl();

        }


        //初始化菜单
        public void InitMenu()
        {
            TopMenu.Mode = TMenuMode.Horizontal_Arrow;
            var FileItem = new MenuItem("文件");

            var File_Create_Item = new MenuItem("创建新项目");
            File_Create_Item.Tag = "create";

            var File_Load_Item = new MenuItem("打开现有项目");
            File_Load_Item.Tag = "open";
            FileItem.SetSub(File_Load_Item, File_Create_Item);
            TopMenu.Items.Add(FileItem);
            var ExportItem = new MenuItem("导出");
            var Export_to_json_Item = new MenuItem("导出为json文件...");
            Export_to_json_Item.Tag = "export_json";
            var Export_to_zip_Item = new MenuItem("导出为zip压缩包...") { Tag = "export_zip" };
            ExportItem.SetSub(Export_to_zip_Item, Export_to_json_Item);
            TopMenu.Items.Add(ExportItem);
            //LCLCToolTip.SetTip(Export_to_zip_Item,"")
            TopMenu.Height = 90;
            TopMenu.SelectChanged += TopMenu_SelectChanged;
        }
        //初始化技能列表
        public void InitSkillTable()
        {
            var id_column = new Column("Skill_id", "技能id");
            id_column.Editable = false;
            IdtSkillTable.Columns.Add(id_column);
            IdtSkillTable.Columns.Add(new Column("Skill_name", "技能名"));
            IdtSkillTable.DataSource = SkillBindingSource;
            IdtSkillTable.CellEndEdit += IdtSkillTable_CellEndEdit;
        }



        //载入汉化文本
        public void LoadIdtText()
        {
            if (LLCPath == null)
                return;
            string Idts_text = File.ReadAllText(Path.Combine(LLCPath, "Personalities.json"));
            IdtList = JsonConvert.DeserializeObject<IdtList>(Idts_text);
            for (int i = 1; i <= 12; i++)
            {
                TreeItem treeItem = new TreeItem();
                treeItem.Text = SinnerNameConverter[i];
                var list = IdtList.dataList.FindAll(x => x.name == SinnerNameConverter[i]);
                List<TreeItem> treeItems = new List<TreeItem>();
                foreach (var item in list)
                {
                    treeItems.Add(new TreeItem() { Text = item.title, Tag = item.id });
                }
                treeItem.SetSubData(treeItems);
                LCIdtTree.Items.Add(treeItem);
            }
            LCIdtTree.ExpandAll(false);

        }
        public void LoadSkillInfo()
        {
            foreach (var file in SKILL_FILES)
            {
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(LLCPath, file + ".json")));
                var dataList = json.dataList;
                var file_name = Path.GetFileName(file);
                foreach (var skill in dataList)
                {
                    var _levelList = skill.levelList;
                    if (_levelList != null)
                        SkillInfos.Add(new SkillInfo(skill.id.ToString(), skill.levelList) { File_name = file_name });
                }
            }
            //MessageBox.Show(SkillInfos.Count.ToString());
        }
        public void LoadLCText()
        {
            LoadIdtText();
            LoadSkillInfo();
            Dictionary<string, int> _tempDic = new Dictionary<string, int>();
            foreach (var item in SinnerNameConverter)
            {
                _tempDic.Add(item.Value, item.Key);
            }
            //MessageBox.Show(_tempDic.Count.ToString());

            foreach (var item in IdtList.dataList)
            {
                if (!_tempDic.ContainsKey(item.name))
                {
                    continue;
                }
                try
                {
                    var parent = LCIdtTree.Items[_tempDic[item.name] - 1];
                    var treenode = parent.Sub.Find(x => x.Text == item.title);
                    if (treenode != null)
                    {
                        var idtSkillList = SkillInfos.FindAll(x => x.Skill_id.StartsWith(item.id.ToString()));

                        IdtSkillBinding.Add(treenode, idtSkillList);
                        //MessageBox.Show("count");
                        //count++;
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    ExceptionNoticeHandle(ex);
                    //MessageBox.Show(ex.ToString());
                }
            }

        }
        //保存人格名
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveCurrentPrjButton_Click(null, null);
            }
        }
        private void LCIdtTree_SelectChanged(object sender, TreeSelectEventArgs e)
        {
            var selected_item = LCIdtTree.SelectItem;
            if (SinnerNameConverter.ContainsValue(selected_item.Text))
            {
                IdtInfoGroup.Enabled = false;
                SkillBindingSource.DataSource = null;
                IdtSkillTable.Refresh();
            }
            else
            {
                IdtInfoGroup.Enabled = true;
                //处理更新逻辑
                IdtNameInput.Text = selected_item.Text;
                if (IdtSkillBinding.ContainsKey(selected_item))
                {
                    SkillBindingSource.DataSource = IdtSkillBinding[selected_item];
                    IdtSkillTable.Refresh();
                }
                else
                {
                    MessageBox.Show("NO THIS IDT:" + selected_item.Text);
                }
            }
        }



        private void TopMenu_SelectChanged(object sender, MenuSelectEventArgs e)
        {
            // 根据被点击菜单项的 Tag 标识分发到对应业务方法
            if (e.Value != null)
            {
                switch (e.Value.Tag as string)
                {
                    case "create":
                        CreateNewProject();
                        break;
                    case "open":
                        OpenExistingProject();
                        break;
                    case "export_json":
                        ExportPrjToJson(); //导出为json
                        break;
                    case "export_zip":
                        ExportPrjToZip(); //导出为zip
                        break;
                }
            }
            // 立即解除选中状态，第二个参数 false 避免递归触发 SelectChanged
            TopMenu.Select(null, false);
        }
        public void ExportPrjToJson()
        {
            try
            {
                if (CurrentPrjFileJson == null)
                {
                    MessageBox.Show("当前无打开的项目文件，请先打开或创建项目。");
                    return;
                }
                if (string.IsNullOrEmpty(LLCPath) || !Directory.Exists(LLCPath))
                {
                    MessageBox.Show("未配置或找不到 LLCPath，请先设置游戏路径。");
                    return;
                }

                var dr = ExportToJsonFolderBrowserDialog.ShowDialog();
                if (dr != DialogResult.OK) return;
                var outDir = ExportToJsonFolderBrowserDialog.SelectedPath;
                // 确保输出目录存在
                Directory.CreateDirectory(outDir);

                // 复制并修改 Personalities.json
                var srcPers = Path.Combine(LLCPath, "Personalities.json");
                var dstPers = Path.Combine(outDir, "Personalities.json");
                if (File.Exists(srcPers))
                {
                    File.Copy(srcPers, dstPers, true);
                    if (CurrentPrjFileJson.IdtNameList != null)
                    {
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(dstPers));
                            var dataList = j["dataList"] as JArray;
                            if (dataList != null)
                            {
                                foreach (var custom in CurrentPrjFileJson.IdtNameList)
                                {
                                    if (custom == null) continue;
                                    foreach (var item in dataList)
                                    {
                                        var idToken = item["id"];
                                        if (idToken == null) continue;
                                        if (idToken.ToString() == custom.id.ToString())
                                        {
                                            if (!string.IsNullOrEmpty(custom.title))
                                                item["title"] = custom.title;
                                            break;
                                        }
                                    }
                                }
                                File.WriteAllText(dstPers, j.ToString(Formatting.Indented));
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, "导出时修改 Personalities.json 出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 复制并修改技能文件（按 File_name 分组）
                if (CurrentPrjFileJson.SkillInfoList != null)
                {
                    var groups = CurrentPrjFileJson.SkillInfoList.Where(x => !string.IsNullOrEmpty(x.File_name)).GroupBy(x => x.File_name);
                    foreach (var g in groups)
                    {
                        var fileName = g.Key;
                        var srcFile = Path.Combine(LLCPath, fileName + ".json");
                        var dstFile = Path.Combine(outDir, fileName + ".json");
                        if (!File.Exists(srcFile)) continue;
                        File.Copy(srcFile, dstFile, true);
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(dstFile));
                            var dataList = j["dataList"] as JArray;
                            if (dataList == null) continue;
                            foreach (var custom in g)
                            {
                                if (custom == null || string.IsNullOrEmpty(custom.Skill_id)) continue;
                                foreach (var item in dataList)
                                {
                                    var idToken = item["id"];
                                    if (idToken == null) continue;
                                    if (idToken.ToString() == custom.Skill_id)
                                    {
                                        var levelList = item["levelList"] as JArray;
                                        if (levelList != null && levelList.Count > 0 && !string.IsNullOrEmpty(custom.Skill_name))
                                        {
                                            foreach (var lvl in levelList)
                                            {
                                                if (lvl is JObject lj)
                                                    lj["name"] = custom.Skill_name;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            File.WriteAllText(dstFile, j.ToString(Formatting.Indented));
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, $"导出时修改技能文件 {fileName}.json 出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                MessageBox.Show("已导出并修改 JSON 文件到: " + outDir);
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "导出为 JSON 时出错");
                Logger.Error(ex.ToString());
            }
        }
        public void ExportPrjToZip()
        {
            try
            {
                // 预设文件名为当前项目名（无扩展名）+ .zip
                var defaultName = "Export.zip";
                if (!string.IsNullOrEmpty(CurrentPrjFilePath))
                    defaultName = Path.GetFileNameWithoutExtension(CurrentPrjFilePath) + ".zip";
                ExportToZipSaveFileDialog.FileName = defaultName;
                ExportToZipSaveFileDialog.DefaultExt = "zip";
                ExportToZipSaveFileDialog.Filter = "Zip Archive|*.zip";
                var dr = ExportToZipSaveFileDialog.ShowDialog();
                if (dr != DialogResult.OK) return;
                var outFile = ExportToZipSaveFileDialog.FileName;
                // 创建临时目录
                var tempDir = Path.Combine(App_path, "temp_export_zip");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // 准备结构 LimbusCompany_Data\Lang\LLC_zh-CN
                var targetRoot = Path.Combine(tempDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN");
                Directory.CreateDirectory(targetRoot);

                // 复制并修改 Personalities.json
                var srcPers = Path.Combine(LLCPath, "Personalities.json");
                var dstPers = Path.Combine(targetRoot, "Personalities.json");
                if (File.Exists(srcPers))
                {
                    File.Copy(srcPers, dstPers, true);
                    if (CurrentPrjFileJson?.IdtNameList != null)
                    {
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(dstPers));
                            var dataList = j["dataList"] as JArray;
                            if (dataList != null)
                            {
                                foreach (var custom in CurrentPrjFileJson.IdtNameList)
                                {
                                    if (custom == null) continue;
                                    foreach (var item in dataList)
                                    {
                                        var idToken = item["id"];
                                        if (idToken == null) continue;
                                        if (idToken.ToString() == custom.id.ToString())
                                        {
                                            if (!string.IsNullOrEmpty(custom.title))
                                                item["title"] = custom.title;
                                            break;
                                        }
                                    }
                                }
                                File.WriteAllText(dstPers, j.ToString(Formatting.Indented));
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, "导出ZIP时修改 Personalities.json 出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 复制并修改技能文件
                if (CurrentPrjFileJson?.SkillInfoList != null)
                {
                    var groups = CurrentPrjFileJson.SkillInfoList.Where(x => !string.IsNullOrEmpty(x.File_name)).GroupBy(x => x.File_name);
                    foreach (var g in groups)
                    {
                        var fileName = g.Key;
                        var srcFile = Path.Combine(LLCPath, fileName + ".json");
                        var dstFile = Path.Combine(targetRoot, fileName + ".json");
                        if (!File.Exists(srcFile)) continue;
                        File.Copy(srcFile, dstFile, true);
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(dstFile));
                            var dataList = j["dataList"] as JArray;
                            if (dataList == null) continue;
                            foreach (var custom in g)
                            {
                                if (custom == null || string.IsNullOrEmpty(custom.Skill_id)) continue;
                                foreach (var item in dataList)
                                {
                                    var idToken = item["id"];
                                    if (idToken == null) continue;
                                    if (idToken.ToString() == custom.Skill_id)
                                    {
                                        var levelList = item["levelList"] as JArray;
                                        if (levelList != null && levelList.Count > 0 && !string.IsNullOrEmpty(custom.Skill_name))
                                        {
                                            foreach (var lvl in levelList)
                                            {
                                                if (lvl is JObject lj)
                                                    lj["name"] = custom.Skill_name;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            File.WriteAllText(dstFile, j.ToString(Formatting.Indented));
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, $"导出ZIP时修改技能文件 {fileName}.json 出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 打包 temp 下的 LimbusCompany_Data 文件夹为 zip
                try
                {
                    if (File.Exists(outFile)) File.Delete(outFile);
                    // 使压缩包第一层为 LimbusCompany_Data：zip tempDir 的内容（tempDir 下的 LimbusCompany_Data 将成为第一层）
                    ZipFile.CreateFromDirectory(tempDir, outFile, CompressionLevel.Optimal, false);
                }
                catch (Exception ex)
                {
                    ExceptionNoticeHandle(ex, "创建 ZIP 文件时出错");
                    Logger.Error(ex.ToString());
                }

                // 清理 temp
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch { }

                MessageBox.Show("已生成 ZIP 文件: " + outFile);
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "导出为 ZIP 时出错");
                Logger.Error(ex.ToString());
            }
        }
        private void IdtSkillTable_CellClick(object sender, TableClickEventArgs e)
        {

        }
        private bool IdtSkillTable_CellEndEdit(object sender, TableEndEditEventArgs e)
        {
            try
            {
                var newVal = e.Value?.ToString() ?? string.Empty;
                var current = SkillBindingSource.Current as SkillInfo;
                if (current == null)
                    return true;

                // 更新绑定数据（DataSource 中的对象）
                current.Skill_name = newVal;

                // 同步到 CurrentPrjFileJson
                if (CurrentPrjFileJson == null)
                    CurrentPrjFileJson = new LCLCFileJson();

                var exist = CurrentPrjFileJson.SkillInfoList.Find(x => x.Skill_id == current.Skill_id);
                if (exist != null)
                {
                    exist.Skill_name = newVal;
                }
                else
                {
                    var add = new SkillInfo()
                    {
                        Skill_id = current.Skill_id,
                        Skill_name = newVal,
                        File_name = current.File_name
                    };
                    CurrentPrjFileJson.SkillInfoList.Add(add);
                }
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "保存技能名时出错");
                Logger.Error(ex.ToString());
            }
            return true;
        }

        /// <summary>
        /// 「创建新项目」菜单项的业务逻辑。
        /// </summary>
        private void CreateNewProject()
        {
            var dia_result = SaveLCLCJsonDialog.ShowDialog();
            if (dia_result == DialogResult.OK)
            {
                var file = SaveLCLCJsonDialog.FileName;
                File.Create(file).Close();
                CurrentPrjFileJson = new LCLCFileJson();
                CurrentPrjFilePath = file;
                File.WriteAllText(file, JsonConvert.SerializeObject(CurrentPrjFileJson, Formatting.Indented));
                panel1.Enabled = true;
                if (Notification.contains("hintproject"))
                    Notification.close_id("hintproject");
            }

        }

        /// <summary>
        /// 「打开现有项目」菜单项的业务逻辑——加载已有项目文件并反序列化到界面。
        /// </summary>
        private void OpenExistingProject()
        {
            // TODO: 实现项目打开逻辑（弹出打开对话框 → 反序列化 .lclcp 文件 → 加载到界面）
            var dia_result = OpenLCLCDJsonDialog.ShowDialog();
            if (dia_result == DialogResult.OK)
            {
                var file = OpenLCLCDJsonDialog.FileName;
                if (File.Exists(file))
                {
                    try
                    {
                        string con = File.ReadAllText(file);
                        CurrentPrjFileJson = JsonConvert.DeserializeObject<LCLCFileJson>(con);
                        CurrentPrjFilePath = file;
                        // 将项目 JSON 中的自定义数据同步到界面
                        ApplyProjectJsonToUI();
                        panel1.Enabled = true;
                        if (Notification.contains("hintproject"))
                            Notification.close_id("hintproject");
                    }
                    catch (Exception ex)
                    {
                        ExceptionNoticeHandle(ex, "在尝试打开项目文件时发生错误，这可能是由于项目文件损坏或格式不正确导致的。");
                        Logger.Error(ex.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// 将当前项目对象保存进文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCurrentPrjButton_Click(object sender, EventArgs e)
        {
            try
            {
                var json = JsonConvert.SerializeObject(CurrentPrjFileJson, Formatting.Indented);
                File.WriteAllText(CurrentPrjFilePath, json);
                MessageBox.Show(json);
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "在保存项目文件时遇到异常:");
                Logger.Error(ex.ToString());
            }
        }
        /// <summary>
        /// 将当前项目文件应用到本地文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyModifyToLocalFilesButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentPrjFileJson == null)
                {
                    MessageBox.Show("当前无打开的项目文件，请先打开或创建项目。");
                    return;
                }

                // 1) 应用人格名到 LLCPath/Personalities.json
                if (!string.IsNullOrEmpty(LLCPath))
                {
                    var persPath = Path.Combine(LLCPath, "Personalities.json");
                    if (File.Exists(persPath) && CurrentPrjFileJson.IdtNameList != null)
                    {
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(persPath));
                            var dataList = j["dataList"] as JArray;
                            if (dataList != null)
                            {
                                foreach (var custom in CurrentPrjFileJson.IdtNameList)
                                {
                                    if (custom == null) continue;
                                    // 匹配 id
                                    foreach (var item in dataList)
                                    {
                                        var idToken = item["id"];
                                        if (idToken == null) continue;
                                        if (idToken.ToString() == custom.id.ToString())
                                        {
                                            if (!string.IsNullOrEmpty(custom.title))
                                                item["title"] = custom.title;
                                            break;
                                        }
                                    }
                                }
                                File.WriteAllText(persPath, j.ToString(Formatting.Indented));
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, "写入 Personalities.json 时出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 2) 应用技能名到对应 LLCPath 下的技能文件（按 File_name 分组）
                if (!string.IsNullOrEmpty(LLCPath) && CurrentPrjFileJson.SkillInfoList != null)
                {
                    var groups = CurrentPrjFileJson.SkillInfoList.Where(x => !string.IsNullOrEmpty(x.File_name)).GroupBy(x => x.File_name);
                    foreach (var g in groups)
                    {
                        var fileName = g.Key;
                        var skillFilePath = Path.Combine(LLCPath, fileName + ".json");
                        if (!File.Exists(skillFilePath))
                            continue;
                        try
                        {
                            var j = JObject.Parse(File.ReadAllText(skillFilePath));
                            var dataList = j["dataList"] as JArray;
                            if (dataList == null) continue;
                            foreach (var custom in g)
                            {
                                if (custom == null || string.IsNullOrEmpty(custom.Skill_id)) continue;
                                foreach (var item in dataList)
                                {
                                    var idToken = item["id"];
                                    if (idToken == null) continue;
                                    if (idToken.ToString() == custom.Skill_id)
                                    {
                                        var levelList = item["levelList"] as JArray;
                                        if (levelList != null && levelList.Count > 0 && !string.IsNullOrEmpty(custom.Skill_name))
                                        {
                                            // 更新 levelList 中每一项的 name
                                            foreach (var lvl in levelList)
                                            {
                                                if (lvl is JObject lj)
                                                    lj["name"] = custom.Skill_name;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            File.WriteAllText(skillFilePath, j.ToString(Formatting.Indented));
                        }
                        catch (Exception ex)
                        {
                            ExceptionNoticeHandle(ex, $"写入技能文件 {fileName}.json 时出错");
                            Logger.Error(ex.ToString());
                        }
                    }
                }

                // 3) 在内存中同步 SkillInfos 与 IdtList，而不重新读取文件
                if (CurrentPrjFileJson.SkillInfoList != null)
                {
                    foreach (var custom in CurrentPrjFileJson.SkillInfoList)
                    {
                        if (custom == null || string.IsNullOrEmpty(custom.Skill_id)) continue;
                        var exist = SkillInfos.Find(s => s.Skill_id == custom.Skill_id);
                        if (exist != null)
                        {
                            exist.Skill_name = custom.Skill_name ?? exist.Skill_name;
                        }
                        else
                        {
                            SkillInfos.Add(new SkillInfo()
                            {
                                Skill_id = custom.Skill_id,
                                Skill_name = custom.Skill_name,
                                File_name = custom.File_name
                            });
                        }
                    }
                }

                if (CurrentPrjFileJson.IdtNameList != null)
                {
                    foreach (var custom in CurrentPrjFileJson.IdtNameList)
                    {
                        if (custom == null) continue;
                        // 更新 IdtList 中所有与 id 匹配的条目的 title
                        var matches = IdtList.dataList.FindAll(d => d.id == custom.id);
                        foreach (var m in matches)
                        {
                            if (!string.IsNullOrEmpty(custom.title))
                                m.title = custom.title;
                        }
                    }
                }

                MessageBox.Show("已将项目中的修改应用到本地文件（LLCPath），并已同步内存数据。保存项目后调用 ApplyProjectJsonToUI 以重新应用项目覆盖到界面。");
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "应用修改到本地文件时出错");
                Logger.Error(ex.ToString());
            }
        }
    }
}
