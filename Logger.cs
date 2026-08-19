using System.IO;

namespace FolhaSienge;

public static class Logger
{
    private static readonly string LogDir = Path.Combine(Path.GetTempPath(), "FolhaSienge");
    private static readonly string LogFile = Path.Combine(LogDir, "log.txt");

    public static void Log(string msg)
    {
        try
        {
            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);
            File.AppendAllText(LogFile,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + msg + "\r\n");
        }
        catch { }
    }

    public static void LogErro(string contexto, Exception ex)
    {
        Log($"[ERRO] {contexto}: {ex.Message}");
        if (ex.InnerException != null)
            Log($"[ERRO] Inner: {ex.InnerException.Message}");
        Log($"[ERRO] Stack: {ex.StackTrace}");
    }

    public static string ObterLog()
    {
        try { return File.Exists(LogFile) ? File.ReadAllText(LogFile) : "(sem log)"; }
        catch { return "(erro ao ler log)"; }
    }

    public static void Limpar()
    {
        try { if (File.Exists(LogFile)) File.Delete(LogFile); } catch { }
    }
}
