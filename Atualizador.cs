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

            string notas = root.TryGetProperty("body", out var b)
                ? b.GetString() ?? ""
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

            return new VersaoRelease(tag.TrimStart('v'), urlZip, notas.Trim());
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
    /// Lança exceção com a descrição do problema em caso de falha.
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
        string nomeExe = Path.GetFileNameWithoutExtension(exe);
        string exeDestino = Path.Combine(dirApp, exe);
        string log = Path.Combine(dirUpd, "instalador.log");
        string bat = Path.Combine(dirApp, "atualizar.bat");
        // Script com log, espera limitada e repetição da cópia:
        // - aguarda o app fechar (máx. ~90s; evita trava se outra janela ficou aberta)
        // - repete o xcopy até 10x (arquivo pode estar travado por antivírus/bloqueio)
        // - em caso de falha, mantém o log em dirUpd em vez de falhar em silêncio
        string conteudo =
            "@echo off\r\n" +
            "setlocal\r\n" +
            $"set \"LOG={log}\"\r\n" +
            "echo [%date% %time%] Atualizador iniciado > \"%LOG%\"\r\n" +
            $"echo Aguardando {exe} fechar... >> \"%LOG%\"\r\n" +
            "set CONT=0\r\n" +
            ":espera\r\n" +
            $"tasklist /fi \"imagename eq {exe}\" 2>nul | find /i \"{nomeExe}\" >nul\r\n" +
            "if not %errorlevel%==0 goto copia\r\n" +
            "set /a CONT+=1\r\n" +
            "if %CONT% GEQ 90 goto copia\r\n" +
            "ping -n 2 127.0.0.1 >nul\r\n" +
            "goto espera\r\n" +
            ":copia\r\n" +
            "echo [%date% %time%] Copiando arquivos novos... >> \"%LOG%\"\r\n" +
            "set TENT=0\r\n" +
            ":tenta\r\n" +
            "set /a TENT+=1\r\n" +
            $"xcopy \"{extraido}\\*\" \"{dirApp}\" /e /y /q >> \"%LOG%\" 2>&1\r\n" +
            "if %errorlevel%==0 goto ok\r\n" +
            "echo [%date% %time%] Tentativa %TENT% falhou (erro %errorlevel%) >> \"%LOG%\"\r\n" +
            "if %TENT% GEQ 10 goto falha\r\n" +
            "ping -n 3 127.0.0.1 >nul\r\n" +
            "goto tenta\r\n" +
            ":ok\r\n" +
            "echo [%date% %time%] Copia concluida. Reiniciando... >> \"%LOG%\"\r\n" +
            $"start \"\" \"{exeDestino}\"\r\n" +
            "ping -n 2 127.0.0.1 >nul\r\n" +
            $"rd /s /q \"{dirUpd}\"\r\n" +
            "del \"%~f0\"\r\n" +
            "exit\r\n" +
            ":falha\r\n" +
            "echo [%date% %time%] FALHA: nao foi possivel substituir os arquivos apos 10 tentativas. >> \"%LOG%\"\r\n" +
            $"echo Feche todas as janelas do app e verifique a permissao de escrita na pasta: {dirApp} >> \"%LOG%\"\r\n" +
            "exit\r\n";
        File.WriteAllText(bat, conteudo, new System.Text.UTF8Encoding(false));

        try
        {
            Process.Start(new ProcessStartInfo(bat)
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }
        catch (Exception ex)
        {
            throw new Exception("Não foi possível iniciar o instalador: " + ex.Message, ex);
        }
        return true;
    }
}

/// <summary>Dados de um Release de atualização.</summary>
public record VersaoRelease(string Versao, string UrlZip, string Notas = "");
