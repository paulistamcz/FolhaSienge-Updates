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

    public List<(int Codigo, string Nome)> ListarCentrosCusto(OdbcConnection conn, string comp, int tipoProcess = 11)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<(int, string)>();
        using var cmd = new OdbcCommand(
            "SELECT DISTINCT e.i_ccustos, c.nome " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = ? " +
            "ORDER BY e.i_ccustos", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        cmd.Parameters.AddWithValue("tipo", tipoProcess);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cod = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            lista.Add((cod, nome));
        }
        return lista;
    }

    public (int Empregados, decimal Total) TotalPorCentro(OdbcConnection conn, string comp, int centro, int tipoProcess = 11)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT COUNT(*), SUM(f.liquido) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = ? " +
            "AND e.i_ccustos = ?", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        cmd.Parameters.AddWithValue("tipo", tipoProcess);
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
        string vencimento, string verba, string competenciaDoc, string observacao = "",
        string obra = "", string unidade = "", string itemOrcamento = "", string departamento = "")
    {
        var sb = new System.Text.StringBuilder();
        int i = 1;
        foreach (var l in linhas)
        {
            var cc = l.Centro.ToString("D4");
            var credorCodigo = string.IsNullOrWhiteSpace(l.CredorCodigo) ? $"CRED{i:D2}" : l.CredorCodigo;
            var credorNome = string.IsNullOrWhiteSpace(l.CredorNome) ? l.Nome : l.CredorNome;
            var valor = l.Total.ToString("0.00", CultureInfo.InvariantCulture);
            // Observação: se informada (ex.: nome do credor), acrescenta o nome do centro por linha.
            var obs = string.IsNullOrWhiteSpace(observacao) ? l.Nome : $"{observacao} - {l.Nome}";
            sb.AppendLine($"{verba};{cc};{credorCodigo};{credorNome};{valor};{vencimento};{obra};{unidade};{itemOrcamento};{departamento};{competenciaDoc};{obs}");
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Folha líquida por empregado (analítico) da competência, tipo_process 11.
    /// Retorna (Centro, NomeEmpregado, Empregado, Liquido) por pessoa.
    /// </summary>
    public List<(int Centro, string NomeEmpregado, int Empregado, decimal Liquido)>
        ListarFolhaAnalitica(OdbcConnection conn, string comp, int tipoProcess = 11)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, TRIM(e.nome), f.i_empregados, ROUND(f.liquido, 2) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = ? " +
            "AND ROUND(f.liquido, 2) > 0 " +
            "ORDER BY e.i_ccustos, e.nome", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        cmd.Parameters.AddWithValue("tipo", tipoProcess);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal liq = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cc, nome, emp, liq));
        }
        return lista;
    }

    /// <summary>
    /// Gera CSV analítico (1 linha por empregado) no layout Sienge.
    /// Layout: verba;centro;credorCodigo;credorNome;valor;vencimento;obra;unidade;item;depto;doc;obs
    /// credorCodigo/credorNome da verba são aplicados a todas as linhas (o credor
    /// do pagamento é a empresa/verba; o nome do empregado vai na observação L).
    /// </summary>
    public static string GerarCsvAnalitico(
        List<(int Centro, string NomeEmpregado, int Empregado, decimal Liquido)> linhas,
        string vencimento, string verba, string credorCodigo, string credorNome,
        string competenciaDoc, string observacao = "",
        string obra = "", string unidade = "", string itemOrcamento = "", string departamento = "")
    {
        var sb = new System.Text.StringBuilder();
        foreach (var l in linhas)
        {
            var cc = l.Centro.ToString("D4");
            var valor = l.Liquido.ToString("0.00", CultureInfo.InvariantCulture);
            var obs = string.IsNullOrWhiteSpace(observacao) ? l.NomeEmpregado : observacao + " - " + l.NomeEmpregado;
            sb.AppendLine($"{verba};{cc};{credorCodigo};{credorNome};{valor};{vencimento};{obra};{unidade};{itemOrcamento};{departamento};{competenciaDoc};{obs}");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Férias por empregado num período de pagamento (analítico).
    /// Retorna (Centro, NomeEmpregado, Empregado, Valor) por pessoa usando VALOR_REMUNERACAO.
    /// </summary>
    public List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)>
        ListarFeriasAnalitica(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, TRIM(e.nome), f.I_EMPREGADOS, ROUND(f.VALOR_REMUNERACAO, 2) " +
            "FROM bethadba.FOFERIAS f " +
            "LEFT JOIN bethadba.foempregados e ON f.CODI_EMP = e.codi_emp AND f.I_EMPREGADOS = e.i_empregados " +
            "WHERE f.CODI_EMP = 1 AND f.DATA_PAGTO >= ? AND f.DATA_PAGTO <= ? " +
            "AND ROUND(f.VALOR_REMUNERACAO, 2) > 0 " +
            "ORDER BY e.i_ccustos, e.nome", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal val = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cc, nome, emp, val));
        }
        return lista;
    }

    /// <summary>
    /// Férias por centro (completo) num período de pagamento.
    /// Retorna (Centro, Nome, Empregados, Total).
    /// </summary>
    public List<(int Centro, string Nome, int Empregados, decimal Total)>
        ResumoFeriasCentros(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(*), ROUND(SUM(f.VALOR_REMUNERACAO), 2) " +
            "FROM bethadba.FOFERIAS f " +
            "LEFT JOIN bethadba.foempregados e ON f.CODI_EMP = e.codi_emp AND f.I_EMPREGADOS = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE f.CODI_EMP = 1 AND f.DATA_PAGTO >= ? AND f.DATA_PAGTO <= ? " +
            "AND ROUND(f.VALOR_REMUNERACAO, 2) > 0 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal tot = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cc, nome, emp, tot));
        }
        return lista;
    }

    /// <summary>
    /// Rescisões por empregado num período (analítico) - base = foguiagrfc (GRRF).
    /// Retorna (Centro, NomeEmpregado, Empregado, Valor) por pessoa.
    /// </summary>
    public List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)>
        ListarRescisaoAnalitica(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, TRIM(e.nome), g.i_empregados, " +
            "ROUND(g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts, 2) " +
            "FROM bethadba.foguiagrfc g " +
            "LEFT JOIN bethadba.foempregados e ON g.codi_emp = e.codi_emp AND g.i_empregados = e.i_empregados " +
            "WHERE g.codi_emp = 1 AND g.vencimento >= ? AND g.vencimento <= ? " +
            "AND (g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts) > 0 " +
            "ORDER BY e.i_ccustos, e.nome", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal val = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cc, nome, emp, val));
        }
        return lista;
    }

    /// <summary>
    /// Rescisões por centro (completo) num período - base = foguiagrfc.
    /// Retorna (Centro, Nome, Empregados, Total).
    /// </summary>
    public List<(int Centro, string Nome, int Empregados, decimal Total)>
        ResumoRescisaoCentros(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(*), " +
            "ROUND(SUM(g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts), 2) " +
            "FROM bethadba.foguiagrfc g " +
            "LEFT JOIN bethadba.foempregados e ON g.codi_emp = e.codi_emp AND g.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE g.codi_emp = 1 AND g.vencimento >= ? AND g.vencimento <= ? " +
            "AND (g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts) > 0 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal tot = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            lista.Add((cc, nome, emp, tot));
        }
        return lista;
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

    /// <summary>
    /// Guia por funcionário (analítico) para INSS/FGTS/IRRF usando as classificações de fomovto.
    /// classe: 12 = INSS, 13 = IRRF, 14 = FGTS.
    /// Retorna (Centro, NomeEmpregado, Empregado, Valor).
    /// </summary>
    public List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)>
        GuiaAnaliticoPorClasse(OdbcConnection conn, string comp, int classe)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<(int, string, int, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, TRIM(e.nome), m.i_empregados, ROUND(SUM(m.valor_cal),2) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos ev ON m.codi_emp = ev.codi_emp AND m.i_eventos = ev.i_eventos " +
            "LEFT JOIN bethadba.foempregados e ON m.codi_emp = e.codi_emp AND m.i_empregados = e.i_empregados " +
            "WHERE m.codi_emp = 1 AND m.data >= ? AND m.data < DATEADD(month,1,?) " +
            "AND m.tipo_proces = 11 AND ev.classificacao = ? " +
            "GROUP BY e.i_ccustos, e.nome, m.i_empregados " +
            "ORDER BY e.i_ccustos, e.nome", conn);
        cmd.Parameters.AddWithValue("ini", sql);
        cmd.Parameters.AddWithValue("fim", sql);
        cmd.Parameters.AddWithValue("cls", classe);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal val = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            if (val != 0)
                lista.Add((cc, nome, emp, val));
        }
        return lista;
    }

    /// <summary>
    /// Guia analítica por funcionário para eCONSIGNADO (empréstimos) ou GRRF (rescisão).
    /// Retorna (Centro, NomeEmpregado, Empregado, Valor).
    /// </summary>
    public List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)>
        GuiaAnaliticoEmprestimos(OdbcConnection conn, int classeEmprestimo = 0)
    {
        var lista = new List<(int, string, int, decimal)>();
        string filtroClasse = classeEmprestimo > 0
            ? " AND ev.classificacao = ?"
            : " AND ev.nome LIKE 'DESC. EMP. CRED. TRAB%'";
        using var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, TRIM(e.nome), m.i_empregados, ROUND(SUM(m.valor_cal),2) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos ev ON m.codi_emp = ev.codi_emp AND m.i_eventos = ev.i_eventos " +
            "LEFT JOIN bethadba.foempregados e ON m.codi_emp = e.codi_emp AND m.i_empregados = e.i_empregados " +
            "WHERE m.codi_emp = 1 AND m.tipo_proces = 11 " +
            "AND m.prov_desc = 'D'" + filtroClasse +
            " GROUP BY e.i_ccustos, e.nome, m.i_empregados " +
            "ORDER BY e.i_ccustos, e.nome", conn);
        if (classeEmprestimo > 0)
            cmd.Parameters.AddWithValue("cls", classeEmprestimo);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int emp = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            decimal val = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
            if (val != 0)
                lista.Add((cc, nome, emp, val));
        }
        return lista;
    }

    /// <summary>Valor da guia de INSS (total_guia) da competência, tipo_process 11.</summary>
    public decimal TotalGuiaInssCompetencia(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT SUM(g.total_guia) FROM bethadba.foguiainss g " +
            "WHERE g.codi_emp = 1 AND g.competencia = ? AND g.tipo_process = 11", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>Valor da guia de IRRF (focalcirrf.valor) por período/vencimento.</summary>
    public decimal TotalGuiaIrrf(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        using var cmd = new OdbcCommand(
            "SELECT SUM(g.valor) FROM bethadba.focalcirrf g " +
            "WHERE g.codi_emp = 1 AND g.vencimento >= ? AND g.vencimento <= ?", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>Valor da guia de FGTS (total_fgts) da competência, tipo_process 11.</summary>
    public decimal TotalGuiaFgts(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT SUM(g.total_fgts) FROM bethadba.fofgtsfilial g " +
            "WHERE g.codi_emp = 1 AND g.competencia = ? AND g.tipo_process = 11", conn);
        cmd.Parameters.AddWithValue("competencia", sql);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>Valor da guia de FGTS consignado (crédito do trabalhador) da competência de rescisão.</summary>
    public decimal TotalGuiaFgtsConsignado(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        using var cmd = new OdbcCommand(
            "SELECT SUM(e.VALOR_SALDO_DEVEDOR) FROM bethadba.FOEMPRESTIMOS_CRED_TRAB_RESCISAO e " +
            "WHERE e.CODI_EMP = 1 AND e.COMPETENCIA_RESCISAO >= ? AND e.COMPETENCIA_RESCISAO <= ?", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToDecimal(rd[0]);
        return 0m;
    }

    /// <summary>
    /// Guias do mês para geração de CSV de guia (1 linha por guia).
    /// Retorna (Descricao, Valor, Vencimento).
    /// </summary>
    public List<(string Descricao, decimal Valor, DateTime Vencimento)>
        ListarGuiasCompetencia(OdbcConnection conn, string comp)
    {
        var lista = new List<(string, decimal, DateTime)>();
        var sql = CompetenciaParaSql(comp);
        using var cmd = new OdbcCommand(
            "SELECT 'INSS', g.total_guia, g.vencimento FROM bethadba.foguiainss g " +
            "WHERE g.codi_emp = 1 AND g.competencia = ? AND g.tipo_process = 11 " +
            "UNION ALL " +
            "SELECT 'FGTS', g.total_fgts, g.vencimento FROM bethadba.fofgtsfilial g " +
            "WHERE g.codi_emp = 1 AND g.competencia = ? AND g.tipo_process = 11", conn);
        cmd.Parameters.AddWithValue("c1", sql);
        cmd.Parameters.AddWithValue("c2", sql);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            string desc = Convert.ToString(rd[0]) ?? "";
            decimal val = rd.IsDBNull(1) ? 0m : Convert.ToDecimal(rd[1]);
            DateTime ven = rd.IsDBNull(2) ? DateTime.MinValue : Convert.ToDateTime(rd[2]);
            if (val > 0)
                lista.Add((desc, val, ven));
        }
        return lista;
    }


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
    /// Lista as guias GRRF de rescisão (bethadba.foguiagrfc) num período de vencimento.
    /// Valor por empregado = mes_ant_valor + resc_valor + aviso_previo_valor + multa_fgts.
    /// </summary>
    /// <summary>
    /// Mapeia a classificação do evento (fomovto) para a verba da tabela_financeira.
    /// Retorna o código da verba e o plano financeiro correspondente.
    /// </summary>
    public static (int CodigoVerba, string Plano) MapearClasseVerba(int classe)
    {
        // Classificação -> verba da tabela_financeira
        switch (classe)
        {
            case 1: case 2: case 3: case 20: case 21: case 22: case 24:
                return (1, "2.02.01.02"); // salário e proventos
            case 12: return (2, "2.01.02.10"); // INSS
            case 10: case 14: return (58, "2.01.02.11"); // FGTS
            case 5: case 6: case 8: case 11: return (3, "2.01.02.02"); // férias
            case 4: case 7: case 9: return (4, "2.01.02.02"); // rescisão/13º/aviso
            case 13: case 51: case 52: case 53: return (10, "2.01.02.16"); // IRRF
            case 28: return (12, "2.01.02.03"); // vale transporte
            case 25: return (6, "2.01.02.02"); // adiantamento salarial
            case 26: case 27: case 29: case 40: case 17: return (5, "2.01.02.01"); // outros descontos (PLR/plano/pensão)
            case 49: case 50: return (20, "2.02.02.25"); // empréstimo consignado
            default: return (0, "");
        }
    }

    /// <summary>
    /// Resumo contábil por centro de custo, agrupado em PROVENTOS / DESCONTOS / ENCARGOS.
    /// </summary>
    public class CentroContabil
    {
        public int Centro { get; set; }
        public string CentroNome { get; set; } = "";
        public int Empregados { get; set; }
        public List<(string Descricao, decimal Valor)> Proventos { get; set; } = new();
        public List<(string Descricao, decimal Valor)> Descontos { get; set; } = new();
        public List<(string Descricao, decimal Valor)> Encargos { get; set; } = new();
        public decimal TotalProventos { get; set; }
        public decimal TotalDescontos { get; set; }
        public decimal TotalEncargos { get; set; }
        /// <summary>Custo total da empresa no centro = proventos + encargos - descontos (ou +, ver negócio).</summary>
        public decimal CustoTotal => TotalProventos + TotalEncargos - TotalDescontos;
    }

    /// <summary>
    /// Monta o resumo contábil por centro de custo da competência.
    /// Agrupa os movimentos por classificação em PROVENTOS (P), DESCONTOS (D) e ENCARGOS (I),
    /// e soma o líquido da folha como provento principal.
    /// </summary>
    public List<CentroContabil> ResumoContabilCentros(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var dic = new Dictionary<int, CentroContabil>();

        // líquido por centro (foliquidosfilepr)
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(DISTINCT f.i_empregados), ROUND(SUM(f.liquido),2) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn))
        {
            cmd.Parameters.AddWithValue("c", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                dic[cc] = new CentroContabil
                {
                    Centro = cc,
                    CentroNome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!,
                    Empregados = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]),
                };
            }
        }

        // movimentos por centro agrupados por classificação
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, ev.classificacao, ev.nome, m.prov_desc, ROUND(SUM(m.valor_cal),2) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos ev ON m.codi_emp = ev.codi_emp AND m.i_eventos = ev.i_eventos " +
            "LEFT JOIN bethadba.foempregados e ON m.codi_emp = e.codi_emp AND m.i_empregados = e.i_empregados " +
            "WHERE m.codi_emp = 1 AND m.data >= ? AND m.data < DATEADD(month,1,?) AND m.tipo_proces = 11 " +
            "GROUP BY e.i_ccustos, ev.classificacao, ev.nome, m.prov_desc " +
            "ORDER BY e.i_ccustos, m.prov_desc, ev.classificacao", conn))
        {
            cmd.Parameters.AddWithValue("ini", sql);
            cmd.Parameters.AddWithValue("fim", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                if (!dic.ContainsKey(cc)) continue;
                string nome = rd.IsDBNull(2) ? "" : Convert.ToString(rd[2])!;
                string prov = rd.IsDBNull(3) ? "" : Convert.ToString(rd[3])!;
                decimal val = rd.IsDBNull(4) ? 0m : Convert.ToDecimal(rd[4]);
                if (val == 0) continue;
                var item = dic[cc];
                if (prov == "D")
                {
                    // descontos: ignora adiantamento (cls 25) e estorno (cls 15) p/ não poluir, mas mantém os demais
                    item.Descontos.Add((nome, Math.Abs(val)));
                    item.TotalDescontos += Math.Abs(val);
                }
                else if (prov == "I")
                {
                    item.Encargos.Add((nome, val));
                    item.TotalEncargos += val;
                }
                else // P
                {
                    item.Proventos.Add((nome, val));
                    item.TotalProventos += val;
                }
            }
        }

        return dic.Values.OrderBy(c => c.Centro).ToList();
    }

    /// <summary>Conta contábil com débito/crédito de um centro de custo.</summary>
    public class RazaoConta
    {
        public string Plano { get; set; } = "";
        public int CodigoVerba { get; set; }
        public string Descricao { get; set; } = "";
        public decimal Debito { get; set; }
        public decimal Credito { get; set; }
        public decimal Saldo => Debito - Credito;
    }

    /// <summary>Razão contábil de todas as contas por centro de custo.</summary>
    public class RazaoCentro
    {
        public int Centro { get; set; }
        public string CentroNome { get; set; } = "";
        public int Empregados { get; set; }
        public List<RazaoConta> Contas { get; set; } = new();
        public decimal TotalDebito { get; set; }
        public decimal TotalCredito { get; set; }
    }

    /// <summary>
    /// Monta a razão contábil por centro de custo: débito (proventos+encargos) e crédito
    /// (descontos/deduções) por conta do plano financeiro, mapeando as classificações de fomovto
    /// para a tabela_financeira.
    /// </summary>
    public List<RazaoCentro> ResumoRazaoContabil(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var dic = new Dictionary<int, RazaoCentro>();

        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(DISTINCT f.i_empregados) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn))
        {
            cmd.Parameters.AddWithValue("c", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                dic[cc] = new RazaoCentro
                {
                    Centro = cc,
                    CentroNome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!,
                    Empregados = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]),
                };
            }
        }

        // líquido principal (verba 1)
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, ROUND(SUM(f.liquido),2) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "GROUP BY e.i_ccustos", conn))
        {
            cmd.Parameters.AddWithValue("c", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                if (!dic.ContainsKey(cc)) continue;
                decimal liq = rd.IsDBNull(1) ? 0m : Convert.ToDecimal(rd[1]);
                var conta = dic[cc].Contas.FirstOrDefault(a => a.CodigoVerba == 1 && a.Plano == "2.02.01.02");
                if (conta == null)
                {
                    conta = new RazaoConta { CodigoVerba = 1, Plano = "2.02.01.02", Descricao = "SALARIO CONTRATUAL / LIQUIDO" };
                    dic[cc].Contas.Add(conta);
                }
                conta.Debito += liq;
                dic[cc].TotalDebito += liq;
            }
        }

        // movimentos por classificação -> conta
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, ev.classificacao, ev.nome, m.prov_desc, ROUND(SUM(m.valor_cal),2) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos ev ON m.codi_emp = ev.codi_emp AND m.i_eventos = ev.i_eventos " +
            "LEFT JOIN bethadba.foempregados e ON m.codi_emp = e.codi_emp AND m.i_empregados = e.i_empregados " +
            "WHERE m.codi_emp = 1 AND m.data >= ? AND m.data < DATEADD(month,1,?) AND m.tipo_proces = 11 " +
            "GROUP BY e.i_ccustos, ev.classificacao, ev.nome, m.prov_desc " +
            "ORDER BY e.i_ccustos, ev.classificacao", conn))
        {
            cmd.Parameters.AddWithValue("ini", sql);
            cmd.Parameters.AddWithValue("fim", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                if (!dic.ContainsKey(cc)) continue;
                int classe = rd.IsDBNull(1) ? 0 : Convert.ToInt32(rd[1]);
                string nome = rd.IsDBNull(2) ? "" : Convert.ToString(rd[2])!;
                string prov = rd.IsDBNull(3) ? "" : Convert.ToString(rd[3])!;
                decimal val = rd.IsDBNull(4) ? 0m : Convert.ToDecimal(rd[4]);
                if (val == 0) continue;

                var (codVerba, plano) = MapearClasseVerba(classe);
                var verba = VerbaFinanceira.PorCodigo(codVerba == 0 ? 1 : codVerba);
                var conta = dic[cc].Contas.FirstOrDefault(a => a.CodigoVerba == codVerba && a.Plano == plano);
                if (conta == null)
                {
                    conta = new RazaoConta
                    {
                        CodigoVerba = codVerba,
                        Plano = plano,
                        Descricao = verba.Descricao,
                    };
                    dic[cc].Contas.Add(conta);
                }
                // débito: proventos e encargos; crédito: descontos
                // Proventos de salário (cls 1) não somam débito (já coberto pelo líquido da folha).
                if (prov == "D")
                {
                    conta.Credito += Math.Abs(val);
                    dic[cc].TotalCredito += Math.Abs(val);
                }
                else if (classe == 1)
                {
                    // salário já contabilizado pelo líquido; não soma de novo
                }
                else
                {
                    conta.Debito += Math.Abs(val);
                    dic[cc].TotalDebito += Math.Abs(val);
                }
            }
        }

        var lista = dic.Values.OrderBy(c => c.Centro).ToList();
        foreach (var c in lista)
        {
            c.Contas = c.Contas.OrderBy(a => a.Plano).ThenBy(a => a.CodigoVerba).ToList();
            // remove duplicatas (mesma verba soma) e contas sem verba mapeada
            c.Contas = c.Contas.Where(a => a.CodigoVerba > 0)
                .GroupBy(a => a.CodigoVerba + "|" + a.Plano)
                .Select(g => new RazaoConta
                {
                    Plano = g.First().Plano,
                    CodigoVerba = g.Key.Split('|')[0] == "" ? 0 : int.Parse(g.Key.Split('|')[0]),
                    Descricao = g.First().Descricao,
                    Debito = g.Sum(x => x.Debito),
                    Credito = g.Sum(x => x.Credito),
                }).OrderBy(a => a.Plano).ThenBy(a => a.CodigoVerba).ToList();
        }
        return lista;
    }

    /// <summary>Resumo por centro de custo e por verba (classificação) da competência.</summary>
    public class CentroVerbaResumo
    {
        public int Centro { get; set; }
        public string CentroNome { get; set; } = "";
        public int Empregados { get; set; }
        public List<(string Descricao, decimal Valor)> Verbas { get; set; } = new();
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Monta o resumo por centro de custo e por verba (classificação do evento) da competência.
    /// Considera o líquido por centro (foliquidosfilepr) + as verbas de fomovto por classificação.
    /// </summary>
    public List<CentroVerbaResumo> ResumoCentrosVerbas(OdbcConnection conn, string comp)
    {
        var sql = CompetenciaParaSql(comp);
        var dic = new Dictionary<int, CentroVerbaResumo>();

        // funcionários por centro (para contar empregados e ter o nome)
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, c.nome, COUNT(DISTINCT f.i_empregados) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "LEFT JOIN bethadba.foccustos c ON e.codi_emp = c.codi_emp AND e.i_ccustos = c.i_ccustos " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 " +
            "GROUP BY e.i_ccustos, c.nome ORDER BY e.i_ccustos", conn))
        {
            cmd.Parameters.AddWithValue("c", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                var item = new CentroVerbaResumo
                {
                    Centro = cc,
                    CentroNome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!,
                    Empregados = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]),
                };
                dic[cc] = item;
            }
        }

        // verbas por centro (classificação do evento) - soma líquido para proventes/descontos
        using (var cmd = new OdbcCommand(
            "SELECT e.i_ccustos, ev.nome, m.prov_desc, ROUND(SUM(m.valor_cal),2) " +
            "FROM bethadba.fomovto m " +
            "LEFT JOIN bethadba.foeventos ev ON m.codi_emp = ev.codi_emp AND m.i_eventos = ev.i_eventos " +
            "LEFT JOIN bethadba.foempregados e ON m.codi_emp = e.codi_emp AND m.i_empregados = e.i_empregados " +
            "WHERE m.codi_emp = 1 AND m.data >= ? AND m.data < DATEADD(month,1,?) AND m.tipo_proces = 11 " +
            "GROUP BY e.i_ccustos, ev.nome, m.prov_desc " +
            "ORDER BY e.i_ccustos, ev.nome", conn))
        {
            cmd.Parameters.AddWithValue("ini", sql);
            cmd.Parameters.AddWithValue("fim", sql);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int cc = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
                if (!dic.ContainsKey(cc)) continue;
                string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
                string prov = rd.IsDBNull(2) ? "" : Convert.ToString(rd[2])!;
                decimal val = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]);
                if (val == 0) continue;
                // Ignora adiantamento/estorno para não inflar; soma o restante como verba
                string rotulo = prov == "D" ? "- " + nome : nome;
                dic[cc].Verbas.Add((rotulo, val));
                dic[cc].Total += prov == "D" ? -val : val;
            }
        }

        return dic.Values.OrderBy(c => c.Centro).ToList();
    }

    public string NomeCentroCusto(OdbcConnection conn, int centro)
    {
        using var cmd = new OdbcCommand(
            "SELECT nome FROM bethadba.foccustos WHERE codi_emp = 1 AND i_ccustos = ?", conn);
        cmd.Parameters.AddWithValue("cc", centro);
        using var rd = cmd.ExecuteReader();
        if (rd.Read() && !rd.IsDBNull(0))
            return Convert.ToString(rd[0]) ?? "";
        return centro.ToString();
    }

    /// <summary>Detalhamento completo de um funcionário para o relatório por centro.</summary>
    public class FuncionarioDetalhe
    {
        public int Empregado { get; set; }
        public string Nome { get; set; } = "";
        public int Centro { get; set; }
        public decimal Liquido { get; set; }
        public List<(string Nome, string ProvDesc, decimal Valor)> Proventos { get; set; } = new();
        public List<(string Nome, string ProvDesc, decimal Valor)> Descontos { get; set; } = new();
        public decimal TotalProventos { get; set; }
        public decimal TotalDescontos { get; set; }
        public List<(string Descricao, decimal Valor, int Parcelas, string Contrato)> Emprestimos { get; set; } = new();
        public decimal TotalEmprestimos { get; set; }
        public decimal Fgts { get; set; }
        public decimal Inss { get; set; }
        public decimal Irrf { get; set; }
    }

    /// <summary>
    /// Monta o detalhamento completo por funcionário de um centro (ou de todos se centro &lt;= 0),
    /// na competência. Proventos/Descontos/FGTS/INSS/IRRF vêm de fomovto; empréstimos de FOEMPRESTIMOS_CONSIGNADOS.
    /// </summary>
    public List<FuncionarioDetalhe> DetalhamentoPorFuncionario(OdbcConnection conn, string comp, int centro = -1)
    {
        var sql = CompetenciaParaSql(comp);
        var lista = new List<FuncionarioDetalhe>();

        // funcionários com líquido na competência
        var sqlFunc = "SELECT e.i_empregados, TRIM(e.nome), e.i_ccustos, ROUND(f.liquido,2) " +
            "FROM bethadba.foliquidosfilepr f " +
            "JOIN bethadba.foliquidosfil l ON f.I_LIQUIDOSFIL = l.I_LIQUIDOSFIL " +
            "LEFT JOIN bethadba.foempregados e ON f.codi_emp = e.codi_emp AND f.i_empregados = e.i_empregados " +
            "WHERE l.competencia = ? AND f.codi_emp = 1 AND l.tipo_process = 11 AND ROUND(f.liquido,2) > 0";
        if (centro > 0) sqlFunc += " AND e.i_ccustos = ?";
        sqlFunc += " ORDER BY e.nome";

        using (var cmd = new OdbcCommand(sqlFunc, conn))
        {
            cmd.Parameters.AddWithValue("c", sql);
            if (centro > 0) cmd.Parameters.AddWithValue("cc", centro);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new FuncionarioDetalhe
                {
                    Empregado = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]),
                    Nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!,
                    Centro = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]),
                    Liquido = rd.IsDBNull(3) ? 0m : Convert.ToDecimal(rd[3]),
                });
            }
        }

        foreach (var f in lista)
        {
            // movimentos por evento
            using var cmd = new OdbcCommand(
                "SELECT e.nome, m.prov_desc, ROUND(SUM(m.valor_cal),2) " +
                "FROM bethadba.fomovto m " +
                "LEFT JOIN bethadba.foeventos e ON m.codi_emp = e.codi_emp AND m.i_eventos = e.i_eventos " +
                "WHERE m.codi_emp = 1 AND m.i_empregados = ? AND m.data >= ? AND m.data < DATEADD(month,1,?) " +
                "AND m.tipo_proces = 11 " +
                "GROUP BY e.nome, m.prov_desc ORDER BY m.prov_desc, e.nome", conn);
            cmd.Parameters.AddWithValue("emp", f.Empregado);
            cmd.Parameters.AddWithValue("ini", sql);
            cmd.Parameters.AddWithValue("fim", sql);
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    string evento = rd.IsDBNull(0) ? "" : Convert.ToString(rd[0])!;
                    string prov = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
                    decimal val = rd.IsDBNull(2) ? 0m : Convert.ToDecimal(rd[2]);
                    if (prov == "P")
                    {
                        f.Proventos.Add((evento, prov, val));
                        f.TotalProventos += val;
                    }
                    else if (prov == "D")
                    {
                        f.Descontos.Add((evento, prov, val));
                        f.TotalDescontos += val;
                    }
                    else if (prov == "I")
                    {
                        // encargos/guias por funcionário
                        if (evento.Contains("F.G.T.S", StringComparison.OrdinalIgnoreCase) || evento.Contains("FGTS", StringComparison.OrdinalIgnoreCase))
                            f.Fgts += val;
                        else if (evento.Contains("I.N.S.S", StringComparison.OrdinalIgnoreCase))
                            f.Inss += val;
                        else if (evento.Contains("IMPOSTO", StringComparison.OrdinalIgnoreCase) || evento.Contains("IRRF", StringComparison.OrdinalIgnoreCase))
                            f.Irrf += val;
                    }
                }
            }

            // empréstimos consignados
            using var cmdEmp = new OdbcCommand(
                "SELECT DESCRICAO, ROUND(VALOR,2), QUANTIDADE_PARCELAS, NUMERO_CONTRATO " +
                "FROM bethadba.FOEMPRESTIMOS_CONSIGNADOS " +
                "WHERE CODI_EMP = 1 AND I_EMPREGADOS = ?", conn);
            cmdEmp.Parameters.AddWithValue("emp", f.Empregado);
            using (var rdEmp = cmdEmp.ExecuteReader())
            {
                while (rdEmp.Read())
                {
                    string desc = rdEmp.IsDBNull(0) ? "" : Convert.ToString(rdEmp[0])!;
                    decimal val = rdEmp.IsDBNull(1) ? 0m : Convert.ToDecimal(rdEmp[1]);
                    int par = rdEmp.IsDBNull(2) ? 0 : Convert.ToInt32(rdEmp[2]);
                    string contr = rdEmp.IsDBNull(3) ? "" : Convert.ToString(rdEmp[3])!;
                    f.Emprestimos.Add((desc, val, par, contr));
                    f.TotalEmprestimos += val;
                }
            }
        }
        return lista;
    }

    public List<(int IEmpregados, string Nome, int ICcustos, DateTime Vencimento, decimal Valor)>
        ListarGrf(OdbcConnection conn, DateTime ini, DateTime fim)
    {
        var lista = new List<(int, string, int, DateTime, decimal)>();
        using var cmd = new OdbcCommand(
            "SELECT e.i_empregados, TRIM(e.nome), e.i_ccustos, g.vencimento, " +
            "ROUND(g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts, 2) AS valor " +
            "FROM bethadba.foguiagrfc g " +
            "LEFT JOIN bethadba.foempregados e ON g.codi_emp = e.codi_emp AND g.i_empregados = e.i_empregados " +
            "WHERE g.codi_emp = 1 AND g.vencimento >= ? AND g.vencimento <= ? " +
            "AND (g.mes_ant_valor + g.resc_valor + g.aviso_previo_valor + g.multa_fgts) > 0 " +
            "ORDER BY g.vencimento, e.nome", conn);
        cmd.Parameters.AddWithValue("ini", ini);
        cmd.Parameters.AddWithValue("fim", fim);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            int emp = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd[0]);
            string nome = rd.IsDBNull(1) ? "" : Convert.ToString(rd[1])!;
            int cc = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
            DateTime ven = rd.IsDBNull(3) ? DateTime.MinValue : Convert.ToDateTime(rd[3]);
            decimal val = rd.IsDBNull(4) ? 0m : Convert.ToDecimal(rd[4]);
            lista.Add((emp, nome, cc, ven, val));
        }
        return lista;
    }

    /// <summary>
    /// Gera o CSV do lote GRRF no formato da planilha manual.
    /// Layout: 59;centro;37;GRRF;valor;vencimento;;;;;GRRF-dd-MM-yyyy-n;NOME
    /// </summary>
    public static string GerarCsvGrf(
        List<(int IEmpregados, string Nome, int ICcustos, DateTime Vencimento, decimal Valor)> linhas,
        string centro, DateTime dataLote, string verba = "59")
    {
        var sb = new System.Text.StringBuilder();
        string lote = dataLote.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        int n = 1;
        foreach (var l in linhas)
        {
            var valor = l.Valor.ToString("0.00", CultureInfo.InvariantCulture);
            var venc = l.Vencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            sb.AppendLine($"{verba};{centro};37;GRRF;{valor};{venc};;;;;GRRF-{lote}-{n};{l.Nome}");
            n++;
        }
        return sb.ToString();
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
