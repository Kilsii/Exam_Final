using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Forms;

namespace PasswordBruteForce;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}