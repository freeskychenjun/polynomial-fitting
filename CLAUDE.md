# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# 中文回答设置

请始终使用中文回答用户的所有问题和请求。无论用户使用什么语言提问，都应该用中文进行回复。

## 语言规则
- 所有回答都使用简体中文
- 技术术语可以保留英文，但要提供中文解释
- 代码注释使用中文
- 错误信息和提示使用中文
- 文档和说明使用中文

## 例外情况
- 代码本身（变量名、函数名等）可以使用英文
- 命令行指令保持原样
- 配置文件内容根据实际需要决定语言

## Build & Run

```bash
# Build the project
dotnet build

# Run the Windows Forms application
dotnet run
```

The project targets .NET 6.0-windows and uses Windows Forms for the GUI.

## Chart Library

**ScottPlot 5.1** is used for data visualization:
- Official website: https://scottplot.net/
- GitHub: https://github.com/ScottPlot/ScottPlot
- License: MIT (free for commercial and personal use)
- NuGet package: `ScottPlot.WinForms` version 5.1.57

### Key ScottPlot 5 API Patterns

```csharp
// Create scatter plots
var scatter = plot.Plot.Add.Scatter(x, y);
scatter.LegendText = "Series Name";
scatter.Color = Colors.Blue;
scatter.MarkerSize = 8;
scatter.LineWidth = 2;

// Create line plots
var line = plot.Plot.Add.ScatterLine(x, y);
line.LineWidth = 2;
line.Color = Colors.Red;

// Show legend
plot.Plot.ShowLegend();

// Set axis limits
plot.Plot.Axes.SetLimitsX(xMin, xMax);
plot.Plot.Axes.SetLimitsY(yMin, yMax);

// Set title
plot.Plot.Axes.Title.Label.Text = "Chart Title";
```

**References:**
- [ScottPlot 5 Cookbook](https://scottplot.net/cookbook/5/)
- [Windows Forms Quickstart](https://scottplot.net/quickstart/winforms/)
- [Scatter Plot Styling Example](https://scottplot.net/cookbook/5/Scatter/ScatterStyling/)

The project targets .NET 6.0-windows and uses Windows Forms for the GUI.

## Project Architecture

This is a Windows Forms application for polynomial curve fitting with the following structure:

### Core Components

**MainForm.cs** - The main UI form containing:
- Interactive drawing panel where users can click to add data points
- Toolbar with controls for curve fitting, data import/export, and parameter adjustment
- Custom rendering for data points, fitted curves, and predicted points
- Coordinate system with dynamic scaling based on data range

**PolynomialRegression.cs** - Implements polynomial regression using least squares method:
- `Fit(x, y, order, lambda)` - Fits polynomial to data points with optional ridge regression (L2 regularization)
- `Predict(x)` - Evaluates the fitted polynomial at x
- `GenerateCurvePoints(xMin, xMax, numPoints)` - Generates points for visualization
- `CalculateRSquared(x, y)` - Calculates coefficient of determination
- Uses Gaussian elimination with partial pivoting to solve the normal equations
- Ridge regularization parameter (lambda) helps prevent overfitting with high-order polynomials

**CubicSpline.cs** - Implements cubic spline interpolation (currently not integrated into the UI):
- `Fit(x, y)` - Fits natural cubic spline through data points
- `Interpolate(x)` - Evaluates spline at x
- `GenerateCurvePoints(pointsPerInterval)` - Generates smooth curve visualization
- Uses Thomas algorithm to solve the tridiagonal system for second derivatives

### Data Flow

1. User adds data points by clicking on the drawing panel (left-click to add, right-click to remove)
2. User adjusts polynomial order (1-10) and regularization strength (0-10) via toolbar
3. "Fit Curve" button triggers polynomial regression using current parameters
4. Fitted curve is rendered as red line through the data points
5. Predictions can be made by importing X values and exporting corresponding Y values

### UI Controls

- **Clear All Points** - Removes all data points
- **Fit Curve** - Performs polynomial regression with current settings
- **Load Sample Data** - Loads predefined sample data from `sample_data.txt`
- **Import Data** - Imports data points from text file (x,y format, one per line)
- **Export Data** - Exports current data points to file
- **Input X to Calculate** - Imports X values from file and calculates Y using fitted model
- **Export Predicted Data** - Exports predicted points to `predicted_data.txt`
- **Polynomial Order** - NumericUpDown control (1-10) for polynomial degree
- **Regularization Strength** - TrackBar (0-10) for ridge regression lambda parameter
- **Show Points/Curve/Predicted** - Toggle visibility of different elements

### File Formats

**sample_data.txt** - Sample data points (one x,y pair per line, space-separated)
**x_values_test.txt** - X values for prediction (one value per line)
**predicted_data.txt** - Output file containing predicted points (x,y format)

### Key Implementation Details

- Coordinates are stored as screen coordinates (pixels) and used directly for fitting
- The Y-axis is NOT flipped in the mathematical model (matches screen coordinates where Y increases downward)
- Dynamic coordinate system scaling adjusts axes based on data range with 10% padding
- Polynomial coefficients are stored from lowest to highest degree (a0 + a1*x + a2*x² + ...)
- Ridge regression adds lambda to diagonal elements of the normal equation matrix (except constant term)
- The UI uses custom ToolStrip-derived controls (ToolStripNumericUpDown, ToolStripTrackBar, ToolStripCheckBox)
