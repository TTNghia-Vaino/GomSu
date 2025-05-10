using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GomSu.Models;

public partial class GomsuContext : DbContext
{
    public GomsuContext()
    {
    }

    public GomsuContext(DbContextOptions<GomsuContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<DanhGiaSanPham> DanhGiaSanPhams { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<LoaiSanPham> LoaiSanPhams { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=THAITUANN\\SQLEXPRESS;Initial Catalog=GOMSU;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => new { e.MaDonHang, e.MaSp }).HasName("PK__ChiTietD__DD39F0EF4D509374");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_DonHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_SanPham");
        });

        modelBuilder.Entity<DanhGiaSanPham>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGiaS__AA9515BF404E91A8");

            entity.ToTable("DanhGiaSanPham");

            entity.HasIndex(e => new { e.MaSp, e.MaTk }, "UQ_DanhGiaSanPham").IsUnique();

            entity.Property(e => e.MaDanhGia).ValueGeneratedNever();
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.NoiDung).HasMaxLength(255);

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__DanhGiaSan__MaSP__5CD6CB2B");

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.DanhGiaSanPhams)
                .HasForeignKey(d => d.MaTk)
                .HasConstraintName("FK__DanhGiaSan__MaTK__5DCAEF64");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD2B01D0C0");

            entity.ToTable("DonHang");

            entity.Property(e => e.MaDonHang).ValueGeneratedNever();
            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.MaVoucher).HasMaxLength(100);
            entity.Property(e => e.NgayDatHang).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaTk)
                .HasConstraintName("FK__DonHang__MaTK__5EBF139D");

            entity.HasOne(d => d.MaVoucherNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaVoucher)
                .HasConstraintName("FK__DonHang__MaVouch__5FB337D6");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => new { e.MaTk, e.MaSp }).HasName("PK__GioHang__5AA0A064C7673A6C");

            entity.ToTable("GioHang");

            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GioHang_SanPham");

            entity.HasOne(d => d.MaTkNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaTk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GioHang_TaiKhoan");
        });

        modelBuilder.Entity<LoaiSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoaiSp).HasName("PK__LoaiSanP__1224CA7CAC318384");

            entity.ToTable("LoaiSanPham");

            entity.HasIndex(e => e.TenLoaiSp, "UQ__LoaiSanP__F434DB4916ABEB85").IsUnique();

            entity.Property(e => e.MaLoaiSp)
                .HasMaxLength(10)
                .HasColumnName("MaLoaiSP");
            entity.Property(e => e.TenLoaiSp)
                .HasMaxLength(100)
                .HasColumnName("TenLoaiSP");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725081C2250BEC7");

            entity.ToTable("SanPham");

            entity.Property(e => e.MaSp)
                .ValueGeneratedNever()
                .HasColumnName("MaSP");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.MaLoaiSp)
                .HasMaxLength(10)
                .HasColumnName("MaLoaiSP");
            entity.Property(e => e.TenSp)
                .HasMaxLength(100)
                .HasColumnName("TenSP");

            entity.HasOne(d => d.MaLoaiSpNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaLoaiSp)
                .HasConstraintName("FK__SanPham__MaLoaiS__60A75C0F");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__27250070CEE32711");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.SoDienThoai, "UQ__TaiKhoan__0389B7BDB67A427C").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0DEBF7B7B").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D105340D37E31C").IsUnique();

            entity.Property(e => e.MaTk)
                .ValueGeneratedNever()
                .HasColumnName("MaTK");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(10);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.MaVoucher).HasName("PK__Voucher__0AAC5B116BF5BCA2");

            entity.ToTable("Voucher");

            entity.Property(e => e.MaVoucher).HasMaxLength(100);
            entity.Property(e => e.DieuKien).HasMaxLength(255);
            entity.Property(e => e.TenVoucher).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
