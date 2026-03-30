using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

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

    public virtual DbSet<Foodpackage> Foodpackages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Userinformation> Userinformations { get; set; }

    public virtual DbSet<LabStudent> Labstudents { get; set; }

    public virtual DbSet<MenuItem> MenuItems { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<StaffAssignment> StaffAssignments { get; set; }

    public virtual DbSet<BookingMenuItem> BookingMenuItems { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PRIMARY");

            entity.ToTable("bookings");

            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("BookingID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.EventLocation)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PackageId)
                .HasMaxLength(10)
                .HasColumnName("PackageID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("UserID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Foodpackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PRIMARY");

            entity.ToTable("foodpackages");

            entity.Property(e => e.PackageId)
                .HasMaxLength(10)
                .HasColumnName("PackageID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.PackageName)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PricePerPerson).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.Property(e => e.PaymentId)
                .HasMaxLength(10)
                .HasColumnName("PaymentID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.AmountPaid).HasPrecision(10, 2);
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("BookingID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.EvidenceImage)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Userinformation>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("userinformation");

            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("UserID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserAddress)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserPhone).HasMaxLength(15);
            entity.Property(e => e.UserPassword)
                .HasMaxLength(255)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });
        modelBuilder.Entity<LabStudent>(entity =>
{
    entity.HasKey(e => e.StdID).HasName("PRIMARY"); // ระบุ Primary Key
    entity.ToTable("labstudent"); // ชื่อ Table ใน Database (ควรใช้ตัวเล็กตามที่สร้างใน SQL)

    entity.Property(e => e.StdID).HasMaxLength(10);
    entity.Property(e => e.StdPASSWORD).HasMaxLength(30);
    entity.Property(e => e.StdName).HasMaxLength(50);
    entity.Property(e => e.STDLastname).HasMaxLength(100);
});

        // MenuItems configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.MenuItemId).HasName("PRIMARY");
            entity.ToTable("menuitems");

            entity.Property(e => e.MenuItemId)
                .HasMaxLength(10)
                .HasColumnName("MenuItemID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
        });

        // Staff configuration
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PRIMARY");
            entity.ToTable("staff");

            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .HasColumnName("StaffID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Phone).HasMaxLength(15);
        });

        // StaffAssignment configuration
        modelBuilder.Entity<StaffAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PRIMARY");
            entity.ToTable("staffassignments");

            entity.Property(e => e.AssignmentId)
                .HasColumnName("AssignmentID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("BookingID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .HasColumnName("StaffID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.AssignmentDate).HasColumnType("datetime");

            entity.HasOne(e => e.Booking)
                .WithMany(b => b.StaffAssignments)
                .HasForeignKey(e => e.BookingId);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.StaffAssignments)
                .HasForeignKey(e => e.StaffId);
        });

        // BookingMenuItem configuration
        modelBuilder.Entity<BookingMenuItem>(entity =>
        {
            entity.HasKey(e => e.BookingMenuItemId).HasName("PRIMARY");
            entity.ToTable("bookingmenuitems");

            entity.Property(e => e.BookingMenuItemId)
                .HasColumnName("BookingMenuItemID");
            entity.Property(e => e.BookingId)
                .HasMaxLength(10)
                .HasColumnName("BookingID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.MenuItemId)
                .HasMaxLength(10)
                .HasColumnName("MenuItemID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);

            entity.HasOne(e => e.Booking)
                .WithMany(b => b.BookingMenuItems)
                .HasForeignKey(e => e.BookingId);

            entity.HasOne(e => e.MenuItem)
                .WithMany(m => m.BookingMenuItems)
                .HasForeignKey(e => e.MenuItemId);
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
