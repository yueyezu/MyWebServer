/*********************************************************************************
 * 机器名称：YUEYEZ-PC
 * 公司名称：励图高科
 * 命名空间：MyWebServer
 * 文件名：  UseTools
 * 创建人：  胡勇超
 * 创建时间：2015/7/24 7:56:43
 * 描述：
 ********************************************************************************/
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyWebServer
{
    public class UseTools
    {
        #region 修改窗口样式，使之不再alt+tab中出现

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowLong(IntPtr hwnd, Int32 index);

        [DllImport("user32.dll")]
        private static extern Int32 SetWindowLong(IntPtr hwnd, Int32 index, Int32 newValue);

        private const int GWL_EXSTYLE = (-20);

        private static void AddWindowExStyle(IntPtr hwnd, Int32 val)
        {
            int oldValue = GetWindowLong(hwnd, GWL_EXSTYLE);
            if (oldValue == 0)
            {
                throw new Win32Exception();
            }
            if (0 == SetWindowLong(hwnd, GWL_EXSTYLE, oldValue | val))
            {
                throw new Win32Exception();
            }
        }
        private static int WS_EX_TOOLWINDOW = 0x00000080;
        //我把这个过程封装下：
        public static void SetFormToolWindowStyle(Form form)
        {
            AddWindowExStyle(form.Handle, WS_EX_TOOLWINDOW);
        }

        #endregion
    }
}
