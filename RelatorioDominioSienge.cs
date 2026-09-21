using System.Data;
using System.Globalization;

namespace FolhaSienge;

/// <summary>
/// Relatório Domínio x Sienge: para cada centro do Domínio mostra os dados locais
/// (funcionários, 1º/último movimento, ativa?) e, após busca na API, os dados no
/// Sienge (obra, situação, criação, alteração). Permite salvar em CSV.
/// </summary>
public class RelatorioDominioSienge : Form
{
    private readonly DataGridView _dgv = new();
    private readonly Button _btnBuscar = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnFechar = new();
    private readonly Label _lblStatus = new();
    private readonly DataTable _dt = new();
    private readonly int _empresa;
    private readonly Dictionary<int, int> _mapa;

    public RelatorioDominioSienge(
        List<(int Centro, string Nome, int Funcs, DateTime PrimeiroMov, DateTime UltimoMov)> dominio,
        int empresa,
        Dictionary<int, int> mapa)
    {
        _empresa = empresa;
        _mapa = new Dictionary<int, int>(mapa);

        Text = $"Relatório Domínio x Sienge - Empresa {empresa}";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(1100, 560);
        MinimumSize = new System.Drawing.Size(900, 400);

        _dt.Columns.Add("CCDom", typeof(int));
        _dt.Columns.Add("NomeDom", typeof(string));
        _dt.Columns.Add("Func", typeof(int));
        _dt.Columns.Add("Primeiro", typeof(string));
        _dt.Columns.Add("Ultimo", typeof(string));
        _dt.Columns.Add("SitDom", typeof(string));
        _dt.Columns.Add("IDSienge", typeof(int));
        _dt.Columns.Add("NomeSienge", typeof(string));
        _dt.Columns.Add("Fonte", typeof(string));
        _dt.Columns.Add("Situacao", typeof(string));
        _dt.Columns.Add("Criacao", typeof(string));
        _dt.Columns.Add("Alteracao", typeof(string));
        _dt.Columns.Add("Setores", typeof(int));
        _dt.Columns.Add("EmpSienge", typeof(string));

        foreach (var d in dominio.OrderBy(x => x.Centro))
        {
            bool ativa = d.Funcs > 0 ||
                (d.UltimoMov != DateTime.MinValue && d.UltimoMov >= DateTime.Now.AddMonths(-3));
            _mapa.TryGetValue(d.Centro, out var idSienge);
            _dt.Rows.Add(d.Centro, d.Nome, d.Funcs,
                d.PrimeiroMov == DateTime.MinValue ? "-" : d.PrimeiroMov.ToString("MM/yyyy"),
                d.UltimoMov == DateTime.MinValue ? "-" : d.UltimoMov.ToString("MM/yyyy"),
                ativa ? "Ativa" : "Inativa",
                idSienge, "", "", "", "", "", 0, "");
        }

        _dgv.Dock = DockStyle.None;
        _dgv.Location = new System.Drawing.Point(12, 12);
        _dgv.Size = new System.Drawing.Size(1076, 452);
        _dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgv.AutoGenerateColumns = false;
        _dgv.AllowUserToAddRows = false;
        _dgv.AllowUserToDeleteRows = false;
        _dgv.RowHeadersVisible = false;
        _dgv.ReadOnly = true;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        void Col(string prop, string titulo, int larg, bool fill = false)
        {
            var c = new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop, HeaderText = titulo, Width = larg,
                AutoSizeMode = fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None
            };
            _dgv.Columns.Add(c);
        }
        Col("CCDom", "CC Dom", 60);
        Col("NomeDom", "Nome Domínio", 220, true);
        Col("Func", "Func", 50);
        Col("Primeiro", "1º Mov", 70);
        Col("Ultimo", "Últ Mov", 70);
        Col("SitDom", "Sit Dom", 60);
        Col("IDSienge", "ID Sienge", 70);
        Col("NomeSienge", "Nome Sienge", 200, true);
        Col("Fonte", "Fonte", 60);
        Col("Situacao", "Situação", 130);
        Col("Criacao", "Criação", 80);
        Col("Alteracao", "Alteração", 80);
        Col("Setores", "Set.", 45);
        Col("EmpSienge", "Emp Sienge", 90);
        _dgv.DataSource = _dt;

