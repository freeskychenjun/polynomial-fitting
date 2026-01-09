using System;

namespace CurveFitting
{
    /// <summary>
    /// 多项式拟合类（使用最小二乘法和岭回归）
    /// </summary>
    public class PolynomialRegression
    {
        private double[]? _coefficients;
        private double _lambda = 0; // 正则化参数

        /// <summary>
        /// 多项式系数（从低次到高次）
        /// </summary>
        public double[]? Coefficients => _coefficients;

        /// <summary>
        /// 当前使用的正则化参数
        /// </summary>
        public double Lambda => _lambda;

        /// <summary>
        /// 拟合多项式
        /// </summary>
        /// <param name="x">X坐标数组</param>
        /// <param name="y">Y坐标数组</param>
        /// <param name="order">多项式阶数</param>
        /// <param name="lambda">正则化参数（0=普通最小二乘法，越大越平滑）</param>
        public void Fit(double[] x, double[] y, int order, double lambda = 0)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("X和Y数组长度必须相同");
            if (x.Length < order + 1)
                throw new ArgumentException($"数据点数({x.Length})必须大于多项式阶数({order})");
            if (order < 1)
                throw new ArgumentException("多项式阶数必须至少为1");
            if (lambda < 0)
                throw new ArgumentException("正则化参数必须非负");

            _lambda = lambda;
            int n = x.Length;
            int k = order;

            // 构建正规方程矩阵: (X^T * X + λI) * a = X^T * y
            // 其中 a 是系数向量，I 是单位矩阵（岭回归）

            // 计算 X^T * X 和 X^T * y
            double[,] xt_x = new double[k + 1, k + 1];
            double[] xt_y = new double[k + 1];

            // 预计算x的幂次和
            double[] xPowerSums = new double[2 * k + 1];
            for (int i = 0; i < xPowerSums.Length; i++)
            {
                xPowerSums[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    xPowerSums[i] += Math.Pow(x[j], i);
                }
            }

            // 填充 X^T * X 矩阵，并添加L2正则化（岭回归）
            for (int i = 0; i <= k; i++)
            {
                for (int j = 0; j <= k; j++)
                {
                    xt_x[i, j] = xPowerSums[i + j];
                }

                // 岭回归：在对角线上添加λ
                if (lambda > 0 && i > 0) // i > 0 表示不惩罚常数项（可选）
                {
                    xt_x[i, i] += lambda;
                }
                else if (lambda > 0 && i == 0)
                {
                    // 如果也想惩罚常数项，取消这个else分支
                    // 通常不惩罚常数项，因为它不影响模型的复杂度
                }
            }

            // 计算 X^T * y
            for (int i = 0; i <= k; i++)
            {
                xt_y[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    xt_y[i] += y[j] * Math.Pow(x[j], i);
                }
            }

            // 解线性方程组
            _coefficients = SolveLinearSystem(xt_x, xt_y, k + 1);
        }

