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
        // lblCompetencia
        // 
        this.lblCompetencia.AutoSize = true;
        this.lblCompetencia.Location = new System.Drawing.Point(16, 24);
        this.lblCompetencia.Name = "lblCompetencia";
        this.lblCompetencia.Size = new System.Drawing.Size(76, 15);
        this.lblCompetencia.TabIndex = 0;
        this.lblCompetencia.Text = "Competência:";
        // 
        // cmbCompetencia
        // 
        this.cmbCompetencia.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbCompetencia.Enabled = false;
        this.cmbCompetencia.Location = new System.Drawing.Point(98, 20);
        this.cmbCompetencia.Name = "cmbCompetencia";
        this.cmbCompetencia.Size = new System.Drawing.Size(180, 23);
        this.cmbCompetencia.TabIndex = 1;
        this.cmbCompetencia.SelectedIndexChanged += new EventHandler(this.cmbCompetencia_SelectedIndexChanged);
        // 
        // btnCarregarCentros
        // 
        this.btnCarregarCentros.Enabled = false;
        this.btnCarregarCentros.Location = new System.Drawing.Point(284, 19);
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
        this.btnRelatorioMensal.Location = new System.Drawing.Point(470, 19);
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
        this.chkTodos.Location = new System.Drawing.Point(660, 23);
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
        this.dgvCentros.Location = new System.Drawing.Point(16, 52);
        this.dgvCentros.Name = "dgvCentros";
        this.dgvCentros.Size = new System.Drawing.Size(928, 238);
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
        this.grpCsv.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
        this.grpCsv.Location = new System.Drawing.Point(12, 432);
        this.grpCsv.Name = "grpCsv";
        this.grpCsv.Size = new System.Drawing.Size(960, 270);
        this.grpCsv.TabIndex = 2;
        this.grpCsv.TabStop = false;
        this.grpCsv.Text = "3. Geração do arquivo CSV (layout Sienge)";
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
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(984, 714);
        this.Controls.Add(this.grpCsv);
        this.Controls.Add(this.grpDados);
        this.Controls.Add(this.grpBanco);
        this.Name = "Form1";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new System.Drawing.Size(984, 714);
        this.Text = "Importação Folha de Pagamento - Sienge (ENGEMAT)";
        this.grpBanco.ResumeLayout(false);
        this.grpDados.ResumeLayout(false);
        this.grpDados.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCentros)).EndInit();
        this.grpCsv.ResumeLayout(false);
        this.grpCsv.PerformLayout();
        this.ResumeLayout(false);
    }
}
