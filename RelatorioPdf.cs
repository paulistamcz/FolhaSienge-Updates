using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;

namespace FolhaSienge;

/// <summary>
/// Gera o Relatório Mensal em PDF com todas as verbas da tabela_financeira
/// e os valores do mês. O texto é quebrado automaticamente dentro das células.
/// </summary>
public static class RelatorioPdf
{
    static RelatorioPdf()
    {
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }

    private static readonly float[] Cols = { 45, 210, 80, 210, 45, 110, 82 };
    private static readonly string[] Headers = { "Código", "Descrição da Verba", "Plano Fin.", "Credor", "Doc.", "Forma de Pagamento", "Valor (R$)" };
    private static readonly float[] ColsCentro = { 60, 400, 110, 212 };
    private static readonly string[] HeadersCentro = { "Centro", "Centro de Custo", "Empregados", "Total (R$)" };
    private static readonly float[] ColsDetalhe = { 50, 70, 500, 162 };
    private static readonly string[] HeadersDetalhe = { "Centro", "Matrícula", "Funcionário", "Líquido (R$)" };
    private const float X0 = 30;
    private const float AlturaMinima = 22;
    private const float Espaco = 3;

    public static void Gerar(string caminho, string competencia,
        List<(VerbaFinanceira Verba, decimal Valor)> itens,
        List<(int Codigo, string Nome, int Empregados, decimal Total)> centros,
        List<(int Centro, string NomeEmpregado, int Empregado, decimal Liquido)>? detalhe = null)
    {
        using var doc = new PdfDocument();
        PdfPage page = NovaPagina(doc);
        XGraphics gfx = XGraphics.FromPdfPage(page);

        var fonteTitulo = new XFont("Arial", 14, XFontStyleEx.Bold);
        var fonteSub = new XFont("Arial", 10, XFontStyleEx.Regular);
        var fonteCab = new XFont("Arial", 9, XFontStyleEx.Bold);
        var fonte = new XFont("Arial", 8, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 8, XFontStyleEx.Bold);

        float y = 30;
        gfx.DrawString("RELATÓRIO MENSAL DE FOLHA DE PAGAMENTO - PLUS CONTABIL",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 22;
        gfx.DrawString($"Competência: {competencia}    Empresa: Plus Contabilidade    Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
            fonteSub, XBrushes.DarkGray, new XPoint(X0, y));
        y += 18;

        // ===== 1. RESUMO POR CENTRO DE CUSTO =====
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, AlturaMinima);
        gfx.DrawString("1. RESUMO POR CENTRO DE CUSTO", fonteCab, XBrushes.Black, new XPoint(X0, y));
        y += 16;
        DrawCentroHeader(gfx, y, fonteCab);
        y += AlturaMinima;

        int totalEmp = 0;
        decimal totalCentros = 0m;
        for (int i = 0; i < centros.Count; i++)
        {
            var c = centros[i];
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, AlturaMinima);
            if (y == 30) { DrawCentroHeader(gfx, y, fonteCab); y += AlturaMinima; }
            DrawCentroRow(gfx, c, y, i % 2 == 0, fonte);
            totalEmp += c.Empregados;
            totalCentros += c.Total;
            y += AlturaMinima;
        }
        DrawCentroTotal(gfx, totalEmp, totalCentros, y, fonteBold);
        y += AlturaMinima + 16;

