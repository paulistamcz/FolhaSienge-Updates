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
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();

    public string Obra => _txtObra.Text.Trim();
    public string Unidade => _txtUnidade.Text.Trim();
    public string Item => _txtItem.Text.Trim();

    public PromptApropriacao(string titulo)
    {
        Text = titulo;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(420, 190);

        var lblObra = new Label { Text = "Código da obra (G):", AutoSize = true, Location = new System.Drawing.Point(16, 22) };
        _txtObra.Location = new System.Drawing.Point(180, 18);
        _txtObra.Size = new System.Drawing.Size(220, 23);

        var lblUnidade = new Label { Text = "Unid. construtiva (H):", AutoSize = true, Location = new System.Drawing.Point(16, 58) };
        _txtUnidade.Location = new System.Drawing.Point(180, 54);
        _txtUnidade.Size = new System.Drawing.Size(220, 23);

        var lblItem = new Label { Text = "Item de orçamento (I):", AutoSize = true, Location = new System.Drawing.Point(16, 94) };
        _txtItem.Location = new System.Drawing.Point(180, 90);
        _txtItem.Size = new System.Drawing.Size(220, 23);

        _btnOk.Text = "OK";
        _btnOk.Location = new System.Drawing.Point(230, 140);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Location = new System.Drawing.Point(320, 140);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        var hint = new Label
        {
            Text = "Deixe em branco para gerar sem apropriação de obra.",
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray,
            Location = new System.Drawing.Point(16, 122)
        };

        Controls.Add(lblObra); Controls.Add(_txtObra);
        Controls.Add(lblUnidade); Controls.Add(_txtUnidade);
        Controls.Add(lblItem); Controls.Add(_txtItem);
        Controls.Add(_btnOk); Controls.Add(_btnCancelar);
        Controls.Add(hint);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
    }
}
