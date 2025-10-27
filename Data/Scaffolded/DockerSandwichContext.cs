using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BackOfTheHouse.Data.Scaffolded;

public partial class DockerSandwichContext : DbContext
{
    public DockerSandwichContext()
    {
    }

    public DockerSandwichContext(DbContextOptions<DockerSandwichContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Sandwich> Sandwiches { get; set; }

    public virtual DbSet<Bread> Breads { get; set; }

    public virtual DbSet<Cheese> Cheeses { get; set; }

    public virtual DbSet<Dressing> Dressings { get; set; }

    public virtual DbSet<Meat> Meats { get; set; }

    public virtual DbSet<Topping> Toppings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Avoid embedding a connection string in source. Prefer DI configuration or an environment variable.
        if (optionsBuilder.IsConfigured) return;

        var conn = Environment.GetEnvironmentVariable("DOCKER_DB_CONNECTION");
        if (!string.IsNullOrEmpty(conn))
        {
            optionsBuilder.UseSqlServer(conn);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sandwich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sandwich__3214EC074E6AD3E1");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            // OwnerUserId maps to owner_user_id column in Docker DB when present
            entity.Property<int?>("OwnerUserId").HasColumnName("owner_user_id");
        });

        modelBuilder.Entity<Bread>(entity =>
        {
            entity.ToTable("tb_bread");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Cheese>(entity =>
        {
            entity.ToTable("tb_cheese");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Dressing>(entity =>
        {
            entity.ToTable("tb_dressing");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Meat>(entity =>
        {
            entity.ToTable("tb_meat");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Topping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tb_veggies");

            entity.ToTable("tb_topping");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
