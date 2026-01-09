# Polynomial Curve Fitting Application
# 多项式曲线拟合应用程序

A Windows Forms application for polynomial regression and cubic spline interpolation with interactive data visualization.

一个用于多项式回归和三次样条插值的Windows Forms应用程序，具有交互式数据可视化功能。

## Features / 功能特性

- **Interactive Data Point Management** / 交互式数据点管理
  - Left-click to add points / 左键点击添加数据点
  - Right-click to remove points / 右键点击删除数据点
  - Clear all points with one click / 一键清除所有数据点

- **Polynomial Regression** / 多项式回归
  - Support for polynomial orders 1-10 / 支持1-10阶多项式
  - Ridge regression (L2 regularization) to prevent overfitting / 岭回归（L2正则化）防止过拟合
  - Adjustable regularization strength (0-10) / 可调节正则化强度（0-10）
  - Coefficient of determination (R²) calculation / 决定系数（R²）计算

- **Cubic Spline Interpolation** / 三次样条插值
  - Natural cubic spline implementation / 自然三次样条实现
  - Smooth curve fitting through all data points / 平滑曲线拟合所有数据点

- **Data Visualization** / 数据可视化
  - Powered by ScottPlot 5.1 / 基于ScottPlot 5.1
  - Real-time curve rendering / 实时曲线渲染
  - Toggle visibility of points, curves, and predictions / 切换显示数据点、曲线和预测点
  - Dynamic coordinate system scaling / 动态坐标系统缩放

- **Data Import/Export** / 数据导入/导出
  - Load sample data / 加载示例数据
  - Import data points from text file / 从文本文件导入数据点
  - Export current data points / 导出当前数据点
  - Predict Y values for given X values / 根据给定X值预测Y值
  - Export predicted data / 导出预测数据

## Screenshots / 截图

*(Add screenshots here to showcase your application)*

## Installation / 安装

### Prerequisites / 前置要求

- .NET 6.0 Runtime or SDK / .NET 6.0 运行时或SDK
- Windows operating system / Windows操作系统

### Build from Source / 从源代码构建

```bash
# Clone the repository / 克隆仓库
git clone https://github.com/freeskychenjun/polynomial-fitting.git
cd polynomial-fitting

# Build the project / 构建项目
dotnet build

# Run the application / 运行应用程序
dotnet run
```

## Usage / 使用说明

### Basic Workflow / 基本工作流程

1. **Add Data Points** / 添加数据点
   - Click on the drawing panel to add points / 在绘图面板上点击添加数据点

2. **Adjust Parameters** / 调整参数
   - Set polynomial order (1-10) / 设置多项式阶数（1-10）
   - Adjust regularization strength using the slider / 使用滑块调整正则化强度

3. **Fit Curve** / 拟合曲线
   - Click "Fit Curve" button to perform polynomial regression / 点击"拟合曲线"按钮执行多项式回归

4. **Make Predictions** / 进行预测
   - Use "Input X to Calculate" to import X values / 使用"输入X值计算"导入X值
   - Export results using "Export Predicted Data" / 使用"导出预测数据"导出结果

### Data File Formats / 数据文件格式

**Import Data File** / 导入数据文件格式:
```
x1 y1
x2 y2
x3 y3
...
```

**X Values File** / X值文件格式:
```
x1
x2
x3
...
```

## Technical Details / 技术细节

### Algorithms / 算法

- **Polynomial Regression**: Uses least squares method with Gaussian elimination / 多项式回归：使用最小二乘法和高斯消元法
- **Ridge Regression**: Adds L2 regularization to prevent overfitting / 岭回归：添加L2正则化防止过拟合
- **Cubic Spline**: Natural cubic spline with Thomas algorithm for tridiagonal system / 三次样条：使用Thomas算法求解三对角系统的自然三次样条

### Dependencies / 依赖项

- **ScottPlot.WinForms** (v5.1.57) - Data visualization library / 数据可视化库
  - License: MIT / 许可证：MIT
  - Website: https://scottplot.net/

## Project Structure / 项目结构

```
polynomial-fitting/
├── MainForm.cs              # Main UI form / 主窗体
├── PolynomialRegression.cs  # Polynomial regression implementation / 多项式回归实现
├── CubicSpline.cs          # Cubic spline interpolation / 三次样条插值
├── Program.cs              # Application entry point / 程序入口
├── CurveFitting.csproj     # Project configuration / 项目配置
├── sample_data.txt         # Sample data points / 示例数据点
├── x_values_test.txt       # Test X values / 测试X值
└── CLAUDE.md               # Project documentation / 项目文档
```

## Contributing / 贡献

Contributions are welcome! Please feel free to submit a Pull Request.

欢迎贡献！请随时提交Pull Request。

## License / 许可证

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

本项目采用MIT许可证 - 详见 [LICENSE](LICENSE) 文件。

## Acknowledgments / 致谢

- [ScottPlot](https://scottplot.net/) - Excellent plotting library for .NET / 优秀的.NET绘图库
- Built with .NET 6.0 and Windows Forms / 使用.NET 6.0和Windows Forms构建

## Contact / 联系方式

- GitHub: [@freeskychenjun](https://github.com/freeskychenjun)

---

**Note**: This application was developed with assistance from Claude Code (Anthropic).

**注意**: 本应用程序在Claude Code (Anthropic) 的协助下开发。