        _lblStatus.AutoSize = true;
        _lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lblStatus.Location = new System.Drawing.Point(12, 474);
        _lblStatus.Size = new System.Drawing.Size(700, 15);
        _lblStatus.ForeColor = System.Drawing.Color.Gray;
        _lblStatus.Text = $"{_dt.Rows.Count} centros no Domínio. Clique em Buscar no Sienge.";

        _btnBuscar.Text = "Buscar no Sienge";
        _btnBuscar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnBuscar.Location = new System.Drawing.Point(700, 500);
        _btnBuscar.Size = new System.Drawing.Size(110, 30);
        _btnBuscar.Click += async (s, e) => await BuscarNoSiengeAsync();

        var btnApi = new Button
        {
            Text = "API...",
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Location = new System.Drawing.Point(816, 500),
            Size = new System.Drawing.Size(66, 30),
        };
        btnApi.Click += (s, e) =>
        {
            using var cfg = new PromptSiengeApi(SiengeApi.CarregarConfig());
            if (cfg.ShowDialog(this) == DialogResult.OK)
                SiengeApi.SalvarConfig(cfg.Config);
        };

        _btnSalvar.Text = "Salvar CSV";
        _btnSalvar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnSalvar.Location = new System.Drawing.Point(888, 500);
        _btnSalvar.Size = new System.Drawing.Size(90, 30);
        _btnSalvar.Click += (s, e) => SalvarCsv();

        _btnFechar.Text = "Fechar";
        _btnFechar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnFechar.Location = new System.Drawing.Point(984, 500);
        _btnFechar.Size = new System.Drawing.Size(80, 30);
        _btnFechar.DialogResult = DialogResult.OK;

        Controls.Add(_dgv);
        Controls.Add(_lblStatus);
        Controls.Add(_btnBuscar);
        Controls.Add(btnApi);
        Controls.Add(_btnSalvar);
        Controls.Add(_btnFechar);

        AcceptButton = _btnFechar;
        CancelButton = _btnFechar;

