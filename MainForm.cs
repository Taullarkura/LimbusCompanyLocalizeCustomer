using AntdUI;
using LinbusCompanyLocalizeCustomer.JsonClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Label = AntdUI.Label;
using LimbusCompanyLocalizeCustomer;

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

        // 返回应用目录下的 LLC_zh-CN（下载的本地副本）
        private string GetLocalLLCSourcePath()
        {
            var local = Path.Combine(App_path, "LLC_zh-CN");
            if (Directory.Exists(local))
                return local;
            else
            {
                MessageBox.Show("LLC_zh-CN 不存在，请检查应用目录下是否有汉化文件夹!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                Application.Exit();
                return "";
            }
            //return LLCPath ?? local;
        }
        /// <summary>
        /// 下载并返回本地保存路径。下载期间会显示一个简单进度窗口，方法完成后文件可立即使用。
        /// 返回 null 表示下载失败。
        /// </summary>
        private string? DownloadLatestZeroAssoLLCAsync()
        {
            try
            {
                var latestUrl = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/latest";

                // 获取 tag（不跟随重定向以读取 Location）
                string? tag = null;
                using (var handler = new HttpClientHandler() { AllowAutoRedirect = false })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("LimbusCompanyLocalizeCustomer/1.0");
                    var resp = client.GetAsync(latestUrl).Result;
                    if (resp.Headers.Location != null)
                    {
                        var baseUri = resp.RequestMessage.RequestUri;
                        var abs = new Uri(baseUri, resp.Headers.Location);
                        tag = abs.Segments.Last().TrimEnd('/');
                    }
                }

                if (string.IsNullOrEmpty(tag))
                {
                    // 退回到允许重定向，再从最终 URL 获取 tag
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("LimbusCompanyLocalizeCustomer/1.0");
                        var final = client.GetAsync(latestUrl).Result;
                        tag = final.RequestMessage?.RequestUri?.Segments.Last()?.TrimEnd('/');
                    }
                }

                if (string.IsNullOrEmpty(tag))
                {
                    throw new Exception("无法解析最新 release 的 tag。");
                }

                var fileName = "LimbusLocalize_" + tag + ".zip";
                var downloadUrl = $"https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/download/{tag}/{fileName}";
                var outPath = Path.Combine(App_path, fileName);

                using (var progressForm = new DownloadProgressForm())
                {
                    progressForm.Show(this);
                    try
                    {
                        using (var dlClient = new HttpClient())
                        {
                            dlClient.DefaultRequestHeaders.UserAgent.ParseAdd("LimbusCompanyLocalizeCustomer/1.0");
                            var response = dlClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result;
                            if (!response.IsSuccessStatusCode)
                                throw new Exception($"下载失败: {response.StatusCode} {downloadUrl}");

                            var total = response.Content.Headers.ContentLength ?? -1L;
                            using (var contentStream = response.Content.ReadAsStreamAsync().Result)
                            using (var fs = File.Create(outPath))
                            {
                                var buffer = new byte[81920];
                                long totalRead = 0;
                                int read;
                                while ((read = contentStream.ReadAsync(buffer, 0, buffer.Length).Result) > 0)
                                {
                                    fs.Write(buffer, 0, read);
                                    totalRead += read;
                                    if (total > 0)
                                    {
                                        var percent = (int)(totalRead * 100 / total);
                                        progressForm.SetProgress(percent, totalRead, total);
                                        // 强制刷新以确保在某些 UI 调度情况下文本/进度条立即重绘
                                        progressForm.Refresh();
                                    }
                                    else
                                    {
                                        progressForm.SetProgress(-1, totalRead, total);
                                        // 强制刷新以确保在某些 UI 调度情况下文本/进度条立即重绘
                                        progressForm.Refresh();
                                    }
                                }
                            }
                        }
                        // 将压缩包中的 LLC_zh-CN 文件夹解压到应用根目录
                        try
                        {
                            using (var archive = ZipFile.OpenRead(outPath))
                            {
                                // 规范前缀，ZIP 内通常使用 '/'
                                var prefix = "LimbusCompany_Data/Lang/LLC_zh-CN/";
                                // 计算需要解压的总大小
                                long totalUncompress = 0;
                                var entries = archive.Entries.Where(e => e.FullName.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
                                foreach (var e in entries)
                                {
                                    // 目录条目长度为 0
                                    try { totalUncompress += e.Length; } catch { }
                                }

                                long totalUnzipped = 0;
                                foreach (var entry in entries)
                                {
                                    var rel = entry.FullName.Replace('\\', '/');
                                    if (rel.EndsWith("/"))
                                        continue; // skip directories

                                    var relativePath = rel.Substring(prefix.Length);
                                    if (string.IsNullOrEmpty(relativePath))
                                        continue;

                                    var destPath = Path.Combine(App_path, "LLC_zh-CN", relativePath);
                                    var destDir = Path.GetDirectoryName(destPath);
                                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                                    using (var entryStream = entry.Open())
                                    using (var outFs = File.Create(destPath))
                                    {
                                        var buffer2 = new byte[81920];
                                        int read2;
                                        while ((read2 = entryStream.Read(buffer2, 0, buffer2.Length)) > 0)
                                        {
                                            outFs.Write(buffer2, 0, read2);
                                            totalUnzipped += read2;
                                            if (totalUncompress > 0)
                                            {
                                                var pct = (int)(totalUnzipped * 100 / totalUncompress);
                                                progressForm.SetUnzip(pct, totalUnzipped, totalUncompress);
                                            }
                                            else
                                            {
                                                progressForm.SetUnzip(-1, totalUnzipped, totalUncompress);
                                            }
                                            // 强制刷新以便 UI 及时更新
                                            progressForm.Refresh();
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("解压失败: " + ex.ToString());
                            // 不要阻止后续关闭窗口与通知；抛出让外层捕获并通知
                            throw;
                        }
                    }
                    finally
                    {
                        // 确保关闭窗口在 UI 线程
                        if (!progressForm.IsDisposed)
                            progressForm.Close();
                    }
                }

                new Notification.Config(new Target(this), "提示", "已下载最新汉化" , TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
                return outPath;
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "下载最新汉化失败");
                Logger.Error(ex.ToString());
                return null;
            }
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
            //检查汉化
            CheckLLC();
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
            ExportItem.Enabled = false;
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
            var src = GetLocalLLCSourcePath();
            if (string.IsNullOrEmpty(src) || !Directory.Exists(src))
                return;
            var personalitiesPath = Path.Combine(src, "Personalities.json");
            if (!File.Exists(personalitiesPath))
                return;
            string Idts_text = File.ReadAllText(personalitiesPath);
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
            var src = GetLocalLLCSourcePath();
            if (string.IsNullOrEmpty(src) || !Directory.Exists(src))
                return;
            foreach (var file in SKILL_FILES)
            {
                var path = Path.Combine(src, file + ".json");
                if (!File.Exists(path)) continue;
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(path));
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
        private void CheckLLC()
        {
            var local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLC_zh-CN");
            if (!Directory.Exists(local))
            {
                var dia_result = MessageBox.Show("检测到应用目录中不存在零协会汉化，是否自动下载最新汉化？\n选择“是”将尝试自动下载汉化，“否”将退出本应用。\n你可以手动复制边狱巴士目录\\LimbusCompany_Data\\Lang下的LLC_zh-CN文件夹到本应用目录再启动本应用", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dia_result == DialogResult.Yes)
                {
                    var result = DownloadLatestZeroAssoLLCAsync();
                    if (result == null)
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

        }
        private void CheckLLCUpdate()
        {
            bool HasUpdate = false;
            //获取本地版本
            dynamic local_version = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(Path.Combine(App_path, "LLC_zh-CN\\Info\\version.json"))).version;
            try
            {
                // 使用 GitHub 的 latest 重定向来获取最新 release 的 tag
                var latestUrl = "https://github.com/LocalizeLimbusCompany/LocalizeLimbusCompany/releases/latest";
                string? latestTag = null;
                using (var handler = new HttpClientHandler() { AllowAutoRedirect = false })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("LimbusCompanyLocalizeCustomer/1.0");
                    var resp = client.GetAsync(latestUrl).Result;
                    if (resp.Headers.Location != null)
                    {
                        var baseUri = resp.RequestMessage.RequestUri;
                        var abs = new Uri(baseUri, resp.Headers.Location);
                        latestTag = abs.Segments.Last().TrimEnd('/');
                    }
                }
                if (string.IsNullOrEmpty(latestTag))
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("LimbusCompanyLocalizeCustomer/1.0");
                        var final = client.GetAsync(latestUrl).Result;
                        latestTag = final.RequestMessage?.RequestUri?.Segments.Last()?.TrimEnd('/');
                    }
                }

                if (!string.IsNullOrEmpty(latestTag))
                {
                    var localStr = local_version?.ToString() ?? string.Empty;
                    HasUpdate = !string.Equals(latestTag, localStr, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    Logger.Error("检查 LLC 更新时无法解析远程 tag。");
                }
            }
            catch (Exception ex)
            {
                HasUpdate = false;
                ExceptionNoticeHandle(ex, "检查LLC更新失败！");
                Logger.Error("检查 LLC 更新失败: " + ex.ToString());
            }
            if (HasUpdate)
            {
                var dia_result = MessageBox.Show("零协会汉化存在更新！是否自动更新？\n不更新也可继续使用本编辑器，但无法修改最新汉化内容\n你也可以手动覆盖文件更新", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dia_result == DialogResult.Yes)
                {
                    var result = DownloadLatestZeroAssoLLCAsync();
                    if (result == null)
                    {
                        MessageBox.Show("更新失败。请重试或尝试手动覆盖更新。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                new Notification.Config(this, "提示", "未发现更新",TType.Info,TAlignFrom.BR).SetShowInWindow().open();
            }
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
                    new Notification.Config(new Target(this), "提示", "NO THIS IDT:" + selected_item.Text, TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
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
                    new Notification.Config(new Target(this), "提示", "当前无打开的项目文件，请先打开或创建项目。", TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
                    return;
                }
                if (string.IsNullOrEmpty(LLCPath) || !Directory.Exists(LLCPath))
                {
                    new Notification.Config(new Target(this), "提示", "未配置或找不到 LLCPath，请先设置游戏路径。", TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
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

                new Notification.Config(new Target(this), "提示", "已导出并修改 JSON 文件到: " + outDir, TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
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

                new Notification.Config(new Target(this), "提示", "已生成 ZIP 文件: " + outFile, TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
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
                TopMenu.Items[1].Enabled = true;
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
                        TopMenu.Items[1].Enabled = true;
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
                    new Notification.Config(new Target(this), "提示", "当前无打开的项目文件，请先打开或创建项目。", TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
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

                // 4）保存项目
                SaveCurrentPrjButton_Click(null, null);
                new Notification.Config(new Target(this), "提示", "已将项目中的修改应用到本地汉化文件，并已自动保存项目。", TType.Info, TAlignFrom.BR).SetShowInWindow().SetAutoClose(5).open();
            }
            catch (Exception ex)
            {
                ExceptionNoticeHandle(ex, "应用修改到本地文件时出错");
                Logger.Error(ex.ToString());
            }
        }

        private void CheckLLCUpdateButton_Click(object sender, EventArgs e)
        {
            CheckLLCUpdate();
        }
    }

}
