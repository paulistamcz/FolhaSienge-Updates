using System.Data;
using System.Globalization;

namespace FolhaSienge;

/// <summary>
/// Janela de seleção de funcionários: lista todos com checkbox por linha,
/// opção "Selecionar todos" e total da seleção. Retorna os escolhidos.
/// </summary>
public class SelecaoFuncionarios : Form
{
    private readonly DataGridView _dgv = new();
    private readonly CheckBox _chkTodos = new();
    private readonly Label _lblTotal = new();
    private readonly DataTable _dt = new();
    private readonly Button _btnOk = new() { Text = "OK" };
    private readonly Button _btnCancelar = new() { Text = "Cancelar" };

    public List<(int Empregado, string Nome, int Centro, decimal Valor)> Selecionados { get; private set; } = new();

    public SelecaoFuncionarios(
        List<(int Empregado, string Nome, int Centro, decimal Valor)> funcionarios,
        string titulo)
    {
        Text = titulo;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        ClientSize = new System.Drawing.Size(840, 560);
        MinimumSize = new System.Drawing.Size(640, 400);

        _dt.Columns.Add("Sel", typeof(bool));
        _dt.Columns.Add("Emp", typeof(int));
        _dt.Columns.Add("Nome", typeof(string));
        _dt.Columns.Add("Centro", typeof(int));
        _dt.Columns.Add("Valor", typeof(decimal));
        foreach (var f in funcionarios)
            _dt.Rows.Add(true, f.Empregado, f.Nome, f.Centro, f.Valor);

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
        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.DialogResult = DialogResult.OK;
        _btnCancelar.DialogResult = DialogResult.Cancel;
        _btnOk.Click += (s, e) => ColetarSelecionados();
        _btnOk.Location = new System.Drawing.Point(rodape.ClientSize.Width - 176, 9);
        _btnCancelar.Location = new System.Drawing.Point(rodape.ClientSize.Width - 92, 9);
        rodape.Controls.Add(_btnOk);
        rodape.Controls.Add(_btnCancelar);

        _dgv.Dock = DockStyle.Fill;
        _dgv.AutoGenerateColumns = false;
        _dgv.AllowUserToAddRows = false;
        _dgv.RowHeadersVisible = false;
        _dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var colSel = new DataGridViewCheckBoxColumn { DataPropertyName = "Sel", HeaderText = "Sel", Width = 40 };
        var colEmp = new DataGridViewTextBoxColumn { DataPropertyName = "Emp", HeaderText = "Emp", Width = 70, ReadOnly = true };
        var colNome = new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Funcionário", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        var colCentro = new DataGridViewTextBoxColumn { DataPropertyName = "Centro", HeaderText = "Centro", Width = 70, ReadOnly = true };
        var colValor = new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Valor",
            HeaderText = "Valor",
            Width = 120,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        };
        _dgv.Columns.AddRange(colSel, colEmp, colNome, colCentro, colValor);
        _dgv.DataSource = _dt;
        _dgv.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgv.IsCurrentCellDirty)
                _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _dgv.CellValueChanged += (s, e) => AtualizarTotal();

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

    private void ColetarSelecionados()
    {
        _dgv.EndEdit();
        _dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
        var lista = new List<(int Empregado, string Nome, int Centro, decimal Valor)>();
        foreach (DataRow r in _dt.Rows)
        {
            if (r["Sel"] is bool b && b)
            {
                lista.Add((
                    Convert.ToInt32(r["Emp"]),
                    Convert.ToString(r["Nome"]) ?? "",
                    Convert.ToInt32(r["Centro"]),
                    Convert.ToDecimal(r["Valor"])));
            }
        }
        Selecionados = lista;
    }
}