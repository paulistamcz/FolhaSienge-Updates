using System.Data;
using System.Data.Odbc;
using System.Globalization;

namespace FolhaSienge;

public partial class Form1 : Form
{
    private OdbcConnection? _conn;
    private List<(int Centro, string Nome, int Empregados, decimal Total, string CredorCodigo, string CredorNome)> _linhas = new();
    private List<(int IEmpregados, string Nome, int ICcustos, DateTime Vencimento, decimal Valor)> _linhasGrf = new();
    private string? _csvGerado;
    private string? _csvGeradoGrf;
    private string? _csvGeradoFolha;
    private string? _csvGeradoGuias;
    private bool _carregandoEmpresas;

    public Form1()
    {
        InitializeComponent();
        this.Shown += Form1_Shown;
        CarregarVerbas();
        CarregarTiposFolha();
        CarregarTiposGuias();
        Text = $"Plus Informática - Folha de Pagamento (Plus Contabilidade)  v{Atualizador.VersaoAtual}";
    }

    private void btnSair_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void CarregarTiposGuias()
    {
        cmbGuiasTipo.Items.Clear();
        cmbGuiasTipo.Items.Add("INSS");
        cmbGuiasTipo.Items.Add("IRRF");
        cmbGuiasTipo.Items.Add("FGTS");
        cmbGuiasTipo.Items.Add("eCONSIGNADO");
        cmbGuiasTipo.Items.Add("GRRF");
        cmbGuiasTipo.SelectedIndex = 0;

        cmbGuiasModo.Items.Clear();
        cmbGuiasModo.Items.Add("Completo (total por centro)");
        cmbGuiasModo.Items.Add("Analítico (por pessoa)");
        cmbGuiasModo.SelectedIndex = 0;
    }

    private void CarregarTiposFolha()
    {
        cmbFolhaTipo.Items.Clear();
        cmbFolhaTipo.Items.Add("Folha Mensal");
        cmbFolhaTipo.Items.Add("Folha Quinzena");
        cmbFolhaTipo.Items.Add("Férias");
        cmbFolhaTipo.Items.Add("Rescisões");
        cmbFolhaTipo.SelectedIndex = 0;

        cmbFolhaModo.Items.Clear();
        cmbFolhaModo.Items.Add("Completo (total por centro)");
        cmbFolhaModo.Items.Add("Analítico (por pessoa)");
        cmbFolhaModo.SelectedIndex = 0;
    }

    private void CarregarVerbas()
    {
        cmbVerba.Items.Clear();
        foreach (var v in VerbaFinanceira.Lista())
            cmbVerba.Items.Add(v);
        if (cmbVerba.Items.Count > 0)
            cmbVerba.SelectedIndex = 0;
    }

    // Busca e conecta automaticamente ao abrir
    private void Form1_Shown(object? sender, EventArgs e)
    {
        Application.DoEvents();
        BuscarServidores(conectarAutomaticamente: true);
        _ = VerificarAtualizacaoAsync();
    }

