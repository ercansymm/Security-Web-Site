using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SecurityWebSite.Models;

public partial class SecurityDbContext : DbContext
{
    public SecurityDbContext()
    {
    }

    public SecurityDbContext(DbContextOptions<SecurityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Firm> Firms { get; set; }

    public virtual DbSet<Personel> Personels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WebDe> WebDes { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // => optionsBuilder.UseSqlServer("Server=ERCAN;Database=SecurityWebSite;Integrated Security=True;TrustServerCertificate=true;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasKey(e => e.Ref); 
            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
        });

        modelBuilder.Entity<Personel>(entity =>
        {
            entity.HasKey(e => e.Ref); 
            entity.ToTable("Personel");
            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Ref); 
            entity.Property(e => e.CardName).HasMaxLength(500);
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(500);
            entity.Property(e => e.UserPassword).HasMaxLength(500);
        });

        modelBuilder.Entity<WebDe>(entity =>
        {
            entity.HasKey(e => e.Ref); 
            entity.Property(e => e.CardDescription).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
