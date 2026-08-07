using System.Data.Odbc;
using System.Globalization;

namespace FolhaSienge;

public class DbService
{
    public const string Usuario = "EXTERNO";
    public const string Senha = "123456";

    public static readonly string[] EnginesCandidatos =
    {
        "srvcontabil",
        "Servidor_Dominio17",
        "SQLANYs_Servidor_Dominio17",
        "servidor_dominio",
        "Dominio",
    };

    public static readonly string[] BancosCandidatos =
    {
        "Contabil",
        "contabil",
        "DOMINIO",
        "dominio",
    };

    public static readonly string[] HostsCandidatos =
    {
        "contabil",
        "srvcontabil",
        "Servidor_Dominio17",
        "servidor_dominio",
    };

    /// <summary>
    /// Detecta os servidores SQL Anywhere que estão respondendo na máquina (via ODBC).
    /// Retorna descrições tipo "srvcontabil / Contabil" ou "contabil / srvcontabil / Contabil".
    /// </summary>
    public static List<string> DetectarServidores()
    {
        var achados = new List<string>();
        foreach (var eng in EnginesCandidatos)
        {
            foreach (var db in BancosCandidatos)
            {
                var cs = $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};ENG={eng};DBN={db}";
                try
                {
                    using var conn = new OdbcConnection(cs);
                    conn.Open();
                    conn.Close();
                    achados.Add($"{eng} / {db}");
                }
                catch
                {
                    // tenta o próximo
                }
            }
        }

