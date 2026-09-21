using System.Data;

namespace FolhaSienge;

/// <summary>
/// Confirma/edita o credor (colunas C/D) em grade estilo seleção de funcionários:
/// mostra Código, Nome no Sienge e Ativo; o código é editável e o Buscar
/// preenche os dados via API. Overrides salvos em credores_sienge.csv.
/// </summary>
public class PromptCredor : Form
{
    private readonly DataGridView _dgv = new();
    private readonly CheckBox _chkUsar = new();
    private readonly Button _btnBuscar = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();
    private readonly Label _lblStatus = new();
    private readonly DataTable _dt = new();

    private readonly string _nomePadrao;

    public PromptCredor(int verba, string codigoAtual, string nomePadrao)
    {
        _nomePadrao = nomePadrao ?? "";

        Text = $"Credor (coluna C/D) - Verba {verba}";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        ClientSize = new System.Drawing.Size(560, 380);
        MinimumSize = new System.Drawing.Size(480, 300);

        int cod0 = int.TryParse((codigoAtual ?? "").Trim(), out var c0) && c0 > 0 ? c0 : 0;

        _dt.Columns.Add("Sel", typeof(bool));
        _dt.Columns.Add("Codigo", typeof(int));
        _dt.Columns.Add("Nome", typeof(string));
        _dt.Columns.Add("Ativo", typeof(string));
        _dt.Columns.Add("Ok", typeof(bool));
        _dt.Rows.Add(true, cod0, "", "", false);

        var topo = new Label
        {
            Text = "Confira/edite o credor. Clique em Buscar para trazer os dados do Sienge.",
            AutoSize = true,
            Location = new System.Drawing.Point(12, 10)
        };

        _dgv.Location = new System.Drawing.Point(12, 32);
        _dgv.Size = new System.Drawing.Size(536, 220);
        _dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgv.AutoGenerateColumns = false;
        _dgv.AllowUserToAddRows = false;
        _dgv.AllowUserToDeleteRows = false;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var colSel = new DataGridViewCheckBoxColumn { DataPropertyName = "Sel", HeaderText = "Sel", Width = 40 };
        var colCod = new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código (C)", Width = 100 };
        var colNome = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome no Sienge (D)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        var colAtivo = new DataGridViewTextBoxColumn { DataPropertyName = "Ativo", HeaderText = "Ativo", Width = 70, ReadOnly = true };
        _dgv.Columns.AddRange(colSel, colCod, colNome, colAtivo);
        _dgv.DataSource = _dt;
        _dgv.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgv.IsCurrentCellDirty)
                _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        _btnBuscar.Text = "Buscar no Sienge";
        _btnBuscar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnBuscar.Location = new System.Drawing.Point(12, 262);
        _btnBuscar.Size = new System.Drawing.Size(140, 28);
        _btnBuscar.Click += async (s, e) => await BuscarAsync();

        _chkUsar.AutoSize = true;
        _chkUsar.Checked = true;
        _chkUsar.CheckState = CheckState.Checked;
        _chkUsar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _chkUsar.Location = new System.Drawing.Point(160, 267);
        _chkUsar.Size = new System.Drawing.Size(200, 19);
        _chkUsar.Text = "Usar nome do Sienge";

        _lblStatus.AutoSize = true;
        _lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lblStatus.Location = new System.Drawing.Point(12, 298);
        _lblStatus.Size = new System.Drawing.Size(400, 15);
        _lblStatus.ForeColor = System.Drawing.Color.Gray;
        _lblStatus.Text = "Sem API, usa o nome padrão.";

