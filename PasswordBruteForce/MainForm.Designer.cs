using System.ComponentModel;

namespace PasswordBruteForce;

partial class MainForm
{
    private IContainer components = null!;

    private Panel pnlGenerator = null!;
    private Panel pnlCracker = null!;
    private Panel pnlResult = null!;
    private Panel pnlPerformance = null!;

    private Label lblGenTitle = null!;
    private Button btnGenerate = null!;
    private TextBox txtPassword = null!;
    private Label lblHashLabel = null!;
    private TextBox txtHash = null!;

    private Label lblCrackerTitle = null!;
    private Label lblThreadInfo = null!;
    private Label lblModeLabel = null!;
    private RadioButton radSingle = null!;
    private RadioButton radMulti = null!;
    private RadioButton radCompare = null!;
    private Button btnStart = null!;
    private Button btnStop = null!;
    private RoundedProgressBar pbProgress = null!;
    private Label lblChecked = null!;
    private Label lblElapsed = null!;
    private Label lblStatus = null!;

    private Label lblResultTitle = null!;
    private Label lblHurray = null!;
    private Label lblFoundLabel = null!;
    private TextBox txtFound = null!;

    private Label lblPerfTitle = null!;
    private RichTextBox rtbLog = null!;
    private Button btnClearLog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        SuspendLayout();

        //  Colours
        var darkBg = Color.FromArgb(28, 28, 35);      
        var cardBg = Color.FromArgb(37, 37, 52);       
        var accentPurple = Color.FromArgb(130, 80, 220);
        var accentGreen = Color.FromArgb(50, 200, 120);
        var accentRed = Color.FromArgb(220, 60, 80);
        var textLight = Color.FromArgb(255, 255, 255);    
        var textDim = Color.FromArgb(140, 140, 170);
        var inputBg = Color.FromArgb(45, 45, 62);

        // Form 
        Text = "Kilz Password Cracker";
        ClientSize = new Size(740, 745);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        BackColor = darkBg;

      
        // PANEL 1 — PASSWORD GENERATOR 
       
        pnlGenerator = MkPanel(10, 10, 720, 125, cardBg);

        lblGenTitle = new Label
        {
            Text = "PASSWORD GENERATOR:",
            Location = new Point(15, 12),
            Size = new Size(350, 22),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = textLight,
            BackColor = cardBg
        };