        // Busca automática ao abrir, se a API já estiver configurada (sem pedir nada).
        Shown += async (s, e) => await BuscarAutomaticaAsync();
    }

    private bool _buscouAutomatico;

    private async Task BuscarAutomaticaAsync()
    {
        if (_buscouAutomatico) return;
        _buscouAutomatico = true;
        if (!SiengeApi.CarregarConfig().Preenchida) return;
        await BuscarNoSiengeAsync();
    }

    private static string TraduzSituacao(string? s)
    {
        var t = (s ?? "").Trim().ToUpperInvariant();
        return t switch
        {
            "IN_PROGRESS" => "Em andamento",
            "FINISHED_WITH_FINANCIAL_PENDENCIES" => "Encerrada com pend.",
            "FINISHED_WITHOUT_FINANCIAL_PENDENCIES" => "Encerrada",
            "COST_ESTIMATING" => "Orçamento",
            "ACTIVE" => "Ativo",
            "INACTIVE_WITH_PENDENCIES" => "Inativo com pend.",
            "INACTIVE_WITHOUT_PENDENCIES" => "Inativo",
            "" => "-",
            _ => (s ?? "").Trim(),
        };
    }

    private static string FmtData(string iso)
    {
        if (DateTime.TryParse((iso ?? "").Trim(), CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d))
            return d.ToString("dd/MM/yyyy");
        var t = (iso ?? "").Trim();
        return t.Length >= 10 ? t.Substring(0, 10) : t;
    }

    private async Task BuscarNoSiengeAsync()
    {
        // 1) Espelho local: nomes e existência (instantâneo, offline).
        var (dataEsp, obrasEsp) = SiengeApi.CarregarEspelhoObras();
        var (dataEmp, empresasEsp) = SiengeApi.CarregarEspelhoEmpresas();
        if (obrasEsp.Count == 0)
        {
            _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
            _lblStatus.Text = "Sem espelho: use o botão API... > Atualizar do Sienge ou Importar CSV.";
            return;
        }
        var porCodigo = new Dictionary<string, (string Nome, string Empresa)>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in obrasEsp) porCodigo[o.Codigo.Trim()] = (o.Nome, o.Empresa);
        var empNome = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in empresasEsp) empNome[e.Id.Trim()] = e.Nome;
        int comMapa = 0;
        foreach (DataRow r in _dt.Rows)
        {
            int sid = Convert.ToInt32(r["IDSienge"]);
            if (sid <= 0) { r["NomeSienge"] = "(sem mapa)"; continue; }
            if (!porCodigo.TryGetValue(sid.ToString(), out var oe) &&
                !porCodigo.TryGetValue(sid.ToString("D4"), out oe))
            {
                r["NomeSienge"] = "(não existe)";
                continue;
            }
            comMapa++;
            r["NomeSienge"] = oe.Nome;
            r["Fonte"] = "Espelho";
            r["EmpSienge"] = empNome.TryGetValue(oe.Empresa.Trim(), out var en) && en != ""
                ? en : oe.Empresa;
        }
        _dgv.Refresh();
        _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
        _lblStatus.Text = $"Espelho ({SiengeApi.IdadeEspelho(dataEsp)}): {comMapa} centro(s) localizado(s). Buscando situação...";
        Application.DoEvents();

        // 2) Detalhe ao vivo (situação/datas/setores) só com API configurada.
        var cfg = SiengeApi.CarregarConfig();
        if (!cfg.Preenchida)
        {
            _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
            _lblStatus.Text += " Detalhe indisponível: configure a API (botão API...).";
            return;
        }
        var ids = _dt.Rows.Cast<DataRow>()
            .Select(r => Convert.ToInt32(r["IDSienge"]))
            .Where(v => v > 0).Distinct().OrderBy(v => v).ToList();
        _btnBuscar.Enabled = false;
        try
        {
            var api = new SiengeApi(cfg);
            int i = 0, ok = 0;
            foreach (var id in ids)
            {
                i++;
                _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
                _lblStatus.Text = $"Detalhe {i}/{ids.Count}: obra {id}...";
                Application.DoEvents();
                var (bok, bmsg, det) = await api.BuscarObraAsync(id);
                if (!bok)
                {
                    _lblStatus.ForeColor = System.Drawing.Color.Red;
                    _lblStatus.Text = bmsg;
                    return;
                }
                if (det != null && det.Encontrado)
                {
                    ok++;
                    foreach (DataRow r in _dt.Rows)
                    {
                        if (Convert.ToInt32(r["IDSienge"]) != id) continue;
                        r["NomeSienge"] = det.Nome;
                        r["Fonte"] = det.Fonte;
                        r["Situacao"] = TraduzSituacao(det.Situacao);
                        r["Criacao"] = FmtData(det.Criacao);
                        r["Alteracao"] = FmtData(det.Alteracao);
                        r["Setores"] = det.Setores;
                        if (det.Empresa.Trim() != "") r["EmpSienge"] = det.Empresa;
                    }
                }
            }
            _dgv.Refresh();
            _lblStatus.ForeColor = System.Drawing.Color.Green;
            _lblStatus.Text = $"OK: detalhe de {ok}/{ids.Count} obra(s) (espelho {SiengeApi.IdadeEspelho(dataEsp)}).";
        }
        finally
        {
            _btnBuscar.Enabled = true;
        }
    }

    private void SalvarCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"Relatorio_Dominio_Sienge_EMP{_empresa}.csv"
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("CC_DOM;NOME_DOMINIO;FUNC;PRIMEIRO_MOV;ULTIMO_MOV;SIT_DOM;ID_SIENGE;NOME_SIENGE;FONTE;SITUACAO;CRIACAO;ALTERACAO;SETORES;EMP_SIENGE");
        foreach (DataRow r in _dt.Rows)
        {
            sb.Append(string.Join(";", new[]
            {
                Convert.ToString(r["CCDom"]), csvEsc(Convert.ToString(r["NomeDom"])),
                Convert.ToString(r["Func"]), Convert.ToString(r["Primeiro"]),
                Convert.ToString(r["Ultimo"]), Convert.ToString(r["SitDom"]),
                Convert.ToString(r["IDSienge"]), csvEsc(Convert.ToString(r["NomeSienge"])),
                Convert.ToString(r["Fonte"]), csvEsc(Convert.ToString(r["Situacao"])),
                Convert.ToString(r["Criacao"]), Convert.ToString(r["Alteracao"]),
                Convert.ToString(r["Setores"]), csvEsc(Convert.ToString(r["EmpSienge"])),
            }));
            sb.AppendLine();
        }
        System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.GetEncoding(28591));
        _lblStatus.ForeColor = System.Drawing.Color.Green;
        _lblStatus.Text = "Arquivo salvo: " + sfd.FileName;
    }

    private static string csvEsc(string? v) => (v ?? "").Replace(";", ",");
}
