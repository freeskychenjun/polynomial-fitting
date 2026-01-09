using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.WinForms;

namespace CurveFitting
{
    public partial class MainForm : Form
    {
        private List<PointF> _dataPoints = new List<PointF>();
        private List<PointF> _predictedPoints = new List<PointF>(); // 预测点
        private FormsPlot? _plotControl;
        private ToolStripButton? _btnClear;
        private ToolStripButton? _btnFitCurve;
        private ToolStripButton? _btnLoadData;
        private ToolStripButton? _btnImportData;
        private ToolStripButton? _btnExportData;
        private ToolStripButton? _btnImportXCalc;
        private ToolStripButton? _btnExportPredicted;
        private ToolStripLabel? _lblInfo;
        private ToolStripCheckBox? _chkShowPoints;
        private ToolStripCheckBox? _chkShowCurve;
        private ToolStripCheckBox? _chkShowPredicted;
        private ToolStripNumericUpDown? _nudPolyOrder;
        private ToolStripLabel? _lblPolyOrder;

        private bool _showCurve = false;
        private (double[] x, double[] y)? _curvePoints;
        private string? _polyExpression;
        private double _currentLambda = 0;
        private PolynomialRegression? _currentPolyRegression = null; // 当前拟合的模型
        private AxisLimits _dataAxesLimits; // 数据的坐标轴范围（不包含文本注释）

        public MainForm()
        {
            InitializeComponent();
            this.Text = "多项式曲线拟合程序";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            // 主绘图控件 - 使用ScottPlot
            _plotControl = new FormsPlot
            {
                Dock = DockStyle.Fill
            };
            _plotControl.MouseClick += PlotControl_MouseClick;
            this.Controls.Add(_plotControl);

            // 工具栏
            var toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                Renderer = new ToolStripProfessionalRenderer()
            };

            // 清除按钮
            _btnClear = new ToolStripButton("清除所有点");
            _btnClear.Click += BtnClear_Click;
            toolStrip.Items.Add(_btnClear);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 拟合曲线按钮
            _btnFitCurve = new ToolStripButton("拟合曲线");
            _btnFitCurve.Click += BtnFitCurve_Click;
            toolStrip.Items.Add(_btnFitCurve);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 加载示例数据按钮
            _btnLoadData = new ToolStripButton("加载示例数据");
            _btnLoadData.Click += BtnLoadData_Click;
            toolStrip.Items.Add(_btnLoadData);

            // 导入数据按钮
            _btnImportData = new ToolStripButton("导入数据");
            _btnImportData.Click += BtnImportData_Click;
            toolStrip.Items.Add(_btnImportData);

            // 导出数据按钮
            _btnExportData = new ToolStripButton("导出数据");
            _btnExportData.Click += BtnExportData_Click;
            toolStrip.Items.Add(_btnExportData);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 输入X值计算Y按钮
            _btnImportXCalc = new ToolStripButton("输入X值计算");
            _btnImportXCalc.Click += BtnImportXCalc_Click;
            toolStrip.Items.Add(_btnImportXCalc);

            // 导出预测数据按钮
            _btnExportPredicted = new ToolStripButton("导出预测数据");
            _btnExportPredicted.Click += BtnExportPredicted_Click;
            toolStrip.Items.Add(_btnExportPredicted);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 多项式阶数标签
            _lblPolyOrder = new ToolStripLabel("多项式阶数:");
            toolStrip.Items.Add(_lblPolyOrder);

            // 多项式阶数数值选择器
            _nudPolyOrder = new ToolStripNumericUpDown(3, 1, 10);
            _nudPolyOrder.Width = 50;
            _nudPolyOrder.ValueChanged += NudPolyOrder_ValueChanged;
            toolStrip.Items.Add(_nudPolyOrder);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 显示点复选框
            _chkShowPoints = new ToolStripCheckBox("显示散点", true);
            _chkShowPoints.CheckedChanged += (s, e) => RefreshPlot();
            toolStrip.Items.Add(_chkShowPoints);

