using System.Data;
using System.Globalization;

namespace FolhaSienge;

/// <summary>Linha da grade de apropriação (seleção + G/H/I/J por linha).</summary>
public class LinhaApropriacao
{
    public int Indice { get; set; }
    public string Descricao { get; set; } = "";
    public int Centro { get; set; }
    public decimal Valor { get; set; }
    public int B { get; set; }
    public string G { get; set; } = "";
    public string H { get; set; } = "";
    public string I { get; set; } = "";
    public string J { get; set; } = "";
    public bool Sel { get; set; } = true;
    /// <summary>Pré-preenchimento sem a camada salva (base do diff: oficial/B-auto/lembrado).</summary>
    public string BaseG { get; set; } = "";
    public string BaseH { get; set; } = "";
    public string BaseI { get; set; } = "";
    public string BaseJ { get; set; } = "";
}

/// <summary>
/// Grade de apropriação por linha: mostra cada linha (funcionário/centro) com B
/// já mapeado e G/H/I/J pré-preenchidos e editáveis; permite marcar todas ou
/// só algumas. O Valor só é editável quando valorEditavel (lote GRRF).
/// Edita a lista recebida e retorna OK/Cancel.
/// </summary>
public class PromptApropriacaoLinhas : Form
{
    private readonly DataGridView _dgv = new();
    private readonly CheckBox _chkTodos = new();
    private readonly Label _lblTotal = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();
    private readonly DataTable _dt = new();
    private readonly List<LinhaApropriacao> _linhas;

