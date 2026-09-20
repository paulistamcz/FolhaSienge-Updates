using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FolhaSienge;

/// <summary>Configuração da API do Sienge (sienge_api.json ao lado do executável).</summary>
public class SiengeConfig
{
    public string Subdominio { get; set; } = "engenhariademateriais";
    public string Usuario { get; set; } = "";
    public string Chave { get; set; } = "";

    public bool Preenchida =>
        !string.IsNullOrWhiteSpace(Subdominio) &&
        !string.IsNullOrWhiteSpace(Usuario) &&
        !string.IsNullOrWhiteSpace(Chave);
}

/// <summary>
/// Cliente mínimo da API REST do Sienge (https://api.sienge.com.br/{sub}/public/api/v1).
/// Fase 1: somente leitura de centros de custo para validar as obras da coluna B.
/// Autenticação: HTTP Basic com usuário + chave de API.
/// </summary>
public class SiengeApi
{
    private readonly SiengeConfig _cfg;

    public SiengeApi(SiengeConfig cfg) { _cfg = cfg; }

    public static string ArquivoConfig =>
        Path.Combine(AppContext.BaseDirectory, "sienge_api.json");

    public static SiengeConfig CarregarConfig()
    {
        try
        {
            var arq = ArquivoConfig;
            if (File.Exists(arq))
            {
                var cfg = JsonSerializer.Deserialize<SiengeConfig>(File.ReadAllText(arq));
                if (cfg != null) return cfg;
            }
        }
        catch { }
        return new SiengeConfig();
    }

    public static void SalvarConfig(SiengeConfig cfg)
    {
        try
        {
            File.WriteAllText(ArquivoConfig,
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    public static string UrlBase(string subdominio) =>
        $"https://api.sienge.com.br/{subdominio.Trim().Trim('/')}/public/api/v1";

    public static string CabecalhoAuth(string usuario, string chave) =>
        "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{usuario.Trim()}:{chave.Trim()}"));

    private HttpClient NovoHttp()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("FolhaSienge");
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.Authorization =
            AuthenticationHeaderValue.Parse(CabecalhoAuth(_cfg.Usuario, _cfg.Chave));
        return http;
    }

    private static string ErroAmigavel(HttpRequestException ex, HttpStatusCode? status, string corpo)
    {
        if (status == HttpStatusCode.Unauthorized)
            return "Usuário/chave inválidos ou sem autorização na API do Sienge.";
        if (status == HttpStatusCode.Forbidden)
            return "Usuário sem autorização para centros de custo (aba Autorizações no Sienge).";
        if (status != null)
        {
            var detalhe = (corpo ?? "").Trim();
            if (detalhe.Length > 200) detalhe = detalhe.Substring(0, 200) + "...";
            return $"Sienge retornou {(int)status}. {detalhe}".Trim();
        }
        return "Sem acesso à API do Sienge. Confira internet, subdomínio e usuário/chave.";
    }

    /// <summary>Testa a conexão/autenticação (1 centro de custo).</summary>
    public async Task<(bool Ok, string Mensagem)> TestarConexaoAsync()
    {
        try
        {
            using var http = NovoHttp();
            using var resp = await http.GetAsync(UrlBase(_cfg.Subdominio) + "/cost-centers?limit=1&offset=0");
            var corpo = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
                return (true, "Conexão OK com a API do Sienge.");
            return (false, ErroAmigavel(new HttpRequestException(), resp.StatusCode, corpo));
        }
        catch (HttpRequestException ex)
        {
            return (false, ErroAmigavel(ex, ex.StatusCode, ""));
        }
        catch (Exception ex)
        {
            return (false, "Falha ao testar: " + ex.Message);
        }
    }

    /// <summary>Lista todas as obras/centros de custo do Sienge (paginado).</summary>
    public async Task<(bool Ok, string Mensagem, List<(string Codigo, string Nome)> Obras)> ListarObrasAsync()
    {
        var obras = new List<(string Codigo, string Nome)>();
        try
        {
            using var http = NovoHttp();
            int offset = 0;
            const int limite = 200;
            for (int pag = 0; pag < 25; pag++)
            {
                using var resp = await http.GetAsync(
                    UrlBase(_cfg.Subdominio) + $"/cost-centers?limit={limite}&offset={offset}");
                var corpo = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    return (false, ErroAmigavel(new HttpRequestException(), resp.StatusCode, corpo), obras);
                var lote = ExtrairObras(corpo);
                if (lote.Count == 0) break;
                obras.AddRange(lote);
                if (lote.Count < limite) break;
                offset += limite;
            }
            return (true, $"{obras.Count} obra(s) lida(s) do Sienge.", obras);
        }
        catch (HttpRequestException ex)
        {
            return (false, ErroAmigavel(ex, ex.StatusCode, ""), obras);
        }
        catch (Exception ex)
        {
            return (false, "Falha ao listar obras: " + ex.Message, obras);
        }
    }

    /// <summary>Extrai (codigo, nome) de um payload JSON em vários formatos de envelope.</summary>
    public static List<(string Codigo, string Nome)> ExtrairObras(string json)
    {
        var lista = new List<(string Codigo, string Nome)>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var item in Desembrulhar(doc.RootElement))
            {
                var cod = Campo(item, "code", "codigo", "costCenterCode", "number", "numero");
                var nome = Campo(item, "name", "nome", "description", "descricao");
                if (!string.IsNullOrWhiteSpace(cod))
                    lista.Add((cod.Trim(), (nome ?? "").Trim()));
            }
        }
        catch { }
        return lista;
    }

    private static List<JsonElement> Desembrulhar(JsonElement raiz)
    {
        var itens = new List<JsonElement>();
        if (raiz.ValueKind == JsonValueKind.Array)
        {
            itens.AddRange(raiz.EnumerateArray());
            return itens;
        }
        if (raiz.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in new[] { "results", "data", "items", "content", "rows" })
                if (raiz.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    itens.AddRange(arr.EnumerateArray());
                    return itens;
                }
            foreach (var p in raiz.EnumerateObject())
            {
                if (p.Value.ValueKind != JsonValueKind.Object) continue;
                foreach (var q in new[] { "results", "data", "items" })
                    if (p.Value.TryGetProperty(q, out var a2) && a2.ValueKind == JsonValueKind.Array)
                    {
                        itens.AddRange(a2.EnumerateArray());
                        return itens;
                    }
            }
        }
        return itens;
    }

    private static string Campo(JsonElement item, params string[] nomes)
    {
        if (item.ValueKind != JsonValueKind.Object) return "";
        foreach (var n in nomes)
        {
            if (!item.TryGetProperty(n, out var v)) continue;
            if (v.ValueKind == JsonValueKind.String) return v.GetString() ?? "";
            if (v.ValueKind == JsonValueKind.Number) return v.GetRawText();
        }
        return "";
    }
}
