using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MK3MoveBook.WebLauncher
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string webDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "web"
            );
            string indexPath = Path.Combine(webDirectory, "index.html");
            if (!File.Exists(indexPath))
            {
                MessageBox.Show(
                    "Не найден файл веб-версии:\n" + indexPath +
                    "\n\nНе переносите этот файл отдельно от папки web.",
                    "MK3 MoveBook — веб-версия",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = indexPath;
                startInfo.WorkingDirectory = webDirectory;
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось открыть веб-версию в браузере.\n\n" +
                    exception.Message,
                    "MK3 MoveBook — веб-версия",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
