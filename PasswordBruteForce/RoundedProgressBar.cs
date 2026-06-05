using System.Drawing.Drawing2D;

namespace PasswordBruteForce;

public class RoundedProgressBar : ProgressBar
{
    public int CornerRadius { get; set; } = 8;
    public Color BarColor { get; set; } = Color.FromArgb(130, 80, 220);

    public RoundedProgressBar()
    {
        SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var rect = new Rectangle(0, 0, Width, Height);
        double pct = (double)Value / Maximum;
        int fillW = (int)(rect.Width * pct);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var bgBrush = new SolidBrush(BackColor);
        FillRounded(e.Graphics, bgBrush, rect, CornerRadius);

        if (fillW > 0)
        {
            var fill = new Rectangle(0, 0, fillW, Height);
            using var fillBrush = new SolidBrush(BarColor);
            FillRounded(e.Graphics, fillBrush, fill, CornerRadius);
        }
    }

    private static void FillRounded(Graphics g, Brush brush, Rectangle r, int radius)
    {
        using var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}