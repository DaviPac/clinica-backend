using Clinica.Domain.ReadModels;

namespace Clinica.Application.Interfaces;

public interface IRelatorioSessoesPdfGenerator
{
    byte[] Gerar(RelatorioSessoes relatorio);
}
