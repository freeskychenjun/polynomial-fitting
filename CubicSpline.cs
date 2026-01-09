using System;

namespace CurveFitting
{
    /// <summary>
    /// 三次样条插值类
    /// 用于将离散点拟合成光滑曲线
    /// </summary>
    public class CubicSpline
    {
        private double[]? _x;
        private double[]? _y;
        private double[]? _h;
        private double[]? _a;
        private double[]? _b;
        private double[]? _c;
        private double[]? _d;
        private int _n;

        /// <summary>
        /// 拟合样条曲线
        /// </summary>
        /// <param name="x">X坐标数组（必须单调递增）</param>
        /// <param name="y">Y坐标数组</param>
        public void Fit(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("X和Y数组长度必须相同");
            if (x.Length < 2)
                throw new ArgumentException("至少需要2个点");

            _n = x.Length - 1;
            _x = x;
            _y = y;

            // 计算步长
            _h = new double[_n];
            for (int i = 0; i < _n; i++)
            {
                _h[i] = x[i + 1] - x[i];
                if (_h[i] <= 0)
                    throw new ArgumentException("X数组必须严格单调递增");
            }

            // 计算二阶导数
            CalculateSecondDerivatives();

            // 计算样条系数
            _a = new double[_n];
            _b = new double[_n];
            // _c已经在CalculateSecondDerivatives中创建，长度为_n + 1
            _d = new double[_n];

            for (int i = 0; i < _n; i++)
            {
                _a[i] = _y[i];
                _b[i] = (_y[i + 1] - _y[i]) / _h[i] - _h[i] * (2 * _c![i] + _c[i + 1]) / 6;
                _d[i] = (_c![i + 1] - _c[i]) / (6 * _h[i]);
            }
        }

        /// <summary>
        /// 计算二阶导数（使用自然样条边界条件）
        /// </summary>
        private void CalculateSecondDerivatives()
        {
            _c = new double[_n + 1];

            if (_n == 1)
            {
                // 只有两个点，使用线性插值
                _c[0] = 0;
                _c[1] = 0;
                return;
            }

            // 构建三对角矩阵
            double[] diag = new double[_n - 1];    // 主对角线
            double[] upper = new double[_n - 2];   // 上对角线
            double[] lower = new double[_n - 2];   // 下对角线
            double[] rhs = new double[_n - 1];     // 右端项

            for (int i = 0; i < _n - 1; i++)
            {
                diag[i] = 2 * (_h[i] + _h[i + 1]);
                rhs[i] = 6 * ((_y![i + 2] - _y[i + 1]) / _h[i + 1] - (_y[i + 1] - _y[i]) / _h[i]);

                if (i < _n - 2)
                {
                    upper[i] = _h[i + 1];
                    lower[i] = _h[i + 1];
                }
            }

            // 解三对角方程组（Thomas算法）
            SolveTridiagonal(diag, upper, lower, rhs);

            // 自然样条边界条件
            _c[0] = 0;
            _c[_n] = 0;
            for (int i = 1; i < _n; i++)
            {
                _c[i] = rhs[i - 1];
            }
        }

        /// <summary>
        /// Thomas算法求解三对角方程组
        /// </summary>
        private void SolveTridiagonal(double[] diag, double[] upper, double[] lower, double[] rhs)
        {
            int n = diag.Length;

            // 前向消元
            for (int i = 1; i < n; i++)
            {
                double factor = lower[i - 1] / diag[i - 1];
                diag[i] -= factor * upper[i - 1];
                rhs[i] -= factor * rhs[i - 1];
            }

            // 回代
            rhs[n - 1] /= diag[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                rhs[i] = (rhs[i] - upper[i] * rhs[i + 1]) / diag[i];
            }
        }

        /// <summary>
        /// 计算给定x处的插值
        /// </summary>
        /// <param name="x">要插值的x坐标</param>
        /// <returns>对应的y值</returns>
        public double Interpolate(double x)
        {
            if (_x == null || _y == null || _a == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            // 找到x所在的区间
            int i = FindInterval(x);

            double dx = x - _x[i];
            return _a[i] + _b[i] * dx + _c![i] * dx * dx + _d![i] * dx * dx * dx;
        }

        /// <summary>
        /// 找到x所在的区间
        /// </summary>
        private int FindInterval(double x)
        {
            // 处理边界情况
            if (x <= _x![0])
                return 0;
            if (x >= _x[_n])
                return _n - 1;

            // 二分查找
            int left = 0;
            int right = _n;

            while (right - left > 1)
            {
                int mid = (left + right) / 2;
                if (_x[mid] <= x)
                    left = mid;
                else
                    right = mid;
            }

            return left;
        }

        /// <summary>
        /// 生成拟合曲线的点集
        /// </summary>
        /// <param name="pointsPerInterval">每个区间生成的点数</param>
        /// <returns>拟合曲线的点集</returns>
        public (double[] x, double[] y) GenerateCurvePoints(int pointsPerInterval = 20)
        {
            if (_x == null)
                throw new InvalidOperationException("必须先调用Fit方法");

            int totalPoints = _n * pointsPerInterval + 1;
            double[] xCurve = new double[totalPoints];
            double[] yCurve = new double[totalPoints];

            int index = 0;
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < pointsPerInterval; j++)
                {
                    double t = (double)j / pointsPerInterval;
                    double x = _x[i] + t * (_x[i + 1] - _x[i]);
                    xCurve[index] = x;
                    yCurve[index] = Interpolate(x);
                    index++;
                }
            }

            // 添加最后一个点
            xCurve[index] = _x[_n];
            yCurve[index] = _y![_n];

            return (xCurve, yCurve);
        }
    }
}
