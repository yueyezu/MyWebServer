using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MyWebServer
{
    public partial class MismonMapServer : Form
    {
        private MyWebLibrary server;
        private string addr = "http://localhost:887";
        private string folder = AppDomain.CurrentDomain.BaseDirectory + "Maps\\";
        private string keyName = "mismonMapServer";

        public MismonMapServer()
        {
            InitializeComponent();

            try
            {
                server = new MyWebLibrary();
                server.Begin(addr, folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (!GetIsHaveKey(keyName))
            {
                //if (MessageBox.Show("设置开机启动项？", "观测网本地地图服务器（提示）", MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                //        MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                //{
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue(keyName, path);
                rk2.Close();
                rk.Close();
                //}
            }


        }

        #region 服务相关的操作

        /// <summary>
        /// 启动服务器按钮的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (server.IsRun())
            {
                return;
            }
            server.Begin(addr, folder);

            label1.Text = "服务器运行中...";
            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
            Process.Start("explorer.exe", "http://localhost:887/index.html");
        }

        /// <summary>
        /// 关闭服务器按钮的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopServer_Click(object sender, EventArgs e)
        {
            if (server.IsRun())
            {
                server.Stop();
            }

            label1.Text = "服务器运行停止";
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
        }

        /// <summary>
        /// 开机启动切换时的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxStartRun_CheckStateChanged(object sender, EventArgs e)
        {
            if (cbxStartRun.Checked) //设置开机自启动  
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue(keyName, path);
                rk2.Close();
                rk.Close();
            }
            else //取消开机自启动
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue(keyName, false);
                rk2.Close();
                rk.Close();
            }
        }

        public static bool isfirstShow = true;

        /// <summary>
        /// 检测是否已经添加到了开机启动中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MismonMapServer_Shown(object sender, EventArgs e)
        {
            if (GetIsHaveKey(keyName))
            {
                cbxStartRun.Checked = true;
            }

            if (isfirstShow)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Opacity = 1;
                isfirstShow = false;
            }
        }

        /// <summary>
        /// 获取是否已经添加到了开机启动项
        /// </summary>
        /// <returns></returns>
        private bool GetIsHaveKey(string name)
        {
            bool result = false;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            string[] subkeyNames = rk2.GetValueNames();
            foreach (string subkey in subkeyNames)
            {
                if (subkey == name)
                {
                    result = true;
                    break;
                }
            }
            rk2.Close();
            rk.Close();
            return result;
        }

        #endregion

        #region 托盘区操作

        /// <summary>
        /// 当托盘区图标双击时还原主窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal; //还原窗体   //这个必须要放在下一句的前边
                ShowInTaskbar = true; //显示在系统任务栏
                notifyIcon1.Visible = false; //托盘图标隐藏
            }
        }

        private void 打开主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal; //还原窗体   //这个必须要放在下一句的前边
                ShowInTaskbar = true; //显示在系统任务栏
                notifyIcon1.Visible = false; //托盘图标隐藏
            }
        }

        private void 关闭程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Dispose();
            server.Stop();
            Environment.Exit(0);
        }

        #endregion

        /// <summary>
        /// 当点击最小化时，最小化到托盘区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MismonMapServer_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)  //判断是否最小化
            {
                notifyIcon1.Visible = true;  //托盘图标可见
                notifyIcon1.ShowBalloonTip(1000, "提示", "观测网服务器运行中。。。", ToolTipIcon.Info);
                ShowInTaskbar = false;  //不显示在系统任务栏
                UseTools.SetFormToolWindowStyle(this);  //不出现在alt+tab中
            }
        }

        private void MismonMapServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (MessageBox.Show("确定退出系统？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    this.WindowState = FormWindowState.Minimized;
                    e.Cancel = true;
                }
            }
        }
        /// <summary>
        /// 窗口关闭时，清理线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MismonMapServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Dispose();
            server.Stop();
            Environment.Exit(0);
        }


        /// <summary>
        /// 测试安装的本地服务器是否正常运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", "http://localhost:887/index.html");
        }
    }
}