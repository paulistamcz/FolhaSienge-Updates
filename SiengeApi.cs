using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

    public static string ArquivoConfig => DbService.ArquivoDados("sienge_api.json");

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
            return "Usuário sem autorização para este recurso (aba Autorizações no Sienge).";
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

    private static readonly string[] CamposCodigoObra = { "id", "code", "codigo", "costCenterId", "costCenterCode", "number", "numero" };
    private static readonly string[] CamposNomeObra = { "name", "nome", "description", "descricao" };
    private static readonly string[] CamposIdEmpresa = { "id", "companyId", "empresaId" };
    private static readonly string[] CamposCodigoEmpresa = { "code", "codigo", "companyCode", "number", "numero" };
    private static readonly string[] CamposNomeEmpresa = { "name", "tradeName", "nome", "fantasia", "razaoSocial", "companyName", "description", "descricao" };
    private static readonly string[] CamposCodigoDepto = { "departmentId", "id", "code", "codigo" };
    private static readonly string[] CamposNomeDepto = { "departmentName", "name", "nome", "description", "descricao" };

    /// <summary>GET paginado genérico: acumula os itens de todas as páginas (clonados).</summary>
    private async Task<(bool Ok, string Mensagem, List<JsonElement> Itens)> GetTodosAsync(string endpoint, int maxPag, string rotulo)
    {
        var itens = new List<JsonElement>();
        try
        {
            using var http = NovoHttp();
            int offset = 0;
            const int limite = 200;
            for (int pag = 0; pag < maxPag; pag++)
            {
                using var resp = await http.GetAsync(
                    UrlBase(_cfg.Subdominio) + $"{endpoint}?limit={limite}&offset={offset}");
                var corpo = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    return (false, ErroAmigavel(new HttpRequestException(), resp.StatusCode, corpo), itens);
                List<JsonElement> lote;
                try
                {
                    using var doc = JsonDocument.Parse(corpo);
                    lote = Desembrulhar(doc.RootElement).Select(e => e.Clone()).ToList();
                }
                catch { lote = new(); }
                if (lote.Count == 0) break;
                itens.AddRange(lote);
                if (lote.Count < limite) break;
                offset += limite;
            }
            return (true, "ok", itens);
        }
        catch (HttpRequestException ex)
        {
            return (false, ErroAmigavel(ex, ex.StatusCode, ""), itens);
        }
        catch (Exception ex)
        {
            return (false, $"Falha ao listar {rotulo}: " + ex.Message, itens);
        }
    }

    private static List<(string Codigo, string Nome)> MapearPares(List<JsonElement> itens, string[] cod, string[] nome)
    {
        var lista = new List<(string Codigo, string Nome)>();
        foreach (var item in itens)
        {
            var c = Campo(item, cod);
            var n = Campo(item, nome);
            if (!string.IsNullOrWhiteSpace(c))
                lista.Add((c.Trim(), (n ?? "").Trim()));
        }
        return lista;
    }

    private static List<(string Id, string Codigo, string Nome)> MapearTrios(
        List<JsonElement> itens, string[] id, string[] cod, string[] nome)
    {
        var lista = new List<(string Id, string Codigo, string Nome)>();
        foreach (var item in itens)
        {
            var i = Campo(item, id);
            var c = Campo(item, cod);
            var n = Campo(item, nome);
            if (!string.IsNullOrWhiteSpace(i) || !string.IsNullOrWhiteSpace(c) || !string.IsNullOrWhiteSpace(n))
                lista.Add((i.Trim(), c.Trim(), (n ?? "").Trim()));
        }
        return lista;
    }

    /// <summary>Lista todas as obras/centros de custo do Sienge (paginado).</summary>
    public async Task<(bool Ok, string Mensagem, List<(string Codigo, string Nome, string Empresa)> Obras)> ListarObrasAsync()
    {
        var (ok, msg, itens) = await GetTodosAsync("/cost-centers", 25, "obras");
        var obras = new List<(string Codigo, string Nome, string Empresa)>();
        foreach (var item in itens)
        {
            var c = Campo(item, CamposCodigoObra);
            var n = Campo(item, CamposNomeObra);
            if (string.IsNullOrWhiteSpace(c)) continue;
            obras.Add((c.Trim(), (n ?? "").Trim(), Campo(item, "idCompany", "companyId", "empresa", "empresaId").Trim()));
        }
        return (ok, ok ? $"{obras.Count} obra(s) lida(s) do Sienge." : msg, obras);
    }

    /// <summary>Lista as empresas do Sienge (paginado).</summary>
    public async Task<(bool Ok, string Mensagem, List<(string Id, string Codigo, string Nome)> Empresas)> ListarEmpresasAsync()
    {
        var (ok, msg, itens) = await GetTodosAsync("/companies", 10, "empresas");
        var empresas = MapearTrios(itens, CamposIdEmpresa, CamposCodigoEmpresa, CamposNomeEmpresa);
        return (ok, ok ? $"{empresas.Count} empresa(s) lida(s) do Sienge." : msg, empresas);
    }

    /// <summary>Extrai (id, codigo, nome) de empresas em vários formatos de envelope.</summary>
    public static List<(string Id, string Codigo, string Nome)> ExtrairEmpresas(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return MapearTrios(Desembrulhar(doc.RootElement), CamposIdEmpresa, CamposCodigoEmpresa, CamposNomeEmpresa);
        }
        catch { return new(); }
    }

    /// <summary>Lista os departamentos do Sienge (paginado).</summary>
    public async Task<(bool Ok, string Mensagem, List<(string Codigo, string Nome)> Deptos)> ListarDepartamentosAsync()
    {
        var (ok, msg, itens) = await GetTodosAsync("/departments", 10, "departamentos");
        var deptos = MapearPares(itens, CamposCodigoDepto, CamposNomeDepto);
        return (ok, ok ? $"{deptos.Count} departamento(s) lido(s) do Sienge." : msg, deptos);
    }

    /// <summary>Extrai (codigo, nome) de departamentos (departmentId/departmentName).</summary>
    public static List<(string Codigo, string Nome)> ExtrairDepartamentos(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return MapearPares(Desembrulhar(doc.RootElement), CamposCodigoDepto, CamposNomeDepto);
        }
        catch { return new(); }
    }

    /// <summary>Detalhe de obra/empreendimento (situacao, datas, centros associados).</summary>
    public record DetalheObra(bool Encontrado, string Fonte, string Id, string Nome,
        string Situacao, string Criacao, string Alteracao, int Setores, string Empresa);

    /// <summary>Busca detalhe: tenta enterprises/{id} e cai para cost-centers/{id}.</summary>
    public async Task<(bool Ok, string Mensagem, DetalheObra? Detalhe)> BuscarObraAsync(int id)
    {
        try
        {
            using var http = NovoHttp();
            var baseUrl = UrlBase(_cfg.Subdominio);
            using (var resp = await http.GetAsync($"{baseUrl}/enterprises/{id}"))
            {
                var corpo = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                    return (true, "ok", ExtrairDetalheEmpresa(corpo, "Obra"));
                if (resp.StatusCode != HttpStatusCode.NotFound)
                    return (false, ErroAmigavel(new HttpRequestException(), resp.StatusCode, corpo), null);
            }
            using (var resp2 = await http.GetAsync($"{baseUrl}/cost-centers/{id}"))
            {
                var corpo2 = await resp2.Content.ReadAsStringAsync();
                if (resp2.IsSuccessStatusCode)
                    return (true, "ok", ExtrairDetalheCentro(corpo2));
                if (resp2.StatusCode == HttpStatusCode.NotFound)
                    return (true, "ok", new DetalheObra(false, "", id.ToString(), "", "", "", "", 0, ""));
                return (false, ErroAmigavel(new HttpRequestException(), resp2.StatusCode, corpo2), null);
            }
        }
        catch (HttpRequestException ex)
        {
            return (false, ErroAmigavel(ex, ex.StatusCode, ""), null);
        }
        catch (Exception ex)
        {
            return (false, "Falha ao buscar obra: " + ex.Message, null);
        }
    }

    private static DetalheObra ExtrairDetalheEmpresa(string json, string fonte)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;
            string id = Campo(r, "id");
            string nome = Campo(r, "name", "commercialName", "nome");
            string sit = Campo(r, "buildingStatus", "costCenterStatus", "status", "situacao");
            string cri = Campo(r, "creationDate", "dataCadastro", "createdAt");
            string alt = Campo(r, "modificationDate", "dataAlteracao", "updatedAt");
            string emp = Campo(r, "companyName", "companyId", "empresa");
            int setores = 0;
            if (r.ValueKind == JsonValueKind.Object && r.TryGetProperty("associatedCostCenters", out var ac) && ac.ValueKind == JsonValueKind.Array)
                setores = ac.GetArrayLength();
            return new DetalheObra(true, fonte, id, nome, sit, cri, alt, setores, emp);
        }
        catch { return new DetalheObra(false, fonte, "", "", "", "", "", 0, ""); }
    }

    private static DetalheObra ExtrairDetalheCentro(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;
            string id = Campo(r, "id");
            string nome = Campo(r, "name", "nome");
            string emp = Campo(r, "idCompany", "companyId", "empresa");
            int setores = 0;
            if (r.ValueKind == JsonValueKind.Object && r.TryGetProperty("buildingSectors", out var bs) && bs.ValueKind == JsonValueKind.Array)
                setores = bs.GetArrayLength();
            return new DetalheObra(true, "Centro", id, nome, "", "", "", setores, emp);
        }
        catch { return new DetalheObra(false, "Centro", "", "", "", "", "", 0, ""); }
    }

    /// <summary>Extrai (codigo, nome) de um payload JSON em vários formatos de envelope.</summary>
    public static List<(string Codigo, string Nome)> ExtrairObras(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            // Spec oficial: id = "Código do centro de custo".
            return MapearPares(Desembrulhar(doc.RootElement), CamposCodigoObra, CamposNomeObra);
        }
        catch { return new(); }
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

    // ---------------- ESPELHO LOCAL (fonte primária offline) ----------------

    public static string ArquivoEspelhoObras => DbService.ArquivoDados("sienge_obras.csv");

    public static string ArquivoEspelhoEmpresas => DbService.ArquivoDados("sienge_empresas.csv");

    public static string ArquivoEspelhoDeptos => DbService.ArquivoDados("sienge_deptos.csv");

    /// <summary>Salva o espelho de obras (CODIGO;NOME;EMPRESAID) com data da consulta.</summary>
    public static void SalvarEspelhoObras(List<(string Codigo, string Nome)> obras, Dictionary<string, string>? empresaPorCodigo = null)
    {
        try
        {
            var linhas = new List<string> { "#atualizado_em=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            foreach (var o in obras.OrderBy(x => x.Codigo, StringComparer.OrdinalIgnoreCase))
            {
                string emp = "";
                if (empresaPorCodigo != null) empresaPorCodigo.TryGetValue(o.Codigo.Trim(), out emp!);
                linhas.Add($"{o.Codigo.Trim()};{(o.Nome ?? "").Replace(";", ",")};{(emp ?? "").Trim()}");
            }
            File.WriteAllLines(ArquivoEspelhoObras, linhas, Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>Salva o espelho de empresas (ID;NOME;CNPJ) com data da consulta.</summary>
    public static void SalvarEspelhoEmpresas(List<(string Id, string Codigo, string Nome)> empresas)
    {
        try
        {
            var linhas = new List<string> { "#atualizado_em=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            foreach (var e in empresas.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase))
                linhas.Add($"{e.Id.Trim()};{(e.Nome ?? "").Replace(";", ",")};");
            File.WriteAllLines(ArquivoEspelhoEmpresas, linhas, Encoding.UTF8);
        }
        catch { }
    }

    private static (DateTime Data, List<string[]> Linhas) LerEspelho(string caminho, int colunas)
    {
        var linhas = new List<string[]>();
        DateTime data = DateTime.MinValue;
        try
        {
            if (!File.Exists(caminho)) return (data, linhas);
            foreach (var lin in File.ReadAllLines(caminho, Encoding.UTF8))
            {
                var t = (lin ?? "").Trim();
                if (t == "") continue;
                if (t.StartsWith("#"))
                {
                    if (t.StartsWith("#atualizado_em=", StringComparison.OrdinalIgnoreCase) &&
                        DateTime.TryParseExact(t.Substring(15).Trim(), "yyyy-MM-dd HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        data = d;
                    continue;
                }
                var partes = DividirCsv(t);
                while (partes.Count < colunas) partes.Add("");
                linhas.Add(partes.ToArray());
            }
        }
        catch { }
        return (data, linhas);
    }

    /// <summary>Divide linha CSV respeitando aspas (separador ';' ou ',' detectado).</summary>
    public static List<string> DividirCsv(string linha) => DividirLinha(linha, ';');

    public static List<string> DividirLinha(string linha, char sep)
    {
        var partes = new List<string>();
        var atual = new StringBuilder();
        bool aspas = false;
        foreach (char ch in linha ?? "")
        {
            if (ch == '"') { aspas = !aspas; continue; }
            if (ch == sep && !aspas) { partes.Add(atual.ToString()); atual.Clear(); continue; }
            atual.Append(ch);
        }
        partes.Add(atual.ToString());
        return partes;
    }

    private static char DetectarSeparador(string cabecalho)
    {
        int pontoVirg = 0, virg = 0;
        bool aspas = false;
        foreach (char ch in cabecalho ?? "")
        {
            if (ch == '"') { aspas = !aspas; continue; }
            if (aspas) continue;
            if (ch == ';') pontoVirg++;
            else if (ch == ',') virg++;
        }
        return virg > pontoVirg ? ',' : ';';
    }

    /// <summary>Carrega o espelho de obras: (data, lista codigo/nome/empresa).</summary>
    public static (DateTime Data, List<(string Codigo, string Nome, string Empresa)> Obras) CarregarEspelhoObras()
    {
        var (data, linhas) = LerEspelho(ArquivoEspelhoObras, 3);
        return (data, linhas.Select(p => (p[0].Trim(), p[1].Trim(), p[2].Trim()))
            .Where(x => x.Item1 != "").ToList());
    }

    /// <summary>Carrega o espelho de empresas: (data, lista id/nome).</summary>
    public static (DateTime Data, List<(string Id, string Nome)> Empresas) CarregarEspelhoEmpresas()
    {
        var (data, linhas) = LerEspelho(ArquivoEspelhoEmpresas, 2);
        return (data, linhas.Select(p => (p[0].Trim(), p[1].Trim()))
            .Where(x => x.Item1 != "").ToList());
    }

    /// <summary>Salva o espelho de departamentos (CODIGO;NOME) com data da consulta.</summary>
    public static void SalvarEspelhoDeptos(List<(string Codigo, string Nome)> deptos)
    {
        try
        {
            var linhas = new List<string> { "#atualizado_em=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            foreach (var d in deptos.OrderBy(x => x.Codigo, StringComparer.OrdinalIgnoreCase))
                linhas.Add($"{d.Codigo.Trim()};{(d.Nome ?? "").Replace(";", ",")}");
            File.WriteAllLines(ArquivoEspelhoDeptos, linhas, Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>Carrega o espelho de departamentos: (data, lista codigo/nome).</summary>
    public static (DateTime Data, List<(string Codigo, string Nome)> Deptos) CarregarEspelhoDeptos()
    {
        var (data, linhas) = LerEspelho(ArquivoEspelhoDeptos, 2);
        return (data, linhas.Select(p => (p[0].Trim(), p[1].Trim()))
            .Where(x => x.Item1 != "").ToList());
    }

    /// <summary>Texto de idade do espelho ("de dd/MM/yyyy HH:mm" + aviso se velho).</summary>
    public static string IdadeEspelho(DateTime data)
    {
        if (data == DateTime.MinValue) return "sem data";
        int dias = (int)(DateTime.Now - data).TotalDays;
        string baseTxt = "de " + data.ToString("dd/MM/yyyy HH:mm");
        return dias > 30 ? $"{baseTxt} (velho! atualize)" : baseTxt;
    }

    /// <summary>
    /// Importa CSV externo (ex.: exportado pela API) detectando o tipo pelo cabeçalho:
    /// com idCompany = obras; com tradeName = empresas. Retorna (tipo, qtd, mensagem).
    /// </summary>
    public static (string Tipo, int Qtd, string Mensagem) ImportarCsvExterno(string caminho)
    {
        try
        {
            var linhas = File.ReadAllLines(caminho, Encoding.UTF8)
                .Select(l => (l ?? "").Trim()).Where(l => l != "").ToList();
            if (linhas.Count < 2) return ("", 0, "Arquivo vazio ou só com cabeçalho.");
            char sep = DetectarSeparador(linhas[0]);
            var cab = DividirLinha(linhas[0], sep).Select(c => c.Trim().Trim('"').ToLowerInvariant()).ToList();
            bool eObra = cab.Contains("idcompany");
            bool eEmpresa = cab.Contains("tradename");
            bool eDepto = cab.Contains("departmentid") || cab.Contains("departmentname");
            int iId = cab.IndexOf("id");
            int iNome = cab.IndexOf("name");
            if (iNome < 0) iNome = cab.IndexOf("nome");
            if (eDepto)
            {
                if (iId < 0) iId = cab.IndexOf("departmentid");
                if (iNome < 0) iNome = cab.IndexOf("departmentname");
            }
            if (iId < 0 || iNome < 0) return ("", 0, "Formato não reconhecido (precisa de colunas id + name/nome).");
            if (!eObra && !eEmpresa && !eDepto)
                return ("", 0, "Não deu para saber o tipo (sem idCompany/tradeName/departmentId).");
            int iEmp = cab.IndexOf("idcompany");
            int n = 0;
            if (eObra)
            {
                var lista = new List<(string Codigo, string Nome)>();
                var emp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var lin in linhas.Skip(1))
                {
                    var p = DividirLinha(lin, sep);
                    string cod = iId < p.Count ? p[iId].Trim().Trim('"') : "";
                    string nome = iNome < p.Count ? p[iNome].Trim().Trim('"') : "";
                    if (cod == "") continue;
                    lista.Add((cod, nome));
                    if (iEmp >= 0 && iEmp < p.Count) emp[cod] = p[iEmp].Trim().Trim('"');
                    n++;
                }
                SalvarEspelhoObras(lista, emp);
                return ("obras", n, $"{n} obra(s) importada(s).");
            }
            else if (eDepto)
            {
                int iCod = cab.IndexOf("departmentid");
                if (iCod < 0) iCod = iId;
                int iNom = cab.IndexOf("departmentname");
                if (iNom < 0) iNom = iNome;
                var lista = new List<(string Codigo, string Nome)>();
                foreach (var lin in linhas.Skip(1))
                {
                    var p = DividirLinha(lin, sep);
                    string cod = iCod < p.Count ? p[iCod].Trim().Trim('"') : "";
                    string nome = iNom < p.Count ? p[iNom].Trim().Trim('"') : "";
                    if (cod == "") continue;
                    lista.Add((cod, nome));
                    n++;
                }
                SalvarEspelhoDeptos(lista);
                return ("departamentos", n, $"{n} departamento(s) importado(s).");
            }
            else
            {
                var lista = new List<(string Id, string Codigo, string Nome)>();
                foreach (var lin in linhas.Skip(1))
                {
                    var p = DividirLinha(lin, sep);
                    string id = iId < p.Count ? p[iId].Trim().Trim('"') : "";
                    string nome = iNome < p.Count ? p[iNome].Trim().Trim('"') : "";
                    if (id == "") continue;
                    lista.Add((id, "", nome));
                    n++;
                }
                SalvarEspelhoEmpresas(lista);
                return ("empresas", n, $"{n} empresa(s) importada(s).");
            }
        }
        catch (Exception ex)
        {
            return ("", 0, "Falha ao importar: " + ex.Message);
        }
    }

    /// <summary>Busca um credor por ID (nome exato do cadastro + ativo?).</summary>
    public async Task<(bool Ok, string Mensagem, bool Encontrado, string Nome, bool Ativo)> BuscarCredorAsync(int id)
    {
        try
        {
            using var http = NovoHttp();
            using var resp = await http.GetAsync(UrlBase(_cfg.Subdominio) + $"/creditors/{id}");
            var corpo = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(corpo);
                    var r = doc.RootElement;
                    string nome = Campo(r, "name", "nome");
                    bool ativo = true;
                    if (r.ValueKind == JsonValueKind.Object && r.TryGetProperty("active", out var a))
                        ativo = a.ValueKind == JsonValueKind.True ||
                            (a.ValueKind == JsonValueKind.String && (a.GetString() ?? "").Trim().ToUpperInvariant() == "S");
                    return (true, "ok", true, nome.Trim(), ativo);
                }
                catch { return (false, "Resposta inesperada do Sienge.", false, "", true); }
            }
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return (true, "ok", false, "", true);
            return (false, ErroAmigavel(new HttpRequestException(), resp.StatusCode, corpo), false, "", true);
        }
        catch (HttpRequestException ex)
        {
            return (false, ErroAmigavel(ex, ex.StatusCode, ""), false, "", true);
        }
        catch (Exception ex)
        {
            return (false, "Falha ao buscar credor: " + ex.Message, false, "", true);
        }
    }

    /// <summary>Normaliza nome para comparação (maiúsculas, sem acento/pontuação).</summary>
    public static HashSet<string> Toks(string? s)
    {
        string t = (s ?? "").ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char ch in t)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        t = sb.ToString().Replace("D'", "DE ").Replace("D’", "DE ");
        t = Regex.Replace(t, @"[^A-Z0-9 ]", " ");
        return t.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    /// <summary>Similaridade Dice entre tokens (+bônus se compartilham número).</summary>
    public static double Similaridade(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        int inter = a.Count(t => b.Contains(t));
        double dice = 2.0 * inter / (a.Count + b.Count);
        var numsA = a.Where(t => t.Any(char.IsDigit)).ToList();
        if (numsA.Count > 0 && numsA.Any(t => b.Contains(t)))
            dice = Math.Min(1.0, dice + 0.25);
        return dice;
    }

    /// <summary>
    /// Sugere obra do Sienge para cada centro do Domínio (melhor match &gt;= 0.35,
    /// +0.15 se a empresa coincide). Sempre conferir na tela: sugestão não é certeza.
    /// </summary>
    public static Dictionary<int, int> SugerirMapa(
        List<(int Centro, string Nome)> dominios,
        List<(string Codigo, string Nome, string Empresa)> obras,
        int empresaDom = 0)
    {
        var sug = new Dictionary<int, int>();
        var prep = obras
            .Where(o => int.TryParse((o.Codigo ?? "").Trim(), out _))
            .Select(o => (Codigo: o.Codigo.Trim(), Toks: Toks(o.Nome), Empresa: (o.Empresa ?? "").Trim()))
            .ToList();
        string empDom = empresaDom.ToString();
        foreach (var d in dominios)
        {
            var td = Toks(d.Nome);
            double best = 0;
            string bestCod = "";
            foreach (var o in prep)
            {
                double s = Similaridade(td, o.Toks);
                if (empresaDom > 0 && o.Empresa != "" &&
                    o.Empresa.Equals(empDom, StringComparison.OrdinalIgnoreCase))
                    s = Math.Min(1.0, s + 0.15);
                if (s > best) { best = s; bestCod = o.Codigo; }
            }
            if (best >= 0.35 && int.TryParse(bestCod, out var cod))
                sug[d.Centro] = cod;
        }
        return sug;
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