            // 显示曲线复选框
            _chkShowCurve = new ToolStripCheckBox("显示拟合曲线", true);
            _chkShowCurve.CheckedChanged += (s, e) => RefreshPlot();
            toolStrip.Items.Add(_chkShowCurve);

            // 显示预测点复选框
            _chkShowPredicted = new ToolStripCheckBox("显示预测点", true);
            _chkShowPredicted.CheckedChanged += (s, e) => RefreshPlot();
            toolStrip.Items.Add(_chkShowPredicted);

            toolStrip.Items.Add(new ToolStripSeparator());

            // 信息标签
            _lblInfo = new ToolStripStatusLabel(" 点击图表添加数据点");
            ((ToolStripStatusLabel)_lblInfo).Spring = true;
            toolStrip.Items.Add(_lblInfo);

            this.Controls.Add(toolStrip);

            // 窗体事件
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            RefreshPlot();
            UpdateInfo();
        }

        private void PlotControl_MouseClick(object? sender, MouseEventArgs e)
        {
            // 鼠标点击功能已禁用，请使用"导入数据"或"加载示例数据"按钮
        }

        private void RefreshPlot()
        {
            if (_plotControl == null) return;

            _plotControl.Plot.Clear();

            // 绘制原始数据点
            if (_chkShowPoints!.Checked && _dataPoints.Count > 0)
            {
                double[] x = _dataPoints.Select(p => (double)p.X).ToArray();
                double[] y = _dataPoints.Select(p => (double)p.Y).ToArray();
                var scatter = _plotControl.Plot.AddScatter(x, y);
                scatter.MarkerSize = 8;
                scatter.LineWidth = 0;
                scatter.Color = System.Drawing.Color.Blue;
                scatter.Label = "原始数据";
            }

            // 绘制拟合曲线
            if (_showCurve && _curvePoints.HasValue && _chkShowCurve!.Checked)
            {
                var (xCurve, yCurve) = _curvePoints.Value;
                var scatter = _plotControl.Plot.AddScatterLines(xCurve, yCurve, System.Drawing.Color.Red);
                scatter.LineWidth = 2;
                scatter.Label = "拟合曲线";
            }

            // 绘制预测点
            if (_chkShowPredicted!.Checked && _predictedPoints.Count > 0)
            {
                double[] x = _predictedPoints.Select(p => (double)p.X).ToArray();
                double[] y = _predictedPoints.Select(p => (double)p.Y).ToArray();
                var scatter = _plotControl.Plot.AddScatter(x, y);
                scatter.MarkerSize = 10;
                scatter.LineWidth = 0;
                scatter.Color = System.Drawing.Color.Green;
                scatter.Label = "预测数据";
            }

            // 显示图例
            _plotControl.Plot.Legend();

            // 设置坐标轴标题
            _plotControl.Plot.Title("多项式曲线拟合");

            // 先自动调整坐标轴范围以适应数据
            _plotControl.Plot.AxisAuto();

            // 保存数据坐标轴范围（在添加文本之前）
            double xMin = _plotControl.Plot.XAxis.Dims.Min;
            double xMax = _plotControl.Plot.XAxis.Dims.Max;
            double yMin = _plotControl.Plot.YAxis.Dims.Min;
            double yMax = _plotControl.Plot.YAxis.Dims.Max;
            _dataAxesLimits = new AxisLimits(xMin, xMax, yMin, yMax);

            // 在图表上方显示拟合公式（完整显示）
            if (_polyExpression != null && _showCurve)
            {
                // 使用PlottableText添加文本（ScottPlot 4使用System.Drawing，对中文支持更好）
                var text = _plotControl.Plot.AddText(_polyExpression, (xMin + xMax) / 2, yMax);
                text.Alignment = ScottPlot.Alignment.UpperCenter;
                text.Font.Size = 12;
                text.Font.Bold = true;
                text.Color = System.Drawing.Color.Black;
                text.BackgroundColor = System.Drawing.Color.FromArgb(230, 255, 255, 255);
            }

            // 恢复数据坐标轴范围（避免文本注释影响坐标轴）
            _plotControl.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);