        // ===== 2. RESUMO POR VERBA =====
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, AlturaMinima);
        gfx.DrawString("2. RESUMO POR VERBA", fonteCab, XBrushes.Black, new XPoint(X0, y));
        y += 16;
        DrawHeader(gfx, y, fonteCab);
        y += AlturaMinima;

        decimal total = 0;
        int r = 0;
        while (r < itens.Count)
        {
            var (v, valor) = itens[r];
            float alt = CalcularAlturaLinha(v, valor, gfx, fonte);
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, alt);
            if (y == 30) { DrawHeader(gfx, y, fonteCab); y += AlturaMinima; }
            total += valor;
            DrawRow(gfx, v, valor, y, alt, r % 2 == 0, fonte, fonteBold);
            y += alt;
            r++;
        }
        DrawTotal(gfx, total, y, fonteBold);

        // ===== 3. DETALHAMENTO POR FUNCIONÁRIO (quando 1 centro selecionado) =====
        if (detalhe != null && detalhe.Count > 0)
        {
            y += AlturaMinima + 16;
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, AlturaMinima);
            gfx.DrawString("3. DETALHAMENTO POR FUNCIONÁRIO", fonteCab, XBrushes.Black, new XPoint(X0, y));
            y += 16;
            DrawDetalheHeader(gfx, y, fonteCab);
            y += AlturaMinima;

            decimal totalDetalhe = 0m;
            for (int i = 0; i < detalhe.Count; i++)
            {
                var d = detalhe[i];
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, AlturaMinima);
                if (y == 30) { DrawDetalheHeader(gfx, y, fonteCab); y += AlturaMinima; }
                DrawDetalheRow(gfx, d, y, i % 2 == 0, fonte);
                totalDetalhe += d.Liquido;
                y += AlturaMinima;
            }
            DrawDetalheTotal(gfx, detalhe.Count, totalDetalhe, y, fonteBold);
        }

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        doc.Save(caminho);
    }

    private static PdfPage NovaPagina(PdfDocument doc)
    {
        var page = doc.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        page.Orientation = PdfSharp.PageOrientation.Landscape;
        return page;
    }

    private static (XGraphics, PdfPage, float) GarantirEspaco(XGraphics gfx, PdfDocument doc, PdfPage page,
        float y, float alturaNecessaria)
    {
        if (y + alturaNecessaria <= page.Height.Point - 40) return (gfx, page, y);
        gfx.Dispose();
        var nova = NovaPagina(doc);
        return (XGraphics.FromPdfPage(nova), nova, 30);
    }

    private static void DrawCentroHeader(XGraphics gfx, float y, XFont fonteCab)
    {
        float x = X0;
        for (int i = 0; i < HeadersCentro.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, ColsCentro[i], AlturaMinima);
            gfx.DrawString(HeadersCentro[i], fonteCab, XBrushes.White,
                new XRect(x, y, ColsCentro[i], AlturaMinima), XStringFormats.Center);
            x += ColsCentro[i];
        }
    }

    private static void DrawCentroRow(XGraphics gfx, (int Codigo, string Nome, int Empregados, decimal Total) c,
        float y, bool zebra, XFont fonte)
    {
        string[] cells =
        {
            c.Codigo.ToString(),
            c.Nome,
            c.Empregados.ToString(),
            VerbaFinanceira.FormatarValor(c.Total),
        };
        float x = X0;
        for (int i = 0; i < ColsCentro.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.Silver, x, y, ColsCentro[i], AlturaMinima);
            gfx.DrawRectangle(zebra ? XBrushes.LightGray : XBrushes.White,
                x + 0.5, y + 0.5, ColsCentro[i] - 1, AlturaMinima - 1);
            var rect = new XRect(x + Espaco, y + Espaco, ColsCentro[i] - (Espaco * 2), AlturaMinima - (Espaco * 2));
            if (i == 3)
            {
                gfx.DrawString(cells[i], fonte, XBrushes.Black, rect, XStringFormats.CenterRight);
            }
            else
            {
                var formatter = new XTextFormatter(gfx)
                {
                    Alignment = i == 2 ? XParagraphAlignment.Center : XParagraphAlignment.Left
                };
                formatter.DrawString(cells[i], fonte, XBrushes.Black, rect);
            }
            x += ColsCentro[i];
        }
    }

    private static void DrawCentroTotal(XGraphics gfx, int emp, decimal total, float y, XFont fonteBold)
    {
        float x = X0;
        for (int i = 0; i < ColsCentro.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, ColsCentro[i], AlturaMinima);
            x += ColsCentro[i];
        }
        gfx.DrawString("TOTAL", fonteBold, XBrushes.White,
            new XRect(X0, y + 2, ColsCentro[0] + ColsCentro[1], AlturaMinima - 4), XStringFormats.Center);
        gfx.DrawString(emp.ToString(), fonteBold, XBrushes.White,
            new XRect(X0 + ColsCentro[0] + ColsCentro[1], y + 2, ColsCentro[2] - 6, AlturaMinima - 4), XStringFormats.Center);
        gfx.DrawString(VerbaFinanceira.FormatarValor(total), fonteBold, XBrushes.White,
            new XRect(X0 + ColsCentro[0] + ColsCentro[1] + ColsCentro[2], y + 2, ColsCentro[3] - 6, AlturaMinima - 4), XStringFormats.CenterRight);
    }

    private static void DrawHeader(XGraphics gfx, float y, XFont fonteCab)
    {
        float x = X0;
        for (int i = 0; i < Headers.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, Cols[i], AlturaMinima);
            gfx.DrawString(Headers[i], fonteCab, XBrushes.White,
                new XRect(x, y, Cols[i], AlturaMinima), XStringFormats.Center);
            x += Cols[i];
        }
    }

    private static void DrawRow(XGraphics gfx, VerbaFinanceira v, decimal valor, float y,
        float altura, bool zebra, XFont fonte, XFont fonteBold)
    {
        string[] cells =
        {
            v.Codigo.ToString(),
            v.Descricao,
            v.PlanoFinanceiro,
            v.Credor,
            v.Documento,
            v.FormaPagamento,
            VerbaFinanceira.FormatarValor(valor),
        };

        float x = X0;
        for (int i = 0; i < Cols.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.Silver, x, y, Cols[i], altura);
            gfx.DrawRectangle(zebra ? XBrushes.LightGray : XBrushes.White,
                x + 0.5, y + 0.5, Cols[i] - 1, altura - 1);

            var rect = new XRect(x + Espaco, y + Espaco, Cols[i] - (Espaco * 2), altura - (Espaco * 2));
            if (i == 6)
            {
                gfx.DrawString(cells[i], fonteBold, XBrushes.Black, rect, XStringFormats.CenterRight);
            }
            else
            {
                var formatter = new XTextFormatter(gfx)
                {
                    Alignment = i == 0 || i == 4 ? XParagraphAlignment.Center : XParagraphAlignment.Left
                };
                formatter.DrawString(cells[i], fonte, XBrushes.Black, rect);
            }
            x += Cols[i];
        }
    }

    private static float CalcularAlturaLinha(VerbaFinanceira v, decimal valor, XGraphics gfx, XFont fonte)
    {
        string[] textos = { v.Descricao, v.Credor, v.FormaPagamento };
        float[] larguras = { Cols[1], Cols[3], Cols[5] };
        float maxAlt = AlturaMinima;
        for (int i = 0; i < textos.Length; i++)
        {
            int nLinhas = ContarLinhasQuebradas(textos[i], fonte, larguras[i] - (Espaco * 2));
            float alt = nLinhas * (float)fonte.GetHeight();
            if (alt > maxAlt) maxAlt = alt;
        }
        return maxAlt + (Espaco * 2);
    }

    private static int ContarLinhasQuebradas(string texto, XFont fonte, float largura)
    {
        if (string.IsNullOrWhiteSpace(texto)) return 1;
        int linhas = 0;
        string atual = "";
        foreach (var palavra in texto.Trim().Split(' '))
        {
            var tentativa = atual.Length == 0 ? palavra : atual + " " + palavra;
            if (atual.Length > 0 && gfxMedir(tentativa, fonte) > largura)
            {
                linhas++;
                atual = palavra;
            }
            else
            {
                atual = tentativa;
            }
        }
        if (atual.Length > 0) linhas++;
        return Math.Max(1, linhas);
    }

    private static float gfxMedir(string texto, XFont fonte)
    {
        using var tmp = XGraphics.CreateMeasureContext(new XSize(10000, 10000), XGraphicsUnit.Point,
            XPageDirection.Downwards);
        return (float)tmp.MeasureString(texto, fonte).Width;
    }

    private static void DrawTotal(XGraphics gfx, decimal total, float y, XFont fonteBold)
    {
        float larguraTotal = Cols.Sum();
        float x = X0;
        for (int i = 0; i < Cols.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, Cols[i], AlturaMinima);
            x += Cols[i];
        }
        gfx.DrawString("TOTAL", fonteBold, XBrushes.White,
            new XRect(X0, y + 2, larguraTotal - Cols[Cols.Length - 1], AlturaMinima - 4), XStringFormats.Center);
        gfx.DrawString(VerbaFinanceira.FormatarValor(total), fonteBold, XBrushes.White,
            new XRect(X0 + larguraTotal - Cols[Cols.Length - 1], y + 2, Cols[Cols.Length - 1] - 6, AlturaMinima - 4), XStringFormats.CenterRight);
    }

    private static void DrawDetalheHeader(XGraphics gfx, float y, XFont fonteCab)
    {
        float x = X0;
        for (int i = 0; i < HeadersDetalhe.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, ColsDetalhe[i], AlturaMinima);
            gfx.DrawString(HeadersDetalhe[i], fonteCab, XBrushes.White,
                new XRect(x, y, ColsDetalhe[i], AlturaMinima), XStringFormats.Center);
            x += ColsDetalhe[i];
        }
    }

    private static void DrawDetalheRow(XGraphics gfx,
        (int Centro, string NomeEmpregado, int Empregado, decimal Liquido) d,
        float y, bool zebra, XFont fonte)
    {
        string[] cells =
        {
            d.Centro.ToString("D4"),
            d.Empregado.ToString(),
            d.NomeEmpregado,
            VerbaFinanceira.FormatarValor(d.Liquido),
        };
        float x = X0;
        for (int i = 0; i < ColsDetalhe.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.Silver, x, y, ColsDetalhe[i], AlturaMinima);
            gfx.DrawRectangle(zebra ? XBrushes.LightGray : XBrushes.White,
                x + 0.5, y + 0.5, ColsDetalhe[i] - 1, AlturaMinima - 1);
            var rect = new XRect(x + Espaco, y + Espaco, ColsDetalhe[i] - (Espaco * 2), AlturaMinima - (Espaco * 2));
            if (i == 3)
            {
                gfx.DrawString(cells[i], fonte, XBrushes.Black, rect, XStringFormats.CenterRight);
            }
            else
            {
                var formatter = new XTextFormatter(gfx)
                {
                    Alignment = i <= 1 ? XParagraphAlignment.Center : XParagraphAlignment.Left
                };
                formatter.DrawString(cells[i], fonte, XBrushes.Black, rect);
            }
            x += ColsDetalhe[i];
        }
    }

    private static void DrawDetalheTotal(XGraphics gfx, int emp, decimal total, float y, XFont fonteBold)
    {
        float x = X0;
        for (int i = 0; i < ColsDetalhe.Length; i++)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, x, y, ColsDetalhe[i], AlturaMinima);
            x += ColsDetalhe[i];
        }
        gfx.DrawString($"TOTAL ({emp} funcionários)", fonteBold, XBrushes.White,
            new XRect(X0, y + 2, ColsDetalhe[0] + ColsDetalhe[1] + ColsDetalhe[2], AlturaMinima - 4), XStringFormats.Center);
        gfx.DrawString(VerbaFinanceira.FormatarValor(total), fonteBold, XBrushes.White,
            new XRect(X0 + ColsDetalhe[0] + ColsDetalhe[1] + ColsDetalhe[2], y + 2, ColsDetalhe[3] - 6, AlturaMinima - 4), XStringFormats.CenterRight);
    }

    /// <summary>
    /// Gera o relatório detalhado por funcionário no layout do EXTRATO MENSAL do Sienge.
    /// Uma página (ou mais) por funcionário, com proventos/descontos detalhados, bases e resumo.
    /// </summary>
    public static void GerarDetalhado(string caminho, string competencia,
        int centroCodigo, string centroNome, List<DbService.FuncionarioDetalhe> funcionarios,
        List<DbService.RubricaResumo>? rubricas = null, DbService.ResumoBases? resumoBases = null)
    {
        Logger.Log($"[PDF] Início GerarDetalhado - {funcionarios.Count} funcionários");
        using var doc = new PdfDocument();
        PdfPage page = NovaPagina(doc);
        XGraphics gfx = XGraphics.FromPdfPage(page);

        var fonteTitulo = new XFont("Arial", 11, XFontStyleEx.Bold);
        var fonteCab = new XFont("Arial", 7, XFontStyleEx.Bold);
        var fonte = new XFont("Arial", 7, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 7, XFontStyleEx.Bold);
        var fontePeq = new XFont("Arial", 6.5f, XFontStyleEx.Regular);
        const float lh = 12;
        int pagina = 1;

        void DesenharCabecalho()
        {
            float y2 = 20;
            gfx.DrawString("Empresa:", fonteCab, XBrushes.Black, new XPoint(X0, y2));
            gfx.DrawString("1 - ENGENHARIA DE MATERIAIS LTDA", fonte, XBrushes.Black, new XPoint(X0 + 50, y2));
            gfx.DrawString("CNPJ:", fonteCab, XBrushes.Black, new XPoint(X0 + 320, y2));
            gfx.DrawString("41.157.967/0001-69", fonte, XBrushes.Black, new XPoint(X0 + 355, y2));
            gfx.DrawString("Competência:", fonteCab, XBrushes.Black, new XPoint(X0 + 520, y2));
            gfx.DrawString(competencia, fonte, XBrushes.Black, new XPoint(X0 + 595, y2));
            y2 += 11;
            gfx.DrawString("C. Custos:", fonteCab, XBrushes.Black, new XPoint(X0, y2));
            gfx.DrawString($"{centroCodigo:D4} - {centroNome}", fonte, XBrushes.Black, new XPoint(X0 + 50, y2));
            gfx.DrawString("Cálculo:", fonteCab, XBrushes.Black, new XPoint(X0 + 520, y2));
            gfx.DrawString("Folha Mensal", fonte, XBrushes.Black, new XPoint(X0 + 565, y2));
            y2 += 11;
            gfx.DrawLine(XPens.DarkGray, X0, y2, 776, y2);
            y2 += 4;
            gfx.DrawString($"EXTRATO MENSAL", fonteTitulo, XBrushes.Black, new XPoint(X0 + 310, y2));
            y2 += 14;
            gfx.DrawString($"Página: {pagina}", fontePeq, XBrushes.DarkGray, new XPoint(X0 + 680, y2));
            gfx.DrawString($"Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}", fontePeq, XBrushes.DarkGray, new XPoint(X0 + 560, y2));
        }

        // ===== PLANILHA: COLUNAS =====
        float cMat = 35, cNom = 160, cSit = 30, cSal = 60, cProv = 60, cDesc = 60, cLiq = 60, cBaseIr = 60, cBaseFg = 60;
        float largTotal = cMat + cNom + cSit + cSal + cProv + cDesc + cLiq + cBaseIr + cBaseFg;

        void DesenharCabecalhoColunas(float yy)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, yy, largTotal, lh);
            float cx = X0;
            gfx.DrawString("Empr.", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cMat - 2, lh), XStringFormats.TopCenter); cx += cMat;
            gfx.DrawString("Nome", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cNom - 2, lh), XStringFormats.TopLeft); cx += cNom;
            gfx.DrawString("Sit.", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cSit - 2, lh), XStringFormats.TopCenter); cx += cSit;
            gfx.DrawString("Salário", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cSal - 2, lh), XStringFormats.TopRight); cx += cSal;
            gfx.DrawString("Proventos", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cProv - 2, lh), XStringFormats.TopRight); cx += cProv;
            gfx.DrawString("Descontos", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cDesc - 2, lh), XStringFormats.TopRight); cx += cDesc;
            gfx.DrawString("Líquido", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cLiq - 2, lh), XStringFormats.TopRight); cx += cLiq;
            gfx.DrawString("Base IRRF", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cBaseIr - 2, lh), XStringFormats.TopRight); cx += cBaseIr;
            gfx.DrawString("Base FGTS", fonteCab, XBrushes.White, new XRect(cx + 1, yy, cBaseFg - 2, lh), XStringFormats.TopRight);
        }

        DesenharCabecalho();
        float y = 68;

        // ===== PLANILHA DE FUNCIONÁRIOS =====
        DesenharCabecalhoColunas(y);
        y += lh;

        for (int idx = 0; idx < funcionarios.Count; idx++)
        {
            var f = funcionarios[idx];
            Logger.Log($"[PDF] Funcionário {idx + 1}/{funcionarios.Count}: {f.Empregado} {f.Nome}");

            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, lh);
            if (y <= 20) { pagina++; DesenharCabecalho(); y = 68; DesenharCabecalhoColunas(y); y += lh; }

            // zebra striping
            var bg = idx % 2 == 0 ? XBrushes.White : XBrushes.AliceBlue;
            gfx.DrawRectangle(bg, X0, y, largTotal, lh);

            float cx = X0;
            gfx.DrawString(f.Empregado.ToString(), fonte, XBrushes.Black, new XRect(cx + 1, y, cMat - 2, lh), XStringFormats.TopCenter); cx += cMat;
            gfx.DrawString(f.Nome, fonte, XBrushes.Black, new XRect(cx + 1, y, cNom - 2, lh), XStringFormats.TopLeft); cx += cNom;
            gfx.DrawString(f.Situacao, fonte, XBrushes.Black, new XRect(cx + 1, y, cSit - 2, lh), XStringFormats.TopCenter); cx += cSit;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.Salario), fonte, XBrushes.Black, new XRect(cx + 1, y, cSal - 2, lh), XStringFormats.TopRight); cx += cSal;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.TotalProventos), fonte, XBrushes.Black, new XRect(cx + 1, y, cProv - 2, lh), XStringFormats.TopRight); cx += cProv;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.TotalDescontos), fonte, XBrushes.Black, new XRect(cx + 1, y, cDesc - 2, lh), XStringFormats.TopRight); cx += cDesc;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.Liquido), fonteBold, XBrushes.Black, new XRect(cx + 1, y, cLiq - 2, lh), XStringFormats.TopRight); cx += cLiq;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.BaseIrrf), fonte, XBrushes.Black, new XRect(cx + 1, y, cBaseIr - 2, lh), XStringFormats.TopRight); cx += cBaseIr;
            gfx.DrawString(VerbaFinanceira.FormatarValor(f.BaseFgts), fonte, XBrushes.Black, new XRect(cx + 1, y, cBaseFg - 2, lh), XStringFormats.TopRight);

            y += lh;
        }

        // --- TOTAL GERAL ---
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, lh);
        decimal totProv = funcionarios.Sum(f => f.TotalProventos);
        decimal totDesc = funcionarios.Sum(f => f.TotalDescontos);
        decimal totLiq = funcionarios.Sum(f => f.Liquido);
        decimal totBaseIr = funcionarios.Sum(f => f.BaseIrrf);
        decimal totBaseFg = funcionarios.Sum(f => f.BaseFgts);

        gfx.DrawRectangle(XBrushes.DarkSlateBlue, X0, y, largTotal, lh);
        float ct = X0;
        gfx.DrawString("", fonteBold, XBrushes.White, new XRect(ct + 1, y, cMat - 2, lh), XStringFormats.TopCenter); ct += cMat;
        gfx.DrawString("TOTAL GERAL", fonteBold, XBrushes.White, new XRect(ct + 1, y, cNom - 2, lh), XStringFormats.TopLeft); ct += cNom;
        gfx.DrawString($"{funcionarios.Count}", fonteBold, XBrushes.White, new XRect(ct + 1, y, cSit - 2, lh), XStringFormats.TopCenter); ct += cSit;
        gfx.DrawString("", fonteBold, XBrushes.White, new XRect(ct + 1, y, cSal - 2, lh), XStringFormats.TopRight); ct += cSal;
        gfx.DrawString(VerbaFinanceira.FormatarValor(totProv), fonteBold, XBrushes.White, new XRect(ct + 1, y, cProv - 2, lh), XStringFormats.TopRight); ct += cProv;
        gfx.DrawString(VerbaFinanceira.FormatarValor(totDesc), fonteBold, XBrushes.White, new XRect(ct + 1, y, cDesc - 2, lh), XStringFormats.TopRight); ct += cDesc;
        gfx.DrawString(VerbaFinanceira.FormatarValor(totLiq), fonteBold, XBrushes.White, new XRect(ct + 1, y, cLiq - 2, lh), XStringFormats.TopRight); ct += cLiq;
        gfx.DrawString(VerbaFinanceira.FormatarValor(totBaseIr), fonteBold, XBrushes.White, new XRect(ct + 1, y, cBaseIr - 2, lh), XStringFormats.TopRight); ct += cBaseIr;
        gfx.DrawString(VerbaFinanceira.FormatarValor(totBaseFg), fonteBold, XBrushes.White, new XRect(ct + 1, y, cBaseFg - 2, lh), XStringFormats.TopRight);
        y += lh + 12;

        // ===== RESUMO POR RUBRICA =====
        if (rubricas != null && rubricas.Count > 0)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, 30);
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, 776, lh);
            gfx.DrawString("Resumo por Rubrica", fonteCab, XBrushes.White, new XRect(X0 + 4, y, 770, lh), XStringFormats.TopLeft);
            y += lh;

            float rcCod = 40, rcDesc2 = 300, rcHoras = 60, rcValor = 100, rcTipo = 30;
            float rcLarg = rcCod + rcDesc2 + rcHoras + rcValor + rcTipo;

            gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, rcLarg, lh);
            float rx = X0;
            gfx.DrawString("Cód", fonteCab, XBrushes.White, new XRect(rx + 1, y, rcCod - 2, lh), XStringFormats.TopCenter); rx += rcCod;
            gfx.DrawString("Descrição", fonteCab, XBrushes.White, new XRect(rx + 1, y, rcDesc2 - 2, lh), XStringFormats.TopLeft); rx += rcDesc2;
            gfx.DrawString("Horas", fonteCab, XBrushes.White, new XRect(rx + 1, y, rcHoras - 2, lh), XStringFormats.TopRight); rx += rcHoras;
            gfx.DrawString("Valor", fonteCab, XBrushes.White, new XRect(rx + 1, y, rcValor - 2, lh), XStringFormats.TopRight); rx += rcValor;
            gfx.DrawString("Tipo", fonteCab, XBrushes.White, new XRect(rx + 1, y, rcTipo - 2, lh), XStringFormats.TopCenter);
            y += lh;

            decimal totalRubrica = 0;
            foreach (var r in rubricas)
            {
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, lh);
                if (y <= 20) { pagina++; DesenharCabecalho(); y = 68; }

                var zr = rubricas.IndexOf(r) % 2 == 0 ? XBrushes.White : XBrushes.AliceBlue;
                gfx.DrawRectangle(zr, X0, y, rcLarg, lh);
                rx = X0;
                gfx.DrawString(r.CodigoEvento.ToString(), fonte, XBrushes.Black, new XRect(rx + 1, y, rcCod - 2, lh), XStringFormats.TopCenter); rx += rcCod;
                gfx.DrawString(r.NomeEvento, fonte, XBrushes.Black, new XRect(rx + 1, y, rcDesc2 - 2, lh), XStringFormats.TopLeft); rx += rcDesc2;
                string h2 = r.Horas > 0 ? $"{(int)r.Horas}:{((r.Horas % 1) * 60):00}" : "";
                gfx.DrawString(h2, fonte, XBrushes.Black, new XRect(rx + 1, y, rcHoras - 2, lh), XStringFormats.TopRight); rx += rcHoras;
                gfx.DrawString(VerbaFinanceira.FormatarValor(r.Valor), fonte, XBrushes.Black, new XRect(rx + 1, y, rcValor - 2, lh), XStringFormats.TopRight); rx += rcValor;
                gfx.DrawString(r.ProvDesc, fonte, XBrushes.Black, new XRect(rx + 1, y, rcTipo - 2, lh), XStringFormats.TopCenter);
                totalRubrica += r.ProvDesc == "D" ? -r.Valor : r.Valor;
                y += lh;
            }

            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, lh);
            gfx.DrawRectangle(XBrushes.DarkSlateBlue, X0, y, rcLarg, lh);
            gfx.DrawString("TOTAL", fonteBold, XBrushes.White, new XRect(X0 + 1, y, rcCod + rcDesc2 + rcHoras - 2, lh), XStringFormats.TopRight);
            gfx.DrawString(VerbaFinanceira.FormatarValor(totalRubrica), fonteBold, XBrushes.White, new XRect(X0 + rcCod + rcDesc2 + rcHoras + 1, y, rcValor - 2, lh), XStringFormats.TopRight);
            y += lh + 12;
        }

        // ===== RESUMO DAS BASES =====
        if (resumoBases != null)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, 160);
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, 776, lh);
            gfx.DrawString("Resumo das Bases", fonteCab, XBrushes.White, new XRect(X0 + 4, y, 770, lh), XStringFormats.TopLeft);
            y += lh;

            void LinhaBase(string txt, string val, bool bold)
            {
                gfx.DrawString(txt, bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0 + 4, y, 350, 11), XStringFormats.TopLeft);
                gfx.DrawString(val, bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0 + 360, y, 200, 11), XStringFormats.TopRight);
                y += 11;
            }

            gfx.DrawString("Situações", fonteCab, XBrushes.Black, new XPoint(X0 + 4, y)); y += 11;
            LinhaBase("Número de empregados:", resumoBases.NumEmpregados.ToString(), false);
            LinhaBase("Trabalhando:", resumoBases.Trabalhando.ToString(), false);
            LinhaBase("Demitido:", resumoBases.Demitido.ToString(), false);
            LinhaBase("Afastado:", resumoBases.Afastado.ToString(), false);
            LinhaBase("Admissões:", resumoBases.Admissoes.ToString(), false);
            y += 3;

            gfx.DrawString("INSS", fonteCab, XBrushes.Black, new XPoint(X0 + 4, y)); y += 11;
            LinhaBase("Salário contribuição empregados:", VerbaFinanceira.FormatarValor(resumoBases.SalarioContribEmpregados), false);
            LinhaBase("Base total:", VerbaFinanceira.FormatarValor(resumoBases.BaseTotalInss), false);
            LinhaBase("Total INSS:", VerbaFinanceira.FormatarValor(resumoBases.TotalInss), true);
            y += 3;

            gfx.DrawString("IRRF", fonteCab, XBrushes.Black, new XPoint(X0 + 4, y)); y += 11;
            LinhaBase("Base IRRF Mensal:", VerbaFinanceira.FormatarValor(resumoBases.BaseIrrfMensal), false);
            LinhaBase("Valor IRRF Mensal:", VerbaFinanceira.FormatarValor(resumoBases.ValorIrrfMensal), false);
            y += 3;

            gfx.DrawString("FGTS", fonteCab, XBrushes.Black, new XPoint(X0 + 4, y)); y += 11;
            LinhaBase("Base do FGTS:", VerbaFinanceira.FormatarValor(resumoBases.BaseFgts), false);
            LinhaBase("Valor do FGTS:", VerbaFinanceira.FormatarValor(resumoBases.ValorFgts), false);
            LinhaBase("Base FGTS Aprendiz:", VerbaFinanceira.FormatarValor(resumoBases.BaseFgtsAprendiz), false);
        }

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        Logger.Log($"[PDF] Salvando em: {caminho}");
        var tempPath = caminho + ".tmp";
        doc.Save(tempPath);
        if (File.Exists(caminho)) File.Delete(caminho);
        File.Move(tempPath, caminho);
        Logger.Log("[PDF] Arquivo salvo com sucesso!");
    }

    /// <summary>
    /// Gera a planilha por centro de custo com o resumo por verba (descrição + valor).
    /// Uma linha por verba por centro, com totais por centro e geral.
    /// </summary>
    public static void GerarPorCentroVerbas(string caminho, string competencia,
        List<DbService.CentroVerbaResumo> centros)
    {
        using var doc = new PdfDocument();
        PdfPage page = NovaPagina(doc);
        XGraphics gfx = XGraphics.FromPdfPage(page);

        var fonteTitulo = new XFont("Arial", 13, XFontStyleEx.Bold);
        var fonteSub = new XFont("Arial", 9, XFontStyleEx.Regular);
        var fonteCab = new XFont("Arial", 8, XFontStyleEx.Bold);
        var fonte = new XFont("Arial", 8, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 8, XFontStyleEx.Bold);
        const float linhaAlt = 18;
        float colC = 50, colNome = 250, colEmp = 70, colVerba = 320, colVal = 92;
        float largTotal = colC + colNome + colEmp + colVerba + colVal;

        float y = 28;
        gfx.DrawString("RELATÓRIO POR CENTRO DE CUSTO - RESUMO POR VERBA - PLUS CONTABIL",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 18;
        gfx.DrawString($"Competência: {competencia}    Empresa: Plus Contabilidade    " +
                       $"Centros: {centros.Count}    Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
            fonteSub, XBrushes.DarkGray, new XPoint(X0, y));
        y += 14;

        void DrawHeader(float yy)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, yy, largTotal, linhaAlt);
            float xx = X0;
            gfx.DrawString("Centro", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colC - 4, linhaAlt), XStringFormats.Center);
            xx += colC;
            gfx.DrawString("Centro de Custo", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colNome - 4, linhaAlt), XStringFormats.CenterLeft);
            xx += colNome;
            gfx.DrawString("Emp", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colEmp - 4, linhaAlt), XStringFormats.Center);
            xx += colEmp;
            gfx.DrawString("Verba", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colVerba - 4, linhaAlt), XStringFormats.CenterLeft);
            xx += colVerba;
            gfx.DrawString("Valor (R$)", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colVal - 4, linhaAlt), XStringFormats.CenterRight);
        }

        DrawHeader(y);
        y += linhaAlt;

        decimal totalGeral = 0m;
        int linha = 0;
        foreach (var c in centros)
        {
            decimal totalCentro = 0m;
            foreach (var (desc, val) in c.Verbas)
            {
                totalCentro += val;
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
                if (y == 30) { DrawHeader(y); y += linhaAlt; }
                var zebra = linha % 2 == 0 ? XBrushes.White : XBrushes.AliceBlue;
                float xx = X0;
                gfx.DrawRectangle(zebra, xx, y, colC, linhaAlt);
                gfx.DrawString(c.Centro.ToString("D4"), fonte, XBrushes.Black, new XRect(xx + 2, y, colC - 4, linhaAlt), XStringFormats.Center);
                xx += colC;
                gfx.DrawRectangle(zebra, xx, y, colNome, linhaAlt);
                gfx.DrawString(c.CentroNome, fonte, XBrushes.Black, new XRect(xx + 2, y, colNome - 4, linhaAlt), XStringFormats.TopLeft);
                xx += colNome;
                gfx.DrawRectangle(zebra, xx, y, colEmp, linhaAlt);
                gfx.DrawString(c.Empregados.ToString(), fonte, XBrushes.Black, new XRect(xx + 2, y, colEmp - 4, linhaAlt), XStringFormats.Center);
                xx += colEmp;
                gfx.DrawRectangle(zebra, xx, y, colVerba, linhaAlt);
                gfx.DrawString(desc, fonte, XBrushes.Black, new XRect(xx + 2, y, colVerba - 4, linhaAlt), XStringFormats.TopLeft);
                xx += colVerba;
                gfx.DrawRectangle(zebra, xx, y, colVal, linhaAlt);
                gfx.DrawString(VerbaFinanceira.FormatarValor(val), fonte, XBrushes.Black, new XRect(xx + 2, y, colVal - 4, linhaAlt), XStringFormats.CenterRight);
                y += linhaAlt;
                linha++;
            }
            // subtotal do centro
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
            if (y == 30) { DrawHeader(y); y += linhaAlt; }
            float x2 = X0;
            gfx.DrawRectangle(XBrushes.SteelBlue, x2, y, colC, linhaAlt); x2 += colC;
            gfx.DrawRectangle(XBrushes.SteelBlue, x2, y, colNome, linhaAlt); x2 += colNome;
            gfx.DrawRectangle(XBrushes.SteelBlue, x2, y, colEmp, linhaAlt); x2 += colEmp;
            gfx.DrawRectangle(XBrushes.SteelBlue, x2, y, colVerba, linhaAlt);
            gfx.DrawString($"SUBTOTAL {c.Centro:D4} {c.CentroNome}", fonteBold, XBrushes.White, new XRect(x2 + 2, y, colVerba - 4, linhaAlt), XStringFormats.CenterLeft);
            x2 += colVerba;
            gfx.DrawRectangle(XBrushes.SteelBlue, x2, y, colVal, linhaAlt);
            gfx.DrawString(VerbaFinanceira.FormatarValor(totalCentro), fonteBold, XBrushes.White, new XRect(x2 + 2, y, colVal - 4, linhaAlt), XStringFormats.CenterRight);
            y += linhaAlt;
            totalGeral += totalCentro;
        }

        // total geral
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
        if (y == 30) { DrawHeader(y); y += linhaAlt; }
        float xg = X0;
        gfx.DrawRectangle(XBrushes.DarkSlateBlue, xg, y, colC + colNome + colEmp, linhaAlt);
        gfx.DrawString($"TOTAL GERAL ({centros.Count} centros)", fonteBold, XBrushes.White, new XRect(xg + 2, y, colC + colNome + colEmp - 4, linhaAlt), XStringFormats.CenterLeft);
        xg += colC + colNome + colEmp;
        gfx.DrawRectangle(XBrushes.DarkSlateBlue, xg, y, colVerba, linhaAlt); xg += colVerba;
        gfx.DrawRectangle(XBrushes.DarkSlateBlue, xg, y, colVal, linhaAlt);
        gfx.DrawString(VerbaFinanceira.FormatarValor(totalGeral), fonteBold, XBrushes.White, new XRect(xg + 2, y, colVal - 4, linhaAlt), XStringFormats.CenterRight);

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        doc.Save(caminho);
    }

    /// <summary>
    /// Relatório contábil por centro de custo, organizado em PROVENTOS / DESCONTOS / ENCARGOS,
    /// com subtotais por seção, total por centro e total geral.
    /// </summary>
    public static void GerarResumoContabil(string caminho, string competencia,
        List<DbService.CentroContabil> centros)
    {
        using var doc = new PdfDocument();
        PdfPage page = NovaPagina(doc);
        XGraphics gfx = XGraphics.FromPdfPage(page);

        var fonteTitulo = new XFont("Arial", 13, XFontStyleEx.Bold);
        var fonteSec = new XFont("Arial", 9, XFontStyleEx.Bold);
        var fonteSub = new XFont("Arial", 9, XFontStyleEx.Regular);
        var fonte = new XFont("Arial", 8, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 8, XFontStyleEx.Bold);
        const float linhaAlt = 17;
        float colDesc = 620;
        float colVal = 162;

        float y = 26;
        gfx.DrawString("FOLHA DE PAGAMENTO POR CENTRO DE CUSTO - RESUMO CONTÁBIL - PLUS CONTABIL",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 18;
        gfx.DrawString($"Competência: {competencia}    Empresa: Plus Contabilidade    Centros: {centros.Count}    " +
                       $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
            fonteSub, XBrushes.DarkGray, new XPoint(X0, y));
        y += 14;

        decimal totalGeral = 0m;
        foreach (var c in centros)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt + 6);
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, colDesc + colVal, linhaAlt + 2);
            gfx.DrawString($"CENTRO {c.Centro:D4} - {c.CentroNome}   ({c.Empregados} funcionários)",
                fonteBold, XBrushes.White, new XRect(X0 + 4, y, colDesc - 4, linhaAlt + 2), XStringFormats.CenterLeft);
            y += linhaAlt + 2;

            void Secao(string titulo, List<(string D, decimal V)> itens, decimal subTotal, XColor cor)
            {
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
                gfx.DrawRectangle(new XSolidBrush(cor), X0, y, colDesc + colVal, linhaAlt);
                gfx.DrawString(titulo, fonteSec, XBrushes.White, new XRect(X0 + 4, y, colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
                y += linhaAlt;
                foreach (var (d, v) in itens)
                {
                    (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
                    gfx.DrawString("   " + d, fonte, XBrushes.Black, new XRect(X0, y - 8, colDesc, linhaAlt), XStringFormats.TopLeft);
                    gfx.DrawString(VerbaFinanceira.FormatarValor(v), fonte, XBrushes.Black, new XRect(X0 + colDesc + 8, y - 8, colVal - 8, linhaAlt), XStringFormats.TopRight);
                    y += linhaAlt;
                }
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
                gfx.DrawString("   SUBTOTAL " + titulo, fonteBold, XBrushes.Black, new XRect(X0, y - 8, colDesc, linhaAlt), XStringFormats.TopLeft);
                gfx.DrawString(VerbaFinanceira.FormatarValor(subTotal), fonteBold, XBrushes.Black, new XRect(X0 + colDesc + 8, y - 8, colVal - 8, linhaAlt), XStringFormats.TopRight);
                y += linhaAlt + 2;
            }

            Secao("PROVENTOS", c.Proventos, c.TotalProventos, XColor.FromArgb(46, 125, 50));
            Secao("DESCONTOS", c.Descontos, c.TotalDescontos, XColor.FromArgb(198, 40, 40));
            Secao("ENCARGOS", c.Encargos, c.TotalEncargos, XColor.FromArgb(21, 101, 192));

            // total do centro
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
            gfx.DrawRectangle(XBrushes.DarkSlateBlue, X0, y, colDesc + colVal, linhaAlt);
            gfx.DrawString($"CUSTO TOTAL DO CENTRO (Proventos + Encargos - Descontos)", fonteBold, XBrushes.White, new XRect(X0 + 4, y, colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
            gfx.DrawString(VerbaFinanceira.FormatarValor(c.CustoTotal), fonteBold, XBrushes.White, new XRect(X0 + colDesc + 8, y, colVal - 8, linhaAlt), XStringFormats.CenterRight);
            y += linhaAlt;
            totalGeral += c.CustoTotal;
            y += 10;
        }

        // total geral
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
        gfx.DrawRectangle(XBrushes.Black, X0, y, colDesc + colVal, linhaAlt);
        gfx.DrawString($"TOTAL GERAL ({centros.Count} centros)", fonteBold, XBrushes.White, new XRect(X0 + 4, y, colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
        gfx.DrawString(VerbaFinanceira.FormatarValor(totalGeral), fonteBold, XBrushes.White, new XRect(X0 + colDesc + 8, y, colVal - 8, linhaAlt), XStringFormats.CenterRight);

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        doc.Save(caminho);
    }

    /// <summary>
    /// Relatório de RAZÃO CONTÁBIL por centro de custo: débito/crédito por conta do plano
    /// financeiro, com saldo por conta, subtotais por centro e total geral. Adequado ao
    /// fechamento de balanço / lançamentos contábeis da folha.
    /// </summary>
    public static void GerarRazaoContabil(string caminho, string competencia,
        List<DbService.RazaoCentro> centros)
    {
        using var doc = new PdfDocument();
        PdfPage page = NovaPagina(doc);
        XGraphics gfx = XGraphics.FromPdfPage(page);

        var fonteTitulo = new XFont("Arial", 13, XFontStyleEx.Bold);
        var fonteSub = new XFont("Arial", 9, XFontStyleEx.Regular);
        var fonteCab = new XFont("Arial", 8, XFontStyleEx.Bold);
        var fonte = new XFont("Arial", 8, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 8, XFontStyleEx.Bold);
        const float linhaAlt = 18;
        float colPlano = 80, colDesc = 420, colDeb = 94, colCred = 94, colSaldo = 94;
        float largTotal = colPlano + colDesc + colDeb + colCred + colSaldo;

        float y = 26;
        gfx.DrawString("RAZÃO CONTÁBIL DA FOLHA POR CENTRO DE CUSTO - PLUS CONTABIL",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 18;
        gfx.DrawString($"Competência: {competencia}    Empresa: Plus Contabilidade    Centros: {centros.Count}    " +
                       $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
            fonteSub, XBrushes.DarkGray, new XPoint(X0, y));
        y += 14;

        void DrawHeader(float yy)
        {
            gfx.DrawRectangle(XBrushes.SteelBlue, X0, yy, largTotal, linhaAlt);
            float xx = X0;
            gfx.DrawString("Plano Fin.", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colPlano - 4, linhaAlt), XStringFormats.Center);
            xx += colPlano;
            gfx.DrawString("Conta / Verba", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
            xx += colDesc;
            gfx.DrawString("Débito (R$)", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colDeb - 4, linhaAlt), XStringFormats.CenterRight);
            xx += colDeb;
            gfx.DrawString("Crédito (R$)", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colCred - 4, linhaAlt), XStringFormats.CenterRight);
            xx += colCred;
            gfx.DrawString("Saldo (R$)", fonteCab, XBrushes.White, new XRect(xx + 2, yy, colSaldo - 4, linhaAlt), XStringFormats.CenterRight);
        }

        decimal totalDebitoGeral = 0, totalCreditoGeral = 0;
        int linha = 0;
        foreach (var c in centros)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt + 2);
            gfx.DrawRectangle(XBrushes.DarkSlateBlue, X0, y, largTotal, linhaAlt + 2);
            gfx.DrawString($"CENTRO {c.Centro:D4} - {c.CentroNome}   ({c.Empregados} funcionários)",
                fonteBold, XBrushes.White, new XRect(X0 + 4, y, largTotal - 4, linhaAlt + 2), XStringFormats.CenterLeft);
            y += linhaAlt + 2;

            DrawHeader(y);
            y += linhaAlt;
            foreach (var a in c.Contas)
            {
                (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
                if (y == 30) { DrawHeader(y); y += linhaAlt; }
                var zebra = linha % 2 == 0 ? XBrushes.White : XBrushes.AliceBlue;
                float xx = X0;
                gfx.DrawRectangle(zebra, xx, y, colPlano, linhaAlt);
                gfx.DrawString(a.Plano, fonte, XBrushes.Black, new XRect(xx + 2, y, colPlano - 4, linhaAlt), XStringFormats.Center);
                xx += colPlano;
                gfx.DrawRectangle(zebra, xx, y, colDesc, linhaAlt);
                gfx.DrawString($"{a.CodigoVerba:D4} - {a.Descricao}", fonte, XBrushes.Black, new XRect(xx + 2, y, colDesc - 4, linhaAlt), XStringFormats.TopLeft);
                xx += colDesc;
                gfx.DrawRectangle(zebra, xx, y, colDeb, linhaAlt);
                gfx.DrawString(VerbaFinanceira.FormatarValor(a.Debito), fonte, XBrushes.Black, new XRect(xx + 2, y, colDeb - 4, linhaAlt), XStringFormats.CenterRight);
                xx += colDeb;
                gfx.DrawRectangle(zebra, xx, y, colCred, linhaAlt);
                gfx.DrawString(VerbaFinanceira.FormatarValor(a.Credito), fonte, XBrushes.Black, new XRect(xx + 2, y, colCred - 4, linhaAlt), XStringFormats.CenterRight);
                xx += colCred;
                gfx.DrawRectangle(zebra, xx, y, colSaldo, linhaAlt);
                gfx.DrawString(VerbaFinanceira.FormatarValor(a.Saldo), fonteBold, XBrushes.Black, new XRect(xx + 2, y, colSaldo - 4, linhaAlt), XStringFormats.CenterRight);
                y += linhaAlt;
                linha++;
            }
            // subtotal do centro
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
            float xs = X0;
            gfx.DrawRectangle(XBrushes.SteelBlue, xs, y, colPlano + colDesc, linhaAlt);
            gfx.DrawString($"SUBTOTAL {c.Centro:D4}", fonteBold, XBrushes.White, new XRect(xs + 2, y, colPlano + colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
            xs += colPlano + colDesc;
            gfx.DrawRectangle(XBrushes.SteelBlue, xs, y, colDeb, linhaAlt);
            gfx.DrawString(VerbaFinanceira.FormatarValor(c.TotalDebito), fonteBold, XBrushes.White, new XRect(xs + 2, y, colDeb - 4, linhaAlt), XStringFormats.CenterRight);
            xs += colDeb;
            gfx.DrawRectangle(XBrushes.SteelBlue, xs, y, colCred, linhaAlt);
            gfx.DrawString(VerbaFinanceira.FormatarValor(c.TotalCredito), fonteBold, XBrushes.White, new XRect(xs + 2, y, colCred - 4, linhaAlt), XStringFormats.CenterRight);
            xs += colCred;
            gfx.DrawRectangle(XBrushes.SteelBlue, xs, y, colSaldo, linhaAlt);
            gfx.DrawString(VerbaFinanceira.FormatarValor(c.TotalDebito - c.TotalCredito), fonteBold, XBrushes.White, new XRect(xs + 2, y, colSaldo - 4, linhaAlt), XStringFormats.CenterRight);
            y += linhaAlt;
            totalDebitoGeral += c.TotalDebito;
            totalCreditoGeral += c.TotalCredito;
            y += 10;
        }

        // total geral
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
        float xg = X0;
        gfx.DrawRectangle(XBrushes.Black, xg, y, colPlano + colDesc, linhaAlt);
        gfx.DrawString($"TOTAL GERAL ({centros.Count} centros)", fonteBold, XBrushes.White, new XRect(xg + 2, y, colPlano + colDesc - 4, linhaAlt), XStringFormats.CenterLeft);
        xg += colPlano + colDesc;
        gfx.DrawRectangle(XBrushes.Black, xg, y, colDeb, linhaAlt);
        gfx.DrawString(VerbaFinanceira.FormatarValor(totalDebitoGeral), fonteBold, XBrushes.White, new XRect(xg + 2, y, colDeb - 4, linhaAlt), XStringFormats.CenterRight);
        xg += colDeb;
        gfx.DrawRectangle(XBrushes.Black, xg, y, colCred, linhaAlt);
        gfx.DrawString(VerbaFinanceira.FormatarValor(totalCreditoGeral), fonteBold, XBrushes.White, new XRect(xg + 2, y, colCred - 4, linhaAlt), XStringFormats.CenterRight);
        xg += colCred;
        gfx.DrawRectangle(XBrushes.Black, xg, y, colSaldo, linhaAlt);
        gfx.DrawString(VerbaFinanceira.FormatarValor(totalDebitoGeral - totalCreditoGeral), fonteBold, XBrushes.White, new XRect(xg + 2, y, colSaldo - 4, linhaAlt), XStringFormats.CenterRight);

        // ===== DETALHAMENTO EXPLICADO DO TOTAL GERAL =====
        y += linhaAlt + 12;
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
        gfx.DrawString("DETALHAMENTO DO TOTAL GERAL", fonteCab, XBrushes.SteelBlue, new XPoint(X0, y));
        y += 14;

        decimal SaldoVerba(int cod, string plano) =>
            centros.Sum(c => c.Contas.Where(a => a.CodigoVerba == cod && a.Plano == plano)
                .Sum(a => a.Debito - a.Credito));

        void LinhaDetalhe(string txt, decimal val, bool bold)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
            gfx.DrawString("   " + txt, bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0, y - 8, 560, linhaAlt), XStringFormats.TopLeft);
            gfx.DrawString(VerbaFinanceira.FormatarValor(val), bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0 + 560 + 8, y - 8, 200 - 8, linhaAlt), XStringFormats.TopRight);
            y += linhaAlt;
        }

        decimal sSal = SaldoVerba(1, "2.02.01.02");
        decimal sFer = SaldoVerba(3, "2.01.02.02");
        decimal sInss = SaldoVerba(2, "2.01.02.10");
        decimal sFgts = SaldoVerba(58, "2.01.02.11");
        decimal sIrrf = SaldoVerba(10, "2.01.02.16");
        decimal sEmp = SaldoVerba(20, "2.02.02.25");
        decimal resto = (totalDebitoGeral - totalCreditoGeral) - (sSal + sFer + sInss + sFgts + sIrrf + sEmp);

        LinhaDetalhe($"Salário / líquido ({centros.Count} centros)", sSal, false);
        LinhaDetalhe("  (+) Férias", sFer, false);
        LinhaDetalhe("  (+) FGTS", sFgts, false);
        LinhaDetalhe("  (-) INSS retido", sInss, false);
        LinhaDetalhe("  (-) IRRF retido", sIrrf, false);
        LinhaDetalhe("  (-) Empréstimos consignados", sEmp, false);
        LinhaDetalhe("  (+) Demais contas/verbas", resto, false);
        LinhaDetalhe("", 0, false);
        LinhaDetalhe("TOTAL GERAL (débito - crédito)", totalDebitoGeral - totalCreditoGeral, true);

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        doc.Save(caminho);
    }

    /// <summary>
    /// Desenha o rodapé de divulgação do desenvolvedor na última página do PDF.
    /// </summary>
    private static void DrawRodape(XGraphics gfx, PdfPage page, float y)
    {
        var fonte = new XFont("Arial", 8, XFontStyleEx.Regular);
        var fonteBold = new XFont("Arial", 8, XFontStyleEx.Bold);
        float rodapeY = (float)(page.Height.Point - 38);
        gfx.DrawRectangle(XBrushes.SteelBlue, X0 - 6, rodapeY - 12, 800, 2);
        gfx.DrawString("Desenvolvido por PLUS INFORMÁTICA - Tecnologia que resolve. Confiança que fica.",
            fonteBold, XBrushes.SteelBlue, new XPoint(X0, rodapeY));
        gfx.DrawString("Eduardo Aquino Silva  |  WhatsApp (82) 91593-591  |  plusinformaticamcz.com.br  |  webmaster@pluscont.com.br",
            fonte, XBrushes.DarkGray, new XPoint(X0, rodapeY + 12));
    }
}
