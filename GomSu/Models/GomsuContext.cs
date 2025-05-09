using Microsoft.EntityFrameworkCore;

namespace GomSu.Models
{
    public partial class GOMSUContext : DbContext
    {
        public GOMSUContext()
        {
        }

        public GOMSUContext(DbContextOptions<GOMSUContext> options)
            : base(options)
        {
        }

        public virtual DbSet<SanPham> SanPham { get; set; }
        public virtual DbSet<LoaiSanPham> LoaiSanPham { get; set; }
        public virtual DbSet<DanhGiaSanPham> DanhGiaSanPham { get; set; }
        public virtual DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
        public virtual DbSet<DonHang> DonHang { get; set; }
        public virtual DbSet<GioHang> GioHangs { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoan { get; set; }
        public virtual DbSet<Voucher> Voucher { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=THAITUANN\\SQLEXPRESS;Database=GOMSU;Trusted_Connection=True;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.ToTable("ChiTietDonHang");
                entity.HasKey(c => new { c.MaDonHang, c.MaSP });
                entity.HasOne(c => c.MaDonHangNavigation)
                      .WithMany(d => d.ChiTietDonHangs)
                      .HasForeignKey(c => c.MaDonHang)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.MaSanPhamNavigation)
                      .WithMany(s => s.ChiTietDonHangs)
                      .HasForeignKey(c => c.MaSP)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.ToTable("TaiKhoan");
                entity.HasKey(t => t.MaTK);
                entity.HasMany(t => t.DonHangs)
                      .WithOne(d => d.MaKhachHangNavigation)
                      .HasForeignKey(d => d.MaKhachHang);
                entity.HasMany(t => t.GioHangs)
                      .WithOne(g => g.MaKhachHangNavigation)
                      .HasForeignKey(g => g.MaKhachHang);
                entity.HasMany(t => t.DanhGiaSanPhams)
                      .WithOne(d => d.MaTKNavigation)
                      .HasForeignKey(d => d.MaTK);
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.ToTable("DonHang");
                entity.HasKey(d => d.MaDonHang);
                entity.HasOne(d => d.MaVoucherNavigation)
                      .WithMany(v => v.DonHangs)
                      .HasForeignKey(d => d.MaVoucher);
                entity.HasMany(d => d.ChiTietDonHangs)
                      .WithOne(c => c.MaDonHangNavigation)
                      .HasForeignKey(c => c.MaDonHang);
            });

            modelBuilder.Entity<LoaiSanPham>(entity =>
            {
                entity.ToTable("LoaiSanPham");
                entity.HasKey(l => l.MaLoaiSP);
                entity.HasMany(l => l.SanPhams)
                      .WithOne(s => s.MaLoaiSPNavigation)
                      .HasForeignKey(s => s.MaLoaiSP);
            });

            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.ToTable("SanPham");
                entity.HasKey(s => s.MaSP);
                entity.HasMany(s => s.DanhGiaSanPhams)
                      .WithOne(d => d.MaSPNavigation)
                      .HasForeignKey(d => d.MaSP);
                entity.HasMany(s => s.GioHangs)
                      .WithOne(g => g.MaSPNavigation)
                      .HasForeignKey(g => g.MaSP);
                entity.HasMany(s => s.ChiTietDonHangs)
                      .WithOne(c => c.MaSanPhamNavigation)
                      .HasForeignKey(c => c.MaSP);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Voucher");
                entity.HasKey(v => v.MaVoucher);
                entity.Property(v => v.MaVoucherNavigation).HasColumnName("MaVoucherNavigation");
            });

            modelBuilder.Entity<DanhGiaSanPham>(entity =>
            {
                entity.ToTable("DanhGiaSanPham");
                entity.HasKey(d => d.MaDanhGia);
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.ToTable("GioHang");
                entity.HasKey(g => new { g.MaKhachHang, g.MaSP });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}