using System;
using System.Collections.Generic;
using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

public partial class TeslaDbContext : DbContext
{
    public TeslaDbContext()
    {
    }

    public TeslaDbContext(DbContextOptions<TeslaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<Pool> Pools { get; set; }

    public virtual DbSet<RideRequest> RideRequests { get; set; }

    public virtual DbSet<RideStatusHistory> RideStatusHistories { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<Zone> Zones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=Dbconn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Drivers_UserId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LicenseNo).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Driver)
                .HasForeignKey<Driver>(d => d.UserId)
                .HasConstraintName("FK_Drivers_Users");
        });

        modelBuilder.Entity<Pool>(entity =>
        {
            entity.HasIndex(e => e.DriverId, "IX_Pools_DriverId");

            entity.HasIndex(e => e.Status, "IX_Pools_Status");

            entity.HasIndex(e => e.VehicleId, "IX_Pools_VehicleId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Waiting");

            entity.HasOne(d => d.Driver).WithMany(p => p.Pools)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pools_Drivers");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Pools)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pools_Vehicles");
        });

        modelBuilder.Entity<RideRequest>(entity =>
        {
            entity.HasIndex(e => e.DropoffZoneId, "IX_RideRequests_DropoffZoneId");

            entity.HasIndex(e => e.PassengerId, "IX_RideRequests_PassengerId");

            entity.HasIndex(e => e.PickupZoneId, "IX_RideRequests_PickupZoneId");

            entity.HasIndex(e => e.PoolId, "IX_RideRequests_PoolId");

            entity.HasIndex(e => e.Status, "IX_RideRequests_Status");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SeatsRequested).HasDefaultValue(1);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Waiting");

            entity.HasOne(d => d.DropoffZone).WithMany(p => p.RideRequestDropoffZones)
                .HasForeignKey(d => d.DropoffZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RideRequests_DropoffZone");

            entity.HasOne(d => d.Passenger).WithMany(p => p.RideRequests)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RideRequests_Passenger");

            entity.HasOne(d => d.PickupZone).WithMany(p => p.RideRequestPickupZones)
                .HasForeignKey(d => d.PickupZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RideRequests_PickupZone");

            entity.HasOne(d => d.Pool).WithMany(p => p.RideRequests)
                .HasForeignKey(d => d.PoolId)
                .HasConstraintName("FK_RideRequests_Pool");
        });

        modelBuilder.Entity<RideStatusHistory>(entity =>
        {
            entity.ToTable("RideStatusHistory");

            entity.HasIndex(e => e.RideRequestId, "IX_History_RideRequestId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FromStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ToStatus)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.RideRequest).WithMany(p => p.RideStatusHistories)
                .HasForeignKey(d => d.RideRequestId)
                .HasConstraintName("FK_History_RideRequest");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.RideRequestId, "IX_Transactions_RideRequestId");

            entity.HasIndex(e => e.WalletId, "IX_Transactions_WalletId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.RideRequest).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.RideRequestId)
                .HasConstraintName("FK_Transactions_RideRequest");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("FK_Transactions_Wallet");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Phone, "UQ_Users_Phone").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Passenger");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.DriverId, "IX_Vehicles_DriverId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Driver).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK_Vehicles_Drivers");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Wallets_UserId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .HasConstraintName("FK_Wallets_Users");
        });

        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Zones_Name").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
