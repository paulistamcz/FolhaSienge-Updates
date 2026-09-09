namespace FolhaSienge;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private GroupBox grpBanco;
    private Button btnBuscarBanco;
    private ComboBox cmbBancos;
    private Button btnAtualizarBanco;
    private Button btnSelecionarArquivo;
    private Button btnConectar;
    private Label lblStatusBanco;
    private Label lblServidor;
    private TextBox txtServidor;
    private Button btnConectarRede;

    private GroupBox grpDados;
    private Label lblEmpresa;
    private ComboBox cmbEmpresa;
    private Label lblCompetencia;
    private ComboBox cmbCompetencia;
    private Button btnCarregarCentros;
    private Button btnRelatorioMensal;
    private DataGridView dgvCentros;
    private CheckBox chkTodos;

    private GroupBox grpCsv;
    private Label lblVencimento;
    private MaskedTextBox mtbVencimento;
    private Label lblVerba;
    private ComboBox cmbVerba;
    private Label lblDoc;
    private TextBox txtDoc;
    private Label lblObs;
    private TextBox txtObs;
    private Label lblCredorCodigo;
    private TextBox txtCredorCodigo;
    private Label lblCredorNome;
    private TextBox txtCredorNome;
    private Button btnGerarCsv;
    private Button btnSalvarCsv;
    private Label lblTotal;
    private TextBox txtResultado;

    private GroupBox grpGrf;
    private Label lblGrfPeriodo;
    private MaskedTextBox mtbGrfIni;
    private Label lblGrfAte;
    private MaskedTextBox mtbGrfFim;
    private Label lblGrfLote;
    private MaskedTextBox mtbGrfLote;
    private Label lblGrfCentro;
    private TextBox txtGrfCentro;
    private Button btnCarregarGrf;
    private Button btnGerarGrf;
    private Button btnSalvarGrf;
    private Label lblTotalGrf;
    private DataGridView dgvGrf;
    private TextBox txtResultadoGrf;

    private GroupBox grpFolha;
    private Label lblFolhaTipo;
    private ComboBox cmbFolhaTipo;
    private Label lblFolhaModo;
    private ComboBox cmbFolhaModo;
    private Label lblFolhaCentro;
    private ComboBox cmbFolhaCentro;
    private Button btnGerarFolha;
    private Button btnSalvarFolha;
    private Label lblTotalFolha;
    private DataGridView dgvFolha;
    private TextBox txtResultadoFolha;

    private GroupBox grpGuias;
    private Label lblGuiasTipo;
    private ComboBox cmbGuiasTipo;
    private Label lblGuiasModo;
    private ComboBox cmbGuiasModo;
    private Button btnGerarGuias;
    private Button btnSalvarGuias;
    private Label lblTotalGuias;
    private DataGridView dgvGuias;
    private TextBox txtResultadoGuias;

    private TabControl tabExport;
    private TabPage tabGrf;
    private TabPage tabFolha;
    private TabPage tabGuias;

    private Button btnSair;

    private void InitializeComponent()
    {
        this.grpBanco = new GroupBox();
        this.btnBuscarBanco = new Button();
        this.cmbBancos = new ComboBox();
        this.btnAtualizarBanco = new Button();
        this.btnSelecionarArquivo = new Button();
        this.btnConectar = new Button();
        this.lblStatusBanco = new Label();
        this.lblServidor = new Label();
        this.txtServidor = new TextBox();
        this.btnConectarRede = new Button();
        this.grpDados = new GroupBox();
        this.lblEmpresa = new Label();
        this.cmbEmpresa = new ComboBox();
        this.lblCompetencia = new Label();
        this.cmbCompetencia = new ComboBox();
        this.btnCarregarCentros = new Button();
        this.btnRelatorioMensal = new Button();
        this.dgvCentros = new DataGridView();
        this.chkTodos = new CheckBox();
        this.grpCsv = new GroupBox();
        this.lblVencimento = new Label();
        this.mtbVencimento = new MaskedTextBox();
        this.lblVerba = new Label();
        this.cmbVerba = new ComboBox();
        this.lblDoc = new Label();
        this.txtDoc = new TextBox();
        this.lblObs = new Label();
        this.txtObs = new TextBox();
        this.lblCredorCodigo = new Label();
        this.txtCredorCodigo = new TextBox();
        this.lblCredorNome = new Label();
        this.txtCredorNome = new TextBox();
        this.btnGerarCsv = new Button();
        this.btnSalvarCsv = new Button();
        this.lblTotal = new Label();
        this.txtResultado = new TextBox();
        this.grpGrf = new GroupBox();
        this.lblGrfPeriodo = new Label();
        this.mtbGrfIni = new MaskedTextBox();
        this.lblGrfAte = new Label();
        this.mtbGrfFim = new MaskedTextBox();
        this.lblGrfLote = new Label();
        this.mtbGrfLote = new MaskedTextBox();
        this.lblGrfCentro = new Label();
        this.txtGrfCentro = new TextBox();
        this.btnCarregarGrf = new Button();
        this.btnGerarGrf = new Button();
        this.btnSalvarGrf = new Button();
        this.lblTotalGrf = new Label();
        this.dgvGrf = new DataGridView();
        this.txtResultadoGrf = new TextBox();
        this.grpFolha = new GroupBox();
        this.lblFolhaTipo = new Label();
        this.cmbFolhaTipo = new ComboBox();
        this.lblFolhaModo = new Label();
        this.cmbFolhaModo = new ComboBox();
        this.lblFolhaCentro = new Label();
        this.cmbFolhaCentro = new ComboBox();
        this.btnGerarFolha = new Button();
        this.btnSalvarFolha = new Button();
        this.lblTotalFolha = new Label();
        this.dgvFolha = new DataGridView();
        this.txtResultadoFolha = new TextBox();
        this.grpGuias = new GroupBox();
        this.lblGuiasTipo = new Label();
        this.cmbGuiasTipo = new ComboBox();
        this.lblGuiasModo = new Label();
        this.cmbGuiasModo = new ComboBox();
        this.btnGerarGuias = new Button();
        this.btnSalvarGuias = new Button();
        this.lblTotalGuias = new Label();
        this.dgvGuias = new DataGridView();
        this.txtResultadoGuias = new TextBox();
        this.tabExport = new TabControl();
        this.tabGrf = new TabPage();
        this.tabFolha = new TabPage();
        this.tabGuias = new TabPage();
        this.btnSair = new Button();
        this.grpBanco.SuspendLayout();
        this.grpDados.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCentros)).BeginInit();
        this.grpCsv.SuspendLayout();
        this.SuspendLayout();
        // 
        // grpBanco
        // 
        this.grpBanco.Controls.Add(this.btnBuscarBanco);
        this.grpBanco.Controls.Add(this.cmbBancos);
        this.grpBanco.Controls.Add(this.btnAtualizarBanco);
        this.grpBanco.Controls.Add(this.btnSelecionarArquivo);
        this.grpBanco.Controls.Add(this.btnConectar);
        this.grpBanco.Controls.Add(this.lblStatusBanco);
        this.grpBanco.Controls.Add(this.lblServidor);
        this.grpBanco.Controls.Add(this.txtServidor);
        this.grpBanco.Controls.Add(this.btnConectarRede);
        this.grpBanco.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpBanco.Location = new System.Drawing.Point(12, 12);
        this.grpBanco.Name = "grpBanco";
        this.grpBanco.Size = new System.Drawing.Size(960, 108);
        this.grpBanco.TabIndex = 0;
        this.grpBanco.TabStop = false;
        this.grpBanco.Text = "1. Banco de dados (busca automática no servidor)";
        // 
        // btnBuscarBanco
        // 
        this.btnBuscarBanco.Location = new System.Drawing.Point(16, 24);
        this.btnBuscarBanco.Name = "btnBuscarBanco";
        this.btnBuscarBanco.Size = new System.Drawing.Size(170, 28);
        this.btnBuscarBanco.TabIndex = 0;
        this.btnBuscarBanco.Text = "Buscar banco no sistema";
        this.btnBuscarBanco.UseVisualStyleBackColor = true;
        this.btnBuscarBanco.Click += new EventHandler(this.btnBuscarBanco_Click);
        // 
        // btnConectar
        // 
        this.btnConectar.Enabled = false;
        this.btnConectar.Location = new System.Drawing.Point(192, 24);
        this.btnConectar.Name = "btnConectar";
        this.btnConectar.Size = new System.Drawing.Size(190, 28);
        this.btnConectar.TabIndex = 3;
        this.btnConectar.Text = "Conectar (EXTERNO/123456)";
        this.btnConectar.UseVisualStyleBackColor = true;
        this.btnConectar.Click += new EventHandler(this.btnConectar_Click);
        // 
        // btnSelecionarArquivo
        // 
        this.btnSelecionarArquivo.Location = new System.Drawing.Point(388, 24);
        this.btnSelecionarArquivo.Name = "btnSelecionarArquivo";
        this.btnSelecionarArquivo.Size = new System.Drawing.Size(150, 28);
        this.btnSelecionarArquivo.TabIndex = 5;
        this.btnSelecionarArquivo.Text = "Selecionar arquivo...";
        this.btnSelecionarArquivo.UseVisualStyleBackColor = true;
        this.btnSelecionarArquivo.Click += new EventHandler(this.btnSelecionarArquivo_Click);
        // 
        // lblServidor
        // 
        this.lblServidor.AutoSize = true;
        this.lblServidor.Location = new System.Drawing.Point(560, 31);
        this.lblServidor.Name = "lblServidor";
        this.lblServidor.Size = new System.Drawing.Size(61, 15);
        this.lblServidor.TabIndex = 6;
        this.lblServidor.Text = "Servidor:";
        // 
        // txtServidor
        // 
        this.txtServidor.Location = new System.Drawing.Point(623, 27);
        this.txtServidor.Name = "txtServidor";
        this.txtServidor.Size = new System.Drawing.Size(130, 23);
        this.txtServidor.TabIndex = 7;
        this.txtServidor.Text = "contabil";
        // 
        // btnConectarRede
        // 
        this.btnConectarRede.Location = new System.Drawing.Point(759, 25);
        this.btnConectarRede.Name = "btnConectarRede";
        this.btnConectarRede.Size = new System.Drawing.Size(185, 28);
        this.btnConectarRede.TabIndex = 8;
        this.btnConectarRede.Text = "Conectar pela rede (TCP/IP)";
        this.btnConectarRede.UseVisualStyleBackColor = true;
        this.btnConectarRede.Click += new EventHandler(this.btnConectarRede_Click);
        // 
        // cmbBancos
        // 
        this.cmbBancos.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbBancos.Location = new System.Drawing.Point(16, 60);
        this.cmbBancos.Name = "cmbBancos";
        this.cmbBancos.Size = new System.Drawing.Size(640, 23);
        this.cmbBancos.TabIndex = 1;
        // 
        // btnAtualizarBanco
        // 
        this.btnAtualizarBanco.Location = new System.Drawing.Point(662, 59);
        this.btnAtualizarBanco.Name = "btnAtualizarBanco";
        this.btnAtualizarBanco.Size = new System.Drawing.Size(90, 26);
        this.btnAtualizarBanco.TabIndex = 2;
        this.btnAtualizarBanco.Text = "Atualizar";
        this.btnAtualizarBanco.UseVisualStyleBackColor = true;
        this.btnAtualizarBanco.Click += new EventHandler(this.btnAtualizarBanco_Click);
        // 
        // lblStatusBanco
        // 
        this.lblStatusBanco.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)));
        this.lblStatusBanco.Location = new System.Drawing.Point(760, 63);
        this.lblStatusBanco.Name = "lblStatusBanco";
        this.lblStatusBanco.Size = new System.Drawing.Size(190, 20);
        this.lblStatusBanco.TabIndex = 4;
        this.lblStatusBanco.Text = "Procurando servidor...";
        this.lblStatusBanco.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // grpDados
        // 
        this.grpDados.Controls.Add(this.lblEmpresa);
        this.grpDados.Controls.Add(this.cmbEmpresa);
        this.grpDados.Controls.Add(this.lblCompetencia);
        this.grpDados.Controls.Add(this.cmbCompetencia);
        this.grpDados.Controls.Add(this.btnCarregarCentros);
        this.grpDados.Controls.Add(this.btnRelatorioMensal);
        this.grpDados.Controls.Add(this.dgvCentros);
        this.grpDados.Controls.Add(this.chkTodos);
        this.grpDados.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpDados.Location = new System.Drawing.Point(12, 126);
        this.grpDados.Name = "grpDados";
        this.grpDados.Size = new System.Drawing.Size(960, 300);
        this.grpDados.TabIndex = 1;
        this.grpDados.TabStop = false;
        this.grpDados.Text = "2. Competência e centros de custo";
        // 
        // lblEmpresa
        // 
        this.lblEmpresa.AutoSize = true;
        this.lblEmpresa.Location = new System.Drawing.Point(16, 24);
        this.lblEmpresa.Name = "lblEmpresa";
        this.lblEmpresa.Size = new System.Drawing.Size(56, 15);
        this.lblEmpresa.TabIndex = 0;
        this.lblEmpresa.Text = "Empresa:";
        // 
        // cmbEmpresa
        // 
        this.cmbEmpresa.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbEmpresa.Enabled = false;
        this.cmbEmpresa.Location = new System.Drawing.Point(76, 20);
        this.cmbEmpresa.Name = "cmbEmpresa";
        this.cmbEmpresa.Size = new System.Drawing.Size(200, 23);
        this.cmbEmpresa.TabIndex = 1;
        this.cmbEmpresa.SelectedIndexChanged += new EventHandler(this.cmbEmpresa_SelectedIndexChanged);
        // 
        // lblCompetencia
        // 
        this.lblCompetencia.AutoSize = true;
        this.lblCompetencia.Location = new System.Drawing.Point(290, 24);
        this.lblCompetencia.Name = "lblCompetencia";
        this.lblCompetencia.Size = new System.Drawing.Size(76, 15);
        this.lblCompetencia.TabIndex = 0;
        this.lblCompetencia.Text = "Competência:";
        // 
        // cmbCompetencia
        // 
        this.cmbCompetencia.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbCompetencia.Enabled = false;
        this.cmbCompetencia.Location = new System.Drawing.Point(372, 20);
        this.cmbCompetencia.Name = "cmbCompetencia";
        this.cmbCompetencia.Size = new System.Drawing.Size(180, 23);
        this.cmbCompetencia.TabIndex = 1;
        this.cmbCompetencia.SelectedIndexChanged += new EventHandler(this.cmbCompetencia_SelectedIndexChanged);
        // 
        // btnCarregarCentros
        // 
        this.btnCarregarCentros.Enabled = false;
        this.btnCarregarCentros.Location = new System.Drawing.Point(560, 19);
        this.btnCarregarCentros.Name = "btnCarregarCentros";
        this.btnCarregarCentros.Size = new System.Drawing.Size(180, 26);
        this.btnCarregarCentros.TabIndex = 2;
        this.btnCarregarCentros.Text = "Carregar centros de custo";
        this.btnCarregarCentros.UseVisualStyleBackColor = true;
        this.btnCarregarCentros.Click += new EventHandler(this.btnCarregarCentros_Click);
        // 
        // btnRelatorioMensal
        // 
        this.btnRelatorioMensal.Enabled = false;
        this.btnRelatorioMensal.Location = new System.Drawing.Point(760, 19);
        this.btnRelatorioMensal.Name = "btnRelatorioMensal";
        this.btnRelatorioMensal.Size = new System.Drawing.Size(180, 26);
        this.btnRelatorioMensal.TabIndex = 30;
        this.btnRelatorioMensal.Text = "Relatório Mensal (PDF)";
        this.btnRelatorioMensal.UseVisualStyleBackColor = true;
        this.btnRelatorioMensal.Click += new EventHandler(this.btnRelatorioMensal_Click);
        // 
        // chkTodos
        // 
        this.chkTodos.AutoSize = true;
        this.chkTodos.Checked = true;
        this.chkTodos.CheckState = CheckState.Checked;
        this.chkTodos.Enabled = false;
        this.chkTodos.Location = new System.Drawing.Point(16, 55);
        this.chkTodos.Name = "chkTodos";
        this.chkTodos.Size = new System.Drawing.Size(119, 19);
        this.chkTodos.TabIndex = 3;
        this.chkTodos.Text = "Selecionar todos";
        this.chkTodos.UseVisualStyleBackColor = true;
        this.chkTodos.CheckedChanged += new EventHandler(this.chkTodos_CheckedChanged);
        // 
        // dgvCentros
        // 
        this.dgvCentros.AllowUserToAddRows = false;
        this.dgvCentros.AllowUserToDeleteRows = false;
        this.dgvCentros.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.dgvCentros.Location = new System.Drawing.Point(16, 84);
        this.dgvCentros.Name = "dgvCentros";
        this.dgvCentros.Size = new System.Drawing.Size(928, 206);
        this.dgvCentros.TabIndex = 4;
        this.dgvCentros.ReadOnly = false;
        this.dgvCentros.RowHeadersVisible = false;
        this.dgvCentros.AllowUserToResizeRows = false;
        this.dgvCentros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvCentros.MultiSelect = true;
        // 
        // grpCsv
        // 
        this.grpCsv.Controls.Add(this.lblVencimento);
        this.grpCsv.Controls.Add(this.mtbVencimento);
        this.grpCsv.Controls.Add(this.lblVerba);
        this.grpCsv.Controls.Add(this.cmbVerba);
        this.grpCsv.Controls.Add(this.lblDoc);
        this.grpCsv.Controls.Add(this.txtDoc);
        this.grpCsv.Controls.Add(this.lblObs);
        this.grpCsv.Controls.Add(this.txtObs);
        this.grpCsv.Controls.Add(this.lblCredorCodigo);
        this.grpCsv.Controls.Add(this.txtCredorCodigo);
        this.grpCsv.Controls.Add(this.lblCredorNome);
        this.grpCsv.Controls.Add(this.txtCredorNome);
        this.grpCsv.Controls.Add(this.btnGerarCsv);
        this.grpCsv.Controls.Add(this.btnSalvarCsv);
        this.grpCsv.Controls.Add(this.lblTotal);
        this.grpCsv.Controls.Add(this.txtResultado);
        this.grpCsv.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpCsv.Location = new System.Drawing.Point(12, 432);
        this.grpCsv.Name = "grpCsv";
        this.grpCsv.Size = new System.Drawing.Size(960, 270);
        this.grpCsv.TabIndex = 2;
        this.grpCsv.TabStop = false;
        this.grpCsv.Text = "3. Geração do arquivo CSV (layout Folha de Pagamento)";
        // 
        // lblVencimento
        // 
        this.lblVencimento.AutoSize = true;
        this.lblVencimento.Location = new System.Drawing.Point(16, 24);
        this.lblVencimento.Name = "lblVencimento";
        this.lblVencimento.Size = new System.Drawing.Size(95, 15);
        this.lblVencimento.TabIndex = 0;
        this.lblVencimento.Text = "Vencimento (F):";
        // 
        // mtbVencimento
        // 
        this.mtbVencimento.Location = new System.Drawing.Point(117, 20);
        this.mtbVencimento.Mask = "00/00/0000";
        this.mtbVencimento.Name = "mtbVencimento";
        this.mtbVencimento.Size = new System.Drawing.Size(90, 23);
        this.mtbVencimento.TabIndex = 1;
        this.mtbVencimento.Text = "31012026";
        // 
        // lblVerba
        // 
        this.lblVerba.AutoSize = true;
        this.lblVerba.Location = new System.Drawing.Point(230, 24);
        this.lblVerba.Name = "lblVerba";
        this.lblVerba.Size = new System.Drawing.Size(61, 15);
        this.lblVerba.TabIndex = 2;
        this.lblVerba.Text = "Verba (A):";
        // 
        // cmbVerba
        // 
        this.cmbVerba.DisplayMember = "Display";
        this.cmbVerba.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbVerba.Location = new System.Drawing.Point(296, 20);
        this.cmbVerba.Name = "cmbVerba";
        this.cmbVerba.Size = new System.Drawing.Size(260, 23);
        this.cmbVerba.TabIndex = 3;
        this.cmbVerba.SelectedIndexChanged += new EventHandler(this.cmbVerba_SelectedIndexChanged);
        // 
        // lblDoc
        // 
        this.lblDoc.AutoSize = true;
        this.lblDoc.Location = new System.Drawing.Point(560, 24);
        this.lblDoc.Name = "lblDoc";
        this.lblDoc.Size = new System.Drawing.Size(88, 15);
        this.lblDoc.TabIndex = 4;
        this.lblDoc.Text = "Documento (K):";
        // 
        // txtDoc
        // 
        this.txtDoc.Location = new System.Drawing.Point(654, 20);
        this.txtDoc.Name = "txtDoc";
        this.txtDoc.Size = new System.Drawing.Size(100, 23);
        this.txtDoc.TabIndex = 5;
        this.txtDoc.Text = "122025";
        // 
        // lblObs
        // 
        this.lblObs.AutoSize = true;
        this.lblObs.Location = new System.Drawing.Point(16, 126);
        this.lblObs.Name = "lblObs";
        this.lblObs.Size = new System.Drawing.Size(82, 15);
        this.lblObs.TabIndex = 20;
        this.lblObs.Text = "Observação (L):";
        // 
        // txtObs
        // 
        this.txtObs.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
        this.txtObs.Location = new System.Drawing.Point(110, 122);
        this.txtObs.Name = "txtObs";
        this.txtObs.Size = new System.Drawing.Size(830, 23);
        this.txtObs.TabIndex = 21;
        // 
        // lblCredorCodigo
        // 
        this.lblCredorCodigo.AutoSize = true;
        this.lblCredorCodigo.Location = new System.Drawing.Point(16, 58);
        this.lblCredorCodigo.Name = "lblCredorCodigo";
        this.lblCredorCodigo.Size = new System.Drawing.Size(106, 15);
        this.lblCredorCodigo.TabIndex = 10;
        this.lblCredorCodigo.Text = "Código credor (C):";
        // 
        // txtCredorCodigo
        // 
        this.txtCredorCodigo.Location = new System.Drawing.Point(126, 54);
        this.txtCredorCodigo.Name = "txtCredorCodigo";
        this.txtCredorCodigo.Size = new System.Drawing.Size(120, 23);
        this.txtCredorCodigo.TabIndex = 11;
        this.txtCredorCodigo.Text = "CRED01";
        this.txtCredorCodigo.TextChanged += new EventHandler(this.txtCredor_TextChanged);
        // 
        // lblCredorNome
        // 
        this.lblCredorNome.AutoSize = true;
        this.lblCredorNome.Location = new System.Drawing.Point(270, 58);
        this.lblCredorNome.Name = "lblCredorNome";
        this.lblCredorNome.Size = new System.Drawing.Size(101, 15);
        this.lblCredorNome.TabIndex = 12;
        this.lblCredorNome.Text = "Nome credor (D):";
        // 
        // txtCredorNome
        // 
        this.txtCredorNome.Location = new System.Drawing.Point(375, 54);
        this.txtCredorNome.Name = "txtCredorNome";
        this.txtCredorNome.Size = new System.Drawing.Size(569, 23);
        this.txtCredorNome.TabIndex = 13;
        this.txtCredorNome.TextChanged += new EventHandler(this.txtCredor_TextChanged);
        // 
        // btnGerarCsv
        // 
        this.btnGerarCsv.Enabled = false;
        this.btnGerarCsv.Location = new System.Drawing.Point(16, 92);
        this.btnGerarCsv.Name = "btnGerarCsv";
        this.btnGerarCsv.Size = new System.Drawing.Size(160, 28);
        this.btnGerarCsv.TabIndex = 6;
        this.btnGerarCsv.Text = "Gerar CSV";
        this.btnGerarCsv.UseVisualStyleBackColor = true;
        this.btnGerarCsv.Click += new EventHandler(this.btnGerarCsv_Click);
        // 
        // btnSalvarCsv
        // 
        this.btnSalvarCsv.Enabled = false;
        this.btnSalvarCsv.Location = new System.Drawing.Point(182, 92);
        this.btnSalvarCsv.Name = "btnSalvarCsv";
        this.btnSalvarCsv.Size = new System.Drawing.Size(160, 28);
        this.btnSalvarCsv.TabIndex = 7;
        this.btnSalvarCsv.Text = "Salvar arquivo...";
        this.btnSalvarCsv.UseVisualStyleBackColor = true;
        this.btnSalvarCsv.Click += new EventHandler(this.btnSalvarCsv_Click);
        // 
        // lblTotal
        // 
        this.lblTotal.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
        this.lblTotal.Location = new System.Drawing.Point(640, 96);
        this.lblTotal.Name = "lblTotal";
        this.lblTotal.Size = new System.Drawing.Size(304, 20);
        this.lblTotal.TabIndex = 8;
        this.lblTotal.Text = "Total: R$ 0,00";
        this.lblTotal.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtResultado
        // 
        this.txtResultado.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.txtResultado.Location = new System.Drawing.Point(16, 154);
        this.txtResultado.Multiline = true;
        this.txtResultado.Name = "txtResultado";
        this.txtResultado.ReadOnly = true;
        this.txtResultado.ScrollBars = ScrollBars.Both;
        this.txtResultado.Size = new System.Drawing.Size(928, 106);
        this.txtResultado.TabIndex = 9;
        this.txtResultado.Font = new System.Drawing.Font("Consolas", 9F);
        // 
        // grpFolha
        // 
        this.grpFolha.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpFolha.Controls.Add(this.lblFolhaTipo);
        this.grpFolha.Controls.Add(this.cmbFolhaTipo);
        this.grpFolha.Controls.Add(this.lblFolhaModo);
        this.grpFolha.Controls.Add(this.cmbFolhaModo);
        this.grpFolha.Controls.Add(this.lblFolhaCentro);
        this.grpFolha.Controls.Add(this.cmbFolhaCentro);
        this.grpFolha.Controls.Add(this.btnGerarFolha);
        this.grpFolha.Controls.Add(this.btnSalvarFolha);
        this.grpFolha.Controls.Add(this.lblTotalFolha);
        this.grpFolha.Controls.Add(this.dgvFolha);
        this.grpFolha.Controls.Add(this.txtResultadoFolha);
        this.grpFolha.Dock = System.Windows.Forms.DockStyle.Fill;
        this.grpFolha.Location = new System.Drawing.Point(3, 3);
        this.grpFolha.Name = "grpFolha";
        this.grpFolha.Size = new System.Drawing.Size(954, 302);
        this.grpFolha.TabIndex = 4;
        this.grpFolha.TabStop = false;
        this.grpFolha.Text = "Folha / Férias / Rescisões (por pessoa ou por centro)";
        // 
        // lblFolhaTipo
        // 
        this.lblFolhaTipo.AutoSize = true;
        this.lblFolhaTipo.Location = new System.Drawing.Point(16, 26);
        this.lblFolhaTipo.Name = "lblFolhaTipo";
        this.lblFolhaTipo.Size = new System.Drawing.Size(35, 15);
        this.lblFolhaTipo.TabIndex = 0;
        this.lblFolhaTipo.Text = "Tipo:";
        // 
        // cmbFolhaTipo
        // 
        this.cmbFolhaTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbFolhaTipo.Location = new System.Drawing.Point(52, 22);
        this.cmbFolhaTipo.Name = "cmbFolhaTipo";
        this.cmbFolhaTipo.Size = new System.Drawing.Size(180, 23);
        this.cmbFolhaTipo.TabIndex = 1;
        // 
        // lblFolhaModo
        // 
        this.lblFolhaModo.AutoSize = true;
        this.lblFolhaModo.Location = new System.Drawing.Point(248, 26);
        this.lblFolhaModo.Name = "lblFolhaModo";
        this.lblFolhaModo.Size = new System.Drawing.Size(43, 15);
        this.lblFolhaModo.TabIndex = 2;
        this.lblFolhaModo.Text = "Modo:";
        // 
        // cmbFolhaModo
        // 
        this.cmbFolhaModo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbFolhaModo.Location = new System.Drawing.Point(294, 22);
        this.cmbFolhaModo.Name = "cmbFolhaModo";
        this.cmbFolhaModo.Size = new System.Drawing.Size(160, 23);
        this.cmbFolhaModo.TabIndex = 3;
        // 
        // lblFolhaCentro
        // 
        this.lblFolhaCentro.AutoSize = true;
        this.lblFolhaCentro.Location = new System.Drawing.Point(470, 26);
        this.lblFolhaCentro.Name = "lblFolhaCentro";
        this.lblFolhaCentro.Size = new System.Drawing.Size(46, 15);
        this.lblFolhaCentro.TabIndex = 30;
        this.lblFolhaCentro.Text = "Centro:";
        // 
        // cmbFolhaCentro
        // 
        this.cmbFolhaCentro.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbFolhaCentro.Location = new System.Drawing.Point(518, 22);
        this.cmbFolhaCentro.Name = "cmbFolhaCentro";
        this.cmbFolhaCentro.Size = new System.Drawing.Size(150, 23);
        this.cmbFolhaCentro.TabIndex = 31;
        this.cmbFolhaCentro.SelectedIndexChanged += new EventHandler(this.cmbFolhaCentro_SelectedIndexChanged);
        // 
        // btnGerarFolha
        // 
        this.btnGerarFolha.Enabled = false;
        this.btnGerarFolha.Location = new System.Drawing.Point(680, 20);
        this.btnGerarFolha.Name = "btnGerarFolha";
        this.btnGerarFolha.Size = new System.Drawing.Size(140, 28);
        this.btnGerarFolha.TabIndex = 4;
        this.btnGerarFolha.Text = "Gerar CSV";
        this.btnGerarFolha.UseVisualStyleBackColor = true;
        this.btnGerarFolha.Click += new EventHandler(this.btnGerarFolha_Click);
        // 
        // btnSalvarFolha
        // 
        this.btnSalvarFolha.Enabled = false;
        this.btnSalvarFolha.Location = new System.Drawing.Point(824, 20);
        this.btnSalvarFolha.Name = "btnSalvarFolha";
        this.btnSalvarFolha.Size = new System.Drawing.Size(120, 28);
        this.btnSalvarFolha.TabIndex = 5;
        this.btnSalvarFolha.Text = "Salvar...";
        this.btnSalvarFolha.UseVisualStyleBackColor = true;
        this.btnSalvarFolha.Click += new EventHandler(this.btnSalvarFolha_Click);
        // 
        // lblTotalFolha
        // 
        this.lblTotalFolha.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
        this.lblTotalFolha.Location = new System.Drawing.Point(640, 56);
        this.lblTotalFolha.Name = "lblTotalFolha";
        this.lblTotalFolha.Size = new System.Drawing.Size(304, 20);
        this.lblTotalFolha.TabIndex = 6;
        this.lblTotalFolha.Text = "Total: R$ 0,00";
        this.lblTotalFolha.TextAlign = ContentAlignment.MiddleRight;
        // 
        // dgvFolha
        // 
        this.dgvFolha.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.dgvFolha.AllowUserToAddRows = false;
        this.dgvFolha.AllowUserToDeleteRows = false;
        this.dgvFolha.Location = new System.Drawing.Point(16, 84);
        this.dgvFolha.Name = "dgvFolha";
        this.dgvFolha.RowHeadersVisible = false;
        this.dgvFolha.Size = new System.Drawing.Size(928, 124);
        this.dgvFolha.TabIndex = 7;
        // 
        // txtResultadoFolha
        // 
        this.txtResultadoFolha.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.txtResultadoFolha.Location = new System.Drawing.Point(16, 214);
        this.txtResultadoFolha.Multiline = true;
        this.txtResultadoFolha.Name = "txtResultadoFolha";
        this.txtResultadoFolha.ReadOnly = true;
        this.txtResultadoFolha.ScrollBars = ScrollBars.Both;
        this.txtResultadoFolha.Size = new System.Drawing.Size(928, 56);
        this.txtResultadoFolha.TabIndex = 8;
        this.txtResultadoFolha.Font = new System.Drawing.Font("Consolas", 9F);
        // 
        // grpGuias
        // 
        this.grpGuias.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpGuias.Controls.Add(this.lblGuiasTipo);
        this.grpGuias.Controls.Add(this.cmbGuiasTipo);
        this.grpGuias.Controls.Add(this.lblGuiasModo);
        this.grpGuias.Controls.Add(this.cmbGuiasModo);
        this.grpGuias.Controls.Add(this.btnGerarGuias);
        this.grpGuias.Controls.Add(this.btnSalvarGuias);
        this.grpGuias.Controls.Add(this.lblTotalGuias);
        this.grpGuias.Controls.Add(this.dgvGuias);
        this.grpGuias.Controls.Add(this.txtResultadoGuias);
        this.grpGuias.Dock = System.Windows.Forms.DockStyle.Fill;
        this.grpGuias.Location = new System.Drawing.Point(3, 3);
        this.grpGuias.Name = "grpGuias";
        this.grpGuias.Size = new System.Drawing.Size(954, 302);
        this.grpGuias.TabIndex = 5;
        this.grpGuias.TabStop = false;
        this.grpGuias.Text = "Guias (INSS / IRRF / FGTS / eCONSIGNADO / GRRF)";
        // 
        // lblGuiasTipo
        // 
        this.lblGuiasTipo.AutoSize = true;
        this.lblGuiasTipo.Location = new System.Drawing.Point(16, 26);
        this.lblGuiasTipo.Name = "lblGuiasTipo";
        this.lblGuiasTipo.Size = new System.Drawing.Size(35, 15);
        this.lblGuiasTipo.TabIndex = 0;
        this.lblGuiasTipo.Text = "Tipo:";
        // 
        // cmbGuiasTipo
        // 
        this.cmbGuiasTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbGuiasTipo.Location = new System.Drawing.Point(52, 22);
        this.cmbGuiasTipo.Name = "cmbGuiasTipo";
        this.cmbGuiasTipo.Size = new System.Drawing.Size(150, 23);
        this.cmbGuiasTipo.TabIndex = 1;
        // 
        // lblGuiasModo
        // 
        this.lblGuiasModo.AutoSize = true;
        this.lblGuiasModo.Location = new System.Drawing.Point(216, 26);
        this.lblGuiasModo.Name = "lblGuiasModo";
        this.lblGuiasModo.Size = new System.Drawing.Size(43, 15);
        this.lblGuiasModo.TabIndex = 2;
        this.lblGuiasModo.Text = "Modo:";
        // 
        // cmbGuiasModo
        // 
        this.cmbGuiasModo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbGuiasModo.Location = new System.Drawing.Point(262, 22);
        this.cmbGuiasModo.Name = "cmbGuiasModo";
        this.cmbGuiasModo.Size = new System.Drawing.Size(180, 23);
        this.cmbGuiasModo.TabIndex = 3;
        // 
        // btnGerarGuias
        // 
        this.btnGerarGuias.Enabled = false;
        this.btnGerarGuias.Location = new System.Drawing.Point(456, 20);
        this.btnGerarGuias.Name = "btnGerarGuias";
        this.btnGerarGuias.Size = new System.Drawing.Size(140, 28);
        this.btnGerarGuias.TabIndex = 4;
        this.btnGerarGuias.Text = "Gerar CSV Guias";
        this.btnGerarGuias.UseVisualStyleBackColor = true;
        this.btnGerarGuias.Click += new EventHandler(this.btnGerarGuias_Click);
        // 
        // btnSalvarGuias
        // 
        this.btnSalvarGuias.Enabled = false;
        this.btnSalvarGuias.Location = new System.Drawing.Point(602, 20);
        this.btnSalvarGuias.Name = "btnSalvarGuias";
        this.btnSalvarGuias.Size = new System.Drawing.Size(140, 28);
        this.btnSalvarGuias.TabIndex = 5;
        this.btnSalvarGuias.Text = "Salvar arquivo...";
        this.btnSalvarGuias.UseVisualStyleBackColor = true;
        this.btnSalvarGuias.Click += new EventHandler(this.btnSalvarGuias_Click);
        // 
        // lblTotalGuias
        // 
        this.lblTotalGuias.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
        this.lblTotalGuias.Location = new System.Drawing.Point(640, 24);
        this.lblTotalGuias.Name = "lblTotalGuias";
        this.lblTotalGuias.Size = new System.Drawing.Size(304, 20);
        this.lblTotalGuias.TabIndex = 3;
        this.lblTotalGuias.Text = "Total: R$ 0,00";
        this.lblTotalGuias.TextAlign = ContentAlignment.MiddleRight;
        // 
        // dgvGuias
        // 
        this.dgvGuias.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.dgvGuias.AllowUserToAddRows = false;
        this.dgvGuias.AllowUserToDeleteRows = false;
        this.dgvGuias.Location = new System.Drawing.Point(16, 56);
        this.dgvGuias.Name = "dgvGuias";
        this.dgvGuias.RowHeadersVisible = false;
        this.dgvGuias.Size = new System.Drawing.Size(928, 96);
        this.dgvGuias.TabIndex = 4;
        // 
        // txtResultadoGuias
        // 
        this.txtResultadoGuias.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.txtResultadoGuias.Location = new System.Drawing.Point(16, 158);
        this.txtResultadoGuias.Multiline = true;
        this.txtResultadoGuias.Name = "txtResultadoGuias";
        this.txtResultadoGuias.ReadOnly = true;
        this.txtResultadoGuias.ScrollBars = ScrollBars.Both;
        this.txtResultadoGuias.Size = new System.Drawing.Size(928, 80);
        this.txtResultadoGuias.TabIndex = 5;
        this.txtResultadoGuias.Font = new System.Drawing.Font("Consolas", 9F);
        // 
        // grpGrf
        // 
        this.grpGrf.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpGrf.Controls.Add(this.lblGrfPeriodo);
        this.grpGrf.Controls.Add(this.mtbGrfIni);
        this.grpGrf.Controls.Add(this.lblGrfAte);
        this.grpGrf.Controls.Add(this.mtbGrfFim);
        this.grpGrf.Controls.Add(this.lblGrfLote);
        this.grpGrf.Controls.Add(this.mtbGrfLote);
        this.grpGrf.Controls.Add(this.lblGrfCentro);
        this.grpGrf.Controls.Add(this.txtGrfCentro);
        this.grpGrf.Controls.Add(this.btnCarregarGrf);
        this.grpGrf.Controls.Add(this.btnGerarGrf);
        this.grpGrf.Controls.Add(this.btnSalvarGrf);
        this.grpGrf.Controls.Add(this.lblTotalGrf);
        this.grpGrf.Controls.Add(this.dgvGrf);
        this.grpGrf.Controls.Add(this.txtResultadoGrf);
        this.grpGrf.Dock = System.Windows.Forms.DockStyle.Fill;
        this.grpGrf.Location = new System.Drawing.Point(3, 3);
        this.grpGrf.Name = "grpGrf";
        this.grpGrf.Size = new System.Drawing.Size(954, 302);
        this.grpGrf.TabIndex = 3;
        this.grpGrf.TabStop = false;
        this.grpGrf.Text = "Lote GRRF (rescisões)";
        // 
        // lblGrfPeriodo
        // 
        this.lblGrfPeriodo.AutoSize = true;
        this.lblGrfPeriodo.Location = new System.Drawing.Point(16, 24);
        this.lblGrfPeriodo.Name = "lblGrfPeriodo";
        this.lblGrfPeriodo.Size = new System.Drawing.Size(52, 15);
        this.lblGrfPeriodo.TabIndex = 0;
        this.lblGrfPeriodo.Text = "Venc. de:";
        // 
        // mtbGrfIni
        // 
        this.mtbGrfIni.Location = new System.Drawing.Point(72, 20);
        this.mtbGrfIni.Mask = "00/00/0000";
        this.mtbGrfIni.Name = "mtbGrfIni";
        this.mtbGrfIni.Size = new System.Drawing.Size(80, 23);
        this.mtbGrfIni.TabIndex = 1;
        this.mtbGrfIni.Text = "01082025";
        // 
        // lblGrfAte
        // 
        this.lblGrfAte.AutoSize = true;
        this.lblGrfAte.Location = new System.Drawing.Point(158, 24);
        this.lblGrfAte.Name = "lblGrfAte";
        this.lblGrfAte.Size = new System.Drawing.Size(29, 15);
        this.lblGrfAte.TabIndex = 2;
        this.lblGrfAte.Text = "até:";
        // 
        // mtbGrfFim
        // 
        this.mtbGrfFim.Location = new System.Drawing.Point(190, 20);
        this.mtbGrfFim.Mask = "00/00/0000";
        this.mtbGrfFim.Name = "mtbGrfFim";
        this.mtbGrfFim.Size = new System.Drawing.Size(80, 23);
        this.mtbGrfFim.TabIndex = 3;
        this.mtbGrfFim.Text = "31122025";
        // 
        // lblGrfLote
        // 
        this.lblGrfLote.AutoSize = true;
        this.lblGrfLote.Location = new System.Drawing.Point(290, 24);
        this.lblGrfLote.Name = "lblGrfLote";
        this.lblGrfLote.Size = new System.Drawing.Size(73, 15);
        this.lblGrfLote.TabIndex = 4;
        this.lblGrfLote.Text = "Data do lote:";
        // 
        // mtbGrfLote
        // 
        this.mtbGrfLote.Location = new System.Drawing.Point(368, 20);
        this.mtbGrfLote.Mask = "00/00/0000";
        this.mtbGrfLote.Name = "mtbGrfLote";
        this.mtbGrfLote.Size = new System.Drawing.Size(80, 23);
        this.mtbGrfLote.TabIndex = 5;
        this.mtbGrfLote.Text = "12082025";
        // 
        // lblGrfCentro
        // 
        this.lblGrfCentro.AutoSize = true;
        this.lblGrfCentro.Location = new System.Drawing.Point(470, 24);
        this.lblGrfCentro.Name = "lblGrfCentro";
        this.lblGrfCentro.Size = new System.Drawing.Size(72, 15);
        this.lblGrfCentro.TabIndex = 6;
        this.lblGrfCentro.Text = "Centro (B):";
        // 
        // txtGrfCentro
        // 
        this.txtGrfCentro.Location = new System.Drawing.Point(546, 20);
        this.txtGrfCentro.Name = "txtGrfCentro";
        this.txtGrfCentro.Size = new System.Drawing.Size(90, 23);
        this.txtGrfCentro.TabIndex = 7;
        this.txtGrfCentro.Text = "305";
        // 
        // btnCarregarGrf
        // 
        this.btnCarregarGrf.Enabled = false;
        this.btnCarregarGrf.Location = new System.Drawing.Point(652, 18);
        this.btnCarregarGrf.Name = "btnCarregarGrf";
        this.btnCarregarGrf.Size = new System.Drawing.Size(140, 28);
        this.btnCarregarGrf.TabIndex = 8;
        this.btnCarregarGrf.Text = "Carregar GRRF";
        this.btnCarregarGrf.UseVisualStyleBackColor = true;
        this.btnCarregarGrf.Click += new EventHandler(this.btnCarregarGrf_Click);
        // 
        // btnGerarGrf
        // 
        this.btnGerarGrf.Enabled = false;
        this.btnGerarGrf.Location = new System.Drawing.Point(16, 52);
        this.btnGerarGrf.Name = "btnGerarGrf";
        this.btnGerarGrf.Size = new System.Drawing.Size(140, 28);
        this.btnGerarGrf.TabIndex = 9;
        this.btnGerarGrf.Text = "Gerar CSV GRRF";
        this.btnGerarGrf.UseVisualStyleBackColor = true;
        this.btnGerarGrf.Click += new EventHandler(this.btnGerarGrf_Click);
        // 
        // btnSalvarGrf
        // 
        this.btnSalvarGrf.Enabled = false;
        this.btnSalvarGrf.Location = new System.Drawing.Point(162, 52);
        this.btnSalvarGrf.Name = "btnSalvarGrf";
        this.btnSalvarGrf.Size = new System.Drawing.Size(140, 28);
        this.btnSalvarGrf.TabIndex = 10;
        this.btnSalvarGrf.Text = "Salvar arquivo...";
        this.btnSalvarGrf.UseVisualStyleBackColor = true;
        this.btnSalvarGrf.Click += new EventHandler(this.btnSalvarGrf_Click);
        // 
        // lblTotalGrf
        // 
        this.lblTotalGrf.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
        this.lblTotalGrf.Location = new System.Drawing.Point(640, 56);
        this.lblTotalGrf.Name = "lblTotalGrf";
        this.lblTotalGrf.Size = new System.Drawing.Size(304, 20);
        this.lblTotalGrf.TabIndex = 11;
        this.lblTotalGrf.Text = "Total: R$ 0,00";
        this.lblTotalGrf.TextAlign = ContentAlignment.MiddleRight;
        // 
        // dgvGrf
        // 
        this.dgvGrf.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.dgvGrf.AllowUserToAddRows = false;
        this.dgvGrf.AllowUserToDeleteRows = false;
        this.dgvGrf.Location = new System.Drawing.Point(16, 88);
        this.dgvGrf.Name = "dgvGrf";
        this.dgvGrf.RowHeadersVisible = false;
        this.dgvGrf.Size = new System.Drawing.Size(928, 120);
        this.dgvGrf.TabIndex = 12;
        // 
        // txtResultadoGrf
        // 
        this.txtResultadoGrf.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.txtResultadoGrf.Location = new System.Drawing.Point(16, 214);
        this.txtResultadoGrf.Multiline = true;
        this.txtResultadoGrf.Name = "txtResultadoGrf";
        this.txtResultadoGrf.ReadOnly = true;
        this.txtResultadoGrf.ScrollBars = ScrollBars.Both;
        this.txtResultadoGrf.Size = new System.Drawing.Size(928, 56);
        this.txtResultadoGrf.TabIndex = 13;
        this.txtResultadoGrf.Font = new System.Drawing.Font("Consolas", 9F);
        // 
        // tabExport
        // 
        this.tabExport.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.tabExport.Controls.Add(this.tabGrf);
        this.tabExport.Controls.Add(this.tabFolha);
        this.tabExport.Controls.Add(this.tabGuias);
        this.tabExport.Location = new System.Drawing.Point(12, 710);
        this.tabExport.Name = "tabExport";
        this.tabExport.SelectedIndex = 0;
        this.tabExport.Size = new System.Drawing.Size(960, 330);
        this.tabExport.TabIndex = 40;
        // 
        // tabGrf
        // 
        this.tabGrf.Controls.Add(this.grpGrf);
        this.tabGrf.Location = new System.Drawing.Point(4, 24);
        this.tabGrf.Name = "tabGrf";
        this.tabGrf.Padding = new System.Windows.Forms.Padding(3);
        this.tabGrf.Size = new System.Drawing.Size(952, 302);
        this.tabGrf.TabIndex = 0;
        this.tabGrf.Text = "Lote GRRF";
        this.tabGrf.UseVisualStyleBackColor = true;
        // 
        // tabFolha
        // 
        this.tabFolha.Controls.Add(this.grpFolha);
        this.tabFolha.Location = new System.Drawing.Point(4, 24);
        this.tabFolha.Name = "tabFolha";
        this.tabFolha.Padding = new System.Windows.Forms.Padding(3);
        this.tabFolha.Size = new System.Drawing.Size(952, 302);
        this.tabFolha.TabIndex = 1;
        this.tabFolha.Text = "Folha / Férias / Rescisões";
        this.tabFolha.UseVisualStyleBackColor = true;
        // 
        // tabGuias
        // 
        this.tabGuias.Controls.Add(this.grpGuias);
        this.tabGuias.Location = new System.Drawing.Point(4, 24);
        this.tabGuias.Name = "tabGuias";
        this.tabGuias.Padding = new System.Windows.Forms.Padding(3);
        this.tabGuias.Size = new System.Drawing.Size(952, 302);
        this.tabGuias.TabIndex = 2;
        this.tabGuias.Text = "Guias";
        this.tabGuias.UseVisualStyleBackColor = true;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(984, 1050);
        this.Controls.Add(this.btnSair);
        this.Controls.Add(this.tabExport);
        this.Controls.Add(this.grpCsv);
        this.Controls.Add(this.grpDados);
        this.Controls.Add(this.grpBanco);
        this.Name = "Form1";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new System.Drawing.Size(984, 900);
        this.Text = "Plus Informática - Folha de Pagamento (Plus Contabilidade)";
        // 
        // btnSair
        // 
        this.btnSair.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
        this.btnSair.Location = new System.Drawing.Point(880, 12);
        this.btnSair.Name = "btnSair";
        this.btnSair.Size = new System.Drawing.Size(90, 30);
        this.btnSair.TabIndex = 50;
        this.btnSair.Text = "Sair";
        this.btnSair.UseVisualStyleBackColor = true;
        this.btnSair.Click += new EventHandler(this.btnSair_Click);
        this.grpBanco.ResumeLayout(false);
        this.grpDados.ResumeLayout(false);
        this.grpDados.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCentros)).EndInit();
        this.grpCsv.ResumeLayout(false);
        this.grpCsv.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvGrf)).EndInit();
        this.grpGrf.ResumeLayout(false);
        this.grpGrf.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvFolha)).EndInit();
        this.grpFolha.ResumeLayout(false);
        this.grpFolha.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvGuias)).EndInit();
        this.grpGuias.ResumeLayout(false);
        this.grpGuias.PerformLayout();
        this.tabGrf.ResumeLayout(false);
        this.tabFolha.ResumeLayout(false);
        this.tabGuias.ResumeLayout(false);
        this.tabExport.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
