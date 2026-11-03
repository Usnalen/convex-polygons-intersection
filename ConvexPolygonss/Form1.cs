
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ConvexPolygonss
{
    public partial class Form1 : Form
    {
        List<PointF> polygon1 = new List<PointF>();
        List<PointF> polygon2 = new List<PointF>();
        List<PointF> intersection = new List<PointF>();

        int currentPolygon = 1;
        bool finished = false;

        Panel canvas = new Panel();
        Button btnFinish = new Button();
        Button btnIntersect = new Button();
        Button btnClear = new Button();

        public Form1()
        {
            InitializeComponent();

            Text = "Пересечение выпуклых полигонов";
            Width = 900;
            Height = 650;

            btnFinish.Text = "Завершить полигон";
            btnFinish.SetBounds(10, 10, 180, 35);
            btnFinish.Click += FinishPolygon;

            btnIntersect.Text = "Найти пересечение";
            btnIntersect.SetBounds(200, 10, 180, 35);
            btnIntersect.Click += FindIntersection;

            btnClear.Text = "Очистить";
            btnClear.SetBounds(390, 10, 120, 35);
            btnClear.Click += ClearAll;

            canvas.SetBounds(10, 60, 860, 530);
            canvas.BackColor = Color.White;
            canvas.BorderStyle = BorderStyle.FixedSingle;
            canvas.MouseClick += CanvasClick;
            canvas.Paint += CanvasPaint;

            Controls.Add(btnFinish);
            Controls.Add(btnIntersect);
            Controls.Add(btnClear);
            Controls.Add(canvas);
        }

        private void CanvasClick(object sender, MouseEventArgs e)
        {
            if (finished) return;

            if (currentPolygon == 1)
                polygon1.Add(e.Location);
            else
                polygon2.Add(e.Location);

            intersection.Clear();
            canvas.Invalidate();
        }

        private void FinishPolygon(object sender, EventArgs e)
        {
            if (currentPolygon == 1)
            {
                if (polygon1.Count < 3)
                {
                    MessageBox.Show("Нужно минимум 3 точки");
                    return;
                }
                currentPolygon = 2;
                MessageBox.Show("Теперь рисуйте второй полигон");
            }
            else
            {
                if (polygon2.Count < 3)
                {
                    MessageBox.Show("Нужно минимум 3 точки");
                    return;
                }
                finished = true;
                MessageBox.Show("Оба полигона готовы");
            }

            canvas.Invalidate();
        }

        private void CanvasPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            DrawPolygon(g, polygon1, Color.Blue);
            DrawPolygon(g, polygon2, Color.Red);

            if (intersection.Count >= 3)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(100, Color.Green)))
                    g.FillPolygon(brush, intersection.ToArray());

                g.DrawPolygon(Pens.Green, intersection.ToArray());
            }
        }

        private void DrawPolygon(Graphics g, List<PointF> points, Color color)
        {
            using (Pen pen = new Pen(color, 2))
            {
                for (int i = 0; i < points.Count - 1; i++)
                    g.DrawLine(pen, points[i], points[i + 1]);

                if (points.Count >= 3)
                    g.DrawLine(pen, points[points.Count - 1], points[0]);
            }

            foreach (PointF p in points)
                g.FillEllipse(Brushes.Black, p.X - 3, p.Y - 3, 6, 6);
        }

        private void FindIntersection(object sender, EventArgs e)
        {
            if (!finished)
            {
                MessageBox.Show("Сначала завершите оба полигона");
                return;
            }

            intersection = ClipPolygon(polygon1, polygon2);
            canvas.Invalidate();

            if (intersection.Count < 3)
                MessageBox.Show("Общей области нет");
        }

        private List<PointF> ClipPolygon(List<PointF> subject, List<PointF> clip)
        {
            List<PointF> result = new List<PointF>(subject);

            float area = 0;
            for (int i = 0; i < clip.Count; i++)
            {
                PointF a = clip[i];
                PointF b = clip[(i + 1) % clip.Count];
                area += a.X * b.Y - b.X * a.Y;
            }

            bool counterClockwise = area > 0;

            for (int i = 0; i < clip.Count; i++)
            {
                PointF a = clip[i];
                PointF b = clip[(i + 1) % clip.Count];

                List<PointF> input = new List<PointF>(result);
                result.Clear();

                if (input.Count == 0)
                    break;

                PointF previous = input[input.Count - 1];

                foreach (PointF current in input)
                {
                    bool currentInside = Inside(current, a, b, counterClockwise);
                    bool previousInside = Inside(previous, a, b, counterClockwise);

                    if (currentInside)
                    {
                        if (!previousInside)
                            result.Add(GetIntersection(previous, current, a, b));

                        result.Add(current);
                    }
                    else if (previousInside)
                    {
                        result.Add(GetIntersection(previous, current, a, b));
                    }

                    previous = current;
                }
            }

            return result;
        }

        private bool Inside(PointF p, PointF a, PointF b, bool ccw)
        {
            float cross = (b.X - a.X) * (p.Y - a.Y)
                        - (b.Y - a.Y) * (p.X - a.X);

            return ccw ? cross >= -0.0001f : cross <= 0.0001f;
        }

        private PointF GetIntersection(PointF p1, PointF p2, PointF a, PointF b)
        {
            float dx1 = p2.X - p1.X;
            float dy1 = p2.Y - p1.Y;
            float dx2 = b.X - a.X;
            float dy2 = b.Y - a.Y;

            float denominator = dx1 * dy2 - dy1 * dx2;

            if (Math.Abs(denominator) < 0.000001f)
                return p2;

            float t = ((a.X - p1.X) * dy2 - (a.Y - p1.Y) * dx2) / denominator;

            return new PointF(p1.X + t * dx1, p1.Y + t * dy1);
        }

        private void ClearAll(object sender, EventArgs e)
        {
            polygon1.Clear();
            polygon2.Clear();
            intersection.Clear();

            currentPolygon = 1;
            finished = false;

            canvas.Invalidate();
        }
    }
}

