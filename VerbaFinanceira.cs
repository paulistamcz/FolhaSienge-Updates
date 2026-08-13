using System.Globalization;

namespace FolhaSienge;

/// <summary>
/// Verba da folha conforme a tabela_financeira.xlsx (configuração do Sienge).
/// O campo Codigo é o código de verba cadastrado no Sienge (coluna A do CSV).
/// </summary>
public class VerbaFinanceira
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = "";
    public string PlanoFinanceiro { get; set; } = "";
    public string Credor { get; set; } = "";
    public string CredorCodigo { get; set; } = "";
    public string Documento { get; set; } = "";
    public string FormaPagamento { get; set; } = "";

    public string Display => $"{Codigo} - {Descricao}";

    public static List<VerbaFinanceira> Lista() => new()
    {
        new() { Codigo = 1,    Descricao = "SALARIO CONTRATUAL - exceto ferias e rescisao", PlanoFinanceiro = "2.02.01.02", Credor = "FOLHA", CredorCodigo = "391", Documento = "FL", FormaPagamento = "Pagamento de Salário" },
        new() { Codigo = 2,    Descricao = "INSS", PlanoFinanceiro = "2.01.02.10", Credor = "MINISTERIO DA FAZENDA", CredorCodigo = "298", Documento = "DWEB", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 58,   Descricao = "FGTS - NORMAL", PlanoFinanceiro = "2.01.02.11", Credor = "CAIXA ECONOMICA FEDERAL", CredorCodigo = "", Documento = "FGTS", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 3,    Descricao = "FERIAS", PlanoFinanceiro = "2.01.02.02", Credor = "FERIAS", CredorCodigo = "1224", Documento = "RF", FormaPagamento = "Pagamento de Salário" },
        new() { Codigo = 4,    Descricao = "RESCISAO", PlanoFinanceiro = "2.01.02.02", Credor = "RESCISAO", CredorCodigo = "", Documento = "1", FormaPagamento = "Pagamento de Salário" },
        new() { Codigo = 59,   Descricao = "GRRF", PlanoFinanceiro = "2.01.02.13", Credor = "CAIXA ECONOMICA FEDERAL", CredorCodigo = "37", Documento = "GRRF", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 5,    Descricao = "PLR", PlanoFinanceiro = "2.01.02.01", Credor = "FOLHA", CredorCodigo = "", Documento = "FL", FormaPagamento = "Pagamento de Salário" },
        new() { Codigo = 7,    Descricao = "SESI", PlanoFinanceiro = "2.02.02.24", Credor = "SESI - AL", CredorCodigo = "", Documento = "FAT", FormaPagamento = "Boletos Bancário (outros bancos)" },
        new() { Codigo = 8,    Descricao = "SENAI", PlanoFinanceiro = "2.02.02.25", Credor = "SENAI - AL", CredorCodigo = "", Documento = "FAT", FormaPagamento = "Boletos Bancário (outros bancos)" },
        new() { Codigo = 10,   Descricao = "IRRF 0561", PlanoFinanceiro = "2.01.02.16", Credor = "MINISTERIO DA FAZENDA", CredorCodigo = "", Documento = "0561", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 11,   Descricao = "DARF-DCTFWEB", PlanoFinanceiro = "2.01.02.10", Credor = "MINISTERIO DA FAZENDA", CredorCodigo = "", Documento = "DWEB", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 12,   Descricao = "VALE TRANSPORTE", PlanoFinanceiro = "2.01.02.03", Credor = "SINDICATO DAS EMPRESAS DE TRANSPORTE URBANO DE PASSAG. DO MUNIC. DE MACEIO", CredorCodigo = "", Documento = "MA", FormaPagamento = "Boletos Bancário (outros bancos)" },
        new() { Codigo = 6,    Descricao = "ADTO DE SALARIO CONTRATUAL - ferias e rescisao", PlanoFinanceiro = "2.01.02.02", Credor = "JOSE GERALDO VILLELA CURTY", CredorCodigo = "", Documento = "FL", FormaPagamento = "Pagamento de Salário" },
        new() { Codigo = 9,    Descricao = "INSS ADM", PlanoFinanceiro = "2.02.01.10", Credor = "MINISTERIO DA FAZENDA", CredorCodigo = "", Documento = "DWEB", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 20,   Descricao = "SENAI DN", PlanoFinanceiro = "2.02.02.25", Credor = "SERVICO NACIONAL DE APRENDIZAGEM INDUSTRIAL SENAI", CredorCodigo = "", Documento = "FAT", FormaPagamento = "Boletos Bancário (outros bancos)" },
        new() { Codigo = 1800, Descricao = "PIS CUMULATIVO", PlanoFinanceiro = "2.04.01", Credor = "PIS", CredorCodigo = "", Documento = "APUR", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 1801, Descricao = "COFINS CUMULATIVA", PlanoFinanceiro = "2.04.02", Credor = "COFINS", CredorCodigo = "", Documento = "APUR", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 18021, Descricao = "IRPJ ESTIMATIVA", PlanoFinanceiro = "2.04.38", Credor = "IRPJ", CredorCodigo = "", Documento = "DARF", FormaPagamento = "CEF Tributos / contas de consumo" },
        new() { Codigo = 18031, Descricao = "CSLL ESTIMATIVA", PlanoFinanceiro = "2.04.39", Credor = "CSLL", CredorCodigo = "", Documento = "DARF", FormaPagamento = "CEF Tributos / contas de consumo" },
    };

    /// <summary>Encontra a verba pelo código, ou a primeira (FOLHA) se não achar.</summary>
    public static VerbaFinanceira PorCodigo(int codigo)
    {
        return Lista().FirstOrDefault(v => v.Codigo == codigo) ?? Lista()[0];
    }

    public static string FormatarValor(decimal v) =>
        v.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
}
