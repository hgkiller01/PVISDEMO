using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    public partial class DataDbContext : DbContext
    {

        public DataDbContext(DbContextOptions<DataDbContext> options)
            : base(options)
        {
            ChangeTracker.StateChanged += NeverUpdateAttribute.ChangeTracker_StateChanged;
        }

        public virtual DbSet<AccountApp> AccountApp { get; set; }
        public virtual DbSet<County> County { get; set; }
        public virtual DbSet<FormFileUpload> FormFileUpload { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<ScrapBooking> ScrapBooking { get; set; }
        public virtual DbSet<Town> Town { get; set; }
        public virtual DbSet<UserPvInfo> UserPvInfo { get; set; }
        public virtual DbSet<UserSpInfo> UserSpInfo { get; set; }
        public virtual DbSet<UserStoreAddress> UserStoreAddress { get; set; }
        public virtual DbSet<ClearLocation> ClearLocation { get; set; }
        public virtual DbSet<ScrapBookingReview> ScrapBookingReview { get; set; }
        public virtual DbSet<ApplyPvInfo> ApplyPvInfo { get; set; }
        public virtual DbSet<ApplyPvList> ApplyPvList { get; set; }
        public virtual DbSet<ApplyPvListItem> ApplyPvListItem { get; set; }
        public virtual DbSet<ModuleFacList> ModuleFacList { get; set; }
        public virtual DbSet<Customs> Customs { get; set; }
        public virtual DbSet<AuditPv> AuditPv { get; set; }
        public virtual DbSet<AuditSp> AuditSp { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<Schedule_SB> Schedule_SB { get; set; }
        public virtual DbSet<Schedule_Review> Schedule_Review { get; set; }
        public virtual DbSet<ScheduleAudit> ScheduleAudit { get; set; }
        public virtual DbSet<ScheduleAudit_SB> ScheduleAudit_SB { get; set; }
        public virtual DbSet<ScheduleAudit_Review> ScheduleAudit_Review { get; set; }
        public virtual DbSet<Cleaner> Cleaner { get; set; }
        public virtual DbSet<Treater> Treater { get; set; }
        public virtual DbSet<WasteProd> WasteProd { get; set; }
        public virtual DbSet<RecordItem> RecordItem { get; set; }
        public virtual DbSet<Record> Record { get; set; }
        public virtual DbSet<PvImport> PvImport { get; set; }
        public virtual DbSet<PvTreatment> PvTreatment { get; set; }
        public virtual DbSet<ProofSingle> ProofSingle { get; set; }
        public virtual DbSet<FileUploadErrorLog> FileUploadErrorLog { get; set; }
        public virtual DbSet<Compare_detail> Compare_Detail { get; set; }
        public virtual DbSet<Compare_CLog> Compare_CLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<AccountApp>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .IsClustered(false);

                entity.HasIndex(e => e.CreateDt)
                    .HasDatabaseName("CL_CreateDt")
                    .IsClustered(true);

                entity.Property(e => e.CreateDt)
                    .HasDefaultValueSql("(getdate())")
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.EuicNo).IsUnicode(false);

                entity.Property(e => e.TownId).IsUnicode(false).HasDefaultValueSql("('')");

                entity.Property(e => e.ModDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Tel).IsUnicode(false);

                entity.Property(e => e.ControlNo).IsUnicode(false);

                entity.Property(e => e.IsNotOwner).HasDefaultValue(false);

                entity.Property(e => e.CaseEmail).IsUnicode(false);

            });

            modelBuilder.Entity<County>(entity =>
            {
                entity.Property(e => e.CountyId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<FormFileUpload>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_FormFileUploadId")
                    .IsClustered(false);

                entity.Property(e => e.AppId)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.FileExtName)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FilePath)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Memo).HasDefaultValueSql("('')");

                entity.Property(e => e.OriginalFileName).HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<ScrapBooking>(entity =>
            {
                entity.Property(e => e.Bookingno).IsUnicode(false);

                entity.Property(e => e.Email).IsUnicode(false);



                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.Bookingno).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Uid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Appdate).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });

            modelBuilder.Entity<Town>(entity =>
            {
                entity.Property(e => e.TownId)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CountyId).IsUnicode(false);
            });

            modelBuilder.Entity<UserPvInfo>(entity =>
            {
                entity.Property(e => e.AddrType).IsUnicode(false);

                entity.Property(e => e.Bno).HasDefaultValueSql("('')");

                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.Pvno).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Uid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Createdate).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });

            modelBuilder.Entity<UserSpInfo>(entity =>
            {
                entity.Property(e => e.AlFrame).IsUnicode(false);

                entity.Property(e => e.Brand).HasDefaultValueSql("('')");
                entity.Property(e => e.Module).HasDefaultValueSql("('')");
                entity.Property(e => e.Style).IsUnicode(false).HasDefaultValueSql("('')");
                entity.Property(e => e.Hasno).IsUnicode(false);



                entity.Property(e => e.Status).IsUnicode(false);

                //entity.Property(e => e.Sno).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Uid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Createdate).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });

            modelBuilder.Entity<UserStoreAddress>(entity =>
            {
                entity.Property(e => e.AddrType).IsUnicode(false);

                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.Storeaddr).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Uid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.Createdate).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.HasKey(e => e.Pid)
                    .IsClustered(false);

                entity.HasIndex(e => e.PostDt)
                    .HasDatabaseName("CL_News");

                entity.Property(e => e.CreateUid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.CreateDt).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            });

            modelBuilder.Entity<ClearLocation>(entity =>
            {
                entity.Property(e => e.InsDT).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                entity.Property(e => e.InsUid).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });

            modelBuilder.Entity<ApplyPvInfo>(entity =>
            {
                entity.HasKey(e => new { e.SBPid, e.Pid });
            });

            modelBuilder.Entity<AuditPv>().HasNoKey();

            modelBuilder.Entity<AuditSp>().HasNoKey();

        }
    }
}
