using System;
using System.Diagnostics;

namespace DidiApp.Services;

public static class NotificationService
{
    public static void SendMacNotification(string title, string message)
    {
        if (!OperatingSystem.IsMacOS()) return;
        try
        {
            var escapedTitle = title.Replace("\"", "\\\"");
            var escapedMessage = message.Replace("\"", "\\\"");
            Process.Start(new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display notification \"{escapedMessage}\" with title \"{escapedTitle}\"'",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch(Exception e)
        {
            //read logs
            Console.WriteLine(e.Message);
            // to implement later
        }
    }
}
