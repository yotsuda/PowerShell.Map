using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShell.Map.Helpers;

public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private static readonly string[] BrowserNames =
    [
        "Google Chrome",
        "Microsoft Edge",
        "Mozilla Firefox",
        "Opera",
        "Brave",
        "Safari"
    ];

    public static bool IsBrowserTabOpen(string windowTitle)
    {
        bool found = false;
        
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            var title = new StringBuilder(256);
            GetWindowText(hWnd, title, title.Capacity);
            var currentTitle = title.ToString();
            
            // Check exact match or browser pattern "PowerShell.Map - BrowserName"
            if (currentTitle.Equals(windowTitle, StringComparison.OrdinalIgnoreCase) ||
                BrowserNames.Any(browser => currentTitle.Equals($"{windowTitle} - {browser}", StringComparison.OrdinalIgnoreCase)))
            {
                found = true;
                return false;
            }
            
            return true;
        }, IntPtr.Zero);
        
        return found;
    }
}