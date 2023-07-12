using System;
using System.Threading;
using System.Windows.Forms;

namespace MyWebServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (TestAlreadyRunning())
            {
                MessageBox.Show("程序已经启动！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MismonMapServer());
            }
        }

        private static bool TestAlreadyRunning()
        {
            string mutexName = "myOneApp";
            Mutex mutex;
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
                if (mutex != null)
                {
                    return true;
                }
            }
            catch { }
            finally
            {
                mutex = new Mutex(true, mutexName);
            }
            return false;
        }
    }
}
