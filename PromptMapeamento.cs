using System.Data;

namespace FolhaSienge;

/// <summary>
/// Janela modal que pede o mapeamento da coluna B (centro Domínio -&gt; obra Sienge)
/// no momento de gerar o CSV de Folha/Férias/Rescisões. Vem preenchido com o
/// mapeamento salvo (ou o próprio centro quando não há mapa) e grava ao confirmar.
/// </summary>
public class PromptMapeamento : Form
{
    private readonly DataGridView _dgv = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();
    private readonly Button _btnValidar = new();
    private readonly Button _btnConfig = new();
    private readonly Button _btnRelatorio = new();
    private readonly Label _lblApi = new();
    private readonly DataTable _dt = new();
    private readonly int _empresa;
    private readonly Func<List<(int Centro, string Nome, int Funcs, DateTime PrimeiroMov, DateTime UltimoMov)>>? _carregarDominio;

    public PromptMapeamento(
        List<(int Centro, string Nome)> centros,
        Dictionary<int, int> mapaAtual,
        string titulo,
        int empresa = 0,
        Func<List<(int Centro, string Nome, int Funcs, DateTime PrimeiroMov, DateTime UltimoMov)>>? carregarDominio = null,
        Dictionary<int, int>? sugestoes = null)
    {
        _empresa = empresa;
        _carregarDominio = carregarDominio;
        Text = titulo;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        ClientSize = new System.Drawing.Size(560, 452);
        MinimumSize = new System.Drawing.Size(480, 360);

        _dt.Columns.Add("Centro", typeof(int));
        _dt.Columns.Add("Nome", typeof(string));
        _dt.Columns.Add("Sienge", typeof(int));
        var sugeridos = new HashSet<int>();
        foreach (var c in centros.OrderBy(x => x.Centro))
        {
            int sienge;
            if (mapaAtual.TryGetValue(c.Centro, out var s) && s > 0)
                sienge = s;
            else if (sugestoes != null && sugestoes.TryGetValue(c.Centro, out var sg) && sg > 0)
            {
                sienge = sg;
                sugeridos.Add(c.Centro);
            }
            else
                sienge = c.Centro;
            _dt.Rows.Add(c.Centro, c.Nome, sienge);
        }

        var topo = new Label
        {
            Text = sugeridos.Count > 0
                ? $"Informe a obra do Sienge (coluna B). {sugeridos.Count} sugestão(ões) em amarelo - confira."
                : "Informe a obra do Sienge (coluna B) para cada centro. Salva ao confirmar.",
            AutoSize = true,
            Location = new System.Drawing.Point(12, 10)
        };

        _dgv.Dock = DockStyle.None;
        _dgv.Location = new System.Drawing.Point(12, 34);
        _dgv.Size = new System.Drawing.Size(536, 312);
        _dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgv.AutoGenerateColumns = false;
        _dgv.AllowUserToAddRows = false;
        _dgv.AllowUserToDeleteRows = false;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var colCentro = new DataGridViewTextBoxColumn { Name = "Centro", DataPropertyName = "Centro", HeaderText = "Centro (Domínio)", Width = 110, ReadOnly = true };
        var colNome = new DataGridViewTextBoxColumn { Name = "Nome", DataPropertyName = "Nome", HeaderText = "Nome", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        var colSienge = new DataGridViewTextBoxColumn { Name = "Sienge", DataPropertyName = "Sienge", HeaderText = "Obra (Sienge B)", Width = 110 };
        _dgv.Columns.AddRange(colCentro, colNome, colSienge);
        _dgv.DataSource = _dt;
        _dgv.DataBindingComplete += (s, e) =>
        {
            foreach (DataGridViewRow row in _dgv.Rows)
            {
                if (row.Cells["Centro"].Value != null &&
                    int.TryParse(Convert.ToString(row.Cells["Centro"].Value), out var cc) &&
                    sugeridos.Contains(cc))
                    row.Cells["Sienge"].Style.BackColor = System.Drawing.Color.LightYellow;
            }
        };

        _lblApi.AutoSize = true;
        _lblApi.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lblApi.Location = new System.Drawing.Point(12, 352);
        _lblApi.Size = new System.Drawing.Size(536, 20);
        _lblApi.ForeColor = System.Drawing.Color.Gray;
        _lblApi.Text = "";

        _btnValidar.Text = "Validar obras";
        _btnValidar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnValidar.Location = new System.Drawing.Point(12, 404);
        _btnValidar.Size = new System.Drawing.Size(130, 30);
        _btnValidar.Click += (s, e) => ValidarEspelho();

        _btnConfig.Text = "API...";
        _btnConfig.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnConfig.Location = new System.Drawing.Point(148, 404);
        _btnConfig.Size = new System.Drawing.Size(80, 30);
        _btnConfig.Click += (s, e) =>
        {
            using var cfg = new PromptSiengeApi(SiengeApi.CarregarConfig());
            if (cfg.ShowDialog(this) == DialogResult.OK)
                SiengeApi.SalvarConfig(cfg.Config);
        };

        _btnRelatorio.Text = "Relatório Dom×Sien";
        _btnRelatorio.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnRelatorio.Location = new System.Drawing.Point(234, 404);
        _btnRelatorio.Size = new System.Drawing.Size(132, 30);
        _btnRelatorio.Click += (s, e) => AbrirRelatorio();

        _btnOk.Text = "OK";
        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Location = new System.Drawing.Point(372, 404);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (s, e) => Validar();

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancelar.Location = new System.Drawing.Point(460, 404);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        Controls.Add(topo);
        Controls.Add(_dgv);
        Controls.Add(_lblApi);
        Controls.Add(_btnValidar);
        Controls.Add(_btnConfig);
        Controls.Add(_btnRelatorio);
        Controls.Add(_btnOk);
        Controls.Add(_btnCancelar);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
    }

    private void Validar()
    {
        _dgv.EndEdit();
        foreach (DataRow r in _dt.Rows)
        {
            if (!int.TryParse(Convert.ToString(r["Sienge"]), out var v) || v <= 0)
            {
                MessageBox.Show(this,
                    $"Obra inválida para o centro {r["Centro"]}. Informe um número maior que zero.",
                    "Mapeamento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
        }
    }

    /// <summary>Confere as obras digitadas contra o espelho local (instantâneo, offline).</summary>
    private void ValidarEspelho()
    {
        _dgv.EndEdit();
        var (data, obras) = SiengeApi.CarregarEspelhoObras();
        if (obras.Count == 0)
        {
            _lblApi.ForeColor = System.Drawing.Color.DarkOrange;
            _lblApi.Text = "Sem espelho de obras: use o botão API... > Atualizar do Sienge ou Importar CSV.";
            return;
        }
        var codigos = new HashSet<string>(obras.Select(o => o.Codigo.Trim()),
            StringComparer.OrdinalIgnoreCase);
        var faltando = new List<string>();
        foreach (DataRow r in _dt.Rows)
        {
            var sienge = Convert.ToString(r["Sienge"])?.Trim() ?? "";
            bool existe = codigos.Contains(sienge) ||
                (int.TryParse(sienge, out var n) && codigos.Contains(n.ToString()));
            var row = _dgv.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(x => Convert.ToString(x.Cells["Centro"].Value) == Convert.ToString(r["Centro"]));
            if (row != null)
            {
                row.DefaultCellStyle.BackColor = existe
                    ? System.Drawing.Color.White
                    : System.Drawing.Color.MistyRose;
                row.DefaultCellStyle.SelectionBackColor = existe
                    ? _dgv.DefaultCellStyle.SelectionBackColor
                    : System.Drawing.Color.Salmon;
            }
            if (!existe) faltando.Add(Convert.ToString(r["Centro"]) ?? "?");
        }
        _dgv.Refresh();
        string idade = SiengeApi.IdadeEspelho(data);
        if (faltando.Count == 0)
        {
            _lblApi.ForeColor = System.Drawing.Color.Green;
            _lblApi.Text = $"OK: {_dt.Rows.Count} obra(s) no espelho ({idade}).";
        }
        else
        {
            _lblApi.ForeColor = System.Drawing.Color.Red;
            _lblApi.Text = $"Faltam no espelho ({idade}): {string.Join(", ", faltando)}.";
        }
    }

    /// <summary>Abre o relatório Domínio x Sienge com o mapa atual da grade.</summary>
    private void AbrirRelatorio()
    {
        if (_carregarDominio == null)
        {
            MessageBox.Show(this, "Relatório indisponível neste ponto.",
                "Relatório", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Cursor = Cursors.WaitCursor;
        List<(int Centro, string Nome, int Funcs, DateTime PrimeiroMov, DateTime UltimoMov)>? dados = null;
        Exception? erro = null;
        try { dados = _carregarDominio(); }
        catch (Exception ex) { erro = ex; }
        finally { Cursor = Cursors.Default; }
        if (erro != null || dados == null)
        {
            MessageBox.Show(this, "Falha ao ler centros do Domínio:\n\n" + erro?.Message,
                "Relatório", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        _dgv.EndEdit();
        var mapa = new Dictionary<int, int>();
        foreach (DataRow r in _dt.Rows)
            if (int.TryParse(Convert.ToString(r["Centro"]), out var cc))
                mapa[cc] = int.TryParse(Convert.ToString(r["Sienge"]), out var s) ? s : 0;
        using var rep = new RelatorioDominioSienge(dados, _empresa, mapa);
        rep.ShowDialog(this);
    }

    /// <summary>Mapa confirmado (centro Domínio -&gt; obra Sienge).</summary>
    public Dictionary<int, int> Mapa
    {
        get
        {
            var mapa = new Dictionary<int, int>();
            foreach (DataRow r in _dt.Rows)
                mapa[Convert.ToInt32(r["Centro"])] = Convert.ToInt32(r["Sienge"]);
            return mapa;
        }
    }
}
