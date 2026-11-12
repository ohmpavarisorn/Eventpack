using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EventPack.Models.Db;

public partial class Csi402projectContext : DbContext
{
    public Csi402projectContext()
    {
    }

    public Csi402projectContext(DbContextOptions<Csi402projectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Userinformation> Userinformations { get; set; }

    public virtual DbSet<MenuItem> MenuItems { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var mysql = Database.IsMySql();
        if (mysql) modelBuilder.UseCollation("utf8mb4_general_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.ToTable("bookings");

            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("BookingID");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(20);
            entity.Property(e => e.EventDate).HasColumnType(mysql ? "datetime" : "timestamp without time zone");
            entity.Property(e => e.EventLocation)
                .HasMaxLength(255);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("UserID");
            entity.Property(e => e.MenuItemsJson)
                .HasColumnType(mysql ? "longtext" : "text");
            
            // Foreign key relationship to Userinformation
            entity.HasOne(d => d.UserIdNavigation)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("bookings_ibfk_1");
        });

        modelBuilder.Entity<Userinformation>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("userinformation");

            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("UserID");
            entity.Property(e => e.FullName)
                .HasMaxLength(50);
            entity.Property(e => e.UserAddress)
                .HasMaxLength(255);
            entity.Property(e => e.UserPhone).HasMaxLength(15);
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("customer");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.MenuItemId);
            entity.ToTable("menuitems");

            entity.Property(e => e.MenuItemId)
                .HasMaxLength(10)
                .HasColumnName("MenuItemID");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100);
            entity.Property(e => e.Category)
                .HasMaxLength(50);
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
        });

        if (mysql)
        {
            modelBuilder.Entity<Booking>().HasKey(e => e.BookingId).HasName("PRIMARY");
            modelBuilder.Entity<Userinformation>().HasKey(e => e.UserId).HasName("PRIMARY");
            modelBuilder.Entity<MenuItem>().HasKey(e => e.MenuItemId).HasName("PRIMARY");
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetProperties()))
            {
                if (property.ClrType == typeof(string) && property.Name != nameof(Userinformation.UserPhone) &&
                    property.DeclaringType.ClrType != typeof(MenuItem))
                {
                    property.SetCollation("utf8mb3_general_ci");
                    property.SetCharSet("utf8mb3");
                }
            }
        }
        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
