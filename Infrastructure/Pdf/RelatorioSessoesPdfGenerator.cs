using System.Globalization;
using Clinica.Application.Interfaces;
using Clinica.Domain.Enums;
using Clinica.Domain.ReadModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinica.Infrastructure.Pdf;

public class RelatorioSessoesPdfGenerator : IRelatorioSessoesPdfGenerator
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly TimeZoneInfo FusoBrasil = ObterFusoBrasil();
    private static readonly byte[] Logo = CarregarLogo();

    public byte[] Gerar(RelatorioSessoes relatorio)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Element(container => ComposeHeader(container, relatorio));
                page.Content().Element(container => ComposeContent(container, relatorio));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, RelatorioSessoes relatorio)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(40).Image(Logo).FitArea();
                row.RelativeItem().PaddingLeft(10).Column(col =>
                {
                    col.Item().Text("Instituto CIN").FontSize(16).Bold();
                    col.Item().Text("Relatório por Sessão").FontSize(11).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(150).AlignRight().Text(
                    $"Gerado em {TimeZoneInfo.ConvertTime(DateTimeOffset.Now, FusoBrasil):dd/MM/yyyy HH:mm}"
                ).FontSize(8).FontColor(Colors.Grey.Medium);
            });

            column.Item().PaddingTop(10).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Profissional: ").SemiBold();
                    text.Span(relatorio.NomeProfissional);
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Período: ").SemiBold();
                    text.Span($"{relatorio.Inicio:dd/MM/yyyy} a {relatorio.Fim:dd/MM/yyyy}");
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, RelatorioSessoes relatorio)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Spacing(14);
            column.Item().Element(c => ComposeTotais(c, relatorio.Totais));
            column.Item().Element(c => ComposeTabela(c, relatorio.Sessoes));
        });
    }

    private static void ComposeTotais(IContainer container, TotaisRelatorioSessoes totais)
    {
        container.Row(row =>
        {
            row.Spacing(8);
            row.RelativeItem().Element(c => CartaoTotal(c,
                "Sessões", totais.QuantidadeSessoes.ToString(), $"Total {FormatarValor(totais.ValorTotal)}"));
            row.RelativeItem().Element(c => CartaoTotal(c,
                "Recebido pela clínica", FormatarValor(totais.RecebidoPelaClinica), null));
            row.RelativeItem().Element(c => CartaoTotal(c,
                "Recebido pelo profissional", FormatarValor(totais.RecebidoPeloProfissional), null));
            row.RelativeItem().Element(c => CartaoTotal(c,
                "Devido ao profissional", FormatarValor(totais.DevidoAoProfissional), null));
            row.RelativeItem().Element(c => CartaoTotal(c,
                "Devido à clínica", FormatarValor(totais.DevidoAClinica), null));
        });
    }

    private static void CartaoTotal(IContainer container, string rotulo, string valor, string? rodape)
    {
        container.Background(Colors.Grey.Lighten4).Padding(8).Column(column =>
        {
            column.Item().Text(rotulo).FontSize(7).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(2).Text(valor).FontSize(12).Bold();
            if (rodape is not null)
                column.Item().PaddingTop(2).Text(rodape).FontSize(6).FontColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeTabela(IContainer container, IReadOnlyList<SessaoRelatorio> sessoes)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.2f); // Data
                columns.RelativeColumn(3f);   // Paciente
                columns.RelativeColumn(2.6f); // Serviço
                columns.RelativeColumn(1.8f); // Status
                columns.RelativeColumn(1.8f); // Valor
                columns.RelativeColumn(1.2f); // % com.
                columns.RelativeColumn(2.4f); // Recebido
                columns.RelativeColumn(2.4f); // Devido
            });

            table.Header(header =>
            {
                header.Cell().Element(CelulaCabecalho).Text("Data");
                header.Cell().Element(CelulaCabecalho).Text("Paciente");
                header.Cell().Element(CelulaCabecalho).Text("Serviço");
                header.Cell().Element(CelulaCabecalho).Text("Status");
                header.Cell().Element(CelulaCabecalho).AlignRight().Text("Valor");
                header.Cell().Element(CelulaCabecalho).AlignRight().Text("% com.");
                header.Cell().Element(CelulaCabecalho).AlignRight().Text("Recebido");
                header.Cell().Element(CelulaCabecalho).AlignRight().Text("Devido");

                static IContainer CelulaCabecalho(IContainer c) => c
                    .DefaultTextStyle(x => x.FontSize(7.5f).SemiBold().FontColor(Colors.Grey.Darken2))
                    .PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });

            if (sessoes.Count == 0)
            {
                table.Cell().ColumnSpan(8).Padding(16).AlignCenter()
                    .Text("Nenhuma sessão no período selecionado.")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
                return;
            }

            foreach (var s in sessoes)
            {
                table.Cell().Element(Celula).Text(FormatarDataHora(s.DataHoraInicio));
                table.Cell().Element(Celula).Text(s.NomePaciente);
                table.Cell().Element(Celula).Text(s.NomeServico);
                table.Cell().Element(Celula).Text(TraduzirStatus(s.Status));
                table.Cell().Element(Celula).AlignRight().Text(FormatarValor(s.ValorSessao));
                table.Cell().Element(Celula).AlignRight().Text($"{s.PercentualComissao}%");
                table.Cell().Element(Celula).AlignRight().Text(DescreverRecebido(s));
                table.Cell().Element(Celula).AlignRight().Text(DescreverDevido(s));

                static IContainer Celula(IContainer c) => c
                    .DefaultTextStyle(x => x.FontSize(7.5f))
                    .PaddingVertical(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
            }
        });
    }

    private static string DescreverRecebido(SessaoRelatorio s)
    {
        if (!s.PagoPeloPaciente) return "Não pago";
        return s.ProfissionalRecebe
            ? $"prof. {FormatarValor(s.RecebidoPeloProfissional)}"
            : $"clínica {FormatarValor(s.RecebidoPelaClinica)}";
    }

    private static string DescreverDevido(SessaoRelatorio s)
    {
        if (s.DevidoAoProfissional > 0) return $"→ prof. {FormatarValor(s.DevidoAoProfissional)}";
        if (s.DevidoAClinica > 0) return $"→ clínica {FormatarValor(s.DevidoAClinica)}";
        return "—";
    }

    private static string TraduzirStatus(StatusAgendamento status) => status switch
    {
        StatusAgendamento.AGENDADO => "Agendado",
        StatusAgendamento.REALIZADO => "Realizado",
        StatusAgendamento.FALTA => "Falta",
        StatusAgendamento.CANCELADO => "Cancelado",
        _ => status.ToString(),
    };

    private static string FormatarValor(decimal valor) => valor.ToString("C2", PtBr);

    private static string FormatarDataHora(DateTimeOffset dataHora) =>
        TimeZoneInfo.ConvertTime(dataHora, FusoBrasil).ToString("dd/MM/yyyy HH:mm", PtBr);

    private static TimeZoneInfo ObterFusoBrasil()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
    }

    private static byte[] CarregarLogo()
    {
        var assembly = typeof(RelatorioSessoesPdfGenerator).Assembly;
        using var stream = assembly.GetManifestResourceStream("Clinica.Infrastructure.Assets.logo.png")
            ?? throw new InvalidOperationException("Logo do instituto não encontrado nos recursos embutidos.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
