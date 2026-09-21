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
}

/// <summary>
/// Grade de apropriação por linha: mostra cada linha (funcionário/centro) com B
/// já mapeado e G/H/I/J pré-preenchidos e editáveis; permite marcar todas ou
/// só algumas. Edita a lista recebida e retorna OK/Cancel.
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

    public PromptApropriacaoLinhas(List<LinhaApropriacao> linhas, string titulo)
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
        _dt.Columns.Add("Valor", typeof(decimal));
        foreach (var l in linhas)
            _dt.Rows.Add(l.Sel, l.Descricao, l.B, l.G, l.H, l.I, l.J, l.Valor);

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
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        };
        _dgv.Columns.AddRange(colSel, colDesc, colB, colG, colH, colI, colJ, colValor);
        _dgv.DataSource = _dt;
        _dgv.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgv.IsCurrentCellDirty)
                _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _dgv.CellValueChanged += (s, e) =>
        {
            if (e.ColumnIndex == 0) AtualizarTotal();
        };
        _dgv.Resize += (s, e) => ReposicionarLabelTotal(topo);

        Controls.Add(_dgv);
        Controls.Add(topo);
        Controls.Add(rodape);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
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

    private void AtualizarTotal()
    {
        decimal total = 0m;
        foreach (DataRow r in _dt.Rows)
        {
            if (r["Sel"] is bool b && b && r["Valor"] is decimal v)
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
