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
    /// Gera o relatório detalhado por funcionário de um centro de custo.
    /// Mostra líquido, proventos, descontos, encargos/guias e empréstimos de cada pessoa.
    /// </summary>
    /// <summary>Colunas da tabela detalhada (planilha).</summary>
    private static readonly float[] ColsFunc = { 40, 55, 175, 75, 78, 78, 62, 62, 62, 75 };
    private static readonly string[] HeadersFunc =
    {
        "Matr.", "Centro", "Funcionário", "Líquido", "Proventos", "Descontos", "FGTS", "INSS", "IRRF", "Empréstimos"
    };

    public static void GerarDetalhado(string caminho, string competencia,
        int centroCodigo, string centroNome, List<DbService.FuncionarioDetalhe> funcionarios)
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

        float y = 28;
        gfx.DrawString("RELATÓRIO MENSAL POR FUNCIONÁRIO - CENTRO DE CUSTO - PLUS CONTABIL",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 18;
        gfx.DrawString($"Competência: {competencia}    Centro: {centroCodigo:D4} - {centroNome}    " +
                       $"Funcionários: {funcionarios.Count}    Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
            fonteSub, XBrushes.DarkGray, new XPoint(X0, y));
        y += 14;

        // cabeçalho da tabela
        float largTotal = ColsFunc.Sum();
        DrawFuncHeader(gfx, y, fonteCab, linhaAlt);
        y += linhaAlt;

        // totais do centro
        decimal tLiq = 0, tProv = 0, tDesc = 0, tFgts = 0, tInss = 0, tIrrf = 0, tEmp = 0;
        foreach (var f in funcionarios)
        {
            tLiq += f.Liquido; tProv += f.TotalProventos; tDesc += f.TotalDescontos;
            tFgts += f.Fgts; tInss += f.Inss; tIrrf += f.Irrf; tEmp += f.TotalEmprestimos;
        }

        // linhas
        float x;
        for (int i = 0; i < funcionarios.Count; i++)
        {
            var f = funcionarios[i];
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
            if (y == 30) { DrawFuncHeader(gfx, y, fonteCab, linhaAlt); y += linhaAlt; }
            var zebra = i % 2 == 0 ? XBrushes.White : XBrushes.AliceBlue;
            x = X0;
            var cells = new string[]
            {
                f.Empregado.ToString(),
                f.Centro.ToString("D4"),
                f.Nome,
                VerbaFinanceira.FormatarValor(f.Liquido),
                VerbaFinanceira.FormatarValor(f.TotalProventos),
                VerbaFinanceira.FormatarValor(f.TotalDescontos),
                VerbaFinanceira.FormatarValor(f.Fgts),
                VerbaFinanceira.FormatarValor(f.Inss),
                VerbaFinanceira.FormatarValor(f.Irrf),
                VerbaFinanceira.FormatarValor(f.TotalEmprestimos),
            };
            for (int c = 0; c < ColsFunc.Length; c++)
            {
                gfx.DrawRectangle(zebra, x, y, ColsFunc[c], linhaAlt);
                gfx.DrawRectangle(XBrushes.Silver, x, y, ColsFunc[c], 0.4f); // borda inferior fina
                var rect = new XRect(x + 2, y, ColsFunc[c] - 4, linhaAlt);
                var alinh = c == 2 ? XStringFormats.TopLeft : c >= 3 ? XStringFormats.CenterRight : XStringFormats.Center;
                gfx.DrawString(cells[c], c == 2 ? fonte : fonte, XBrushes.Black, rect, alinh);
                x += ColsFunc[c];
            }
            y += linhaAlt;
        }

        // linha de totais por coluna
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt);
        if (y == 30) { DrawFuncHeader(gfx, y, fonteCab, linhaAlt); y += linhaAlt; }
        gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, largTotal, linhaAlt);
        var totais = new string[]
        {
            "", "", $"TOTAL ({funcionarios.Count})",
            VerbaFinanceira.FormatarValor(tLiq), VerbaFinanceira.FormatarValor(tProv),
            VerbaFinanceira.FormatarValor(tDesc), VerbaFinanceira.FormatarValor(tFgts),
            VerbaFinanceira.FormatarValor(tInss), VerbaFinanceira.FormatarValor(tIrrf),
            VerbaFinanceira.FormatarValor(tEmp),
        };
        x = X0;
        for (int c = 0; c < ColsFunc.Length; c++)
        {
            var rect = new XRect(x + 2, y, ColsFunc[c] - 4, linhaAlt);
            var alinh = c == 2 ? XStringFormats.CenterLeft : c >= 3 ? XStringFormats.CenterRight : XStringFormats.Center;
            gfx.DrawString(totais[c], fonteBold, XBrushes.White, rect, alinh);
            x += ColsFunc[c];
        }

        // ===== DETALHAMENTO EXPLICADO DO TOTAL DO CENTRO =====
        y += linhaAlt + 16;
        (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, 180);
        // fundo destacado do bloco (cobre título + 8 linhas)
        gfx.DrawRectangle(XBrushes.Ivory, X0 - 6, y - 4, 776, 170);
        gfx.DrawRectangle(XBrushes.SteelBlue, X0 - 6, y - 4, 776, 2);
        gfx.DrawString("DETALHAMENTO DO TOTAL DO CENTRO", fonteCab, XBrushes.SteelBlue, new XPoint(X0, y));
        y += 16;

        void LinhaDetalhe(string txt, decimal val, bool bold)
        {
            (gfx, page, y) = GarantirEspaco(gfx, doc, page, y, linhaAlt + 2);
            gfx.DrawString("   " + txt, bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0, y - 8, 560, linhaAlt + 2), XStringFormats.TopLeft);
            gfx.DrawString(VerbaFinanceira.FormatarValor(val), bold ? fonteBold : fonte, XBrushes.Black, new XRect(X0 + 560 + 8, y - 8, 200 - 8, linhaAlt + 2), XStringFormats.TopRight);
            y += linhaAlt;
        }

        decimal liqTotal = funcionarios.Sum(f => f.Liquido);
        decimal provTotal = funcionarios.Sum(f => f.TotalProventos);
        decimal descTotal = funcionarios.Sum(f => f.TotalDescontos);
        decimal encTotal = funcionarios.Sum(f => f.Fgts + f.Inss + f.Irrf);
        decimal empTotal = funcionarios.Sum(f => f.TotalEmprestimos);

        LinhaDetalhe($"Líquido pago aos {funcionarios.Count} funcionários", liqTotal, true);
        LinhaDetalhe("  (+) Proventos totais da folha", provTotal, false);
        LinhaDetalhe("  (-) Descontos totais da folha", -descTotal, false);
        LinhaDetalhe("  (=) Líquido (proventos - descontos)", provTotal - descTotal, true);
        LinhaDetalhe("  (+) Encargos da empresa (FGTS + INSS + IRRF)", encTotal, false);
        LinhaDetalhe("  Empréstimos consignados (valor dos contratos)", empTotal, false);
        LinhaDetalhe("", 0, false);
        LinhaDetalhe("CUSTO TOTAL DO CENTRO (liquidado + encargos)", liqTotal + encTotal, true);

        DrawRodape(gfx, page, y);
        gfx.Dispose();
        doc.Save(caminho);
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

    private static void DrawFuncHeader(XGraphics gfx, float y, XFont fonteCab, float linhaAlt)
    {
        gfx.DrawRectangle(XBrushes.SteelBlue, X0, y, ColsFunc.Sum(), linhaAlt);
        float x = X0;
        for (int i = 0; i < HeadersFunc.Length; i++)
        {
            var alinh = i >= 3 ? XStringFormats.CenterRight : XStringFormats.Center;
            gfx.DrawString(HeadersFunc[i], fonteCab, XBrushes.White,
                new XRect(x + 2, y, ColsFunc[i] - 4, linhaAlt), alinh);
            x += ColsFunc[i];
        }
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
