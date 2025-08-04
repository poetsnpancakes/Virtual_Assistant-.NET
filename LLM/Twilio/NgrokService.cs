using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Virtual_Assistant.LLM.Twilio
{
    public static class NgrokService
    {
        public static string PublicUrl { get; private set; } = string.Empty;
        private static Process? _ngrokProcess;

        public static void StartNgrok(int port)
        {
            try
            {
                // Kill all existing ngrok processes
                foreach (var process in Process.GetProcessesByName("ngrok"))
                {
                    try { process.Kill(true); process.WaitForExit(); }
                    catch { /* Ignore failures */ }
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ngrok.exe", // or full path
                    Arguments = $"http {port} --log=stdout",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _ngrokProcess = Process.Start(startInfo);
                if (_ngrokProcess == null)
                    throw new Exception("Failed to start ngrok process.");

                // Read stdout to get public URL
                Task.Run(() =>
                {
                    using var reader = _ngrokProcess.StandardOutput;
                    string? line;
                    var timeout = DateTime.Now.AddSeconds(10);
                    while ((line = reader.ReadLine()) != null && DateTime.Now < timeout)
                    {
                        var match = Regex.Match(line, @"url=(https://[^\s]+)");
                        if (match.Success)
                        {
                            PublicUrl = match.Groups[1].Value;
                            Console.WriteLine("🌐 Ngrok URL: " + PublicUrl);
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting ngrok: " + ex.Message);
            }
        }

        public static void StopNgrok()
        {
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    Console.WriteLine("Killing ngrok process...");
                    _ngrokProcess.Kill(true);
                    _ngrokProcess.WaitForExit();
                    Console.WriteLine("Ngrok stopped.");
                }
                else
                {
                    Console.WriteLine("No tracked ngrok process to kill. Killing all fallback.");
                    foreach (var process in Process.GetProcessesByName("ngrok"))
                    {
                        try { process.Kill(true); process.WaitForExit(); }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping ngrok: " + ex.Message);
            }
        }
    }


}
