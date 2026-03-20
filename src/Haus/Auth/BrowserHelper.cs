using System.Diagnostics;

namespace Haus.Auth;

public static class BrowserHelper
{
    public static void Open(string url)
    {
        if (OperatingSystem.IsLinux())
            Process.Start(new ProcessStartInfo("xdg-open", url) { UseShellExecute = false });
        else if (OperatingSystem.IsMacOS())
            Process.Start(new ProcessStartInfo("open", url) { UseShellExecute = false });
        else if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
