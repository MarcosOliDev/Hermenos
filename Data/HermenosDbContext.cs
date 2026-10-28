using Hermenos.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Data;

public sealed class HermenosDbContext(DbContextOptions<HermenosDbContext> options) : DbContext(options)
{
	public DbSet<Empresa> Empresas => Set<Empresa>();
	public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
	public DbSet<Usuario> Usuarios => Set<Usuario>();
	public DbSet<Contrato> Contratos => Set<Contrato>();
	public DbSet<LoteImportacao> LotesImportacao => Set<LoteImportacao>();
	public DbSet<DocumentoFiscal> DocumentosFiscais => Set<DocumentoFiscal>();
	public DbSet<RegistroERP> RegistrosERP => Set<RegistroERP>();
	public DbSet<Conciliacao> Conciliacoes => Set<Conciliacao>();
	public DbSet<Divergencia> Divergencias => Set<Divergencia>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Empresa>(entity =>
		{
			entity.ToTable("Empresas");
			entity.HasKey(x => x.IdEmpresa);
			entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
			entity.Property(x => x.Cnpj).HasMaxLength(18).IsRequired();
			entity.HasIndex(x => x.Cnpj).IsUnique();
			entity.HasOne<Usuario>().WithMany().HasForeignKey(x => x.CriadoPorUsuarioId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		modelBuilder.Entity<Fornecedor>(entity =>
		{
			entity.ToTable("Fornecedores");
			entity.HasKey(x => x.IdFornecedor);
			entity.Property(x => x.NomeRazaoSocial).HasMaxLength(200).IsRequired();
			entity.Property(x => x.NomeFantasia).HasMaxLength(200);
			entity.Property(x => x.DocumentoFiscal).HasMaxLength(18).IsRequired();
			entity.Property(x => x.Email).HasMaxLength(254);
			entity.Property(x => x.Telefone).HasMaxLength(20);
			entity.HasIndex(x => x.DocumentoFiscal).IsUnique();
			entity.HasOne<Usuario>().WithMany().HasForeignKey(x => x.CriadoPorUsuarioId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		modelBuilder.Entity<Usuario>(entity =>
		{
			entity.ToTable("Usuarios");
			entity.HasKey(x => x.IdUsuario);
			entity.Property(x => x.Nome).HasMaxLength(200).IsRequired();
			entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
			entity.HasIndex(x => x.Email).IsUnique();
		});

		modelBuilder.Entity<Contrato>(entity =>
		{
			entity.ToTable("Contratos");
			entity.HasKey(x => x.IdContrato);
			entity.Property(x => x.Numero).HasMaxLength(50).IsRequired();
			entity.Property(x => x.Descricao).HasMaxLength(1000).IsRequired();
			entity.HasIndex(x => new { x.IdEmpresa, x.Numero }).IsUnique();
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.IdEmpresa)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<Usuario>().WithMany().HasForeignKey(x => x.CriadoPorUsuarioId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<LoteImportacao>(entity =>
		{
			entity.ToTable("LotesImportacao");
			entity.HasKey(x => x.LoteImpId);
			entity.Property(x => x.NomeArquivo).HasMaxLength(260).IsRequired();
			entity.Property(x => x.Formato).HasMaxLength(20).IsRequired();
			entity.Property(x => x.Origem).HasMaxLength(100).IsRequired();
			entity.HasIndex(x => new { x.IdEmpresa, x.Competencia });
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.IdEmpresa)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<Contrato>().WithMany().HasForeignKey(x => x.ContratoId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RecebidoPorUsuarioId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		modelBuilder.Entity<DocumentoFiscal>(entity =>
		{
			entity.ToTable("DocumentosFiscais");
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Numero).HasMaxLength(60).IsRequired();
			entity.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
			entity.Property(x => x.ChaveAcesso).HasMaxLength(60);
			entity.Property(x => x.ValorDocumento).HasPrecision(18, 2);
			entity.HasIndex(x => x.ChaveAcesso).IsUnique();
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<Fornecedor>().WithMany().HasForeignKey(x => x.FornecedorId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<LoteImportacao>().WithMany().HasForeignKey(x => x.LoteImportacaoId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<RegistroERP>(entity =>
		{
			entity.ToTable("RegistrosERP");
			entity.HasKey(x => x.RegERPId);
			entity.Property(x => x.NumeroDocumento).HasMaxLength(60).IsRequired();
			entity.Property(x => x.Fornecedor).HasMaxLength(200).IsRequired();
			entity.Property(x => x.ValorDocumento).HasPrecision(18, 2);
			entity.Property(x => x.QuantidadeItens).HasPrecision(18, 4);
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<DocumentoFiscal>().WithMany().HasForeignKey(x => x.DocumentoFiscalId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Conciliacao>(entity =>
		{
			entity.ToTable("Conciliacoes");
			entity.HasKey(x => x.Id);
			entity.Property(x => x.RegraAplicada).HasMaxLength(500);
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<DocumentoFiscal>().WithMany().HasForeignKey(x => x.DocumentoFiscalId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<RegistroERP>().WithMany().HasForeignKey(x => x.RegistroERPId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Divergencia>(entity =>
		{
			entity.ToTable("Divergencias");
			entity.HasKey(x => x.IdDivergencia);
			entity.Property(x => x.Categoria).HasMaxLength(100).IsRequired();
			entity.Property(x => x.CampoAfetado).HasMaxLength(100).IsRequired();
			entity.Property(x => x.ValorFonteXml).HasMaxLength(1000).IsRequired();
			entity.Property(x => x.ValorFonteERP).HasMaxLength(1000).IsRequired();
			entity.Property(x => x.ResponsavelDemonstrativo).HasMaxLength(200);
			entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.IdEmpresa)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne<DocumentoFiscal>().WithMany().HasForeignKey(x => x.IdDocumentoFiscal)
				.OnDelete(DeleteBehavior.Restrict);
		});
	}
}
