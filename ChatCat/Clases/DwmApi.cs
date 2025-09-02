using System.Runtime.InteropServices;

public static class DwmApi
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    // DWMWA_USE_IMMERSIVE_DARK_MODE attribute value
    // Use 19 for Windows versions before 20H1 (18985)
    // Use 20 for Windows versions 20H1 (18985) and later
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) // Windows 10 October 2018 Update or later
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985)) // Windows 10 20H1 Update or later
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }
        return false;
    }
}