    private async Task VerificarAtualizacaoAsync()
    {
        try
        {
            // Se a última auto-atualização falhou, o instalador deixou o log para trás: avisa onde está.
            try
            {
                var logFalha = Path.Combine(Path.GetTempPath(), "FolhaSienge_update", "instalador.log");
                if (File.Exists(logFalha) && (await File.ReadAllTextAsync(logFalha)).Contains("FALHA"))
                {
                    MessageBox.Show(this,
                        "A última tentativa de atualização automática falhou.\n\nDetalhes em:\n" + logFalha +
                        "\n\nFeche todas as janelas do app e tente de novo, ou atualize manualmente pelo GitHub.",
                        "Atualização", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try { File.Move(logFalha, logFalha + ".verificado", true); } catch { }
                }
            }
            catch { }
            var release = await Atualizador.BuscarUltimaVersaoAsync();
            if (Atualizador.TemVersaoNova(release))
            {
                var notas = (release!.Notas ?? "").Trim();
                if (notas.Length > 1000) notas = notas.Substring(0, 1000) + "...";
                var texto = $"Existe uma nova versão disponível: {release.Versao}\n\n" +
                    $"Sua versão atual: {Atualizador.VersaoAtual}\n\n";
                if (!string.IsNullOrWhiteSpace(notas))
                    texto += $"O que mudou:\n{notas}\n\n";
                texto += "Deseja atualizar agora? O app será fechado e reaberto automaticamente.";
                var resposta = MessageBox.Show(this, texto,
                    "Atualização disponível",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (resposta == DialogResult.Yes)
                {
                    await Atualizador.BaixarEInstalarAsync(release);
                    Application.Exit();
                }
            }
        }
        catch (Exception ex)
        {
            try
            {
                var log = Path.Combine(Path.GetTempPath(), "FolhaSienge_update", "erro_update.txt");
                System.IO.File.WriteAllText(log,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                    ex.ToString());
            }
            catch { }
            MessageBox.Show(this,
                "Não foi possível instalar a atualização:\n\n" + ex.Message,
                "Falha na atualização", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBuscarBanco_Click(object sender, EventArgs e)
    {
        BuscarServidores(conectarAutomaticamente: false);
    }

    private void btnAtualizarBanco_Click(object sender, EventArgs e)
    {
        BuscarServidores(conectarAutomaticamente: false);
    }

    private void BuscarServidores(bool conectarAutomaticamente)
    {
        lblStatusBanco.Text = "Procurando servidor SQL Anywhere em execução...";
        lblStatusBanco.ForeColor = System.Drawing.Color.DarkOrange;
        Cursor = Cursors.WaitCursor;
        try
        {
            var itens = new List<string>();
            var servidores = DbService.DetectarServidores();
            foreach (var s in servidores)
                itens.Add(s);

            foreach (var b in DbService.BuscarBancos())
                if (!itens.Contains(b))
                    itens.Add(b);

            cmbBancos.Items.Clear();
            foreach (var i in itens)
                cmbBancos.Items.Add(i);

            if (itens.Count == 0)
            {
                lblStatusBanco.Text = "Nenhum servidor/banco encontrado. Use \"Selecionar arquivo...\".";
                lblStatusBanco.ForeColor = System.Drawing.Color.Red;
                btnConectar.Enabled = false;
            }
            else
            {
                cmbBancos.SelectedIndex = 0;
                lblStatusBanco.Text = $"{itens.Count} banco(s)/servidor(es) encontrado(s).";
                lblStatusBanco.ForeColor = System.Drawing.Color.Green;
                btnConectar.Enabled = true;
                if (conectarAutomaticamente)
                    Conectar();
            }
        }
        catch (Exception ex)
        {
            lblStatusBanco.Text = "Erro na busca: " + ex.Message;
            lblStatusBanco.ForeColor = System.Drawing.Color.Red;
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnSelecionarArquivo_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Banco SQL Anywhere (*.db)|*.db|Todos os arquivos (*.*)|*.*",
            Title = "Selecionar banco contabil.db"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            cmbBancos.Items.Add(ofd.FileName);
            cmbBancos.SelectedItem = ofd.FileName;
            btnConectar.Enabled = true;
            lblStatusBanco.Text = "Arquivo selecionado: " + ofd.FileName;
            lblStatusBanco.ForeColor = System.Drawing.Color.Green;
        }
    }

    private void btnConectarRede_Click(object sender, EventArgs e)
    {
        string host = txtServidor.Text.Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            MessageBox.Show("Informe o nome ou IP do servidor.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        lblStatusBanco.Text = $"Procurando banco em {host} (TCP/IP 2638)...";
        lblStatusBanco.ForeColor = System.Drawing.Color.DarkOrange;
        Cursor = Cursors.WaitCursor;
        try
        {
            var achado = DbService.DetectarHost(host);
            if (achado != null)
            {
                cmbBancos.Items.Clear();
                cmbBancos.Items.Add(achado);
                cmbBancos.SelectedIndex = 0;
                lblStatusBanco.Text = $"Banco encontrado em {host}.";
                lblStatusBanco.ForeColor = System.Drawing.Color.Green;
                btnConectar.Enabled = true;
                Conectar();
            }
            else
            {
                lblStatusBanco.Text = $"Nenhum banco em {host}. Verifique nome/IP, porta e driver.";
                lblStatusBanco.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show("Não foi possível achar o banco em " + host +
                    ".\n\nConfira: nome/IP do servidor, porta 2638 aberta, " +
                    "engine do SQL Anywhere rodando e driver ODBC instalado.",
                    "Banco não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatusBanco.Text = "Erro: " + ex.Message;
            lblStatusBanco.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show("Erro ao conectar pela rede:\n\n" + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnConectar_Click(object sender, EventArgs e)
    {
        Conectar();
    }

    private void Conectar()
    {
        if (cmbBancos.SelectedItem == null)
        {
            MessageBox.Show("Selecione um banco na lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string banco = cmbBancos.SelectedItem.ToString()!;
        string engine = Path.GetFileNameWithoutExtension(banco);
        var cs = new DbService().MontarConnectionString(banco, engine);
        try
        {
            _conn = new DbService().TestarConexao(cs);
            lblStatusBanco.Text = $"Conectado ao banco: {banco}";
            lblStatusBanco.ForeColor = System.Drawing.Color.Green;
            cmbCompetencia.Enabled = true;
            btnCarregarGrf.Enabled = true;
            btnGerarFolha.Enabled = true;
            btnGerarGuias.Enabled = true;
            CarregarEmpresas();
            btnGerarCsv.Enabled = false;
            btnSalvarCsv.Enabled = false;
            dgvCentros.DataSource = null;
        }
        catch (Exception ex)
        {
            lblStatusBanco.Text = "Falha na conexão: " + ex.Message;
            lblStatusBanco.ForeColor = System.Drawing.Color.Red;
            cmbCompetencia.Enabled = false;
            btnCarregarCentros.Enabled = false;
            MessageBox.Show("Falha na conexão com o banco.\n\n" + ex.Message,
                "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CarregarEmpresas()
    {
        try
        {
            _carregandoEmpresas = true;
            cmbEmpresa.Items.Clear();
            var empresas = new DbService().ListarEmpresas(_conn!);
            foreach (var (cod, nome) in empresas)
                cmbEmpresa.Items.Add(new ComboEmpresa(cod, nome));
            cmbEmpresa.Enabled = cmbEmpresa.Items.Count > 0;
            if (cmbEmpresa.Items.Count > 0)
            {
                // Seleciona a última empresa escolhida (ou a 1) e recarrega competências.
                if (DbService.Empresa <= 0 || !empresas.Any(e => e.Codigo == DbService.Empresa))
                    DbService.Empresa = 1;
                var atual = cmbEmpresa.Items.Cast<ComboEmpresa>().FirstOrDefault(e => e.Codigo == DbService.Empresa);
                cmbEmpresa.SelectedItem = atual ?? cmbEmpresa.Items[0];
            }
            else
            {
                cmbCompetencia.Items.Clear();
                btnCarregarCentros.Enabled = false;
                btnRelatorioMensal.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            lblStatusBanco.Text = "Erro ao listar empresas: " + ex.Message;
            lblStatusBanco.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show("Erro ao listar empresas:\n\n" + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _carregandoEmpresas = false;
        }
    }

    private void cmbEmpresa_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_carregandoEmpresas) return;
        if (cmbEmpresa.SelectedItem is ComboEmpresa emp)
        {
            DbService.Empresa = emp.Codigo;
            dgvCentros.DataSource = null;
            dgvFolha.DataSource = null;
            dgvGrf.DataSource = null;
            dgvGuias.DataSource = null;
            txtResultado.Clear();
            txtResultadoFolha.Clear();
            txtResultadoGrf.Clear();
            txtResultadoGuias.Clear();
            _csvGerado = null;
            _csvGeradoGrf = null;
            _csvGeradoFolha = null;
            _csvGeradoGuias = null;
            btnGerarCsv.Enabled = false;
            btnSalvarCsv.Enabled = false;
            btnGerarGrf.Enabled = false;
            btnSalvarGrf.Enabled = false;
            btnGerarFolha.Enabled = true;
            btnSalvarFolha.Enabled = false;
            btnGerarGuias.Enabled = true;
            btnSalvarGuias.Enabled = false;
            CarregarCompetencias();
        }
    }

    private void CarregarCompetencias()
    {
        try
        {
            cmbCompetencia.Items.Clear();
            var comps = new DbService().ListarCompetencias(_conn!);
            foreach (var c in comps)
                cmbCompetencia.Items.Add(c);
            if (cmbCompetencia.Items.Count > 0)
            {
                cmbCompetencia.SelectedIndex = 0;
                btnCarregarCentros.Enabled = true;
                btnRelatorioMensal.Enabled = true;
                lblStatusBanco.Text += " Competência carregada.";
            }            else
            {
                btnCarregarCentros.Enabled = false;
                btnRelatorioMensal.Enabled = false;
                lblStatusBanco.Text += " Nenhuma competência encontrada.";
            }
            AtualizarDocumento();
        }
        catch (Exception ex)
        {
            lblStatusBanco.Text = "Erro ao listar competências: " + ex.Message;
            lblStatusBanco.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show("Erro ao listar competências:\n\n" + ex.Message,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cmbCompetencia_SelectedIndexChanged(object sender, EventArgs e)
    {
        AtualizarDocumento();
        AtualizarDatasCompetencia();
    }

    /// <summary>
    /// Ajusta os campos de data (vencimento, período GRRF, data do lote) para
    /// o mês da competência selecionada, para que todas as consultas usem esse período.
    /// </summary>
    private void AtualizarDatasCompetencia()
    {
        string comp = cmbCompetencia.SelectedItem?.ToString() ?? "";
        if (!DateTime.TryParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var mes))
            return;

        // Vencimento (F): último dia do mês da competência
        var venc = new DateTime(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));
        mtbVencimento.Text = venc.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        // Período GRRF: primeiro ao último dia do mês
        var ini = new DateTime(mes.Year, mes.Month, 1);
        mtbGrfIni.Text = ini.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
        mtbGrfFim.Text = venc.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        // Período próprio da tela da Folha (férias/rescisões): mesmo padrão mensal.
        mtbFolhaIni.Text = ini.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
        mtbFolhaFim.Text = venc.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        // Data do lote: primeiro dia do mês
        mtbGrfLote.Text = ini.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        // Preenche o seletor de centro da aba Folha
        PreencherCentrosFolha();
    }

    /// <summary>Preenche o combo de centro de custo da aba Folha com os centros da competência.</summary>
    private void PreencherCentrosFolha()
    {
        string comp = cmbCompetencia.SelectedItem?.ToString() ?? "";
        int tipoProcess = TipoProcessoTipoFolha();
        cmbFolhaCentro.Items.Clear();
        cmbFolhaCentro.Items.Add(new ComboCentro(0, "Todos os centros"));
        try
        {
            if (_conn != null && !string.IsNullOrWhiteSpace(comp))
            {
                foreach (var c in new DbService().ListarCentrosCusto(_conn, comp, tipoProcess))
                    cmbFolhaCentro.Items.Add(new ComboCentro(c.Codigo, c.Nome));
            }
        }
        catch { }
        cmbFolhaCentro.SelectedIndex = 0;
    }

    /// <summary>Retorna o tipo de processo da folha conforme a seleção: Quinzena = 41, demais = 11.</summary>
    private int TipoProcessoTipoFolha()
    {
        return cmbFolhaTipo.SelectedIndex == 1 ? 41 : 11;
    }

    private void cmbFolhaCentro_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Ao mudar o centro na aba Folha, não faz nada automático; só informa o filtro.
    }

    /// <summary>
    /// Pede o mapeamento da coluna B (Domínio -&gt; Sienge) para os centros do lote.
    /// Vem preenchido com o mapa salvo (ou o próprio centro) e grava ao confirmar.
    /// Retorna false se o usuário cancelar (chamador deve abortar a geração).
    /// </summary>
    private bool PedirMapaCentros(IEnumerable<int> centros)
    {
        var svc = new DbService();
        var lista = centros.Distinct().OrderBy(c => c)
            .Select(c => (Centro: c, Nome: svc.NomeCentroCusto(_conn!, c)))
            .ToList();
        if (lista.Count == 0) return true;
        var mapa = DbService.CarregarMapaCentros(DbService.Empresa);
        using var dlg = new PromptMapeamento(lista, mapa, "Mapeamento coluna B - Domínio → Sienge");
        if (dlg.ShowDialog(this) != DialogResult.OK) return false;
        mapa = dlg.Mapa;
        DbService.SalvarMapaCentros(DbService.Empresa, mapa);
        DbService.MapaCentrosSienge = mapa;
        return true;
    }

    /// <summary>Exibe o CSV gerado no campo de visualização, forçando o redesenho imediato.</summary>
    private static void MostrarPrevia(TextBox txt, string texto)
    {
        txt.Text = texto;
        txt.SelectionStart = 0;
        txt.SelectionLength = 0;
        txt.ScrollToCaret();
        txt.Refresh();
        txt.Update();
        Application.DoEvents();
    }

    /// <summary>Ao trocar o tipo de folha (mensal/quinzena/férias/rescisões), recarrega os centros.</summary>
    private void cmbFolhaTipo_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Período próprio da tela da Folha: só vale para Férias/Rescisões.
        bool comPeriodo = cmbFolhaTipo.SelectedIndex >= 2;
        mtbFolhaIni.Enabled = comPeriodo;
        mtbFolhaFim.Enabled = comPeriodo;
        PreencherCentrosFolha();
    }

    private void cmbVerba_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbVerba.SelectedItem is VerbaFinanceira v)
        {
            txtCredorNome.Text = v.Credor;
            txtObs.Text = v.Descricao;
            if (!string.IsNullOrWhiteSpace(v.CredorCodigo))
                txtCredorCodigo.Text = v.CredorCodigo;
            // Documento (K): não sobrescreve — o sistema usa a competência quando vazio,
            // e o usuário pode editar manualmente.
            AtualizarDocumento();
        }
    }

    private void txtCredor_TextChanged(object sender, EventArgs e)
    {
        if (dgvCentros.DataSource is DataTable dt && dt.Columns.Contains("CredorCodigo"))
        {
            foreach (DataRow r in dt.Rows)
            {
                r["CredorCodigo"] = txtCredorCodigo.Text;
                r["CredorNome"] = txtCredorNome.Text;
            }
            dgvCentros.Refresh();
        }
    }

    private void AtualizarDocumento()
    {
        // O documento (K) fica vazio = base automática RRRRVVVCCCDDMMAA na geração.
        // Se o usuário digitar, usa o digitado (+ sequência se marcada).
        txtDoc.Text = "";
        txtDoc.PlaceholderText = "vazio = automático";
    }

    private void btnCarregarCentros_Click(object sender, EventArgs e)
    {
        if (cmbCompetencia.SelectedItem == null)
        {
            MessageBox.Show("Selecione uma competência.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string comp = cmbCompetencia.SelectedItem.ToString()!;
        var centros = new DbService().ListarCentrosCusto(_conn!, comp, TipoProcessoTipoFolha());

        var dt = new DataTable();
        dt.Columns.Add("Sel", typeof(bool));
        dt.Columns.Add("Centro", typeof(int));
        dt.Columns.Add("Nome", typeof(string));
        dt.Columns.Add("Emp", typeof(int));
        dt.Columns.Add("Total", typeof(decimal));
        dt.Columns.Add("CredorCodigo", typeof(string));
        dt.Columns.Add("CredorNome", typeof(string));
        foreach (var c in centros)
            dt.Rows.Add(chkTodos.Checked, c.Codigo, c.Nome, 0, 0m, txtCredorCodigo.Text, txtCredorNome.Text);

        dgvCentros.DataSource = dt;
        dgvCentros.Columns["Sel"].Width = 40;
        dgvCentros.Columns["Centro"].Width = 70;
        dgvCentros.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgvCentros.Columns["Emp"].Width = 50;
        dgvCentros.Columns["Total"].DefaultCellStyle.Format = "N2";
        dgvCentros.Columns["CredorCodigo"].HeaderText = "Credor (C)";
        dgvCentros.Columns["CredorCodigo"].Width = 110;
        dgvCentros.Columns["CredorNome"].HeaderText = "Nome credor (D)";
        dgvCentros.Columns["CredorNome"].Width = 220;
        dgvCentros.Columns["Sel"].ReadOnly = false;
        dgvCentros.Columns["Centro"].ReadOnly = true;
        dgvCentros.Columns["Nome"].ReadOnly = true;
        dgvCentros.Columns["Emp"].ReadOnly = true;
        dgvCentros.Columns["Total"].ReadOnly = true;
        dgvCentros.CurrentCell = null;

        chkTodos.Enabled = true;
        btnGerarCsv.Enabled = true;
        _linhas = new List<(int, string, int, decimal, string, string)>();
        lblTotal.Text = "Total: R$ 0,00";
        txtResultado.Clear();
    }

    private void chkTodos_CheckedChanged(object sender, EventArgs e)
    {
        if (dgvCentros.DataSource is DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
                r["Sel"] = chkTodos.Checked;
            dgvCentros.Refresh();
        }
    }

    private void btnGerarCsv_Click(object sender, EventArgs e)
    {
        if (_conn == null) return;
        if (cmbCompetencia.SelectedItem == null)
        {
            MessageBox.Show("Selecione uma competência.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string comp = cmbCompetencia.SelectedItem.ToString()!;
        string venc = mtbVencimento.Text.Trim();
        string verba = cmbVerba.SelectedItem is VerbaFinanceira vf ? vf.Codigo.ToString() : "FOLHA";
        string doc = txtDoc.Text.Trim();
        string obs = txtObs.Text.Trim();

        if (!ValidarData(venc))
        {
            MessageBox.Show("Vencimento inválido. Use o formato DD/MM/AAAA.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var dt = dgvCentros.DataSource as DataTable;
        var selecionados = dt?.Rows.Cast<DataRow>().Where(r => (bool)r["Sel"]).ToList() ?? new();

        if (selecionados.Count == 0)
        {
            MessageBox.Show("Marque pelo menos um centro de custo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            var svc = new DbService();
            _linhas = new List<(int, string, int, decimal, string, string)>();
            foreach (var r in selecionados)
            {
                int centro = Convert.ToInt32(r["Centro"]);
                string nome = Convert.ToString(r["Nome"]) ?? "";
                string codCredor = Convert.ToString(r["CredorCodigo"]) ?? "";
                string nomeCredor = Convert.ToString(r["CredorNome"]) ?? "";
                var (n, tot) = svc.TotalPorCentro(_conn, comp, centro);
                _linhas.Add((centro, nome, n, tot, codCredor, nomeCredor));
                r["Emp"] = n;
                r["Total"] = tot;
            }
            dgvCentros.Refresh();

            decimal total = _linhas.Sum(l => l.Total);
            lblTotal.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";

            _csvGerado = DbService.GerarCsv(_linhas, venc, verba, doc, obs);
            MostrarPrevia(txtResultado, _csvGerado);
            btnSalvarCsv.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao consolidar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnSalvarCsv_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_csvGerado)) return;
        string comp = cmbCompetencia.SelectedItem?.ToString()?.Replace("/", "").Replace("-", "") ?? "comp";
        using var sfd = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"Importacao_Folha_{comp}.csv"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            // latin-1 para evitar problemas de acento no Excel
            System.IO.File.WriteAllText(sfd.FileName, _csvGerado, System.Text.Encoding.GetEncoding(28591));
            MessageBox.Show("Arquivo salvo: " + sfd.FileName, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnRelatorioMensal_Click(object sender, EventArgs e)
    {
        if (_conn == null) return;
        if (cmbCompetencia.SelectedItem == null)
        {
            MessageBox.Show("Selecione uma competência.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string comp = cmbCompetencia.SelectedItem.ToString()!;
        Cursor = Cursors.WaitCursor;
        Logger.Limpar();
        Logger.Log($"Início geração relatório - competência: {comp}");
        try
        {
            var svc = new DbService();
            var selecionados = CentrosSelecionadosNaSecao2();
            Logger.Log($"Centros selecionados: {selecionados.Count}");

            using var sfd = new SaveFileDialog
            {
                Filter = "Arquivo PDF (*.pdf)|*.pdf",
                FileName = $"Relatorio_Mensal_{comp.Replace("/", "")}.pdf"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Logger.Log($"Arquivo saída: {sfd.FileName}");
                if (selecionados.Count == 1)
                {
                    int centro = selecionados.First();
                    Logger.Log($"Centro: {centro}");
                    string centroNome = svc.NomeCentroCusto(_conn!, centro);
                    Logger.Log($"Nome centro: {centroNome}");
                    var funcionarios = svc.DetalhamentoPorFuncionario(_conn!, comp, centro);
                    Logger.Log($"Funcionários: {funcionarios.Count}");
                    var rubricas = svc.ResumoPorRubrica(_conn!, comp, centro);
                    Logger.Log($"Rubricas: {rubricas.Count}");
                    var bases = svc.ObterResumoBases(_conn!, comp, centro);
                    Logger.Log($"Bases - Emp: {bases.NumEmpregados}, Liq: {bases.Liquido}, Prov: {bases.Proventos}, Desc: {bases.Descontos}");
                    Logger.Log($"Bases - INSS: {bases.TotalInss}, BaseINSS: {bases.SalarioContribEmpregados}, BaseIRRf: {bases.BaseIrrfMensal}, IRRF: {bases.ValorIrrfMensal}");
                    Logger.Log($"Bases - FGTS: {bases.ValorFgts}, BaseFgts: {bases.BaseFgts}");
                    if (funcionarios.Count > 0)
                    {
                        var f0 = funcionarios[0];
                        Logger.Log($"[DEBUG] Func1: {f0.Nome}, Salario: {f0.Salario}, Prov: {f0.TotalProventos}, Desc: {f0.TotalDescontos}, Liq: {f0.Liquido}, Linhas: {f0.Linhas.Count}");
                        if (f0.Linhas.Count > 0)
                            foreach (var l in f0.Linhas.Take(5))
                                Logger.Log($"  Evento: {l.CodigoEvento} {l.NomeEvento} [{l.ProvDesc}] {l.Valor}");
                    }
                    Logger.Log("Gerando PDF...");
                    RelatorioPdf.GerarDetalhado(sfd.FileName, comp, centro, centroNome, funcionarios, rubricas, bases);
                    Logger.Log("PDF gerado com sucesso!");
                }
                else
                {
                    Logger.Log("Modo razão contábil...");
                    var razao = svc.ResumoRazaoContabil(_conn!, comp);
                    RelatorioPdf.GerarRazaoContabil(sfd.FileName, comp, razao);
                }
                MessageBox.Show("Relatório salvo: " + sfd.FileName, "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            Logger.LogErro("btnRelatorioMensal_Click", ex);
            MessageBox.Show("Erro ao gerar relatório: " + ex.Message + "\n\nLog: " + Logger.ObterLog(),
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private static bool ValidarData(string texto)
    {
        return DateTime.TryParseExact(texto, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Retorna os centros de custo marcados na seção 2 (dgvCentros).
    /// Se nenhum estiver marcado, retorna lista vazia (não filtra).
    /// </summary>
    private HashSet<int> CentrosSelecionadosNaSecao2()
    {
        var set = new HashSet<int>();
        if (dgvCentros.DataSource is DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                if (r["Sel"] is bool b && b && r["Centro"] is not DBNull)
                    set.Add(Convert.ToInt32(r["Centro"]));
            }
        }
        return set;
    }

    /// <summary>Filtra uma lista de linhas (Centro, ...) pelos centros selecionados na seção 2.</summary>
    private List<T> FiltrarPorCentro<T>(IEnumerable<T> linhas, Func<T, int> getCentro, HashSet<int> centros)
    {
        if (centros.Count == 0) return linhas.ToList();
        return linhas.Where(l => centros.Contains(getCentro(l))).ToList();
    }

    private void btnCarregarGrf_Click(object sender, EventArgs e)
    {
        if (_conn == null)
        {
            MessageBox.Show("Conecte ao banco primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!DateTime.TryParseExact(mtbGrfIni.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ini) ||
            !DateTime.TryParseExact(mtbGrfFim.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fim))
        {
            MessageBox.Show("Período de vencimento inválido (use DD/MM/AAAA).", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Cursor = Cursors.WaitCursor;
        try
        {
            var centros = CentrosSelecionadosNaSecao2();
            _linhasGrf = FiltrarPorCentro(
                new DbService().ListarGrf(_conn, ini, fim),
                x => x.ICcustos, centros);
            var dt = new DataTable();
            dt.Columns.Add("Sel", typeof(bool));
            dt.Columns.Add("Emp", typeof(int));
            dt.Columns.Add("Nome", typeof(string));
            dt.Columns.Add("Centro", typeof(int));
            dt.Columns.Add("Vencimento", typeof(string));
            dt.Columns.Add("Valor", typeof(decimal));
            foreach (var l in _linhasGrf)
                dt.Rows.Add(true, l.IEmpregados, l.Nome, l.ICcustos,
                    l.Vencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), l.Valor);

            dgvGrf.DataSource = dt;
            dgvGrf.Columns["Sel"].Width = 40;
            dgvGrf.Columns["Emp"].Width = 70;
            dgvGrf.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvGrf.Columns["Centro"].Width = 70;
            dgvGrf.Columns["Vencimento"].Width = 90;
            dgvGrf.Columns["Valor"].DefaultCellStyle.Format = "N2";
            dgvGrf.Columns["Sel"].ReadOnly = true;

            decimal total = _linhasGrf.Sum(x => x.Valor);
            lblTotalGrf.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";
            btnGerarGrf.Enabled = _linhasGrf.Count > 0;
            txtResultadoGrf.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar GRRF: " + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnGerarGrf_Click(object sender, EventArgs e)
    {
        if (_linhasGrf.Count == 0) return;
        if (!DateTime.TryParseExact(mtbGrfLote.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var lote))
        {
            MessageBox.Show("Data do lote inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        string centro = txtGrfCentro.Text.Trim();
        var centrosFiltro = CentrosSelecionadosNaSecao2();
        var selecionadas = _linhasGrf
            .Select((l, i) => new { l, i })
            .Where(x => dgvGrf.Rows[x.i].Cells["Sel"].Value is bool b && b)
            .Where(x => centrosFiltro.Count == 0 || centrosFiltro.Contains(x.l.ICcustos))
            .Select(x => x.l)
            .ToList();
        if (selecionadas.Count == 0)
        {
            MessageBox.Show("Marque pelo menos uma linha de GRRF.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _csvGeradoGrf = DbService.GerarCsvGrf(selecionadas, centro, lote);
        MostrarPrevia(txtResultadoGrf, _csvGeradoGrf);
        btnSalvarGrf.Enabled = true;
    }

    private void btnSalvarGrf_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_csvGeradoGrf)) return;
        using var sfd = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = "Lote_GRRF.csv"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            System.IO.File.WriteAllText(sfd.FileName, _csvGeradoGrf, System.Text.Encoding.GetEncoding(28591));
            MessageBox.Show("Arquivo salvo: " + sfd.FileName, "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnGerarFolha_Click(object sender, EventArgs e)
    {
        if (_conn == null)
        {
            MessageBox.Show("Conecte ao banco primeiro.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string venc = mtbVencimento.Text.Trim();
        if (!ValidarData(venc))
        {
            MessageBox.Show("Vencimento inválido. Use DD/MM/AAAA.", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        int tipo = cmbFolhaTipo.SelectedIndex;
        bool analitico = cmbFolhaModo.SelectedIndex == 1;
        string comp = cmbCompetencia.SelectedItem?.ToString() ?? "";
        string obs = txtObs.Text.Trim();
        // Código da verba (coluna A): numérico, conforme a tabela_financeira.
        // Folha Mensal/Quinzena = verba 1 (SALARIO/FOLHA), Férias = 3, Rescisão = 4.
        int codigoVerba = tipo switch
        {
            0 => 1,
            1 => 1,
            2 => 3,
            _ => 4,
        };
        int tipoProcess = tipo == 1 ? 41 : 11;
        // Credor (C = código, D = nome) conforme a verba da tabela_financeira.
        var vVerba = VerbaFinanceira.PorCodigo(codigoVerba);
        string credorCod = string.IsNullOrWhiteSpace(vVerba.CredorCodigo) ? "1" : vVerba.CredorCodigo;
        string credorNome = vVerba.Credor;
        string verba = codigoVerba.ToString();

        // Documento (K): o digitado, ou base automática RRRRVVVCCCDDMMAA
        // (aleatório único por arquivo); a sequência por linha garante que
        // nenhum documento se repita.
        string doc = txtDoc.Text.Trim();
        if (string.IsNullOrWhiteSpace(doc))
        {
            doc = DbService.GerarDocBase(codigoVerba, credorCod, DateTime.Now, Random.Shared.Next(1000, 10000));
            txtDoc.Text = doc;
        }

        // Férias e Rescisões usam o período próprio da tela da Folha (mtbFolhaIni/mtbFolhaFim)
        DateTime ini, fim;
        if (tipo == 2 || tipo == 3)
        {
            if (!DateTime.TryParseExact(mtbFolhaIni.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out ini) ||
                !DateTime.TryParseExact(mtbFolhaFim.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out fim))
            {
                MessageBox.Show("Período (Folha) inválido. Use DD/MM/AAAA.", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        else
        {
            ini = fim = DateTime.MinValue;
        }
        // Sufixo final do campo L: "FUNÇÃO MM/AA" (ex.: ADIANTAMENTO 09/26).
        // Férias/Rescisões usam o mês final do período; folha usa a competência.
        string funcaoFolha = tipo switch { 0 => "MENSAL", 1 => "ADIANTAMENTO", 2 => "FERIAS", _ => "RESCISAO" };
        string compSufixo;
        if (tipo == 2 || tipo == 3)
            compSufixo = fim.ToString("MM/yyyy", CultureInfo.InvariantCulture);
        else if (DateTime.TryParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var mesComp))
            compSufixo = mesComp.ToString("MM/yyyy", CultureInfo.InvariantCulture);
        else
            compSufixo = "";
        string sufixoObs = $"{funcaoFolha} {compSufixo}".Trim();

        Cursor = Cursors.WaitCursor;
        try
        {
            var svc = new DbService();
            // Filtro por centro: usa o seletor da aba Folha (preferencial) ou a seleção da seção 2.
            var centrosFiltro = CentrosSelecionadosNaSecao2();
            if (cmbFolhaCentro.SelectedItem is ComboCentro cc && cc.Codigo > 0)
                centrosFiltro = new HashSet<int> { cc.Codigo };

            // Pedir dados de apropriação (obra G, unidade H, item I, departamento J) ao gerar
            string obra = "", unidade = "", itemOrc = "", departamento = "";
            using (var dlg = new PromptApropriacao("Apropriação - Folha / Férias / Rescisões"))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                obra = dlg.Obra;
                unidade = dlg.Unidade;
                itemOrc = dlg.Item;
                departamento = dlg.Departamento;
            }

            if (tipo == 2 || tipo == 3)
            {
                if (analitico)
                {
                    List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)> linhas;
                    // Férias carrega os períodos para a obs; a seleção devolve 4 campos.
                    List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor, DateTime IniGozo, DateTime FimGozo, int Dias)>? ferFull = null;
                    if (tipo == 2)
                    {
                        ferFull = FiltrarPorCentro(
                            svc.ListarFeriasAnalitica(_conn, ini, fim),
                            x => x.Centro, centrosFiltro);
                        if (ferFull.Count == 0)
                        {
                            MessageBox.Show("Nenhum registro no período.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        linhas = ferFull.Select(x => (x.Centro, x.NomeEmpregado, x.Empregado, x.Valor)).ToList();
                    }
                    else
                    {
                        linhas = FiltrarPorCentro(
                            svc.ListarRescisaoAnalitica(_conn, ini, fim),
                            x => x.Centro, centrosFiltro);
                        if (linhas.Count == 0)
                        {
                            MessageBox.Show("Nenhum registro no período.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    // Seleção de funcionários (mostra Emp/Nome/Centro/Valor), igual ao completo.
                    using (var sel = new SelecaoFuncionarios(
                        linhas.Select(x => (x.Empregado, x.NomeEmpregado, x.Centro, x.Valor)).ToList(),
                        $"Selecionar funcionários - {cmbFolhaTipo.SelectedItem}"))
                    {
                        if (sel.ShowDialog(this) != DialogResult.OK) return;
                        var escolhidos = sel.Selecionados;
                        if (escolhidos.Count == 0)
                        {
                            MessageBox.Show("Marque pelo menos um funcionário.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        linhas = escolhidos.Select(s => (s.Centro, s.Nome, s.Empregado, s.Valor)).ToList();
                    }
                    if (tipo == 2 && ferFull != null)
                    {
                        // Recupera os períodos das linhas selecionadas.
                        var lookupFer = ferFull.ToLookup(x => (x.Empregado, x.Centro, x.Valor));
                        var comPer = linhas.Select(a => {
                            var f = lookupFer[(a.Empregado, a.Centro, a.Valor)].FirstOrDefault();
                            return (a.Centro, a.NomeEmpregado, a.Valor, f.IniGozo, f.FimGozo, f.Dias);
                        }).ToList();
                        if (!PedirMapaCentros(comPer.Select(x => x.Centro))) return;
                        _csvGeradoFolha = DbService.GerarCsvFerias(comPer,
                            venc, verba, credorCod, credorNome, doc, obra, unidade, itemOrc, departamento, chkDocSeq.Checked);
                    }
                    else
                    {
                        if (!PedirMapaCentros(linhas.Select(x => x.Centro))) return;
                        _csvGeradoFolha = DbService.GerarCsvAnalitico(linhas, venc, verba, credorCod, credorNome, doc, credorNome, obra, unidade, itemOrc, departamento, sufixoObs, chkDocSeq.Checked);
                    }
                    decimal total = linhas.Sum(x => x.Valor);
                    lblTotalFolha.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";

                    var dt = new DataTable();
                    dt.Columns.Add("Centro", typeof(string));
                    dt.Columns.Add("Empregado", typeof(int));
                    dt.Columns.Add("Nome", typeof(string));
                    dt.Columns.Add("Valor", typeof(decimal));
                    foreach (var l in linhas)
                        dt.Rows.Add(l.Centro.ToString("D4"), l.Empregado, l.NomeEmpregado, l.Valor);
                    dgvFolha.DataSource = dt;
                    dgvFolha.Columns["Centro"].Width = 70;
                    dgvFolha.Columns["Empregado"].Width = 80;
                    dgvFolha.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvFolha.Columns["Valor"].DefaultCellStyle.Format = "N2";
                }
                else
                {
                    List<(int Centro, string Nome, int Empregados, decimal Total)> centros;
                    if (tipo == 2)
                    {
                        // Férias por centro com período (menor início / maior fim do centro).
                        var centrosFer = FiltrarPorCentro(
                            svc.ResumoFeriasCentros(_conn, ini, fim),
                            x => x.Centro, centrosFiltro);
                        if (centrosFer.Count == 0)
                        {
                            MessageBox.Show("Nenhum registro no período.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (!PedirMapaCentros(centrosFer.Select(c => c.Centro))) return;
                        _csvGeradoFolha = DbService.GerarCsvFerias(
                            centrosFer.Select(c => (c.Centro, c.Nome, c.Total, c.IniGozo, c.FimGozo, c.Dias)).ToList(),
                            venc, verba, credorCod, credorNome, doc, obra, unidade, itemOrc, departamento, chkDocSeq.Checked);
                        centros = centrosFer.Select(c => (c.Centro, c.Nome, c.Empregados, c.Total)).ToList();
                    }
                    else
                    {
                        centros = FiltrarPorCentro(
                            svc.ResumoRescisaoCentros(_conn, ini, fim),
                            x => x.Centro, centrosFiltro);
                        if (centros.Count == 0)
                        {
                            MessageBox.Show("Nenhum registro no período.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        var linhasCompletas = centros
                            .Select(c => (c.Centro, c.Nome, c.Empregados, c.Total, credorCod, credorNome))
                            .ToList();
                        _csvGeradoFolha = DbService.GerarCsv(linhasCompletas, venc, verba, doc, credorNome, obra, unidade, itemOrc, departamento, sufixoObs, chkDocSeq.Checked);
                    }
                    decimal total = centros.Sum(x => x.Total);
                    lblTotalFolha.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";

                    var dt = new DataTable();
                    dt.Columns.Add("Centro", typeof(string));
                    dt.Columns.Add("Nome", typeof(string));
                    dt.Columns.Add("Emp", typeof(int));
                    dt.Columns.Add("Total", typeof(decimal));
                    foreach (var l in centros)
                        dt.Rows.Add(l.Centro.ToString("D4"), l.Nome, l.Empregados, l.Total);
                    dgvFolha.DataSource = dt;
                    dgvFolha.Columns["Centro"].Width = 70;
                    dgvFolha.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvFolha.Columns["Emp"].Width = 50;
                    dgvFolha.Columns["Total"].DefaultCellStyle.Format = "N2";
                }
            }
            else if (analitico)
            {
                var linhas = FiltrarPorCentro(
                    svc.ListarFolhaAnalitica(_conn, comp, tipoProcess),
                    x => x.Centro, centrosFiltro);
                if (linhas.Count == 0)
                {
                    MessageBox.Show("Nenhum empregado com líquido nesta competência.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Seleção de funcionários (mostra Emp/Nome/Centro/Valor), igual ao completo.
                using (var selFolha = new SelecaoFuncionarios(
                    linhas.Select(x => (x.Empregado, x.NomeEmpregado, x.Centro, x.Liquido)).ToList(),
                    $"Selecionar funcionários - {cmbFolhaTipo.SelectedItem}"))
                {
                    if (selFolha.ShowDialog(this) != DialogResult.OK) return;
                    var escolhidosFolha = selFolha.Selecionados;
                    if (escolhidosFolha.Count == 0)
                    {
                        MessageBox.Show("Marque pelo menos um funcionário.", "Aviso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    linhas = escolhidosFolha.Select(s => (s.Centro, s.Nome, s.Empregado, s.Valor)).ToList();
                }
                if (!PedirMapaCentros(linhas.Select(x => x.Centro))) return;
                _csvGeradoFolha = DbService.GerarCsvAnalitico(linhas, venc, verba, credorCod, credorNome, doc, credorNome, obra, unidade, itemOrc, departamento, sufixoObs, chkDocSeq.Checked);
                decimal total = linhas.Sum(x => x.Liquido);
                lblTotalFolha.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";

                var dt = new DataTable();
                dt.Columns.Add("Centro", typeof(string));
                dt.Columns.Add("Empregado", typeof(int));
                dt.Columns.Add("Nome", typeof(string));
                dt.Columns.Add("Liquido", typeof(decimal));
                foreach (var l in linhas)
                    dt.Rows.Add(l.Centro.ToString("D4"), l.Empregado, l.NomeEmpregado, l.Liquido);
                dgvFolha.DataSource = dt;
                dgvFolha.Columns["Centro"].Width = 70;
                dgvFolha.Columns["Empregado"].Width = 80;
                dgvFolha.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFolha.Columns["Liquido"].DefaultCellStyle.Format = "N2";
            }
            else
            {
                // Lista analítica (por funcionário) para permitir seleção antes de agrupar por centro.
                List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)> analiticas;
                List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor, DateTime IniGozo, DateTime FimGozo, int Dias)>? feriasBase = null;
                if (tipo == 2)
                {
                    feriasBase = FiltrarPorCentro(svc.ListarFeriasAnalitica(_conn, ini, fim), x => x.Centro, centrosFiltro);
                    analiticas = feriasBase.Select(x => (x.Centro, x.NomeEmpregado, x.Empregado, x.Valor)).ToList();
                }
                else if (tipo == 3)
                    analiticas = svc.ListarRescisaoAnalitica(_conn, ini, fim);
                else
                    analiticas = svc.ListarFolhaAnalitica(_conn, comp, tipoProcess)
                        .Select(x => (x.Centro, x.NomeEmpregado, x.Empregado, x.Liquido))
                        .ToList();

                analiticas = FiltrarPorCentro(analiticas, x => x.Centro, centrosFiltro);
                if (analiticas.Count == 0)
                {
                    MessageBox.Show("Nenhum funcionário no período.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var sel = new SelecaoFuncionarios(
                    analiticas.Select(x => (x.Empregado, x.NomeEmpregado, x.Centro, x.Valor)).ToList(),
                    $"Selecionar funcionários - {cmbFolhaTipo.SelectedItem}"))
                {
                    if (sel.ShowDialog(this) != DialogResult.OK) return;
                    var escolhidos = sel.Selecionados;
                    if (escolhidos.Count == 0)
                    {
                        MessageBox.Show("Marque pelo menos um funcionário.", "Aviso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    analiticas = escolhidos.Select(s => (s.Centro, s.Nome, s.Empregado, s.Valor)).ToList();
                }

                var agrupados = analiticas
                    .GroupBy(x => x.Centro)
                    .OrderBy(g => g.Key)
                    .Select(g => (Centro: g.Key, Empregados: g.Count(), Total: g.Sum(x => x.Valor)))
                    .ToList();
                if (agrupados.Count == 0)
                {
                    MessageBox.Show("Nenhum valor na seleção.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var linhasCompletas = new List<(int Centro, string Nome, int Empregados, decimal Total, string CredorCodigo, string CredorNome)>();
                foreach (var g in agrupados)
                    linhasCompletas.Add((g.Centro, svc.NomeCentroCusto(_conn!, g.Centro), g.Empregados, g.Total, credorCod, credorNome));
                if (linhasCompletas.Count == 0)
                {
                    MessageBox.Show("Nenhum valor nesta competência.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!PedirMapaCentros(linhasCompletas.Select(x => x.Centro))) return;
                if (tipo == 2 && feriasBase != null)
                {
                    // Períodos de gozo por centro a partir das linhas de férias selecionadas.
                    var lookupFer = feriasBase.ToLookup(x => (x.Empregado, x.Centro, x.Valor));
                    var perCentro = analiticas
                        .Select(a => lookupFer[(a.Empregado, a.Centro, a.Valor)].FirstOrDefault())
                        .GroupBy(x => x.Centro)
                        .ToDictionary(g => g.Key, g => (Ini: g.Min(x => x.IniGozo), Fim: g.Max(x => x.FimGozo), Dias: g.Sum(x => x.Dias)));
                    _csvGeradoFolha = DbService.GerarCsvFerias(
                        linhasCompletas.Select(l => {
                            perCentro.TryGetValue(l.Centro, out var p);
                            return (l.Centro, l.Nome, l.Total, p.Ini, p.Fim, p.Dias);
                        }).ToList(),
                        venc, verba, credorCod, credorNome, doc, obra, unidade, itemOrc, departamento, chkDocSeq.Checked);
                }
                else
                    _csvGeradoFolha = DbService.GerarCsv(linhasCompletas, venc, verba, doc, credorNome, obra, unidade, itemOrc, departamento, sufixoObs, chkDocSeq.Checked);
                decimal total = linhasCompletas.Sum(x => x.Total);
                lblTotalFolha.Text = $"Total: R$ {total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";

                var dt = new DataTable();
                dt.Columns.Add("Centro", typeof(string));
                dt.Columns.Add("Nome", typeof(string));
                dt.Columns.Add("Emp", typeof(int));
                dt.Columns.Add("Total", typeof(decimal));
                foreach (var l in linhasCompletas)
                    dt.Rows.Add(l.Centro.ToString("D4"), l.Nome, l.Empregados, l.Total);
                dgvFolha.DataSource = dt;
                dgvFolha.Columns["Centro"].Width = 70;
                dgvFolha.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvFolha.Columns["Emp"].Width = 50;
                dgvFolha.Columns["Total"].DefaultCellStyle.Format = "N2";
            }

            MostrarPrevia(txtResultadoFolha, _csvGeradoFolha);
            btnSalvarFolha.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao gerar: " + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnSalvarFolha_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_csvGeradoFolha)) return;
        string comp = cmbCompetencia.SelectedItem?.ToString()?.Replace("/", "").Replace("-", "") ?? "folha";
        string tipo = cmbFolhaTipo.SelectedItem?.ToString()?.Replace(" ", "_") ?? "Folha";
        using var sfd = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"Importacao_{tipo}_{comp}.csv"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            System.IO.File.WriteAllText(sfd.FileName, _csvGeradoFolha, System.Text.Encoding.GetEncoding(28591));
            MessageBox.Show("Arquivo salvo: " + sfd.FileName, "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnGerarGuias_Click(object sender, EventArgs e)
    {
        if (_conn == null || cmbCompetencia.SelectedItem == null)
        {
            MessageBox.Show("Conecte e selecione uma competência.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string comp = cmbCompetencia.SelectedItem.ToString()!;
        string venc = mtbVencimento.Text.Trim();
        if (!ValidarData(venc))
        {
            MessageBox.Show("Vencimento inválido. Use DD/MM/AAAA.", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        string doc = string.IsNullOrWhiteSpace(txtDoc.Text.Trim())
            ? comp.Replace("/", "")
            : txtDoc.Text.Trim();

        int tipoIdx = cmbGuiasTipo.SelectedIndex;
        bool analitico = cmbGuiasModo.SelectedIndex == 1;

        // Pedir dados de apropriação (obra G, unidade H, item I, departamento J) ao gerar
        string obra = "", unidade = "", itemOrc = "", departamento = "";
        using (var dlg = new PromptApropriacao("Apropriação - Guias"))
        {
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            obra = dlg.Obra;
            unidade = dlg.Unidade;
            itemOrc = dlg.Item;
            departamento = dlg.Departamento;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            var svc = new DbService();
            var centrosFiltro = CentrosSelecionadosNaSecao2();

            // Código/nome do credor por tipo (tabela_financeira)
            string descricao = cmbGuiasTipo.SelectedItem?.ToString() ?? "INSS";
            string codCredor, nomeCredor;
            switch (descricao.ToUpperInvariant())
            {
                case "INSS": codCredor = "298"; nomeCredor = "MINISTERIO DA FAZENDA"; break;
                case "IRRF": codCredor = ""; nomeCredor = "MINISTERIO DA FAZENDA"; break;
                case "FGTS": codCredor = ""; nomeCredor = "CAIXA ECONOMICA FEDERAL"; break;
                case "ECONSIGNADO": codCredor = ""; nomeCredor = "CAIXA ECONOMICA FEDERAL"; break;
                default: codCredor = "37"; nomeCredor = "GRRF"; break;
            }
            if (string.IsNullOrWhiteSpace(codCredor)) codCredor = "1";

            // Modo analítico: por funcionário (1 linha por pessoa), filtrado por centro da seção 2.
            List<(int Centro, string NomeEmpregado, int Empregado, decimal Valor)> analiticoLinhas = new();

            if (analitico)
            {
                switch (descricao.ToUpperInvariant())
                {
                    case "INSS":
                        analiticoLinhas = FiltrarPorCentro(svc.GuiaAnaliticoPorClasse(_conn, comp, 12), x => x.Centro, centrosFiltro);
                        break;
                    case "IRRF":
                        analiticoLinhas = FiltrarPorCentro(svc.GuiaAnaliticoPorClasse(_conn, comp, 13), x => x.Centro, centrosFiltro);
                        break;
                    case "FGTS":
                        analiticoLinhas = FiltrarPorCentro(svc.GuiaAnaliticoPorClasse(_conn, comp, 14), x => x.Centro, centrosFiltro);
                        break;
                    case "ECONSIGNADO":
                        analiticoLinhas = FiltrarPorCentro(svc.GuiaAnaliticoEmprestimos(_conn, 49), x => x.Centro, centrosFiltro);
                        break;
                    case "GRRF":
                        analiticoLinhas = FiltrarPorCentro(
                            svc.ListarRescisaoAnalitica(_conn, DateTime.ParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture).AddMonths(-1),
                                DateTime.ParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture)),
                            x => x.Centro, centrosFiltro);
                        break;
                }
                if (analiticoLinhas.Count == 0)
                {
                    MessageBox.Show("Nenhum registro por funcionário no período.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var sb = new System.Text.StringBuilder();
                foreach (var l in analiticoLinhas)
                {
                    var cc = l.Centro.ToString("D4");
                    var valor = l.Valor.ToString("0.00", CultureInfo.InvariantCulture);
                    sb.AppendLine($"{descricao};{cc};{codCredor};{nomeCredor};{valor};{venc};{obra};{unidade};{itemOrc};{departamento};{doc};{l.NomeEmpregado}");
                }
                _csvGeradoGuias = sb.ToString();
                MostrarPrevia(txtResultadoGuias, _csvGeradoGuias);
                decimal tot = analiticoLinhas.Sum(x => x.Valor);
                lblTotalGuias.Text = $"Total: R$ {tot.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";
                btnSalvarGuias.Enabled = true;

                var dt = new DataTable();
                dt.Columns.Add("Centro", typeof(string));
                dt.Columns.Add("Empregado", typeof(int));
                dt.Columns.Add("Nome", typeof(string));
                dt.Columns.Add("Valor", typeof(decimal));
                foreach (var l in analiticoLinhas)
                    dt.Rows.Add(l.Centro.ToString("D4"), l.Empregado, l.NomeEmpregado, l.Valor);
                dgvGuias.DataSource = dt;
                dgvGuias.Columns["Centro"].Width = 70;
                dgvGuias.Columns["Empregado"].Width = 80;
                dgvGuias.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvGuias.Columns["Valor"].DefaultCellStyle.Format = "N2";
            }
            else
            {
                // Modo completo: total por centro ou total geral da guia.
                var linhasCompletas = new List<(int Centro, string Nome, decimal Total)>();
                if (descricao.ToUpperInvariant() == "GRRF")
                {
                    var mes = DateTime.ParseExact(comp, "MM/yyyy", CultureInfo.InvariantCulture);
                    var centros = svc.ResumoRescisaoCentros(_conn, mes.AddMonths(-1), mes);
                    foreach (var c in centros)
                        if (centrosFiltro.Count == 0 || centrosFiltro.Contains(c.Centro))
                            linhasCompletas.Add((c.Centro, c.Nome, c.Total));
                }
                else
                {
                    // INSS/FGTS/IRRF/eCONSIGNADO completos por centro
                    var analiticoTodos = descricao.ToUpperInvariant() switch
                    {
                        "INSS" => svc.GuiaAnaliticoPorClasse(_conn, comp, 12),
                        "IRRF" => svc.GuiaAnaliticoPorClasse(_conn, comp, 13),
                        "FGTS" => svc.GuiaAnaliticoPorClasse(_conn, comp, 14),
                        _ => svc.GuiaAnaliticoEmprestimos(_conn, 49),
                    };
                    var agrupado = analiticoTodos
                        .Where(l => centrosFiltro.Count == 0 || centrosFiltro.Contains(l.Centro))
                        .GroupBy(l => l.Centro)
                        .Select(g => (g.Key, "", g.Sum(x => x.Valor)))
                        .OrderBy(g => g.Item1)
                        .ToList();
                    linhasCompletas = agrupado;
                }
                if (linhasCompletas.Count == 0)
                {
                    MessageBox.Show("Nenhuma guia encontrada.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var sb2 = new System.Text.StringBuilder();
                int i = 1;
                foreach (var l in linhasCompletas)
                {
                    var cc = l.Centro.ToString("D4");
                    var valor = l.Total.ToString("0.00", CultureInfo.InvariantCulture);
                    string nomeCentro = string.IsNullOrWhiteSpace(l.Nome) ? svc.NomeCentroCusto(_conn!, l.Centro) : l.Nome;
                    sb2.AppendLine($"{descricao};{cc};{codCredor};{nomeCredor};{valor};{venc};{obra};{unidade};{itemOrc};{departamento};{doc};{nomeCentro}");
                    i++;
                }
                _csvGeradoGuias = sb2.ToString();
                MostrarPrevia(txtResultadoGuias, _csvGeradoGuias);
                decimal total2 = linhasCompletas.Sum(x => x.Total);
                lblTotalGuias.Text = $"Total: R$ {total2.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}";
                btnSalvarGuias.Enabled = true;

                var dt2 = new DataTable();
                dt2.Columns.Add("Centro", typeof(string));
                dt2.Columns.Add("Nome", typeof(string));
                dt2.Columns.Add("Total", typeof(decimal));
                foreach (var l in linhasCompletas)
                    dt2.Rows.Add(l.Centro.ToString("D4"), string.IsNullOrWhiteSpace(l.Nome) ? svc.NomeCentroCusto(_conn!, l.Centro) : l.Nome, l.Total);
                dgvGuias.DataSource = dt2;
                dgvGuias.Columns["Centro"].Width = 70;
                dgvGuias.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvGuias.Columns["Total"].DefaultCellStyle.Format = "N2";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao gerar guias: " + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnSalvarGuias_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_csvGeradoGuias)) return;
        string comp = cmbCompetencia.SelectedItem?.ToString()?.Replace("/", "").Replace("-", "") ?? "guias";
        using var sfd = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"Guias_{comp}.csv"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            System.IO.File.WriteAllText(sfd.FileName, _csvGeradoGuias, System.Text.Encoding.GetEncoding(28591));
            MessageBox.Show("Arquivo salvo: " + sfd.FileName, "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

/// <summary>Item do combo de centro de custo (código + nome).</summary>
public class ComboCentro
{
    public int Codigo { get; }
    public string Nome { get; }
    public ComboCentro(int codigo, string nome) { Codigo = codigo; Nome = nome; }
    public override string ToString() => Codigo == 0 ? Nome : $"{Codigo:D4} - {Nome}";
}

/// <summary>Item do combo de empresa (codi_emp + nome fantasia/razão).</summary>
public class ComboEmpresa
{
    public int Codigo { get; }
    public string Nome { get; }
    public ComboEmpresa(int codigo, string nome) { Codigo = codigo; Nome = nome; }
    public override string ToString() => $"{Codigo} - {Nome}";
}