        // tenta pela rede (TCP/IP) em cada host candidato, quando o broadcast não funciona
        foreach (var host in HostsCandidatos)
        {
            if (!PortaAberta(host, 2638, 1500)) continue;
            foreach (var eng in EnginesCandidatos)
            {
                foreach (var db in BancosCandidatos)
                {
                    var cs = MontarConnectionStringRede(host, eng, db);
                    try
                    {
                        using var conn = new OdbcConnection(cs);
                        conn.Open();
                        conn.Close();
                        achados.Add($"{host} / {eng} / {db}");
                    }
                    catch
                    {
                        // tenta o próximo
                    }
                }
            }
        }
        return achados.Distinct().ToList();
    }

    /// <summary>
    /// Tenta conectar em um host específico (nome ou IP) pela rede (TCP/IP, porta 2638).
    /// Retorna a descrição do banco no formato "host / engine / db", ou null se falhar.
    /// </summary>
    public static string? DetectarHost(string host)
    {
        // pré-checa a porta 2638 (timeout curto) para não travar em host inalcançável
        if (!PortaAberta(host, 2638, 2000)) return null;

        foreach (var eng in EnginesCandidatos)
        {
            foreach (var db in BancosCandidatos)
            {
                var cs = MontarConnectionStringRede(host, eng, db);
                try
                {
                    using var conn = new OdbcConnection(cs);
                    conn.Open();
                    conn.Close();
                    return $"{host} / {eng} / {db}";
                }
                catch
                {
                    // tenta o próximo
                }
            }
        }
        return null;
    }

    private static bool PortaAberta(string host, int porta, int timeoutMs)
    {
        try
        {
            using var cliente = new System.Net.Sockets.TcpClient();
            var t = cliente.ConnectAsync(host, porta);
            return t.Wait(timeoutMs) && cliente.Connected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Busca arquivos contabil.db apenas em caminhos já conhecidos (rápido).
    /// </summary>
    public static List<string> BuscarBancos(string caminho = "")
    {
        var nomes = new[] { "contabil.db", "contabil.db1", "domínio.db", "dominio.db" };
        var conhecidos = new[]
        {
            @"C:\Users\Eduardo\Desktop\api engemat\banco\contabil.db",
            @"C:\Users\Eduardo\Desktop\api engemat\contabil.db",
            @"C:\Users\Eduardo\Downloads\contabil.db",
            @"C:\Contabil\contabil.db",
        };
        var encontrados = new List<string>();

        if (!string.IsNullOrWhiteSpace(caminho) && Directory.Exists(caminho))
        {
            try
            {
                encontrados.AddRange(new DirectoryInfo(caminho)
                    .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Where(f => nomes.Contains(f.Name.ToLowerInvariant()))
                    .Select(f => f.FullName));
            }
            catch { }
        }

        foreach (var c in conhecidos)
        {
            if (File.Exists(c))
                encontrados.Add(c);
        }

        return encontrados.Distinct().ToList();
    }

    public string MontarConnectionString(string banco, string engine)
    {
        // banco pode ser um arquivo (.db) ou uma descrição "engine / db" ou "host / engine / db"
        if (banco.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
            File.Exists(banco))
            return $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};DBF={banco}";

        var partes = banco.Split('/');
        if (partes.Length >= 3)
            return $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};ENG={partes[1].Trim()};DBN={partes[2].Trim()};LINKS=tcpip(host={partes[0].Trim()};port=2638);ConnectionTimeout=3";
        if (partes.Length == 2)
            return $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};ENG={partes[0].Trim()};DBN={partes[1].Trim()}";

        return $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};ENG={engine};DBN={banco}";
    }

    public static string MontarConnectionStringRede(string host, string engine, string db)
    {
        return $"DRIVER={{SQL Anywhere 17}};UID={Usuario};PWD={Senha};ENG={engine.Trim()};DBN={db.Trim()};LINKS=tcpip(host={host.Trim()};port=2638);ConnectionTimeout=3";
    }

    public OdbcConnection TestarConexao(string connectionString)
    {
        var conn = new OdbcConnection(connectionString);
        conn.Open();
        return conn;
    }

    public static string FormatarCompetencia(object? valor)
    {
        if (valor == null || valor == DBNull.Value)
            return "";
        if (valor is DateTime dt)
            return dt.ToString("MM/yyyy");
        string s = valor.ToString() ?? "";
        if (DateTime.TryParse(s, out var dt2))
            return dt2.ToString("MM/yyyy");
        return s.Length >= 7 ? s.Substring(3, 2) + "/" + s.Substring(0, 4) : s;
    }

    public static string CompetenciaParaSql(string comp)
    {
        // "12/2025" -> "2025-12-01"
        if (DateTime.TryParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            return dt.ToString("yyyy-MM-dd");
        return comp;
    }

    public List<string> ListarCompetencias(OdbcConnection conn)
    {
        var lista = new List<string>();
        using var cmd = new OdbcCommand(
            "SELECT DISTINCT l.competencia FROM bethadba.foliquidosfil l " +
            "WHERE l.codi_emp = 1 ORDER BY l.competencia DESC", conn);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            var f = FormatarCompetencia(rd[0]);
            if (!string.IsNullOrEmpty(f))
                lista.Add(f);
        }
        return lista;
    }

    public List<(int Codigo, string Nome)> ListarCentrosCusto(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<(int, string)>();
        using var cmd = new OdbcCommand(
            "SELECT DISTINCT e.i_ccustos, c.nome " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 " +
            "ORDER BY e.i_ccustos", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cod = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            lista.Add((cod, nome));
        }
        return lista;
    }

    public (int Empregados, decimal Total) TotalPorCentro(OdbcConnection conn, string comp, int centro)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT COUNT(*), SUM(f.liquido) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "AND e.i_ccustos = ?", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        cmd.Parameters.AddWithValue("centro", centro);
        using var rd = cmd.ExecuteReader();
        if (rd.Read())
        {
            int n = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            decimal tot = rd.IsDBNull(1) ? 0m : Convert.ToDecimal(rd[1]);
            return (n, tot);
        }
        return (0, 0m);
    }

    public List<(int Codigo, string Nome, int Empregados, decimal Total)> ResumoCentrosCusto(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(*), SUM(f.liquido) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cod = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal tot = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cod, nome, emp, tot));
        }
        return lista;
    }

    public static string GerarCsv(
        List<(int Centro, string Nome, int Empregados, decimal Total, string CredorCodigo, string CredorNome)> linhas,
        string vencimento, string verba, string competenciaDoc, string observacao = "")
    {
        var sb = new System.Text.StringBuilder();
        int i = 1;
        foreach (var l in linhas)
        {
            var cc = l.Centro.ToString("D4");
            var credorCodigo = string.IsNullOrWhiteSpace(l.CredorCodigo) ? $"CRED{i:D2}" : l.CredorCodigo;
            var credorNome = string.IsNullOrWhiteSpace(l.CredorNome) ? l.Nome : l.CredorNome;
            var valor = l.Total.ToString("0.00", CultureInfo.InvariantCulture);
            sb.AppendLine($"{verba};{cc};{credorCodigo};{credorNome};{valor};{vencimento};;;;;{competenciaDoc};{observacao}");
            i++;
        }
        return sb.ToString();
    }

    /// <summary>Total líquido da folha da competência (tipo_process 11), mesma base do CSV.</summary>
    public decimal TotalLiquidoFolha(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT SUM(f.liquido) FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>Soma dos valores de fomovto por classificação do evento (tipo_proces 11).</summary>
    public Dictionary<int, decimal> TotaisPorClasse(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var dic = new Dictionary<int, decimal>();
        using var cmd = new OdbcCommand(
            "SELECT e.classificacao, SUM(m.valor_cal) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos e ON m.codi_emp = e.codi_emp AND m.i_eventos = e.i_eventos " +
            "WHERE m.codi_emp = 1 AND m.data >= ? AND m.data < DATEADD(month, 1, ?) AND m.tipo_proces = 11 " +
            "GROUP BY e.classificacao", conn);
        cmd.Parameters.AddWithValue("ini", sql);
        cmd.Parameters.AddWithValue("fim", sql);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            if (!rd.IsDBNull(0) && !rd.IsDBNull(1))
            {
                int cls = Convert.ToInt32(rd[0]);
                decimal val = Convert.ToDecimal(rd[1]);
                dic[cls] = val;
            }
        }
        return dic;
    }

    /// <summary>Total das guias de INSS (foguiainss) do mês, tipo_process 11.</summary>
    public decimal TotalGuiasInss(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT SUM(g.total_guia) FROM bethadba.foguiainss g " +
            "WHERE g.codi_emp = 1 AND g.competencia >= ? AND g.competencia < DATEADD(month, 1, ?) " +
            "AND g.tipo_process = 11", conn);
        cmd.Parameters.AddWithValue("ini", sql);
        cmd.Parameters.AddWithValue("fim", sql);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>
    /// Monta o relatório mensal por verba da tabela_financeira, buscando os valores do mês no banco.
    /// Retorna tupla (Verba, Valor). Verbas sem fonte de valor ficam com 0.
    /// </summary>
    public List<(VerbaFinanceira Verba, decimal Valor)> RelatorioMensalPorVerba(OdbcConnection conn, string comp)
    {
        var liquid = TotalLiquidoFolha(conn, comp);
        var classes = TotaisPorClasse(conn, comp);
        var inss = TotalGuiasInss(conn, comp);

        decimal Cls(params int[] cods) => cods.Sum(c => classes.ContainsKey(c) ? classes[c] : 0m);

        var mapa = new Dictionary<int, decimal>
        {
            // SALARIO CONTRATUAL: líquido da folha (igual ao CSV atual)
            [1] = liquid,
            // INSS: guias oficiais do mês
            [2] = inss,
            // FGTS - NORMAL: classe 14 (I) do fomovto
            [58] = Cls(14),
            // FERIAS: classe 5
            [3] = Cls(5),
            // RESCISAO: não há guia mensal simples; mantém 0
            [4] = 0m,
            // GRRF: mantém 0 (apuração por rescisão)
            [59] = 0m,
            // PLR
            [5] = 0m,
            // SESI / SENAI / SENAI DN / INSS ADM: não individualizados na folha mensal
            [7] = 0m,
            [8] = 0m,
            [20] = 0m,
            [9] = 0m,
            // IRRF 0561: classe 13
            [10] = Cls(13),
            // DARF-DCTFWEB
            [11] = 0m,
            // VALE TRANSPORTE: classe 28
            [12] = Cls(28),
            // ADTO DE SALARIO: classe 25 (adiantamento salarial)
            [6] = Cls(25),
            // Impostos da empresa (PIS/COFINS/IRPJ/CSLL): fora da folha mensal
            [1800] = 0m,
            [1801] = 0m,
            [18021] = 0m,
            [18031] = 0m,
        };

        var lista = new List<(VerbaFinanceira, decimal)>();
        foreach (var v in VerbaFinanceira.Lista())
            lista.Add((v, mapa.ContainsKey(v.Codigo) ? mapa[v.Codigo] : 0m));
        return lista;
    }
}
