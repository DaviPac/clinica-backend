using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<PacienteProfissional> PacienteProfissionais => Set<PacienteProfissional>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<DespesaClinica> DespesasClinica => Set<DespesaClinica>();
    public DbSet<AcertoComissao> AcertosComissao => Set<AcertoComissao>();
    public DbSet<Prontuario> Prontuarios => Set<Prontuario>();
    public DbSet<ProntuarioVersao> ProntuarioVersoes => Set<ProntuarioVersao>();
    public DbSet<AnotacoesAgendamento> AnotacoesAgendamentos => Set<AnotacoesAgendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelUsuarios(modelBuilder);
        ModelPacientes(modelBuilder);
        ModelPacienteProfissional(modelBuilder);
        ModelServicos(modelBuilder);
        ModelAgendamentos(modelBuilder);
        ModelDespesasClinica(modelBuilder);
        ModelAcertosComissao(modelBuilder);
        ModelProntuarios(modelBuilder);
        ModelProntuarioVersoes(modelBuilder);
        ModelAnotacoesAgendamento(modelBuilder);
    }
    private static void ModelUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");

            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id").UseSerialColumn();

            e.Property(u => u.Nome).HasColumnName("nome").IsRequired();
            e.Property(u => u.Email).HasColumnName("email").IsRequired();
            e.Property(u => u.SenhaHash).HasColumnName("senha_hash").IsRequired();

            e.Property(u => u.Role)
             .HasColumnName("role")
             .HasConversion<string>();

            e.Property(u => u.Profissao).HasColumnName("profissao");
            e.Property(u => u.TaxaComissaoPadrao).HasColumnName("taxa_comissao_padrao");
            e.Property(u => u.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();

            e.Property(u => u.ProfissionalRecebe)
             .HasColumnName("profissional_recebe")
             .HasDefaultValue(false);

            e.Property(u => u.Ativo)
             .HasColumnName("ativo")
             .HasDefaultValue(true);

            e.HasIndex(u => u.Email).IsUnique();
        });
    }
    private static void ModelPacientes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paciente>(e =>
        {
            e.ToTable("pacientes");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(p => p.Nome)
             .HasColumnName("nome")
             .IsRequired()
             .HasMaxLength(150);

            e.Property(p => p.Cpf)
             .HasColumnName("cpf")
             .HasMaxLength(14);
             
            e.HasIndex(p => p.Cpf)
             .IsUnique();

            e.Property(p => p.Rg)
             .HasColumnName("rg")
             .HasMaxLength(20);

            e.Property(p => p.EnderecoCompleto)
             .HasColumnName("endereco_completo")
             .HasMaxLength(255);

            e.Property(p => p.Telefone)
             .HasColumnName("telefone")
             .HasMaxLength(20);

            e.Property(p => p.DataNascimento)
             .HasColumnName("data_nascimento");

            e.Property(p => p.Ativo)
             .HasColumnName("ativo")
             .HasDefaultValue(true);

            e.Property(p => p.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();
        });
    }
    private static void ModelPacienteProfissional(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PacienteProfissional>(e =>
        {
            e.ToTable("paciente_profissional");

            e.HasKey(pp => new { pp.PacienteId, pp.ProfissionalId });

            e.Property(pp => pp.PacienteId)
             .HasColumnName("paciente_id");

            e.Property(pp => pp.ProfissionalId)
             .HasColumnName("profissional_id");

            e.Property(pp => pp.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();

            e.HasOne(pp => pp.Paciente)
             .WithMany(p => p.ProfissionaisVinculados)
             .HasForeignKey(pp => pp.PacienteId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pp => pp.Profissional)
             .WithMany(u => u.PacientesAtendidos)
             .HasForeignKey(pp => pp.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    
    }
    private static void ModelServicos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Servico>(e =>
        {
            e.ToTable("servicos");

            e.HasKey(s => s.Id);
            e.Property(s => s.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(s => s.Nome)
             .HasColumnName("nome")
             .IsRequired()
             .HasMaxLength(100);

            e.Property(s => s.ProfissionalId)
             .HasColumnName("profissional_id");

            e.Property(s => s.ValorAtual)
             .HasColumnName("valor_atual")
             .HasColumnType("decimal(10, 2)")
             .IsRequired();

            e.Property(s => s.Ativo)
             .HasColumnName("ativo")
             .HasDefaultValue(true);

            e.Property(s => s.IsPacote)
             .HasColumnName("is_pacote")
             .HasDefaultValue(false);

            e.HasOne(s => s.Profissional)
             .WithMany(u => u.Servicos)
             .HasForeignKey(s => s.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ModelAgendamentos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agendamento>(e =>
        {
            e.ToTable("agendamentos");

            e.HasKey(a => a.Id);

            e.Property(a => a.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(a => a.PacienteId).HasColumnName("paciente_id");
            e.Property(a => a.ProfissionalId).HasColumnName("profissional_id");
            e.Property(a => a.ServicoId).HasColumnName("servico_id");

            e.Property(a => a.DataHoraInicio)
             .HasColumnName("data_hora_inicio")
             .HasColumnType("timestamptz")
             .IsRequired();

            e.Property(a => a.DataHoraFim)
             .HasColumnName("data_hora_fim")
             .HasColumnType("timestamptz")
             .IsRequired();

            e.Property(a => a.ValorCombinado)
             .HasColumnName("valor_combinado")
             .HasColumnType("decimal(10,2)")
             .IsRequired();

            e.Property(a => a.ValorPacote)
             .HasColumnName("valor_pacote")
             .HasColumnType("decimal(10,2)");

            e.Property(a => a.PercentualComissaoMomento)
             .HasColumnName("percentual_comissao_momento")
             .HasColumnType("decimal(5,2)")
             .IsRequired();

            e.Property(a => a.Status)
             .HasColumnName("status")
             .HasConversion<string>()
             .HasMaxLength(20)
             .IsRequired();

            e.Property(a => a.PagoPeloPaciente)
             .HasColumnName("pago_pelo_paciente")
             .HasDefaultValue(false);

            e.Property(a => a.RecorrenciaGroupId)
             .HasColumnName("recorrencia_group_id")
             .HasMaxLength(50);

            e.Property(a => a.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();

            e.Property(a => a.ProfissionalRecebe)
             .HasColumnName("profissional_recebe")
             .HasDefaultValue(false);

            e.HasOne(a => a.Paciente)
             .WithMany()
             .HasForeignKey(a => a.PacienteId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Profissional)
             .WithMany()
             .HasForeignKey(a => a.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Servico)
             .WithMany()
             .HasForeignKey(a => a.ServicoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ModelDespesasClinica(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DespesaClinica>(e =>
        {
            e.ToTable("despesas_clinica");

            e.HasKey(d => d.Id);
            e.Property(d => d.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(d => d.Descricao)
             .HasColumnName("descricao")
             .IsRequired()
             .HasMaxLength(255);

            e.Property(d => d.Valor)
             .HasColumnName("valor")
             .HasColumnType("decimal(10,2)")
             .IsRequired();

            e.Property(d => d.DataVencimento)
             .HasColumnName("data_vencimento")
             .IsRequired();

            e.Property(d => d.StatusPagamento)
             .HasColumnName("status_pagamento")
             .HasDefaultValue(false);

            e.Property(d => d.Categoria)
             .HasColumnName("categoria")
             .HasConversion<string>()
             .HasMaxLength(20)
             .IsRequired();

            e.Property(d => d.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();
        });
    }
    private static void ModelAcertosComissao(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcertoComissao>(e =>
        {
            e.ToTable("acertos_comissao");

            e.HasKey(a => a.Id);
            e.Property(a => a.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(a => a.ProfissionalId)
             .HasColumnName("profissional_id");

            e.Property(a => a.PeriodoReferencia)
             .HasColumnName("periodo_referencia")
             .HasMaxLength(7)
             .IsRequired();

            e.Property(a => a.ValorPago)
             .HasColumnName("valor_pago")
             .HasColumnType("decimal(10,2)")
             .IsRequired();

            e.Property(a => a.DataPagamento)
             .HasColumnName("data_pagamento")
             .HasColumnType("timestamptz")
             .IsRequired();

            e.Property(a => a.Observacao)
             .HasColumnName("observacao");

            e.Property(a => a.ProfissionalRecebe)
             .HasColumnName("profissional_recebe")
             .IsRequired();

            e.HasOne(a => a.Profissional)
             .WithMany()
             .HasForeignKey(a => a.ProfissionalId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ModelProntuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prontuario>(e =>
        {
            e.ToTable("prontuarios");

            e.HasKey(p => p.Id);
            e.Property(p => p.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(p => p.AgendamentoId)
             .HasColumnName("agendamento_id");

            e.Property(p => p.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();

            e.HasIndex(p => p.AgendamentoId)
             .IsUnique();

            e.HasOne(p => p.Agendamento)
             .WithOne(a => a.Prontuario)
             .HasForeignKey<Prontuario>(p => p.AgendamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ModelProntuarioVersoes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProntuarioVersao>(e =>
        {
            e.ToTable("prontuario_versoes");

            e.HasKey(v => v.Id);
            e.Property(v => v.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(v => v.ProntuarioId)
             .HasColumnName("prontuario_id");

            e.Property(v => v.Conteudo)
             .HasColumnName("conteudo")
             .HasColumnType("jsonb")
             .IsRequired();

            e.Property(v => v.AutorId)
             .HasColumnName("autor_id");

            e.Property(v => v.CriadoEm)
             .HasColumnName("criado_em")
             .HasDefaultValueSql("now()")
             .ValueGeneratedOnAdd();

            e.HasIndex(v => v.ProntuarioId);

            e.HasOne(v => v.Prontuario)
             .WithMany(p => p.Versoes)
             .HasForeignKey(v => v.ProntuarioId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Autor)
             .WithMany()
             .HasForeignKey(v => v.AutorId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
    private static void ModelAnotacoesAgendamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnotacoesAgendamento>(e =>
        {
            e.ToTable("anotacoes_agendamento");

            e.HasKey(a => a.Id);
            e.Property(a => a.Id)
             .HasColumnName("id")
             .UseSerialColumn();

            e.Property(a => a.AgendamentoId)
             .HasColumnName("agendamento_id");

            e.Property(a => a.Conteudo)
             .HasColumnName("conteudo")
             .HasColumnType("jsonb")
             .IsRequired();

            e.HasIndex(a => a.AgendamentoId)
             .IsUnique();

            e.HasOne(a => a.Agendamento)
             .WithOne(ag => ag.Anotacoes)
             .HasForeignKey<AnotacoesAgendamento>(a => a.AgendamentoId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}