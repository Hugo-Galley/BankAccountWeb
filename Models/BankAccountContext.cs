using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace BankAccountWeb.Models;

public partial class BankAccountContext : DbContext
{
    public BankAccountContext()
    {
    }

    public BankAccountContext(DbContextOptions<BankAccountContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Compte> Comptes { get; set; }

    public virtual DbSet<Historique> Historiques { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=8889;database=BankAccount;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.39-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8_general_ci")
            .HasCharSet("utf8");

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("PRIMARY");

            entity.ToTable("Client");

            entity.HasIndex(e => e.Mail, "Mail").IsUnique();

            entity.Property(e => e.IdClient)
                .HasColumnType("int(11)")
                .HasColumnName("Id_Client");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Mail).HasMaxLength(100);
            entity.Property(e => e.Mdp)
                .HasColumnType("text")
                .HasColumnName("mdp");
            entity.Property(e => e.Nom).HasMaxLength(50);
            entity.Property(e => e.Prenom).HasMaxLength(50);
            entity.Property(e => e.Tel).HasMaxLength(15);
        });

        modelBuilder.Entity<Compte>(entity =>
        {
            entity.HasKey(e => e.IdCompte).HasName("PRIMARY");

            entity.HasIndex(e => e.IdClient, "Id_Client");

            entity.HasIndex(e => e.NumeroCompte, "Numero_Compte").IsUnique();

            entity.HasIndex(e => e.Rib, "RIB").IsUnique();

            entity.Property(e => e.IdCompte)
                .HasColumnType("int(11)")
                .HasColumnName("Id_Compte");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.IdClient)
                .HasColumnType("int(11)")
                .HasColumnName("Id_Client");
            entity.Property(e => e.Interet)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("interet");
            entity.Property(e => e.NumeroCompte)
                .HasMaxLength(20)
                .HasColumnName("Numero_Compte");
            entity.Property(e => e.Rib).HasColumnName("RIB");
            entity.Property(e => e.Solde)
                .HasPrecision(10, 2)
                .HasColumnName("solde");
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.Comptes)
                .HasForeignKey(d => d.IdClient)
                .HasConstraintName("comptes_ibfk_1");
        });

        modelBuilder.Entity<Historique>(entity =>
        {
            entity.HasKey(e => e.IdHistorique).HasName("PRIMARY");

            entity.ToTable("Historique");

            entity.HasIndex(e => e.Donneur, "Donneur");

            entity.HasIndex(e => e.Receveur, "Receveur");

            entity.Property(e => e.IdHistorique)
                .HasColumnType("int(11)")
                .HasColumnName("Id_Historique");
            entity.Property(e => e.DateOperation)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Date_Operation");
            entity.Property(e => e.Donneur).HasColumnType("int(11)");
            entity.Property(e => e.Montant).HasPrecision(10, 2);
            entity.Property(e => e.Receveur).HasColumnType("int(11)");
            entity.Property(e => e.TypeOperation)
                .HasMaxLength(50)
                .HasColumnName("Type_Operation");

            entity.HasOne(d => d.DonneurNavigation).WithMany(p => p.HistoriqueDonneurNavigations)
                .HasForeignKey(d => d.Donneur)
                .HasConstraintName("historique_ibfk_2");

            entity.HasOne(d => d.ReceveurNavigation).WithMany(p => p.HistoriqueReceveurNavigations)
                .HasForeignKey(d => d.Receveur)
                .HasConstraintName("historique_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