        _btnOk.Text = "OK";
        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Location = new System.Drawing.Point(384, 330);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (s, e) => Validar();

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancelar.Location = new System.Drawing.Point(468, 330);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        Controls.Add(topo);
        Controls.Add(_dgv);
        Controls.Add(_btnBuscar);
        Controls.Add(_chkUsar);
        Controls.Add(_lblStatus);
        Controls.Add(_btnOk);
        Controls.Add(_btnCancelar);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
    }

    /// <summary>Código final (primeira linha marcada).</summary>
    public string Codigo
    {
        get
        {
            _dgv.EndEdit();
            foreach (DataRow r in _dt.Rows)
                if (r["Sel"] is bool b && b && int.TryParse(Convert.ToString(r["Codigo"]), out var v) && v > 0)
                    return v.ToString();
            return "";
        }
    }

    /// <summary>Nome final: o do Sienge (se marcado e achado) ou o padrão.</summary>
    public string NomeFinal
    {
        get
        {
            _dgv.EndEdit();
            foreach (DataRow r in _dt.Rows)
            {
                if (!(r["Sel"] is bool b && b)) continue;
                var nome = Convert.ToString(r["Nome"]) ?? "";
                if (_chkUsar.Checked && r["Ok"] is bool ok && ok && nome != "")
                    return nome;
                return _nomePadrao;
            }
            return _nomePadrao;
        }
    }

    private void Validar()
    {
        _dgv.EndEdit();
        foreach (DataRow r in _dt.Rows)
        {
            if (!(r["Sel"] is bool b && b)) continue;
            if (!int.TryParse(Convert.ToString(r["Codigo"]), out var v) || v <= 0)
            {
                MessageBox.Show(this, "Marque ao menos uma linha com código maior que zero.",
                    "Credor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            return;
        }
        MessageBox.Show(this, "Marque ao menos um credor.",
            "Credor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        DialogResult = DialogResult.None;
    }

    private async Task BuscarAsync()
    {
        _dgv.EndEdit();
        var cfg = SiengeApi.CarregarConfig();
        if (!cfg.Preenchida)
        {
            _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
            _lblStatus.Text = "API não configurada (janela do mapeamento > API...).";
            return;
        }
        _btnBuscar.Enabled = false;
        _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
        _lblStatus.Text = "Buscando...";
        try
        {
            var api = new SiengeApi(cfg);
            int ok = 0;
            foreach (DataRow r in _dt.Rows)
            {
                if (!int.TryParse(Convert.ToString(r["Codigo"]), out var id) || id <= 0)
                {
                    r["Nome"] = "(código inválido)";
                    r["Ativo"] = "";
                    r["Ok"] = false;
                    continue;
                }
                var (bok, bmsg, encontrado, nome, ativo) = await api.BuscarCredorAsync(id);
                if (!bok)
                {
                    _lblStatus.ForeColor = System.Drawing.Color.Red;
                    _lblStatus.Text = bmsg;
                    return;
                }
                if (!encontrado)
                {
                    r["Nome"] = "(não existe)";
                    r["Ativo"] = "";
                    r["Ok"] = false;
                    continue;
                }
                ok++;
                r["Nome"] = nome;
                r["Ativo"] = ativo ? "Sim" : "NÃO";
                r["Ok"] = true;
            }
            _dgv.Refresh();
            _lblStatus.ForeColor = System.Drawing.Color.Green;
            _lblStatus.Text = $"OK: {ok} credor(es) localizado(s).";
        }
        finally
        {
            _btnBuscar.Enabled = true;
        }
    }

    private static string Arquivo => DbService.ArquivoDados("credores_sienge.csv");

    public static string? CarregarOverride(int verba)
    {
        try
        {
            if (!System.IO.File.Exists(Arquivo)) return null;
            foreach (var lin in System.IO.File.ReadAllLines(Arquivo))
            {
                var p = lin.Split(';');
                if (p.Length >= 2 && int.TryParse(p[0].Trim(), out var v) && v == verba)
                    return p[1].Trim();
            }
        }
        catch { }
        return null;
    }

    /// <summary>Grava o override VERBA;CODIGO (chamado pelo Form1 ao confirmar).</summary>
    public static void SalvarOverride(int verba, string codigo)
    {
        try
        {
            var mapa = new Dictionary<int, string>();
            if (System.IO.File.Exists(Arquivo))
            {
                foreach (var lin in System.IO.File.ReadAllLines(Arquivo))
                {
                    var p = lin.Split(';');
                    if (p.Length >= 2 && int.TryParse(p[0].Trim(), out var v))
                        mapa[v] = p[1].Trim();
                }
            }
            mapa[verba] = (codigo ?? "").Trim();
            var linhas = new List<string> { "VERBA;CODIGO" };
            linhas.AddRange(mapa.OrderBy(k => k.Key).Select(k => $"{k.Key};{k.Value}"));
            System.IO.File.WriteAllLines(Arquivo, linhas);
        }
        catch { }
    }
}