    public PromptApropriacaoLinhas(List<LinhaApropriacao> linhas, string titulo, bool valorEditavel = false)
    {
        _linhas = linhas;
        Text = titulo;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(980, 540);
        MinimumSize = new System.Drawing.Size(800, 420);

        _dt.Columns.Add("Sel", typeof(bool));
        _dt.Columns.Add("Desc", typeof(string));
        _dt.Columns.Add("B", typeof(int));
        _dt.Columns.Add("G", typeof(string));
        _dt.Columns.Add("H", typeof(string));
        _dt.Columns.Add("I", typeof(string));
        _dt.Columns.Add("J", typeof(string));
        // Valor como texto para aceitar digitação livre (vírgula/ponto); validado no OK.
        _dt.Columns.Add("Valor", typeof(string));
        foreach (var l in linhas)
            _dt.Rows.Add(l.Sel, l.Descricao, l.B, l.G, l.H, l.I, l.J,
                l.Valor.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")));

        var topo = new Panel { Dock = DockStyle.Top, Height = 36 };
        _chkTodos.Text = "Selecionar todos";
        _chkTodos.Checked = true;
        _chkTodos.AutoSize = true;
        _chkTodos.Location = new System.Drawing.Point(12, 10);
        _chkTodos.CheckedChanged += (s, e) => MarcarTodos(_chkTodos.Checked);
        _lblTotal.AutoSize = true;
        _lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        AtualizarTotal();
        topo.Controls.Add(_lblTotal);
        topo.Controls.Add(_chkTodos);

        var rodape = new Panel { Dock = DockStyle.Bottom, Height = 48 };
        _btnOk.Text = "OK";
        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.Location = new System.Drawing.Point(rodape.ClientSize.Width - 176, 9);
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (s, e) => Validar();
        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.Location = new System.Drawing.Point(rodape.ClientSize.Width - 92, 9);
        _btnCancelar.DialogResult = DialogResult.Cancel;
        rodape.Controls.Add(_btnOk);
        rodape.Controls.Add(_btnCancelar);
        rodape.Resize += (s, e) =>
        {
            _btnOk.Location = new System.Drawing.Point(rodape.ClientSize.Width - 176, 9);
            _btnCancelar.Location = new System.Drawing.Point(rodape.ClientSize.Width - 92, 9);
        };

        _dgv.Dock = DockStyle.Fill;
        _dgv.AutoGenerateColumns = false;
        _dgv.AllowUserToAddRows = false;
        _dgv.AllowUserToDeleteRows = false;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var colSel = new DataGridViewCheckBoxColumn { DataPropertyName = "Sel", HeaderText = "Sel", Width = 40 };
        var colDesc = new DataGridViewTextBoxColumn { DataPropertyName = "Desc", HeaderText = "Linha", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        var colB = new DataGridViewTextBoxColumn { DataPropertyName = "B", HeaderText = "B", Width = 60, ReadOnly = true };
        var colG = new DataGridViewTextBoxColumn { DataPropertyName = "G", HeaderText = "G", Width = 70 };
        var colH = new DataGridViewTextBoxColumn { DataPropertyName = "H", HeaderText = "H", Width = 70 };
        var colI = new DataGridViewTextBoxColumn { DataPropertyName = "I", HeaderText = "I", Width = 110 };
        var colJ = new DataGridViewTextBoxColumn { DataPropertyName = "J", HeaderText = "J", Width = 70 };
        var colValor = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Valor",
            HeaderText = "Valor",
            Width = 110,
            ReadOnly = !valorEditavel,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        };
        _dgv.Columns.AddRange(colSel, colDesc, colB, colG, colH, colI, colJ, colValor);
        _dgv.DataSource = _dt;
        _dgv.DataError += (s, e) =>
        {
            MessageBox.Show(this, "Valor inválido. Use números (ex.: 1234,56).",
                "Apropriação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.ThrowException = false;
        };
        _dgv.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgv.IsCurrentCellDirty)
                _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _dgv.CellValueChanged += (s, e) =>
        {
            if (e.ColumnIndex == 0 || (e.ColumnIndex >= 0 && _dgv.Columns[e.ColumnIndex].DataPropertyName == "Valor")) AtualizarTotal();
        };
        _dgv.Resize += (s, e) => ReposicionarLabelTotal(topo);

        Controls.Add(_dgv);
        Controls.Add(topo);
        Controls.Add(rodape);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
        // Se fechar (X/Esc/Cancelar) com edição não confirmada, avisa antes de descartar.
        FormClosing += (s, e) =>
        {
            if (DialogResult == DialogResult.OK) return;
            _dgv.EndEdit();
            for (int i = 0; i < _dt.Rows.Count && i < _linhas.Count; i++)
            {
                var r = _dt.Rows[i];
                var l = _linhas[i];
                if ((Convert.ToString(r["G"]) ?? "") != l.G ||
                    (Convert.ToString(r["H"]) ?? "") != l.H ||
                    (Convert.ToString(r["I"]) ?? "") != l.I ||
                    (Convert.ToString(r["J"]) ?? "") != l.J)
                {
                    var resp = MessageBox.Show(this,
                        "Há edições na grade que ainda não foram confirmadas.\n\n" +
                        "Sim = descartar e sair.\nNão = voltar para a grade (use OK para salvar).",
                        "Apropriação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (resp != DialogResult.Yes)
                        e.Cancel = true;
                    return;
                }
            }
        };
    }

    private void ReposicionarLabelTotal(Panel topo)
    {
        _lblTotal.Left = topo.ClientSize.Width - _lblTotal.Width - 12;
        _lblTotal.Top = 10;
    }

    private void MarcarTodos(bool marcar)
    {
        foreach (DataRow r in _dt.Rows)
            r["Sel"] = marcar;
        _dgv.Refresh();
        AtualizarTotal();
    }

    /// <summary>Interpreta número digitado: com vírgula, pt-BR; sem vírgula, invariante.</summary>
    public static bool TentarValor(string? texto, out decimal valor)
    {
        valor = 0m;
        var t = (texto ?? "").Trim();
        if (t == "") return false;
        var cult = t.Contains(',') ? CultureInfo.GetCultureInfo("pt-BR") : CultureInfo.InvariantCulture;
        return decimal.TryParse(t, NumberStyles.Number, cult, out valor);
    }

    private void AtualizarTotal()
    {
        decimal total = 0m;
        foreach (DataRow r in _dt.Rows)
        {
            if (r["Sel"] is bool b && b && TentarValor(Convert.ToString(r["Valor"]), out var v))
                total += v;
        }
        _lblTotal.Text = $"Total selecionado: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";
    }

    private void Validar()
    {
        _dgv.EndEdit();
        _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        int n = 0;
        for (int i = 0; i < _dt.Rows.Count && i < _linhas.Count; i++)
        {
            var r = _dt.Rows[i];
            var l = _linhas[i];
            l.Sel = r["Sel"] is bool b && b;
            l.G = Convert.ToString(r["G"]) ?? "";
            l.H = Convert.ToString(r["H"]) ?? "";
            l.I = Convert.ToString(r["I"]) ?? "";
            l.J = Convert.ToString(r["J"]) ?? "";
            if (!TentarValor(Convert.ToString(r["Valor"]), out var vv) || vv < 0)
            {
                MessageBox.Show(this, $"Valor inválido na linha {i + 1}. Use números maiores ou iguais a zero.",
                    "Apropriação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            l.Valor = Math.Round(vv, 2);
            if (l.Sel) n++;
        }
        if (n == 0)
        {
            MessageBox.Show(this, "Marque pelo menos uma linha.",
                "Apropriação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }
}
