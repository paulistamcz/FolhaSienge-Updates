namespace FolhaSienge;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}

/// <summary>
/// Ajustes de layout para telas pequenas / área remota (RDP): garante que
/// nenhuma janela abra maior que a área útil, sem remover nenhuma função.
/// A janela só encolhe (nunca aumenta); a posição (CenterParent/CenterScreen)
/// é preservada. Conteúdo com âncora se reflow; o resto usa AutoScroll.
/// </summary>
public static class UiAjuste
{
    /// <summary>Calcula o tamanho cabendo na área útil (puro, testável).</summary>
    public static System.Drawing.Size AjustarTamanho(System.Drawing.Size atual,
        System.Drawing.Size minimo, System.Drawing.Size area)
    {
        int w = Math.Min(atual.Width, area.Width);
        int h = Math.Min(atual.Height, area.Height);
        w = Math.Max(w, Math.Min(minimo.Width, area.Width));
        h = Math.Max(h, Math.Min(minimo.Height, area.Height));
        return new System.Drawing.Size(Math.Max(w, 200), Math.Max(h, 150));
    }

    /// <summary>Encolhe a janela (e seu mínimo) para caber na tela atual.</summary>
    public static void CaberNaTela(Form f)
    {
        try
        {
            var area = System.Windows.Forms.Screen.FromControl(f).WorkingArea;
            if (area.Width <= 0 || area.Height <= 0)
                area = System.Windows.Forms.Screen.PrimaryScreen?.WorkingArea ?? area;
            if (area.Width <= 0 || area.Height <= 0) return;
            var a = new System.Drawing.Size(area.Width, area.Height);
            f.MinimumSize = AjustarTamanho(f.MinimumSize, new System.Drawing.Size(0, 0), a);
            f.Size = AjustarTamanho(f.Size, f.MinimumSize, a);
        }
        catch { }
    }
}