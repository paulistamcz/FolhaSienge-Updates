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
    private readonly Label _lblApi = new();
    private readonly DataTable _dt = new();

    public PromptMapeamento(
        List<(int Centro, string Nome)> centros,
        Dictionary<int, int> mapaAtual,
        string titulo)
    {
        Text = titulo;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        ClientSize = new System.Drawing.Size(560, 452);
        MinimumSize = new System.Drawing.Size(480, 360);

        _dt.Columns.Add("Centro", typeof(int));
        _dt.Columns.Add("Nome", typeof(string));
        _dt.Columns.Add("Sienge", typeof(int));
        foreach (var c in centros.OrderBy(x => x.Centro))
        {
            int sienge = mapaAtual.TryGetValue(c.Centro, out var s) && s > 0 ? s : c.Centro;
            _dt.Rows.Add(c.Centro, c.Nome, sienge);
        }

        var topo = new Label
        {
            Text = "Informe a obra do Sienge (coluna B) para cada centro. Salva ao confirmar.",
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

        var colCentro = new DataGridViewTextBoxColumn { DataPropertyName = "Centro", HeaderText = "Centro (Domínio)", Width = 110, ReadOnly = true };
        var colNome = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        var colSienge = new DataGridViewTextBoxColumn { DataPropertyName = "Sienge", HeaderText = "Obra (Sienge B)", Width = 110 };
        _dgv.Columns.AddRange(colCentro, colNome, colSienge);
        _dgv.DataSource = _dt;

        _lblApi.AutoSize = true;
        _lblApi.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lblApi.Location = new System.Drawing.Point(12, 352);
        _lblApi.Size = new System.Drawing.Size(536, 20);
        _lblApi.ForeColor = System.Drawing.Color.Gray;
        _lblApi.Text = "";

        // TODO-API: reexibir ao finalizar a validação via API do Sienge.
        _btnValidar.Visible = false;
        _btnConfig.Visible = false;
        _lblApi.Visible = false;
        _btnValidar.Text = "Validar no Sienge";
        _btnValidar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnValidar.Location = new System.Drawing.Point(12, 404);
        _btnValidar.Size = new System.Drawing.Size(130, 30);
        _btnValidar.Click += async (s, e) => await ValidarNoSiengeAsync();

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

    /// <summary>Confere as obras digitadas contra o cadastro do Sienge via API.</summary>
    private async Task ValidarNoSiengeAsync()
    {
        _dgv.EndEdit();
        var cfg = SiengeApi.CarregarConfig();
        if (!cfg.Preenchida)
        {
            using var dlgCfg = new PromptSiengeApi(cfg);
            if (dlgCfg.ShowDialog(this) != DialogResult.OK) return;
            cfg = dlgCfg.Config;
            SiengeApi.SalvarConfig(cfg);
        }
        _btnValidar.Enabled = false;
        _lblApi.ForeColor = System.Drawing.Color.DarkOrange;
        _lblApi.Text = "Consultando obras no Sienge...";
        try
        {
            var api = new SiengeApi(cfg);
            var (ok, msg, obras) = await api.ListarObrasAsync();
            if (!ok)
            {
                _lblApi.ForeColor = System.Drawing.Color.Red;
                _lblApi.Text = msg;
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
            if (faltando.Count == 0)
            {
                _lblApi.ForeColor = System.Drawing.Color.Green;
                _lblApi.Text = $"OK: {_dt.Rows.Count} obra(s) encontrada(s) no Sienge.";
            }
            else
            {
                _lblApi.ForeColor = System.Drawing.Color.Red;
                _lblApi.Text = $"Faltam no Sienge: {string.Join(", ", faltando)}. Cadastre as obras e valide de novo.";
            }
        }
        finally
        {
            _btnValidar.Enabled = true;
        }
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
