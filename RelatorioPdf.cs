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
    private const float X0 = 30;
    private const float AlturaMinima = 22;
    private const float Espaco = 3;

    public static void Gerar(string caminho, string competencia,
        List<(VerbaFinanceira Verba, decimal Valor)> itens,
        List<(int Codigo, string Nome, int Empregados, decimal Total)> centros)
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
        gfx.DrawString("RELATÓRIO MENSAL DE FOLHA DE PAGAMENTO - SIENGE (ENGEMAT)",
            fonteTitulo, XBrushes.Black, new XPoint(X0, y));
        y += 22;
        gfx.DrawString($"Competência: {competencia}    Empresa: ENGEMAT    Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}",
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
}
