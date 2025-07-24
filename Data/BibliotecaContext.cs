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

            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.Livro)
                .WithMany(b => b.Locacoes)
                .HasForeignKey(l => l.LivroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.Locacoes)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        }
    }
}