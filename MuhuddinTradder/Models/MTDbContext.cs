using Microsoft.EntityFrameworkCore;

namespace MTDBMVC.Models
{
    public class MTDbContext : DbContext
    {
        public MTDbContext(DbContextOptions<MTDbContext> options) : base(options)
        {
        }

        public DbSet<MtItemCate> ItemCategories { get; set; }
        public DbSet<MtUnitMst> Units { get; set; }
        public DbSet<MtItmMst> Items { get; set; }
        public DbSet<MtTraderMst> Traders { get; set; }
        public DbSet<MtPurMst> Purchases { get; set; }
        public DbSet<MtPurDtl> PurchaseDetails { get; set; }
        public DbSet<MtSaleMst> Sales { get; set; }
        public DbSet<MtSaleDtl> SaleDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ================= ITEM CATEGORY =================
            modelBuilder.Entity<MtItemCate>(e =>
            {
                e.ToTable("mt_item_cate");
                e.HasKey(x => x.CatCd);
                e.Property(x => x.CatCd).HasColumnName("cat_cd").HasMaxLength(2);
                e.Property(x => x.CatDesc).HasColumnName("cat_desc").HasMaxLength(100);
                e.Property(x => x.CatStatus).HasColumnName("cat_status").HasMaxLength(1);
            });

            // ================= UNIT MASTER =================
            modelBuilder.Entity<MtUnitMst>(e =>
            {
                e.ToTable("mt_unit_mst");
                e.HasKey(x => x.UnitCd);
                e.Property(x => x.UnitCd).HasColumnName("unit_cd").HasMaxLength(2);
                e.Property(x => x.UnitDesc).HasColumnName("unit_desc").HasMaxLength(100);
                e.Property(x => x.UnitStatus).HasColumnName("unit_status").HasMaxLength(1);
            });

            // ================= ITEM MASTER =================
            modelBuilder.Entity<MtItmMst>(e =>
            {
                e.ToTable("mt_itm_mst");
                e.HasKey(x => x.ItmCd);
                e.Property(x => x.ItmCd).HasColumnName("itm_cd").HasMaxLength(4);
                e.Property(x => x.CatCd).HasColumnName("cat_cd").HasMaxLength(2);
                e.Property(x => x.ItmDesc).HasColumnName("itm_desc").HasMaxLength(100);
                e.Property(x => x.UnitCd).HasColumnName("unit_cd").HasMaxLength(2);
                e.Property(x => x.ItmStatus).HasColumnName("itm_status").HasMaxLength(1);
                e.Property(x => x.ItmShelfLife).HasColumnName("itm_shelf_life");
                e.Property(x => x.ItmMoq).HasColumnName("itm_moq").HasPrecision(18, 2);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CatCd)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Unit)
                    .WithMany()
                    .HasForeignKey(x => x.UnitCd)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= TRADER MASTER =================
            modelBuilder.Entity<MtTraderMst>(e =>
            {
                e.ToTable("mt_trader_mst");
                e.HasKey(x => x.TrdCd);
                e.Property(x => x.TrdCd).HasColumnName("trd_cd").HasMaxLength(4);
                e.Property(x => x.TrdDesc).HasColumnName("trd_desc").HasMaxLength(100);
                e.Property(x => x.TrdType).HasColumnName("trd_type").HasMaxLength(1);
                e.Property(x => x.TrdCate).HasColumnName("trd_cate").HasMaxLength(2);
                e.Property(x => x.TrdAdd).HasColumnName("trd_add").HasMaxLength(100);
                e.Property(x => x.TrdStr).HasColumnName("trd_str").HasMaxLength(50);
                e.Property(x => x.TrdNtn).HasColumnName("trd_ntn").HasMaxLength(50);
            });

            // ================= PURCHASE MASTER =================
            modelBuilder.Entity<MtPurMst>(e =>
            {
                e.ToTable("mt_pur_mst");
                e.HasKey(x => x.InvCd);
                e.Property(x => x.InvCd).HasColumnName("inv_cd").HasMaxLength(20);
                e.Property(x => x.InvDt).HasColumnName("inv_dt");
                e.Property(x => x.TrdCd).HasColumnName("trd_cd").HasMaxLength(4);
                e.Property(x => x.RcvdDt).HasColumnName("rcvd_dt");
                e.Property(x => x.AttachmentPath).HasColumnName("attachment_path").HasMaxLength(255);

                e.HasOne(x => x.Trader)
                    .WithMany()
                    .HasForeignKey(x => x.TrdCd)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= PURCHASE DETAIL =================
            modelBuilder.Entity<MtPurDtl>(e =>
            {
                e.ToTable("mt_pur_dtl");
                e.HasKey(x => new { x.InvCd, x.ItmCd });
                e.Property(x => x.InvCd).HasColumnName("inv_cd").HasMaxLength(20);
                e.Property(x => x.ItmCd).HasColumnName("itm_cd").HasMaxLength(4);
                e.Property(x => x.Dom).HasColumnName("dom");
                e.Property(x => x.Doe).HasColumnName("doe");
                e.Property(x => x.RcvgQty).HasColumnName("rcvg_qty").HasPrecision(18, 2);
                e.Property(x => x.Rate).HasColumnName("rate").HasPrecision(18, 2);
                e.Property(x => x.Disc).HasColumnName("disc").HasPrecision(18, 2);
                e.Property(x => x.Cost).HasColumnName("cost").HasPrecision(18, 2);

                e.HasOne(x => x.PurMst)
                    .WithMany(x => x.Details)
                    .HasForeignKey(x => x.InvCd)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Item)
                    .WithMany()
                    .HasForeignKey(x => x.ItmCd)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= SALE MASTER =================
            modelBuilder.Entity<MtSaleMst>(e =>
            {
                e.ToTable("mt_sale_mst");
                e.HasKey(x => x.InvCd);
                e.Property(x => x.InvCd).HasColumnName("inv_cd").HasMaxLength(20);
                e.Property(x => x.InvDt).HasColumnName("inv_dt");
                e.Property(x => x.TrdCd).HasColumnName("trd_cd").HasMaxLength(4);

                e.HasOne(x => x.Trader)
                    .WithMany()
                    .HasForeignKey(x => x.TrdCd)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= SALE DETAIL =================
            modelBuilder.Entity<MtSaleDtl>(e =>
            {
                e.ToTable("mt_sale_dtl");
                e.HasKey(x => new { x.InvCd, x.ItmCd });
                e.Property(x => x.InvCd).HasColumnName("inv_cd").HasMaxLength(20);
                e.Property(x => x.ItmCd).HasColumnName("itm_cd").HasMaxLength(4);
                e.Property(x => x.PurInv).HasColumnName("pur_inv").HasMaxLength(20);
                e.Property(x => x.PurRate).HasColumnName("pur_rate").HasPrecision(18, 2);
                e.Property(x => x.RcvgQty).HasColumnName("rcvg_qty").HasPrecision(18, 2);
                e.Property(x => x.Rate).HasColumnName("rate").HasPrecision(18, 2);
                e.Property(x => x.Disc).HasColumnName("disc").HasPrecision(18, 2);
                e.Property(x => x.Cost).HasColumnName("cost").HasPrecision(18, 2);

                e.HasOne(x => x.SaleMst)
                    .WithMany(x => x.Details)
                    .HasForeignKey(x => x.InvCd)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Item)
                    .WithMany()
                    .HasForeignKey(x => x.ItmCd)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