        btnGenerate = new Button
        {
            Text = "GENERATE",
            Location = new Point(15, 52),
            Size = new Size(120, 40),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = accentPurple,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += btnGenerate_Click;

        txtPassword = new TextBox
        {
            Location = new Point(145, 55),
            Size = new Size(200, 35),
            ReadOnly = true,
            BackColor = inputBg,
            Font = new Font("Consolas", 11F, FontStyle.Bold),
            ForeColor = textLight,              // white
            BorderStyle = BorderStyle.FixedSingle
        };

        lblHashLabel = new Label
        {
            Text = "HASH :",
            Location = new Point(365, 52),
            Size = new Size(75, 40),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = accentPurple,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };

        txtHash = new TextBox
        {
            Location = new Point(450, 55),
            Size = new Size(255, 35),
            ReadOnly = true,
            BackColor = inputBg,
            Font = new Font("Consolas", 7.5F),
            ForeColor = textDim,
            ScrollBars = ScrollBars.Horizontal,
            BorderStyle = BorderStyle.FixedSingle
        };

        pnlGenerator.Controls.AddRange(new Control[]
            { lblGenTitle, btnGenerate, txtPassword, lblHashLabel, txtHash });

        
        // PANEL 2 — PASSWORD CRACKER
        
        pnlCracker = MkPanel(10, 145, 720, 215, cardBg);

        lblCrackerTitle = new Label
        {
            Text = "PASSWORD CRACKER:",
            Location = new Point(15, 12),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = textLight,
            BackColor = cardBg
        };

        lblThreadInfo = new Label
        {
            Text = "",
            Location = new Point(300, 14),
            Size = new Size(405, 17),
            Font = new Font("Segoe UI", 8F),
            ForeColor = textDim,
            BackColor = cardBg,
            TextAlign = ContentAlignment.MiddleRight
        };

        lblModeLabel = new Label
        {
            Text = "Select attack mode:",
            Location = new Point(15, 50),
            Size = new Size(148, 23),
            Font = new Font("Segoe UI", 9F),
            ForeColor = textLight,
            BackColor = cardBg
        };

        radSingle = MkRadio("Single thread", 168, 50, 115, textLight, cardBg);
        radMulti = MkRadio("Multi thread", 293, 50, 115, textLight, cardBg);
        radCompare = MkRadio("Compare", 418, 50, 100, textLight, cardBg);
        radMulti.Checked = true;

        btnStart = new Button
        {
            Text = "Start  ▶",
            Location = new Point(15, 90),
            Size = new Size(130, 45),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = accentGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.Click += btnStart_Click;

        btnStop = new Button
        {
            Text = "Stop  ⊗",
            Location = new Point(15, 148),
            Size = new Size(130, 45),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = accentRed,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnStop.FlatAppearance.BorderSize = 0;
        btnStop.Click += btnStop_Click;

        
        pbProgress = new RoundedProgressBar
        {
            Location = new Point(175, 97),
            Size = new Size(350, 32),
            Maximum = 10000,
            BackColor = inputBg,
            BarColor = accentGreen,
            CornerRadius = 8
        };

        lblChecked = new Label
        {
            Text = "Checked: 0 / 0   |   Length: — / 6",
            Location = new Point(175, 137),
            Size = new Size(510, 17),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = textLight,
            BackColor = cardBg
        };

        lblElapsed = new Label
        {
            Text = "Time Elapsed: 0.00s",
            Location = new Point(175, 158),
            Size = new Size(300, 17),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = textLight,
            BackColor = cardBg
        };

        lblStatus = new Label
        {
            Text = "Ready.",
            Location = new Point(175, 179),
            Size = new Size(510, 17),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = textDim,
            BackColor = cardBg
        };

        pnlCracker.Controls.AddRange(new Control[]
        {
            lblCrackerTitle, lblThreadInfo, lblModeLabel,
            radSingle, radMulti, radCompare,
            btnStart, btnStop, pbProgress,
            lblChecked, lblElapsed, lblStatus
        });

        // PANEL 3 — RESULT
        
        pnlResult = MkPanel(10, 370, 720, 155, cardBg);

        lblResultTitle = new Label
        {
            Text = "RESULT:",
            Location = new Point(15, 12),
            Size = new Size(200, 22),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = textLight,
            BackColor = cardBg
        };

        lblHurray = new Label
        {
            Text = "Hurray! Password has been cracked successfully!",
            Location = new Point(15, 45),
            Size = new Size(690, 22),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = accentGreen,
            BackColor = cardBg,
            Visible = false
        };

        lblFoundLabel = new Label
        {
            Text = "Password:",
            Location = new Point(15, 95),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = accentPurple,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };

        txtFound = new TextBox
        {
            Location = new Point(125, 97),
            Size = new Size(580, 35),
            ReadOnly = true,
            BackColor = inputBg,
            Font = new Font("Consolas", 13F, FontStyle.Bold),
            ForeColor = textLight,            
            TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.FixedSingle
        };

        pnlResult.Controls.AddRange(new Control[]
            { lblResultTitle, lblHurray, lblFoundLabel, txtFound });

        // PANEL 4 — PERFORMANCE  (reduced)
       
        pnlPerformance = MkPanel(10, 535, 720, 195, cardBg);

        lblPerfTitle = new Label
        {
            Text = "Performance:",
            Location = new Point(15, 12),
            Size = new Size(200, 22),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = textLight,
            BackColor = cardBg
        };

        rtbLog = new RichTextBox
        {
            Location = new Point(15, 42),
            Size = new Size(690, 118),
            ReadOnly = true,
            BackColor = darkBg,
            ForeColor = Color.FromArgb(0, 180, 80),
            Font = new Font("Consolas", 9.5F),
            ScrollBars = RichTextBoxScrollBars.Vertical,
            BorderStyle = BorderStyle.None
        };

        btnClearLog = new Button
        {
            Text = "Clear",
            Location = new Point(630, 163),
            Size = new Size(75, 25),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5F),
            BackColor = accentPurple,
            ForeColor = Color.White
        };
        btnClearLog.FlatAppearance.BorderSize = 0;
        btnClearLog.Click += btnClearLog_Click;

        pnlPerformance.Controls.AddRange(new Control[]
            { lblPerfTitle, rtbLog, btnClearLog });

        Controls.AddRange(new Control[]
            { pnlGenerator, pnlCracker, pnlResult, pnlPerformance });

        ResumeLayout(false);
    }

    private static Panel MkPanel(int x, int y, int w, int h, Color backColor) =>
        new Panel
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            BackColor = backColor
        };

    private static RadioButton MkRadio(string text, int x, int y, int w,
        Color foreColor, Color backColor) =>
        new RadioButton
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(w, 23),
            Font = new Font("Segoe UI", 9F),
            ForeColor = foreColor,
            BackColor = backColor
        };
}