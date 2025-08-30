using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SecurityWebSite.Models;

public partial class SecurityWebSiteContext : DbContext
{
    public SecurityWebSiteContext()
    {
    }

    public SecurityWebSiteContext(DbContextOptions<SecurityWebSiteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Firm> Firms { get; set; }

    public virtual DbSet<Personel> Personels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WebDe> WebDes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ERCAN;Database=SecurityWebSite;Integrated Security=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
        });

        modelBuilder.Entity<Personel>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Personel");

            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.image).HasColumnName("image");
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(500);
            entity.Property(e => e.UserPassword).HasMaxLength(500);
        });

        modelBuilder.Entity<WebDe>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.CardDescription).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
