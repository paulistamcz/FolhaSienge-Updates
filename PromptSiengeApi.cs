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
    private readonly Button _btnAtualizar = new();
    private readonly Button _btnImportar = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancelar = new();
    private readonly Label _lblStatus = new();
    private readonly Label _lblEspelho = new();

    public PromptSiengeApi(SiengeConfig atual)
    {
        Text = "API do Sienge - Configuração";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(440, 340);

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

        _lblEspelho.AutoSize = true;
        _lblEspelho.Location = new System.Drawing.Point(16, 196);
        _lblEspelho.Size = new System.Drawing.Size(408, 15);
        _lblEspelho.ForeColor = System.Drawing.Color.Gray;

        _btnAtualizar.Text = "Atualizar do Sienge";
        _btnAtualizar.Location = new System.Drawing.Point(16, 222);
        _btnAtualizar.Size = new System.Drawing.Size(200, 30);
        _btnAtualizar.Click += async (s, e) => await AtualizarEspelhoAsync();

        _btnImportar.Text = "Importar CSV...";
        _btnImportar.Location = new System.Drawing.Point(224, 222);
        _btnImportar.Size = new System.Drawing.Size(196, 30);
        _btnImportar.Click += (s, e) => ImportarCsv();

        _btnOk.Text = "Salvar";
        _btnOk.Location = new System.Drawing.Point(250, 292);
        _btnOk.Size = new System.Drawing.Size(80, 30);
        _btnOk.DialogResult = DialogResult.OK;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Location = new System.Drawing.Point(340, 292);
        _btnCancelar.Size = new System.Drawing.Size(80, 30);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        Controls.Add(lblSub); Controls.Add(_txtSub);
        Controls.Add(lblUser); Controls.Add(_txtUser);
        Controls.Add(lblKey); Controls.Add(_txtKey);
        Controls.Add(_btnTestar); Controls.Add(_lblStatus);
        Controls.Add(_lblEspelho);
        Controls.Add(_btnAtualizar); Controls.Add(_btnImportar);
        Controls.Add(_btnOk); Controls.Add(_btnCancelar);

        AcceptButton = _btnOk;
        CancelButton = _btnCancelar;

        AtualizarStatusEspelho();
    }

    private void AtualizarStatusEspelho()
    {
        var (dO, o) = SiengeApi.CarregarEspelhoObras();
        var (dE, e) = SiengeApi.CarregarEspelhoEmpresas();
        var (dD, d) = SiengeApi.CarregarEspelhoDeptos();
        _lblEspelho.Text = $"Espelho: {o.Count} obras ({SiengeApi.IdadeEspelho(dO)}); " +
            $"{e.Count} empresas ({SiengeApi.IdadeEspelho(dE)}); " +
            $"{d.Count} deptos ({SiengeApi.IdadeEspelho(dD)}).";
    }

    private async Task AtualizarEspelhoAsync()
    {
        _btnAtualizar.Enabled = false;
        _btnImportar.Enabled = false;
        _lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
        _lblStatus.Text = "Buscando obras...";
        try
        {
            var api = new SiengeApi(Config);
            var (okO, msgO, obras) = await api.ListarObrasAsync();
            if (!okO)
            {
                _lblStatus.ForeColor = System.Drawing.Color.Red;
                _lblStatus.Text = msgO;
                return;
            }
            _lblStatus.Text = "Buscando empresas...";
            Application.DoEvents();
            var (okE, msgE, empresas) = await api.ListarEmpresasAsync();
            if (!okE)
            {
                _lblStatus.ForeColor = System.Drawing.Color.Red;
                _lblStatus.Text = msgE;
                return;
            }
            var emp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in obras) emp[o.Codigo] = o.Empresa;
            SiengeApi.SalvarEspelhoObras(obras.Select(o => (o.Codigo, o.Nome)).ToList(), emp);
            SiengeApi.SalvarEspelhoEmpresas(empresas);
            _lblStatus.Text = "Buscando departamentos...";
            Application.DoEvents();
            var (okD, msgD, deptos) = await api.ListarDepartamentosAsync();
            if (!okD)
            {
                _lblStatus.ForeColor = System.Drawing.Color.Red;
                _lblStatus.Text = msgD + " (obras/empresas salvas; deptos exigem liberação)";
                AtualizarStatusEspelho();
                return;
            }
            SiengeApi.SalvarEspelhoDeptos(deptos);
            SiengeApi.SalvarConfig(Config);
            _lblStatus.ForeColor = System.Drawing.Color.Green;
            _lblStatus.Text = $"Espelho atualizado: {obras.Count} obras, {empresas.Count} empresas, {deptos.Count} deptos.";
            AtualizarStatusEspelho();
        }
        finally
        {
            _btnAtualizar.Enabled = true;
            _btnImportar.Enabled = true;
        }
    }

    private void ImportarCsv()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            Title = "Importar espelho (obras ou empresas)"
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;
        var (tipo, qtd, msg) = SiengeApi.ImportarCsvExterno(ofd.FileName);
        _lblStatus.ForeColor = qtd > 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        _lblStatus.Text = msg;
        AtualizarStatusEspelho();
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
