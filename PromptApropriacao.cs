using System.Windows.Forms;

namespace FolhaSienge;

/// <summary>
/// Janela modal que pede os dados de apropriação (colunas G, H, I do layout Sienge)
/// no momento de gerar o CSV de Folha/Férias/Rescisões.
/// G = código da obra, H = código da unidade construtiva, I = item de orçamento.
/// </summary>
public class PromptApropriacao : Form
{
    private readonly TextBox _txtObra = new();
    private readonly TextBox _txtUnidade = new();
    private readonly TextBox _txtItem = new();
    private readonly TextBox _txtDepartamento = new();
    private readonly Button _btnBuscarDepto = new();
    private readonly Label _lblDepto = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();

    public string Obra => _txtObra.Text.Trim();
    public string Unidade => _txtUnidade.Text.Trim();
    public string Item => _txtItem.Text.Trim();
    public string Departamento => _txtDepartamento.Text.Trim();

    private static string Arquivo => DbService.ArquivoDados("apropriacao.csv");

    /// <summary>Lê os últimos valores usados (obra;unidade;item;depto).</summary>
    public static (string Obra, string Unidade, string Item, string Depto) CarregarUltima()
    {
        try
        {
            if (!System.IO.File.Exists(Arquivo)) return ("", "", "", "");
            foreach (var lin in System.IO.File.ReadAllLines(Arquivo))
            {
                var t = (lin ?? "").Trim();
                if (t == "" || t.StartsWith("#")) continue;
                var p = t.Split(';');
                string v(int i) => p.Length > i ? p[i].Trim() : "";
                return (v(0), v(1), v(2), v(3));
            }
        }
        catch { }
        return ("", "", "", "");
    }

    /// <summary>Grava os valores confirmados para a próxima geração.</summary>
    public static void SalvarUltima(string obra, string unidade, string item, string depto)
    {
        try
        {
            System.IO.File.WriteAllLines(Arquivo,
                new[] { $"{obra?.Trim()};{unidade?.Trim()};{item?.Trim()};{depto?.Trim()}" });
        }
        catch { }
    }

    public PromptApropriacao(string titulo)
    {
        Text = titulo;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(420, 252);

        var ult = CarregarUltima();
        _txtObra.Text = ult.Obra;
        _txtUnidade.Text = ult.Unidade;
        _txtItem.Text = ult.Item;
        _txtDepartamento.Text = ult.Depto;

        var lblObra = new Label { Text = "Código da obra (G):", AutoSize = true, Location = new System.Drawing.Point(16, 22) };
        _txtObra.Location = new System.Drawing.Point(180, 18);
        _txtObra.Size = new System.Drawing.Size(220, 23);

        var lblUnidade = new Label { Text = "Unid. construtiva (H):", AutoSize = true, Location = new System.Drawing.Point(16, 58) };
        _txtUnidade.Location = new System.Drawing.Point(180, 54);
        _txtUnidade.Size = new System.Drawing.Size(220, 23);

        var lblItem = new Label { Text = "Item de orçamento (I):", AutoSize = true, Location = new System.Drawing.Point(16, 94) };
        _txtItem.Location = new System.Drawing.Point(180, 90);
        _txtItem.Size = new System.Drawing.Size(220, 23);

        var lblDepartamento = new Label { Text = "Cód. departamento (J):", AutoSize = true, Location = new System.Drawing.Point(16, 130) };
        _txtDepartamento.Location = new System.Drawing.Point(180, 126);
        _txtDepartamento.Size = new System.Drawing.Size(220, 23);

        _lblDepto.AutoSize = true;
        _lblDepto.ForeColor = System.Drawing.Color.Gray;
        _lblDepto.Location = new System.Drawing.Point(16, 178);
        _lblDepto.Size = new System.Drawing.Size(290, 15);
        _lblDepto.Text = "";

        _btnBuscarDepto.Text = "Sienge";
        _btnBuscarDepto.Location = new System.Drawing.Point(320, 174);
        _btnBuscarDepto.Size = new System.Drawing.Size(80, 24);
        _btnBuscarDepto.Click += (s, e) => BuscarDepto();

        _btnOk.Text = "OK";
        _btnOk.Location = new System.Drawing.Point(230, 206);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (s, e) => ValidarDepto();

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Location = new System.Drawing.Point(320, 206);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        var hint = new Label
        {
            Text = "Deixe em branco para gerar sem apropriação.",
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray,
            Location = new System.Drawing.Point(16, 158)
        };

        Controls.Add(lblObra); Controls.Add(_txtObra);
        Controls.Add(lblUnidade); Controls.Add(_txtUnidade);
        Controls.Add(lblItem); Controls.Add(_txtItem);
        Controls.Add(lblDepartamento); Controls.Add(_txtDepartamento);
        Controls.Add(_lblDepto); Controls.Add(_btnBuscarDepto);
        Controls.Add(_btnOk); Controls.Add(_btnCancelar);
        Controls.Add(hint);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
    }

    /// <summary>Mostra o nome do departamento no espelho para conferência.</summary>
    private void BuscarDepto()
    {
        var cod = _txtDepartamento.Text.Trim();
        if (cod == "")
        {
            _lblDepto.ForeColor = System.Drawing.Color.Gray;
            _lblDepto.Text = "";
            return;
        }
        var (data, deptos) = SiengeApi.CarregarEspelhoDeptos();
        if (deptos.Count == 0)
        {
            _lblDepto.ForeColor = System.Drawing.Color.DarkOrange;
            _lblDepto.Text = "Sem espelho: API... > Atualizar ou Importar CSV.";
            return;
        }
        var achou = deptos.FirstOrDefault(d =>
            d.Codigo.Trim().Equals(cod, StringComparison.OrdinalIgnoreCase));
        if (achou.Codigo == null)
        {
            _lblDepto.ForeColor = System.Drawing.Color.Red;
            _lblDepto.Text = $"Depto {cod} NÃO existe no Sienge.";
        }
        else
        {
            _lblDepto.ForeColor = System.Drawing.Color.Green;
            _lblDepto.Text = $"{achou.Codigo} - {achou.Nome}";
        }
    }

    /// <summary>No OK, avisa se o departamento não existe (permite continuar).</summary>
    private void ValidarDepto()
    {
        var cod = _txtDepartamento.Text.Trim();
        if (cod == "") return;
        var (_, deptos) = SiengeApi.CarregarEspelhoDeptos();
        if (deptos.Count == 0) return;
        bool existe = deptos.Any(d => d.Codigo.Trim().Equals(cod, StringComparison.OrdinalIgnoreCase));
        if (!existe)
        {
            var r = MessageBox.Show(this,
                $"Departamento {cod} não existe no Sienge. Continuar assim mesmo?",
                "Apropriação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes)
                DialogResult = DialogResult.None;
        }
    }
}
