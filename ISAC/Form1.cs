using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageSearchAndCopy
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource? cancellationTokenSource;
        private readonly BatchJobService _batchJobService = new BatchJobService();
        private AppSettings _appSettings = new AppSettings();

        // 日志记录列表（最多保存100条）
        private readonly List<LogEntry> logEntries = new List<LogEntry>();
        private const int MaxLogEntries = 100;
        private const int DisplayLogCount = 5;

        // 文件日志相关
        private readonly object _logLock = new object();
        private string _logFilePath = string.Empty;

        public Form1()
        {
            InitializeComponent();
            InitializeStatusBarInfo();
            InitializeLogDisplay();
            InitializeLogFile();

            // 暂时隐藏多行输入模式
            radioMultiMode.Visible = false;

            LoadSettings();
        }

        private void LoadSettings()
        {
            _appSettings = ConfigurationService.Load();
            if (Directory.Exists(_appSettings.LastSearchFolder))
                txtSearchFolder.Text = _appSettings.LastSearchFolder;
            if (Directory.Exists(_appSettings.LastTargetFolder))
                txtTargetFolder.Text = _appSettings.LastTargetFolder;
            if (File.Exists(_appSettings.LastCsvFile))
                txtCsvFile.Text = _appSettings.LastCsvFile;
        }

        // 添加缺失的方法
        private void InitializeStatusBarInfo()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version?.ToString() ?? "1.1.0";
                var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Align";
                var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright © 2026 Automation Michael Xiang";

                toolStripStatusLabel.Text = $"{company} | {copyright} | v{version}";
            }
            catch
            {
                toolStripStatusLabel.Text = "Align | Copyright © 2026 Automation Michael Xiang | v1.1.0";
            }
        }

        private void InitializeLogFile()
        {
            // 修改：仅在开发调试模式(DEBUG)下启用详细日志文件
            // 当您切换到 Release 模式打包给用户时，这段代码会自动失效，不会生成日志文件
#if DEBUG
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string logDir = Path.Combine(baseDir, "Logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                _logFilePath = Path.Combine(logDir, $"ExecutionLog_{DateTime.Now:yyyyMMdd}.txt");

                // 记录本次启动的分隔线
                LogToFile($"{Environment.NewLine}=== 应用程序启动 {DateTime.Now} ===");
            }
            catch
            {
                // 日志初始化失败不应阻碍程序运行
            }
#endif
        }

        private void InitializeLogDisplay()
        {
            // 简化初始化
            UpdateLogDisplay();
        }

        private void UpdateLogDisplay()
        {
            // 修复：确保在UI线程上更新控件，防止跨线程异常被吞掉导致看不到日志
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateLogDisplay));
                return;
            }

            // 清空当前显示
            listBoxLog.Items.Clear();

            // 只显示最新的5条记录
            int displayCount = Math.Min(DisplayLogCount, logEntries.Count);
            for (int i = 0; i < displayCount; i++)
            {
                listBoxLog.Items.Add(logEntries[i].ToString());
            }

            // 更新日志信息
            lblLogInfo.Text = $"显示最近{displayCount}条记录，共{logEntries.Count}条记录";
        }

        // 日志条目类
        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string PidNumber { get; set; } = string.Empty;
            public string StepNumber { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string Status { get; set; } = "成功";

            public override string ToString()
            {
                return $"{Timestamp:HH:mm:ss} | PID:{PidNumber} | 步数:{StepNumber} | {Status}";
            }
        }

        private void btnBrowseSearch_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择搜索文件夹";
                if (Directory.Exists(txtSearchFolder.Text)) folderDialog.SelectedPath = txtSearchFolder.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSearchFolder.Text = folderDialog.SelectedPath;
                    _appSettings.LastSearchFolder = txtSearchFolder.Text;
                    ConfigurationService.Save(_appSettings);
                }
            }
        }

        private void btnBrowseTarget_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择目标文件夹";
                if (Directory.Exists(txtTargetFolder.Text)) folderDialog.SelectedPath = txtTargetFolder.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtTargetFolder.Text = folderDialog.SelectedPath;
                    _appSettings.LastTargetFolder = txtTargetFolder.Text;
                    ConfigurationService.Save(_appSettings);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        private void btnBrowseCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择CSV文件";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtCsvFile.Text = openFileDialog.FileName;
                    _appSettings.LastCsvFile = txtCsvFile.Text;
                    ConfigurationService.Save(_appSettings);
                }
            }
        }

        private void OperationModeChanged(object sender, EventArgs e)
        {
            bool isSingleMode = radioSingleMode.Checked;
            bool isCsvMode = radioBatchMode.Checked; // 现在这是CSV模式
            bool isMultiMode = radioMultiMode.Checked;

            // 单次处理模式控件
            lblSoNumber.Visible = isSingleMode;
            txtSoNumber.Visible = isSingleMode;
            lblStepNumber.Visible = isSingleMode;
            txtStepNumber.Visible = isSingleMode;

            // 批量处理模式控件
            lblCsvFile.Visible = isCsvMode;
            txtCsvFile.Visible = isCsvMode;
            btnBrowseCsv.Visible = isCsvMode;

            // 多行输入模式控件
            txtBatchInput.Visible = isMultiMode;

            // 搜索文件夹和目标文件夹控件（两种模式都需要）
            lblSearchFolder.Visible = true;
            txtSearchFolder.Visible = true;
            btnBrowseSearch.Visible = true;
            lblTargetFolder.Visible = true;
            txtTargetFolder.Visible = true;
            btnBrowseTarget.Visible = true;

            // 调整控件位置，避免重叠
            if (isSingleMode)
            {
                // 单次模式：PID号和步数在第70行，搜索文件夹在第100行
                lblSoNumber.Location = new Point(20, 74);
                txtSoNumber.Location = new Point(100, 70);
                lblStepNumber.Location = new Point(270, 74);
                txtStepNumber.Location = new Point(320, 70);
                lblSearchFolder.Location = new Point(20, 104);
                txtSearchFolder.Location = new Point(100, 100);
                btnBrowseSearch.Location = new Point(410, 98);
                lblTargetFolder.Location = new Point(20, 134);
                txtTargetFolder.Location = new Point(100, 130);
                btnBrowseTarget.Location = new Point(410, 128);
            }
            else if (isCsvMode)
            {
                // 批量模式：CSV文件在第70行，搜索文件夹在第100行
                lblCsvFile.Location = new Point(20, 74);
                txtCsvFile.Location = new Point(100, 70);
                btnBrowseCsv.Location = new Point(410, 68);
                lblSearchFolder.Location = new Point(20, 104);
                txtSearchFolder.Location = new Point(100, 100);
                btnBrowseSearch.Location = new Point(410, 98);
                lblTargetFolder.Location = new Point(20, 134);
                txtTargetFolder.Location = new Point(100, 130);
                btnBrowseTarget.Location = new Point(410, 128);
            }
            else if (isMultiMode)
            {
                // 多行模式：输入框较高，需下移后续控件
                // 假设 txtBatchInput 高度为 100，起始 Y=70，结束 Y=170
                // 后续控件向下移动约 80px (原 100 -> 180)
                int offset = 80;

                lblSearchFolder.Location = new Point(20, 104 + offset);
                txtSearchFolder.Location = new Point(100, 100 + offset);
                btnBrowseSearch.Location = new Point(410, 98 + offset);

                lblTargetFolder.Location = new Point(20, 134 + offset);
                txtTargetFolder.Location = new Point(100, 130 + offset);
                btnBrowseTarget.Location = new Point(410, 128 + offset);
            }

            // 调整底部按钮和进度条位置 (多行模式下)
            int bottomOffset = isMultiMode ? 80 : 0;
            btnExecute.Location = new Point(200, 165 + bottomOffset);
            btnCancel.Location = new Point(320, 165 + bottomOffset);
            progressBar.Location = new Point(20, 200 + bottomOffset);
            lblStatus.Location = new Point(20, 220 + bottomOffset);
            groupBoxLog.Location = new Point(20, 235 + bottomOffset);

            // 更新执行按钮文本
            if (isSingleMode) btnExecute.Text = "执行搜索复制";
            else if (isCsvMode) btnExecute.Text = "执行CSV导入";
            else btnExecute.Text = "执行批量处理";
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (radioSingleMode.Checked)
            {
                ExecuteSingleMode();
            }
            else if (radioBatchMode.Checked)
            {
                ExecuteBatchMode();
            }
            else
            {
                ExecuteMultiMode();
            }
        }

        private async void ExecuteSingleMode()
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtSoNumber.Text))
            {
                MessageBox.Show("请输入PID号", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtStepNumber.Text))
            {
                MessageBox.Show("请输入步数", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSearchFolder.Text) || !Directory.Exists(txtSearchFolder.Text))
            {
                MessageBox.Show("请选择有效的搜索文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTargetFolder.Text) || !Directory.Exists(txtTargetFolder.Text))
            {
                MessageBox.Show("请选择有效的目标文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string soNumber = txtSoNumber.Text;
            string stepNumber = txtStepNumber.Text;
            string searchFolder = txtSearchFolder.Text;
            string targetFolder = txtTargetFolder.Text;

            // 禁用按钮防止重复点击
            btnExecute.Enabled = false;
            btnCancel.Enabled = true;
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                lblStatus.Text = "正在搜索...";
                await ProcessFilesAsync(searchFolder, targetFolder, soNumber, stepNumber, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "操作已取消";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜索过程中出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "操作失败";
            }
            finally
            {
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        private async void ExecuteBatchMode()
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtCsvFile.Text) || !File.Exists(txtCsvFile.Text))
            {
                MessageBox.Show("请选择有效的CSV文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSearchFolder.Text) || !Directory.Exists(txtSearchFolder.Text))
            {
                MessageBox.Show("请选择有效的搜索文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTargetFolder.Text) || !Directory.Exists(txtTargetFolder.Text))
            {
                MessageBox.Show("请选择有效的目标文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string csvFile = txtCsvFile.Text;
            string searchFolder = txtSearchFolder.Text;
            string targetFolder = txtTargetFolder.Text;

            // 禁用按钮防止重复点击
            btnExecute.Enabled = false;
            btnCancel.Enabled = true;
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                lblStatus.Text = "正在批量处理...";
                await ProcessCsvBatchAsync(csvFile, searchFolder, targetFolder, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "操作已取消";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量处理过程中出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "操作失败";
            }
            finally
            {
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        private async Task ProcessFilesAsync(string searchFolder, string targetFolder, string soNumber, string stepNumber, CancellationToken cancellationToken)
        {
            int scannedCount = 0;
            int foundCount = 0;

            await Task.Run(async () =>
            {
                // 使用 EnumerateFiles 替代 GetFiles，实现流式处理，减少内存占用并提高响应速度
                // 修改：只搜索当前文件夹(RecurseSubdirectories = false)，不搜索子文件夹
                var fileOptions = new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = true };
                var files = Directory.EnumerateFiles(searchFolder, "*.*", fileOptions);

                foreach (string filePath in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // 手动过滤后缀名
                    if (!filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) continue;

                    scannedCount++;

                    try
                    {
                        if (ContainsSoAndStep(Path.GetFileName(filePath), soNumber, stepNumber))
                        {
                            await CopyFileAsync(filePath, targetFolder, cancellationToken);
                            foundCount++;
                            AddLogEntry(soNumber, stepNumber, Path.GetFileName(filePath), "成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录错误但不中断整个过程
                        AddLogEntry(soNumber, stepNumber, Path.GetFileName(filePath), $"出错: {ex.Message}");
                    }

                    // 每处理50个文件更新一次UI，避免过于频繁刷新
                    if (scannedCount % 50 == 0)
                    {
                        await UpdateProgressAsync(scannedCount, 0, foundCount); // 0 表示未知总数
                    }
                }
                // 最后更新一次确保准确
                await UpdateProgressAsync(scannedCount, 0, foundCount);
            }, cancellationToken);

            lblStatus.Text = $"完成！扫描了 {scannedCount} 个文件，找到并复制了 {foundCount} 个文件";

            if (foundCount > 0)
            {
                MessageBox.Show($"成功复制 {foundCount} 个文件到目标文件夹", "完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("未找到包含指定PID号和步数的JPG文件", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void ExecuteMultiMode()
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtBatchInput.Text))
            {
                MessageBox.Show("请输入PID号和步数（每行一条）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSearchFolder.Text) || !Directory.Exists(txtSearchFolder.Text))
            {
                MessageBox.Show("请选择有效的搜索文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTargetFolder.Text) || !Directory.Exists(txtTargetFolder.Text))
            {
                MessageBox.Show("请选择有效的目标文件夹", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string inputText = txtBatchInput.Text;
            string searchFolder = txtSearchFolder.Text;
            string targetFolder = txtTargetFolder.Text;

            // 禁用按钮防止重复点击
            btnExecute.Enabled = false;
            btnCancel.Enabled = true;
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                lblStatus.Text = "正在解析输入并处理...";
                var criteriaList = _batchJobService.ParseManualInput(inputText);
                await ProcessBatchCriteriaAsync(criteriaList, searchFolder, targetFolder, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "操作已取消";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理过程中出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "操作失败";
            }
            finally
            {
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        private async Task ProcessCsvBatchAsync(string csvFile, string searchFolder, string targetFolder, CancellationToken cancellationToken)
        {
            // 读取CSV文件
            List<BatchJobService.SearchCriteria> criteriaList;
            try
            {
                // 使用 BatchJobService 加载 CSV，在后台任务中运行以防阻塞 UI
                criteriaList = await Task.Run(() => _batchJobService.LoadFromCsv(csvFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取CSV文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await ProcessBatchCriteriaAsync(criteriaList, searchFolder, targetFolder, cancellationToken);
        }

        private async Task ProcessBatchCriteriaAsync(List<BatchJobService.SearchCriteria> criteriaList, string searchFolder, string targetFolder, CancellationToken cancellationToken)
        {
            if (criteriaList.Count == 0)
            {
                MessageBox.Show("未找到有效的PID数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 优化：构建查找表 (PID -> List of StepNumbers)
            // 这样可以将复杂度从 O(M*N) 降低到 O(N)，极大提高批量处理速度
            var criteriaLookup = criteriaList
                .GroupBy(c => c.Pid.ToUpper())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => NormalizeStepNumber(c.StepNumber?.ToUpper() ?? "")).ToHashSet()
                );

            // 记录已找到的PID，用于统计未找到的PID
            var foundPids = new HashSet<string>();
            int totalCopied = 0;
            int scannedCount = 0;

            // 预编译正则用于从文件名快速提取 PID 和 Step
            // 假设文件名格式为： {PID}[L|U]{Step}N...
            // 优化：支持 N 或 T 作为后缀 ([NT])
            // Group 1: PID
            // Group 2: Step Digits (数字部分)
            // Group 3: Step Suffix (后缀 N 或 T)
            Regex fileParseRegex = new Regex(@"^(.+?)[LU]0*(\d+)([NT])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            await Task.Run(async () =>
            {
                // 修改：只搜索当前文件夹(RecurseSubdirectories = false)，不搜索子文件夹
                var fileOptions = new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = true };
                var files = Directory.EnumerateFiles(searchFolder, "*.*", fileOptions);

                foreach (string filePath in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // 手动过滤后缀名
                    if (!filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) continue;

                    scannedCount++;

                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        var match = fileParseRegex.Match(fileName);

                        if (match.Success)
                        {
                            string filePid = match.Groups[1].Value.ToUpper();

                            // 优化：尝试清理提取到的 PID 尾部的常见分隔符
                            // 例如文件名 "Part_12345-L05N"，提取出 "Part_12345-"，清理后 "Part_12345"
                            // 但通常问题在于 PID 是 "12345-L..." 提取出 "12345-"，而CSV里是 "12345"
                            char[] trimChars = new[] { '-', '_', ' ', '.' };
                            string cleanFilePid = filePid.TrimEnd(trimChars);

                            // NEW: 特定 PIDV_ 逻辑 (优先级最高，用户明确指出正确信息在 PIDV_ 后)
                            string? pidvPid = null;
                            int pidvIndex = filePid.LastIndexOf("PIDV_", StringComparison.OrdinalIgnoreCase);
                            if (pidvIndex >= 0)
                            {
                                pidvPid = filePid.Substring(pidvIndex + 5); // "PIDV_".Length = 5
                            }

                            // NEW: 尝试提取 PID 的最后一部分（针对 "Prefix_PID" 格式，例如 "ZH1_..._100" -> "100"）
                            string? suffixPid = null;
                            var pidParts = filePid.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pidParts.Length > 0)
                            {
                                suffixPid = pidParts[pidParts.Length - 1];
                            }

                            string fileStepDigits = match.Groups[2].Value;
                            string fileStepSuffix = match.Groups[3].Value;
                            // 标准化步数：数字去零 + 后缀 (例如 "01" + "T" -> "1T")
                            string normalizedFileStep = NormalizeStepNumber(fileStepDigits + fileStepSuffix);

                            // 检查 PID 是否在需求列表中
                            // 优先匹配 PIDV_ 提取结果，其次后缀提取，最后原始匹配
                            HashSet<string>? requiredSteps = null;
                            string? matchedPidKey = null;

                            // 显式检查并记录匹配的是哪个 PID Key，以便加入 foundPids
                            if (pidvPid != null && criteriaLookup.TryGetValue(pidvPid, out requiredSteps)) matchedPidKey = pidvPid;
                            else if (suffixPid != null && criteriaLookup.TryGetValue(suffixPid, out requiredSteps)) matchedPidKey = suffixPid;
                            else if (criteriaLookup.TryGetValue(cleanFilePid, out requiredSteps)) matchedPidKey = cleanFilePid;
                            else if (criteriaLookup.TryGetValue(filePid, out requiredSteps)) matchedPidKey = filePid;

                            if (matchedPidKey != null && requiredSteps != null)
                            {
                                // 检查 Step 是否匹配 (如果需求列表里包含该步骤，或者需求列表里有空步骤表示不限步骤)
                                // 兼容性逻辑：
                                // 1. 精确匹配：CSV "1T" 匹配文件 "1T"
                                // 2. 默认匹配：如果文件是 "N" 后缀 (如 "1N")，也允许匹配 CSV 中的纯数字 "1"
                                bool stepMatch = requiredSteps.Contains(normalizedFileStep) || requiredSteps.Contains("");
                                if (!stepMatch && fileStepSuffix.Equals("N", StringComparison.OrdinalIgnoreCase))
                                {
                                    // 尝试匹配无后缀的数字 (例如文件 "1N", CSV "1" -> normalized "1")
                                    string stepWithoutSuffix = NormalizeStepNumber(fileStepDigits);
                                    if (requiredSteps.Contains(stepWithoutSuffix)) stepMatch = true;
                                }

                                if (stepMatch)
                                {
                                    await CopyFileAsync(filePath, targetFolder, cancellationToken);
                                    totalCopied++;
                                    foundPids.Add(matchedPidKey); // 记录此PID已找到
                                    AddLogEntry(filePid, normalizedFileStep, fileName, "成功");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 仅记录严重错误，避免刷屏
                    }

                    if (scannedCount % 100 == 0)
                    {
                        await UpdateBatchProgressAsync(scannedCount, 0, totalCopied);
                    }
                }
                // 最终更新
                await UpdateBatchProgressAsync(scannedCount, 0, totalCopied);
            }, cancellationToken);

            lblStatus.Text = $"批量处理完成！共扫描 {scannedCount} 个文件，成功复制 {totalCopied} 个文件";

            // 统计未找到的PID
            var missingPids = criteriaLookup.Keys.Except(foundPids).OrderBy(p => p).ToList();

            if (missingPids.Count > 0)
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                string missingLogPath = Path.Combine(logDir, $"MissingPids_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                await File.WriteAllLinesAsync(missingLogPath, missingPids);

                MessageBox.Show($"批量处理完成！\n成功复制 {totalCopied} 个文件。\n\n发现 {missingPids.Count} 个PID未找到匹配文件。\n未找到的PID列表已保存至:\n{missingLogPath}",
                    "存在遗漏", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (totalCopied > 0)
            {
                MessageBox.Show($"批量处理完成！\n共扫描 {scannedCount} 个文件\n成功复制 {totalCopied} 个文件\n完美！所有PID均已找到。",
                    "处理成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("批量处理完成，但未找到任何匹配的文件", "未找到匹配", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 简化AddLogEntry方法，移除步数类型参数
        private void AddLogEntry(string pidNumber, string stepNumber, string fileName, string status = "成功")
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                PidNumber = pidNumber,
                StepNumber = stepNumber,
                FileName = fileName,
                Status = status
            };

            // 写入文件日志 (包含更详细的时间和文件名信息)
            LogToFile($"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss} | PID:{pidNumber} | 步数:{stepNumber} | 文件:{fileName} | {status}");

            // 添加到日志列表（最新的在前面）
            logEntries.Insert(0, logEntry);

            // 限制日志数量
            if (logEntries.Count > MaxLogEntries)
            {
                logEntries.RemoveAt(logEntries.Count - 1);
            }

            // 更新显示
            UpdateLogDisplay();
        }

        private void LogToFile(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    lock (_logLock)
                    {
                        File.AppendAllText(_logFilePath, message + Environment.NewLine);
                    }
                }
            }
            catch { /* 忽略写入错误，避免影响主流程 */ }
        }

        // 精确文件匹配方法，根据固定格式匹配步数，支持前导零忽略，精确PID匹配
        private bool ContainsSoAndStep(string fileName, string soNumber, string stepNumber)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpper();
            string soUpper = soNumber.ToUpper();
            string stepUpper = stepNumber.ToUpper();

            // 处理步数输入 (支持 "01", "01N", "01T")
            string normalizedStep = NormalizeStepNumber(stepUpper);

            // 如果输入的步数已经包含字母后缀 (如 1T, 1N)，则正则不需要再追加 N
            // 否则 (如输入 1)，默认追加 N 以兼容旧逻辑
            string suffixPattern = Regex.IsMatch(normalizedStep, @"[a-zA-Z]$") ? "" : "N";

            // 构建更严格的匹配模式，确保PID号完整匹配
            // 格式要求：{PID号}L{步数}{后缀} 或 {PID号}U{步数}{后缀}
            // 优化：允许 PID 前面有 PIDV_ 或下划线，或者是文件开头 (?:^|PIDV_|_)
            string prefixPattern = @"(?:^|PIDV_|_)";
            string lPattern = $@"{prefixPattern}{Regex.Escape(soUpper)}L0*{Regex.Escape(normalizedStep)}{suffixPattern}";
            string uPattern = $@"{prefixPattern}{Regex.Escape(soUpper)}U0*{Regex.Escape(normalizedStep)}{suffixPattern}";

            // 使用正则表达式精确匹配整个格式
            bool lMatch = Regex.IsMatch(nameWithoutExtension, lPattern);
            bool uMatch = Regex.IsMatch(nameWithoutExtension, uPattern);

            return lMatch || uMatch;
        }

        // 改进的ProcessBatchAsync方法
        private async Task<int> ProcessBatchAsync(string[] files, string soNumber, string stepNumber, string targetFolder, CancellationToken cancellationToken)
        {
            int batchFound = 0;

            foreach (string filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (ContainsSoAndStep(Path.GetFileName(filePath), soNumber, stepNumber))
                    {
                        await CopyFileAsync(filePath, targetFolder, cancellationToken);
                        batchFound++;
                        AddLogEntry(soNumber, stepNumber, Path.GetFileName(filePath), "成功");
                    }
                }
                catch (Exception ex)
                {
                    // 只有在确实匹配的文件上记录失败
                    if (ContainsSoAndStep(Path.GetFileName(filePath), soNumber, stepNumber))
                    {
                        AddLogEntry(soNumber, stepNumber, Path.GetFileName(filePath), $"失败: {ex.Message}");
                    }
                    continue; // 继续处理其他文件
                }
            }

            // 如果没有找到任何匹配文件，记录警告
            if (batchFound == 0)
            {
                AddLogEntry(soNumber, stepNumber, "无匹配文件", "未找到");
            }

            return batchFound;
        }

        private async Task CopyFileAsync(string sourcePath, string targetFolder, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string fileName = Path.GetFileName(sourcePath);
                string destPath = Path.Combine(targetFolder, fileName);

                if (File.Exists(destPath))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    int counter = 1;

                    do
                    {
                        string newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                        destPath = Path.Combine(targetFolder, newFileName);
                        counter++;
                    } while (File.Exists(destPath));
                }

                File.Copy(sourcePath, destPath);
            });
        }

        private async Task UpdateProgressAsync(int processed, int total, int found)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int, int>(UpdateProgress), processed, total, found);
                return;
            }

            UpdateProgress(processed, total, found);
        }

        private async Task UpdateBatchProgressAsync(int completedTasks, int totalTasks, int totalCopied)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int, int>(UpdateBatchProgress), completedTasks, totalTasks, totalCopied);
                return;
            }

            UpdateBatchProgress(completedTasks, totalTasks, totalCopied);
        }

        private void UpdateProgress(int processed, int total, int found)
        {
            if (total > 0)
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = Math.Min(100, (int)((double)processed / total * 100));
                lblStatus.Text = $"已处理 {processed}/{total} 个文件，找到 {found} 个匹配文件";
            }
            else
            {
                // 未知总数，使用跑马灯或者仅显示数字
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = $"正在扫描... 已扫描 {processed} 个文件，找到 {found} 个匹配";
            }
        }

        private void UpdateBatchProgress(int completedTasks, int totalTasks, int totalCopied)
        {
            // 批量模式现在显示扫描文件数
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = $"正在批量处理... 已扫描 {completedTasks} 个文件，已复制 {totalCopied} 个文件";
        }

        private string NormalizeStepNumber(string stepNumber)
        {
            // 移除前导零，但保留至少一位数字
            if (string.IsNullOrEmpty(stepNumber)) return stepNumber;

            // 支持纯数字或数字+字母后缀 (如 01, 01N, 01T)
            // Group 1: 数字部分, Group 2: 可选后缀
            var match = Regex.Match(stepNumber, @"^0*(\d+)([a-zA-Z]*)$");
            if (match.Success)
            {
                return match.Groups[1].Value + match.Groups[2].Value.ToUpper();
            }

            // 如果不是纯数字，保持原样
            return stepNumber;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            base.OnFormClosing(e);
        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {
            // 空实现
        }
    }
}