            _plotControl.Refresh();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _dataPoints.Clear();
            _predictedPoints.Clear();
            _showCurve = false;
            _curvePoints = null;
            _polyExpression = null;
            _currentPolyRegression = null;
            RefreshPlot();
            UpdateInfo();
        }

        private void BtnFitCurve_Click(object? sender, EventArgs e)
        {
            if (_dataPoints.Count < 2)
            {
                MessageBox.Show("至少需要2个点才能拟合曲线", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 按X坐标排序
                var sortedPoints = _dataPoints.OrderBy(p => p.X).ToList();

                // 处理重复或过于接近的X坐标
                var uniquePoints = new List<PointF>();
                const double minDistance = 0.1; // 最小X间距

                foreach (var point in sortedPoints)
                {
                    bool isDuplicate = false;
                    foreach (var existing in uniquePoints)
                    {
                        if (Math.Abs(point.X - existing.X) < minDistance)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                    if (!isDuplicate)
                    {
                        uniquePoints.Add(point);
                    }
                }

                if (uniquePoints.Count < 2)
                {
                    MessageBox.Show("有效点数不足2个，无法拟合曲线。\n请确保点的X坐标分布在不同位置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 提取X和Y坐标
                double[] x = uniquePoints.Select(p => (double)p.X).ToArray();
                double[] y = uniquePoints.Select(p => (double)p.Y).ToArray();

                // 多项式拟合
                int polyOrder = (int)_nudPolyOrder!.Value;

                if (uniquePoints.Count <= polyOrder)
                {
                    MessageBox.Show($"多项式拟合需要至少{polyOrder + 1}个数据点。\n当前有效点数: {uniquePoints.Count}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 自动计算最优正则化参数
                _currentLambda = CalculateOptimalLambda(x, y, polyOrder);

                var poly = new PolynomialRegression();
                poly.Fit(x, y, polyOrder, _currentLambda); // 使用自动计算的岭回归

                // 保存当前拟合的模型
                _currentPolyRegression = poly;

                // 计算R²
                double rSquared = poly.CalculateRSquared(x, y);

                // 生成曲线点
                double xMin = x.Min();
                double xMax = x.Max();
                _curvePoints = poly.GenerateCurvePoints(xMin, xMax, 100);

                // 获取多项式表达式（显示正则化参数）
                string lambdaText = _currentLambda > 0 ? $" λ={_currentLambda:F3}(自动)" : "";
                _polyExpression = $"{polyOrder}阶多项式拟合{lambdaText} (R² = {rSquared:F4})\n{poly.GetPolynomialExpression()}";

                // 如果已有预测点，自动重新计算Y值
                if (_predictedPoints.Count > 0)
                {
                    RecalculatePredictedPoints();
                }

                _showCurve = true;
                RefreshPlot();
                UpdateInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"拟合失败: {ex.GetType().Name}\n\n{ex.Message}\n\n堆栈跟踪:\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 多项式阶数改变时自动重新拟合曲线
        /// </summary>
        private void NudPolyOrder_ValueChanged(object? sender, EventArgs e)
        {
            // 只有当已导入散点数据时才自动拟合
            if (_dataPoints.Count >= 2)
            {
                // 直接调用拟合曲线功能
                BtnFitCurve_Click(sender, e);
            }
        }

        /// <summary>
        /// 自动计算最优正则化参数（基于数据特征和多项式阶数）
        /// </summary>
        private double CalculateOptimalLambda(double[] x, double[] y, int polyOrder)
        {
            int n = x.Length; // 数据点数量

            // 策略1: 低阶多项式 + 充足数据 → 不需要正则化或很小的正则化
            if (polyOrder <= 3 && n >= polyOrder * 5)
            {
                return 0.0; // 不需要正则化
            }

            // 策略2: 高阶多项式或数据点相对较少 → 需要正则化
            // 基于公式: λ = (polyOrder^3) / (n * scaleFactor)
            // 这个公式考虑了：
            // - 多项式阶数越高，正则化需求越大（立方关系）
            // - 数据点越多，正则化需求越小

            double baseLambda = Math.Pow(polyOrder, 3.0) / (n * 2.0);

            // 根据数据范围调整
            double yRange = y.Max() - y.Min();
            double xRange = x.Max() - x.Min();

            // 如果数据范围很大，适当增大正则化
            if (yRange > 1000 || xRange > 100)
            {
                baseLambda *= 1.5;
            }

            // 限制lambda在合理范围内 [0, 10]
            double optimalLambda = Math.Max(0.0, Math.Min(10.0, baseLambda));

            // 对于特别小的数据集和高阶多项式，给予更强的正则化
            if (n < polyOrder * 2 && polyOrder >= 6)
            {
                optimalLambda = Math.Max(2.0, optimalLambda);
            }

            return optimalLambda;
        }

        private void RecalculatePredictedPoints()
        {
            if (_currentPolyRegression == null || _predictedPoints.Count == 0)
                return;

            // 重新计算所有预测点的Y值（保持X值不变）
            for (int i = 0; i < _predictedPoints.Count; i++)
            {
                float x = _predictedPoints[i].X;
                double y = _currentPolyRegression.Predict(x);
                _predictedPoints[i] = new PointF(x, (float)y);
            }
        }

        private void BtnLoadData_Click(object? sender, EventArgs e)
        {
            _dataPoints.Clear();

            // 生成一些示例数据点（正弦波形状）
            Random rand = new Random(42);
            for (int i = 0; i <= 10; i++)
            {
                float x = 50 + i * 80;
                float y = 300 + (float)(Math.Sin(i * 0.5) * 150) + (float)(rand.NextDouble() - 0.5) * 20;
                _dataPoints.Add(new PointF(x, y));
            }

            _showCurve = false;
            _curvePoints = null;
            RefreshPlot();
            UpdateInfo();
        }

        private void BtnImportData_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
                openFileDialog.Title = "导入数据文件";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImportFromFile(openFileDialog.FileName);
                        MessageBox.Show($"成功导入 {_dataPoints.Count} 个数据点", "导入成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportData_Click(object? sender, EventArgs e)
        {
            if (_dataPoints.Count == 0)
            {
                MessageBox.Show("没有数据点可以导出", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
                saveFileDialog.Title = "导出数据文件";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = "data_points.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportToFile(saveFileDialog.FileName);
                        MessageBox.Show($"成功导出 {_dataPoints.Count} 个数据点", "导出成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportFromFile(string filePath)
        {
            _dataPoints.Clear();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // 跳过空行和注释行（以#开头）
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // 尝试解析数据点
                // 支持格式：
                // 1. "x,y" 或 "x, y"（英文逗号）
                // 2. "x，y" 或 "x， y"（中文逗号）
                // 3. "x y"（空格分隔）
                // 4. "x    y"（多个空格）
                // 5. "x	y"（制表符）

                string[] separators = new string[] { ",", "，", " ", "\t" };
                string[] parts = trimmed.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    // 移除数字中的可能逗号（如 "2,000" -> "2000"）
                    string xStr = parts[0].Replace(",", "").Replace("，", "");
                    string yStr = parts[1].Replace(",", "").Replace("，", "");

                    if (float.TryParse(xStr, out float x) && float.TryParse(yStr, out float y))
                    {
                        _dataPoints.Add(new PointF(x, y));
                    }
                }
            }

            if (_dataPoints.Count == 0)
            {
                throw new Exception("文件中没有找到有效的数据点");
            }

            _showCurve = false;
            _curvePoints = null;
            _polyExpression = null;
            _currentPolyRegression = null;
            RefreshPlot();
            UpdateInfo();
        }

        private void ExportToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("# 数据点文件");
                writer.WriteLine("# 格式: x\ty (制表符分隔)");
                writer.WriteLine($"# 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"# 数据点数量: {_dataPoints.Count}");
                writer.WriteLine();

                foreach (var point in _dataPoints)
                {
                    writer.WriteLine($"{point.X:F2}\t{point.Y:F2}");
                }
            }
        }

        private void BtnImportXCalc_Click(object? sender, EventArgs e)
        {
            // 检查是否已拟合模型
            if (_currentPolyRegression == null)
            {
                MessageBox.Show("请先进行多项式拟合，然后再输入X值计算Y值", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
                openFileDialog.Title = "导入X值文件（每行一个X值）";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImportXAndCalculate(openFileDialog.FileName);
                        MessageBox.Show($"成功计算 {_predictedPoints.Count} 个预测点", "计算成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"计算失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportPredicted_Click(object? sender, EventArgs e)
        {
            if (_predictedPoints.Count == 0)
            {
                MessageBox.Show("没有预测数据可以导出\n请先输入X值进行计算", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
                saveFileDialog.Title = "导出预测数据文件";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = "predicted_data.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportPredictedToFile(saveFileDialog.FileName);
                        MessageBox.Show($"成功导出 {_predictedPoints.Count} 个预测点", "导出成功",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportXAndCalculate(string filePath)
        {
            _predictedPoints.Clear();
            string[] lines = File.ReadAllLines(filePath);

            int lineNumber = 0;
            int parsedCount = 0;
            List<string> errors = new List<string>();

            foreach (string line in lines)
            {
                lineNumber++;
                string trimmed = line.Trim();

                // 跳过空行和注释行
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // 尝试解析X值（支持多种格式）
                float x;
                bool parsed = false;

                // 尝试直接解析
                if (float.TryParse(trimmed, out x))
                {
                    parsed = true;
                }
                // 尝试解析整数
                else if (int.TryParse(trimmed, out int intX))
                {
                    x = intX;
                    parsed = true;
                }
                // 尝试解析双精度后转换
                else if (double.TryParse(trimmed, out double doubleX))
                {
                    x = (float)doubleX;
                    parsed = true;
                }

                if (parsed)
                {
                    // 使用当前多项式模型计算Y值
                    if (_currentPolyRegression != null)
                    {
                        try
                        {
                            double y = _currentPolyRegression.Predict(x);
                            _predictedPoints.Add(new PointF(x, (float)y));
                            parsedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"第{lineNumber}行: 计算Y值失败 - {ex.Message}");
                        }
                    }
                }
                else
                {
                    errors.Add($"第{lineNumber}行: 无法解析数字 '{trimmed}'");
                }
            }

            if (_predictedPoints.Count == 0)
            {
                string errorMsg = "文件中没有找到有效的X值\n\n";
                if (errors.Count > 0)
                {
                    errorMsg += "解析错误:\n" + string.Join("\n", errors.Take(5));
                    if (errors.Count > 5)
                    {
                        errorMsg += $"\n... 还有 {errors.Count - 5} 个错误";
                    }
                }
                throw new Exception(errorMsg);
            }

            if (errors.Count > 0)
            {
                MessageBox.Show($"成功解析 {parsedCount} 个X值\n\n警告:\n{string.Join("\n", errors.Take(3))}",
                    "部分成功", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefreshPlot();
            UpdateInfo();
        }

        private void ExportPredictedToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("# 预测数据点文件");
                writer.WriteLine("# 根据拟合的多项式计算得到");
                writer.WriteLine("# 格式: x\ty (制表符分隔)");
                writer.WriteLine($"# 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"# 预测点数量: {_predictedPoints.Count}");
                writer.WriteLine($"# 多项式表达式: {_polyExpression}");
                writer.WriteLine();

                foreach (var point in _predictedPoints)
                {
                    writer.WriteLine($"{point.X:F2}\t{point.Y:F2}");
                }
            }
        }

        private void UpdateInfo()
        {
            string info = $"原始数据: {_dataPoints.Count}";

            if (_predictedPoints.Count > 0)
            {
                info += $" | 预测数据: {_predictedPoints.Count}";
            }

            _lblInfo!.Text = info;
        }
    }

    // 自定义ToolStrip控件
    public class ToolStripCheckBox : ToolStripControlHost
    {
        public CheckBox CheckBoxControl => (CheckBox)Control;

        public ToolStripCheckBox(string text, bool isChecked = false)
            : base(new CheckBox())
        {
            CheckBoxControl.Text = text;
            CheckBoxControl.Checked = isChecked;
            CheckBoxControl.AutoSize = true;
        }

        public bool Checked
        {
            get => CheckBoxControl.Checked;
            set => CheckBoxControl.Checked = value;
        }

        public event EventHandler? CheckedChanged
        {
            add => CheckBoxControl.CheckedChanged += value;
            remove => CheckBoxControl.CheckedChanged -= value;
        }
    }

    public class ToolStripNumericUpDown : ToolStripControlHost
    {
        public NumericUpDown NumericUpDownControl => (NumericUpDown)Control;

        public ToolStripNumericUpDown(int value, int min, int max)
            : base(new NumericUpDown())
        {
            NumericUpDownControl.Value = value;
            NumericUpDownControl.Minimum = min;
            NumericUpDownControl.Maximum = max;
        }

        public decimal Value
        {
            get => NumericUpDownControl.Value;
            set => NumericUpDownControl.Value = value;
        }

        public new int Width
        {
            get => NumericUpDownControl.Width;
            set => NumericUpDownControl.Width = value;
        }

        public event EventHandler? ValueChanged
        {
            add => NumericUpDownControl.ValueChanged += value;
            remove => NumericUpDownControl.ValueChanged -= value;
        }
    }

    public class ToolStripComboBox : ToolStripControlHost
    {
        public ComboBox ComboBoxControl => (ComboBox)Control;

        public ToolStripComboBox()
            : base(new ComboBox())
        {
            ComboBoxControl.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public ComboBox.ObjectCollection Items => ComboBoxControl.Items;

        public int SelectedIndex
        {
            get => ComboBoxControl.SelectedIndex;
            set => ComboBoxControl.SelectedIndex = value;
        }

        public new int Width
        {
            get => ComboBoxControl.Width;
            set => ComboBoxControl.Width = value;
        }

        public event EventHandler? SelectedIndexChanged
        {
            add => ComboBoxControl.SelectedIndexChanged += value;
            remove => ComboBoxControl.SelectedIndexChanged -= value;
        }
    }

    public class ToolStripTrackBar : ToolStripControlHost
    {
        public TrackBar TrackBarControl => (TrackBar)Control;

        public ToolStripTrackBar(int min, int max, int value)
            : base(new TrackBar())
        {
            TrackBarControl.Minimum = min;
            TrackBarControl.Maximum = max;
            TrackBarControl.Value = value;
            TrackBarControl.TickFrequency = 10;
        }

        public int Value
        {
            get => TrackBarControl.Value;
            set => TrackBarControl.Value = value;
        }

        public new int Width
        {
            get => TrackBarControl.Width;
            set => TrackBarControl.Width = value;
        }

        public event EventHandler? ValueChanged
        {
            add => TrackBarControl.ValueChanged += value;
            remove => TrackBarControl.ValueChanged -= value;
        }
    }

    // 使用 System.Windows.Forms.ToolStripLabel，不需要自定义
}
