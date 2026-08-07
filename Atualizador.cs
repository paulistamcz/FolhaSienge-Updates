using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace FolhaSienge;

/// <summary>
/// Controle de versão e auto-atualização via GitHub Releases.
/// O app consulta o repositório público FolhaSienge-Updates, compara a versão
/// do último Release com a versão atual e, se for mais nova, baixa o pacote
/// (FolhaSienge_<versão>.zip), substitui os arquivos e reabre o app.
/// </summary>
public static class Atualizador
{
    public const string Owner = "paulistamcz";
    public const string RepoAtualizacoes = "FolhaSienge-Updates";

    public static readonly string ApiUrl =
        $"https://api.github.com/repos/{Owner}/{RepoAtualizacoes}/releases/latest";

    /// <summary>Versão atual do executável (ex.: 1.0.0).</summary>
    public static string VersaoAtual =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    /// <summary>Busca o último Release do repositório público de atualizações.</summary>
    public static async Task<VersaoRelease?> BuscarUltimaVersaoAsync()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("FolhaSienge");
        http.Timeout = TimeSpan.FromSeconds(10);
        try
        {
            var json = await http.GetStringAsync(ApiUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string tag = root.TryGetProperty("tag_name", out var t)
                ? t.GetString() ?? ""
                : "";

            string urlZip = "";
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var a in assets.EnumerateArray())
                {
                    if (a.TryGetProperty("name", out var n) &&
                        n.GetString()?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true &&
                        a.TryGetProperty("browser_download_url", out var u))
                    {
                        urlZip = u.GetString() ?? "";
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(tag) || string.IsNullOrWhiteSpace(urlZip))
                return null;

            return new VersaoRelease(tag.TrimStart('v'), urlZip);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>True se o Release do repositório for mais novo que a versão atual.</summary>
    public static bool TemVersaoNova(VersaoRelease? release)
    {
        if (release == null) return false;
        if (!Version.TryParse(release.Versao, out var nova)) return false;
        if (!Version.TryParse(VersaoAtual, out var atual)) return false;
        return nova > atual;
    }

    /// <summary>
    /// Baixa o pacote, extrai e dispara um script que aguarda o app fechar,
    /// substitui os arquivos e reabre na nova versão. Retorna true ao iniciar.
    /// </summary>
    public static async Task<bool> BaixarEInstalarAsync(VersaoRelease release)
    {
        string dirApp = AppContext.BaseDirectory;
        string dirUpd = Path.Combine(Path.GetTempPath(), "FolhaSienge_update");
        if (Directory.Exists(dirUpd)) Directory.Delete(dirUpd, true);
        Directory.CreateDirectory(dirUpd);

        string zip = Path.Combine(dirUpd, "pacote.zip");
        using (var http = new HttpClient())
        {
            http.DefaultRequestHeaders.UserAgent.ParseAdd("FolhaSienge");
            http.Timeout = TimeSpan.FromMinutes(5);
            using var stream = await http.GetStreamAsync(release.UrlZip);
            using var fs = new FileStream(zip, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fs);
        }

        string extraido = Path.Combine(dirUpd, "novo");
        ZipFile.ExtractToDirectory(zip, extraido);

        string exe = Path.GetFileName(Environment.ProcessPath)
                     ?? "FolhaSienge.exe";
        string bat = Path.Combine(dirApp, "atualizar.bat");
        string conteudo =
            "@echo off\r\n" +
            "timeout /t 2 /nobreak >nul\r\n" +
            $"xcopy \"{extraido}\\*\" \"{dirApp}\" /e /y /q >nul\r\n" +
            $"rd /s /q \"{extraido}\"\r\n" +
            $"del /q \"{zip}\"\r\n" +
            $"start \"\" \"{Path.Combine(dirApp, exe)}\"\r\n" +
            "del \"%~f0\"\r\n";
        File.WriteAllText(bat, conteudo, System.Text.Encoding.GetEncoding(437));

        Process.Start(new ProcessStartInfo(bat)
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
        return true;
    }
}

/// <summary>Dados de um Release de atualização.</summary>
public record VersaoRelease(string Versao, string UrlZip);
