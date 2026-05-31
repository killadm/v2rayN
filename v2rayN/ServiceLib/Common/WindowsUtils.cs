using Microsoft.Win32;

namespace ServiceLib.Common;

[SupportedOSPlatform("windows")]
internal static class WindowsUtils
{
    private static readonly string _tag = "WindowsUtils";

    public static string? RegReadValue(string path, string name, string def)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.OpenSubKey(path, false);
            var value = regKey?.GetValue(name) as string;
            return value.IsNullOrEmpty() ? def : value;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
        return def;
    }

    public static void RegWriteValue(string path, string name, object value)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.CreateSubKey(path);
            if (value.ToString().IsNullOrEmpty())
            {
                regKey?.DeleteValue(name, false);
            }
            else
            {
                regKey?.SetValue(name, value);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
    }

    public static async Task RemoveTunDevice()
    {
        try
        {
            var sum = MD5.HashData(Encoding.UTF8.GetBytes("wintunsingbox_tun"));
            var guid = new Guid(sum);
            var pnpUtilPath = @"C:\Windows\System32\pnputil.exe";
            var arg = $$""" /remove-device  "SWD\Wintun\{{{guid}}}" """;

            // Try to remove the device
            _ = await Utils.GetCliWrapOutput(pnpUtilPath, arg);
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    public static string GetFreeEthernetName()
    {
        try
        {
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var usedNames = new HashSet<string>(interfaces.Select(i => i.Name));

            string baseName = "Ethernet";
            if (System.Globalization.CultureInfo.InstalledUICulture.Name.StartsWith("zh"))
            {
                baseName = "以太网";
            }

            if (!usedNames.Contains(baseName))
            {
                return baseName;
            }

            int i = 2;
            while (true)
            {
                string name = $"{baseName} {i}";
                if (!usedNames.Contains(name))
                {
                    return name;
                }
                i++;
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return "singbox_tun";
        }
    }
}