        /// <summary>
        /// 使用高斯消元法解线性方程组
        /// </summary>
        private double[] SolveLinearSystem(double[,] matrix, double[] vector, int size)
        {
            // 创建增广矩阵
            double[,] augmented = new double[size, size + 1];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    augmented[i, j] = matrix[i, j];
                }
                augmented[i, size] = vector[i];
            }

            // 高斯消元
            for (int col = 0; col < size; col++)
            {
                // 部分主元选择
                int maxRow = col;
                for (int row = col + 1; row < size; row++)
                {
                    if (Math.Abs(augmented[row, col]) > Math.Abs(augmented[maxRow, col]))
                    {
                        maxRow = row;
                    }
                }

                // 交换行
                if (maxRow != col)
                {
                    for (int j = col; j <= size; j++)
                    {
                        double temp = augmented[col, j];
                        augmented[col, j] = augmented[maxRow, j];
                        augmented[maxRow, j] = temp;
                    }
                }

                // 消元
                for (int row = col + 1; row < size; row++)
                {
                    double factor = augmented[row, col] / augmented[col, col];
                    for (int j = col; j <= size; j++)
                    {
                        augmented[row, j] -= factor * augmented[col, j];
                    }
                }
            }

            // 回代
            double[] solution = new double[size];
            for (int i = size - 1; i >= 0; i--)
            {
                solution[i] = augmented[i, size];
                for (int j = i + 1; j < size; j++)
                {
                    solution[i] -= augmented[i, j] * solution[j];
                }
                solution[i] /= augmented[i, i];
            }

            return solution;
        }

        /// <summary>
        /// 计算给定x处的多项式值
        /// </summary>
        /// <param name="x">要计算的x坐标</param>
        /// <returns>对应的多项式值</returns>
        public double Predict(double x)
        {
            if (_coefficients == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            double result = 0;
            double xPower = 1;

            for (int i = 0; i < _coefficients.Length; i++)
            {
                result += _coefficients[i] * xPower;
                xPower *= x;
            }

            return result;
        }

        /// <summary>
        /// 生成拟合曲线的点集
        /// </summary>
        /// <param name="xMin">X最小值</param>
        /// <param name="xMax">X最大值</param>
        /// <param name="numPoints">生成的点数</param>
        /// <returns>拟合曲线的点集</returns>
        public (double[] x, double[] y) GenerateCurvePoints(double xMin, double xMax, int numPoints = 100)
        {
            if (_coefficients == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            double[] xCurve = new double[numPoints];
            double[] yCurve = new double[numPoints];

            double step = (xMax - xMin) / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                xCurve[i] = xMin + i * step;
                yCurve[i] = Predict(xCurve[i]);
            }

            return (xCurve, yCurve);
        }

        /// <summary>
        /// 计算R²（决定系数）
        /// </summary>
        /// <param name="x">X坐标数组</param>
        /// <param name="y">Y坐标数组</param>
        /// <returns>R²值（越接近1拟合越好）</returns>
        public double CalculateRSquared(double[] x, double[] y)
        {
            if (_coefficients == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            // 计算y的均值
            double yMean = 0;
            foreach (double yi in y)
            {
                yMean += yi;
            }
            yMean /= y.Length;

            // 计算总平方和（SST）和残差平方和（SSR）
            double sst = 0; // 总平方和
            double ssr = 0; // 残差平方和

            for (int i = 0; i < y.Length; i++)
            {
                double predicted = Predict(x[i]);
                sst += Math.Pow(y[i] - yMean, 2);
                ssr += Math.Pow(y[i] - predicted, 2);
            }

            // R² = 1 - SSR/SST
            return 1 - (ssr / sst);
        }

        /// <summary>
        /// 获取多项式表达式字符串
        /// </summary>
        /// <returns>多项式表达式</returns>
        public string GetPolynomialExpression()
        {
            if (_coefficients == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = _coefficients.Length - 1; i >= 0; i--)
            {
                double coef = _coefficients[i];

                if (Math.Abs(coef) < 1e-10)
                    continue;

                if (coef >= 0 && sb.Length > 0)
                    sb.Append(" + ");
                else if (coef < 0)
                {
                    sb.Append(" - ");
                    coef = -coef;
                }

                if (i == 0)
                {
                    sb.AppendFormat("{0:F4}", coef);
                }
                else if (i == 1)
                {
                    if (Math.Abs(coef - 1.0) < 1e-10)
                        sb.Append("x");
                    else
                        sb.AppendFormat("{0:F4}x", coef);
                }
                else
                {
                    if (Math.Abs(coef - 1.0) < 1e-10)
                        sb.AppendFormat("x^{0}", i);
                    else
                        sb.AppendFormat("{0:F4}x^{0}", i);
                }
            }

            if (sb.Length == 0)
                return "0";

            return sb.ToString();
        }
    }
}
