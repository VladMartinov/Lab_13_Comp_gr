using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab_13_Comp.gr._
{
    public partial class Form1 : Form
    {
        // Для масштабирования
        double xmin, xmax, ymin, ymax;
        double Xmin, Xmax, Ymin, Ymax;
        double fx, fy, f, xC, yC, XC, YC, c1, c2;

        // Основные
        const int NMAX = 500;
        const double BIG = 1.0e30;
        Graphics dc; Pen p;
        int n; int[] v; double[] x; double[] y;
        ComboBox tbComboBox1;
        Label label1, label2;
        
        // Для динамических TextBox
        public const int bh = 40; public const int bw = 40;
        public static int iRows = 11, iColumns = 4;
        public TextBox[,] tbArray;
        public Form1()
        {
            InitializeComponent();
            v = new int[NMAX];
            x = new double[NMAX];
            y = new double[NMAX];

            /* Задание границ области вывода по умолчанию */
            Xmin = 0.2; Xmax = 8.2; Ymin = 0.5; Ymax = 6.5;

            x[0] = 5; x[1] = 9; x[2] = 14; x[3] = 19; x[4] = 23; x[5] = 27; x[6] = 24; x[7] = 21;
            x[8] = 18; x[9] = 18; x[10] = 23; x[11] = 17; x[12] = 15; x[13] = 13; x[14] = 11; x[15] = 5;
            x[16] = 10; x[17] = 10; x[18] = 7; x[19] = 4; x[20] = 1;
          
            y[0] = 7; y[1] = 3; y[2] = 1; y[3] = 3; y[4] = 7; y[5] = 13; y[6] = 18; y[7] = 22;
            y[8] = 18; y[9] = 14; y[10] = 14; y[11] = 9; y[12] = 6 ; y[13] = 6; y[14] = 9; y[15] = 14;
            y[16] = 14; y[17] = 18; y[18] = 22; y[19] = 18; y[20] = 13;
     
            dc = pictureBox1.CreateGraphics();
            p = new Pen(Brushes.Red, 1);
            // Создание динамического ComboBox
            label1 = new Label()
            {
                Location = new Point(21, 10),
                Text = "Задайте число вершин",
                Width = 151
            };

            label2 = new Label()
            {
                Location = new Point(21, 70),
                Text = "Задайте координаты вершин",
                Width = 171
            };
            
            tbComboBox1 = new ComboBox()
            {
                Location = new Point(61, 41),
                Width = 121,
                Height = 21
            };

            panel1.Controls.Add(label1);
            panel1.Controls.Add(tbComboBox1);
            panel1.Controls.Add(label2);
            for (int i = 21; i < NMAX; i++) { tbComboBox1.Items.Add(i); }
            tbComboBox1.SelectedItem = 21;
            
            // Создание динамических TextBox
            Create(iRows, iColumns);
        }

        // Создаёт динамически TextBoxы для ввода координат вершин полигона
        public void Create(int rows, int columns)
        {
            tbArray = new TextBox[rows, columns]; int y1 = 70;
            for (int i = 0; i < rows; i++)
            {
                int x1 = 30; y1 += bh - 10;
                for (int j = 0; j < columns; j++)
                {
                    // Если кол-во текст боксов не четное
                    if (j + i *columns == iRows * iColumns - 2 && int.Parse(tbComboBox1.SelectedItem.ToString())%2 != 0)
                        break;
                    
                    tbArray[i, j] = new TextBox(); tbArray[i, j].Name = "TextBox" + i + j;
                    panel1.Controls.Add(tbArray[i, j]);
                    tbArray[i, j].SetBounds(x1, y1, bw, bh);
                    x1 += bw;

                    if (j % 2 == 0)
                        tbArray[i, j].Text = x[(j/2) + i * (columns / 2)].ToString("R");
                    else
                        tbArray[i, j].Text = y[(j/2)+ i * (columns / 2)].ToString("R");
                }

            }
        }

        /* Метод преобразования вещественной координаты X в целую */
        private int IX(double x)
        { double xx = x * (pictureBox1.Size.Width / 10.0) + 0.5; return (int)xx; }
        
        /* Метод преобразования вещественной координаты Y в целую */
        private int IY(double y)
        {
            double yy = pictureBox1.Size.Height - y * (pictureBox1.Size.Height / 7.0) + 0.5;
            return (int)yy;
        }
        
        /* Своя функция вычечивания линии (экран 10х7 условных единиц) */
        private void Draw(double x1, double y1, double x2, double y2)
        {
            Point point1 = new Point(IX(x1), IY(y1)); Point point2 = new Point(IX(x2), IY(y2));
            dc.DrawLine(p, point1, point2);
        }
        
        private unsafe bool counter_clock(int h, int i, int j, double* pdist)
        {
            double xh = x[v[h]], xi = x[v[i]], xj = x[v[j]],
            yh = y[v[h]], yi = y[v[i]], yj = y[v[j]],
            x_hi, y_hi, x_hj, y_hj, Determ;
            x_hi = xi - xh; y_hi = yi - yh; x_hj = xj - xh; y_hj = yj - yh;
            *pdist = x_hj * x_hj + y_hj * y_hj;
            Determ = x_hi * y_hj - x_hj * y_hi;
            return (Determ > 1e-6);
        }

        private void draw_polygon()
        {
            int i; double xold, yold;
            xold = x[n - 1]; yold = y[n - 1];
            for (i = 0; i < n; i++)
            {
                Draw(xold, yold, x[i], y[i]); xold = x[i]; yold = y[i];
            }
        }
        
        /* Основная программа */
        private unsafe void button1_Click(object sender, EventArgs e)
        {
            int i, h, j, m, k, imin = 0; double diag, min_diag;
            n = Convert.ToInt16(tbComboBox1.SelectedItem.ToString());
            if (n >= NMAX)
            {
                MessageBox.Show("Количество вершин слишком велико!", "Ошибка!",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            // Считываем все текст боксы
            for (i = 0; i < iRows; i++)
            {
                for (j = 0; j < iColumns; j++)
                {
                    if (j + i * iColumns == iRows * iColumns - 2 && int.Parse(tbComboBox1.SelectedItem.ToString()) % 2 != 0)
                        break;

                    if (j % 2 == 0)
                        x[(j / 2) + i * (iColumns / 2)] = int.Parse(tbArray[i, j].Text);
                    else
                        y[(j / 2) + i * (iColumns / 2)] = int.Parse(tbArray[i, j].Text);

                    v[j + i * iColumns] = j + i * iColumns;
                }

            }

            xmin = x[0];
            ymin = y[0];

            // Ищим максимум минимум
            for (i = 0; i < iRows; i++)
            {
                for (j = 0;j < iColumns/2; j++)
                {
                    if (j + i * iColumns == iRows * iColumns - 2 && int.Parse(tbComboBox1.SelectedItem.ToString()) % 2 != 0)
                        break;

                    if (x[j + i * (iColumns / 2)] < xmin) xmin = x[j + i * (iColumns / 2)];
                    if (x[j + i * (iColumns / 2)] > xmax) xmax = x[j + i * (iColumns / 2)];
                    if (y[j + i * (iColumns / 2)] < ymin) ymin = y[j + i * (iColumns / 2)];
                    if (y[j + i * (iColumns / 2)] > ymax) ymax = y[j + i * (iColumns / 2)];
                }
            }

            /* Получение коэффициентов формулы перевода мировых
            координат в экранные */
            fx = (Xmax - Xmin) / (xmax - xmin);
            fy = (Ymax - Ymin) / (ymax - ymin);
            f = (fx < fy ? fx : fy);
            xC = 0.5 * (xmin + xmax); yC = 0.5 * (ymin + ymax);
            XC = 0.5 * (Xmin + Xmax); YC = 0.5 * (Ymin + Ymax);
            c1 = XC - f * xC;
            c2 = YC - f * yC;

            // Масштабируем координаты
            for (i = 0; i < iRows; i++)
            {
                for (j = 0; j < iColumns / 2; j++)
                {
                    x[j + i * (iColumns / 2)] = f * x[j + i * (iColumns / 2)] + c1;
                    y[j + i * (iColumns / 2)] = f * y[j + i * (iColumns / 2)] + c2;
                }
            }

            m = n;
            draw_polygon();

            // Задаём штриховую линию (длина штриха, промежуток, длина штриха, промежуток)
            float[] dashValues = { 5, 5, 5, 5 }; p.DashPattern = dashValues;
            while (m > 3)
            {
                min_diag = BIG;
                for (i = 0; i < m; i++)
                {
                    h = (i == 0 ? m - 1 : i - 1); j = (i == m - 1 ? 0 : i + 1);
                    if (counter_clock(h, i, j, &diag) && (diag < min_diag))
                    {
                        min_diag = diag; imin = i;
                    }
                }
                i = imin; h = (i == 0 ? m - 1 : i - 1); j = (i == m - 1 ? 0 : i + 1);
                if (min_diag == BIG)
                {
                    MessageBox.Show("Неправильное направление обхода!", "Ошибка!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                Draw(x[v[h]], y[v[h]], x[v[j]], y[v[j]]);
                m--;
                for (k = i; k < m; k++) v[k] = v[k + 1];
            }
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }
    }
}