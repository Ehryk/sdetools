using System;
using System.Windows.Forms;

namespace PGE.ArcFM.Common.Wrappers
{
    public class WindowWrapper : IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }
        public WindowWrapper(int handle)
        {
            Handle = new IntPtr(handle);
        }
    }

}
