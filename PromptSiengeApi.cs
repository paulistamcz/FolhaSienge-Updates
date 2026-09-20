namespace FolhaSienge;

/// <summary>
/// Janela de configuração da API do Sienge (subdomínio, usuário e chave),
/// salva em sienge_api.json ao lado do executável. Botão Testar valida a conexão.
/// </summary>
public class PromptSiengeApi : Form
{
    private readonly TextBox _txtSub = new();
    private readonly TextBox _txtUser = new();
    private readonly TextBox _txtKey = new();
    private readonly Button _btnTestar = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();
    private readonly Label _lblStatus = new();

    public PromptSiengeApi(SiengeConfig atual)
    {
        Text = "API do Sienge - Configuração";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(440, 265);

        var lblSub = new Label { Text = "Subdomínio:", AutoSize = true, Location = new System.Drawing.Point(16, 22) };
        _txtSub.Location = new System.Drawing.Point(170, 18);
        _txtSub.Size = new System.Drawing.Size(250, 23);
        _txtSub.Text = string.IsNullOrWhiteSpace(atual.Subdominio) ? "engenhariademateriais" : atual.Subdominio;

        var lblUser = new Label { Text = "Usuário de API:", AutoSize = true, Location = new System.Drawing.Point(16, 58) };
        _txtUser.Location = new System.Drawing.Point(170, 54);
        _txtUser.Size = new System.Drawing.Size(250, 23);
        _txtUser.Text = atual.Usuario ?? "";

        var lblKey = new Label { Text = "Chave de API:", AutoSize = true, Location = new System.Drawing.Point(16, 94) };
        _txtKey.Location = new System.Drawing.Point(170, 90);
        _txtKey.Size = new System.Drawing.Size(250, 23);
        _txtKey.UseSystemPasswordChar = true;
        _txtKey.Text = atual.Chave ?? "";

        _btnTestar.Text = "Testar conexão";
        _btnTestar.Location = new System.Drawing.Point(16, 128);
        _btnTestar.Size = new System.Drawing.Size(140, 30);
        _btnTestar.Click += async (s, e) => await TestarAsync();

        _lblStatus.AutoSize = true;
        _lblStatus.Location = new System.Drawing.Point(16, 166);
        _lblStatus.Size = new System.Drawing.Size(404, 30);
        _lblStatus.ForeColor = System.Drawing.Color.Gray;
        _lblStatus.Text = "Usuário criado no Sienge em APIs e Conectores, com acesso a centros de custo.";

        _btnOk.Text = "Salvar";
        _btnOk.Location = new System.Drawing.Point(250, 210);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Location = new System.Drawing.Point(340, 210);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        Controls.Add(lblSub); Controls.Add(_txtSub);
        Controls.Add(lblUser); Controls.Add(_txtUser);
        Controls.Add(lblKey); Controls.Add(_txtKey);
        Controls.Add(_btnTestar); Controls.Add(_lblStatus);
        Controls.Add(_btnOk); Controls.Add(_btnCancelar);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;
    }

    public SiengeConfig Config => new()
    {
        Subdominio = _txtSub.Text.Trim(),
        Usuario = _txtUser.Text.Trim(),
        Chave = _txtKey.Text.Trim(),
    };

    private async Task TestarAsync()
    {
        _btnTestar.Enabled = false;
        _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
        _lblStatus.Text = "Testando conexão...";
        try
        {
            var api = new SiengeApi(Config);
            var (ok, msg) = await api.TestarConexaoAsync();
            _lblStatus.ForeColor = ok ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            _lblStatus.Text = msg;
        }
        finally
        {
            _btnTestar.Enabled = true;
        }
    }
}
