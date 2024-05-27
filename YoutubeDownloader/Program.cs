using System;
using System.Windows.Forms;

namespace YoutubeDownloader
{
    static class Program
    {
        [STAThread] //necessario essa marcacao para não ficar aguardando eventos
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}