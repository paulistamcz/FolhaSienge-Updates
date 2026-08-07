using System.Data;
using System.Data.Odbc;
using System.Globalization;

namespace FolhaSienge;

public partial class Form1 : Form
{
    private OdbcConnection? _conn;
    private List<(int Centro, string Nome, int Empregados, decimal Total, string CredorCodigo, string CredorNome)> _linhas = new();
    private string? _csvGerado;

    public Form1()
    {
        InitializeComponent();
        this.Shown += Form1_Shown;
        CarregarVerbas();
        Text = $"Importação Folha de Pagamento - Sienge (ENGEMAT)  v{Atualizador.VersaoAtual}";
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
            var release = await Atualizador.BuscarUltimaVersaoAsync();
            if (Atualizador.TemVersaoNova(release))
            {
                var resposta = MessageBox.Show(this,
                    $"Existe uma nova versão disponível: {release!.Versao}\n\n" +
                    $"Sua versão atual: {Atualizador.VersaoAtual}\n\n" +
                    "Deseja atualizar agora? O app será fechado e reaberto automaticamente.",
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
            CarregarCompetencias();
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
            }
            else
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
    }

    private void cmbVerba_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbVerba.SelectedItem is VerbaFinanceira v)
        {
            txtCredorNome.Text = v.Credor;
            txtDoc.Text = v.Documento;
            txtObs.Text = v.Descricao;
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
        // O documento (K) agora é definido pela verba selecionada.
        // A competência preenche apenas se o documento estiver vazio.
        if (string.IsNullOrWhiteSpace(txtDoc.Text))
        {
            var comp = cmbCompetencia.SelectedItem?.ToString() ?? "";
            if (!string.IsNullOrEmpty(comp))
                txtDoc.Text = comp.Replace("/", "");
        }
    }

    private void btnCarregarCentros_Click(object sender, EventArgs e)
    {
        if (cmbCompetencia.SelectedItem == null)
        {
            MessageBox.Show("Selecione uma competência.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        string comp = cmbCompetencia.SelectedItem.ToString()!;
        var centros = new DbService().ListarCentrosCusto(_conn!, comp);

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
        dgvCentros.Columns["Sel"].ReadOnly = true;
        dgvCentros.Columns["Centro"].ReadOnly = true;
        dgvCentros.Columns["Nome"].ReadOnly = true;
        dgvCentros.Columns["Emp"].ReadOnly = true;
        dgvCentros.Columns["Total"].ReadOnly = true;

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
            txtResultado.Text = _csvGerado;
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
            // latin-1 para evitar problemas de acento no Excel/Sienge
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
        try
        {
            var svc = new DbService();
            var itens = svc.RelatorioMensalPorVerba(_conn, comp);
            var centros = svc.ResumoCentrosCusto(_conn, comp);
            using var sfd = new SaveFileDialog
            {
                Filter = "Arquivo PDF (*.pdf)|*.pdf",
                FileName = $"Relatorio_Mensal_{comp.Replace("/", "")}.pdf"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                RelatorioPdf.Gerar(sfd.FileName, comp, itens, centros);
                MessageBox.Show("Relatório salvo: " + sfd.FileName, "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao gerar relatório: " + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
