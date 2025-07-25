using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaBiblioteca.Models;

namespace SistemaBiblioteca.Data
{
    public class BibliotecaContext : IdentityDbContext<Usuario>
    {
        public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
            : base(options)
        {
        }

        public DbSet<Livro> Livros { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela Usuarios
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");

                entity.Property(u => u.Nome)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(u => u.Perfil)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.HasIndex(u => u.PhoneNumber)
                    .IsUnique()
                    .HasFilter("[PhoneNumber] IS NOT NULL");
            });

            // Configuração da tabela Livros
            modelBuilder.Entity<Livro>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.Property(l => l.Titulo)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(l => l.Autor)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(l => l.Editora)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(l => l.AnoPublicacao)
                    .IsRequired();

                entity.Property(l => l.ISBN)
                    .HasMaxLength(13)
                    .IsRequired();

                entity.Property(l => l.QuantidadeDisponivel)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.HasIndex(l => l.ISBN)
                    .IsUnique();

                entity.HasIndex(l => l.Titulo);

                entity.HasIndex(l => l.Autor);
            });

            // Configuração da tabela Locacoes
            modelBuilder.Entity<Locacao>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasOne(l => l.Livro)
                    .WithMany(l => l.Locacoes)
                    .HasForeignKey(l => l.LivroId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Usuario)
                    .WithMany(u => u.Locacoes)
                    .HasForeignKey(l => l.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(l => l.DataRetirada)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(l => l.DataDevolucaoPrevista)
                    .IsRequired();

                entity.Property(l => l.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pendente")
                    .HasConversion<string>();

                entity.Property(l => l.Multa)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0m);

                entity.HasIndex(l => l.Status);

                entity.HasIndex(l => l.DataDevolucaoPrevista);

                entity.HasIndex(l => l.DataDevolucaoReal);
            });
        }
    }
}