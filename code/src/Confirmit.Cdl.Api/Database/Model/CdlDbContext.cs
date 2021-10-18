using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Confirmit.Cdl.Api.Database.Model
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CdlDbContext : DbContext
    {
        private static readonly LoggerFactory LoggerFactory =
            new LoggerFactory(new[]
            {
                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
            });

        public CdlDbContext()
        {
        }

        public CdlDbContext(DbContextOptions<CdlDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentAlias> Aliases { get; set; }
        public DbSet<Commit> Commits { get; set; }
        public DbSet<Revision> Revisions { get; set; }
        public DbSet<AccessedDocument> AccessedDocuments { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<CompanyPermission> CompanyPermissions { get; set; }
        public DbSet<EnduserPermission> EnduserPermissions { get; set; }
        public DbSet<EnduserListPermission> EnduserListPermissions { get; set; }
        public DbSet<SelectedEnduserList> SelectedEnduserLists { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Enduser> Endusers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<EnduserList> EnduserLists { get; set; }
        public DbSet<EnduserCompany> EnduserCompanies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(DbLib.GetConnectInfo("CmlStorage").GetConnectStringWithDatabase());
#if DEBUG
                optionsBuilder.UseLoggerFactory(LoggerFactory);
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<AccessedDocument>()
                .HasKey(e => new { e.Id, e.UserId, e.IsUser });

            modelBuilder.Entity<DocumentAlias>()
                .ToTable("Aliases");

            modelBuilder.Entity<SelectedEnduserList>()
                .HasKey(e => new { e.ListId, e.DocumentId });

            modelBuilder.Entity<UserPermission>()
                .HasKey(e => new { e.UserId, e.DocumentId });
            modelBuilder.Entity<CompanyPermission>()
                .HasKey(e => new { e.OrganizationId, e.DocumentId });
            modelBuilder.Entity<CompanyPermission>()
                .Property(e => e.OrganizationId)
                .HasColumnName("CompanyId");
            modelBuilder.Entity<EnduserPermission>()
                .HasKey(e => new { e.UserId, e.DocumentId });
            modelBuilder.Entity<EnduserListPermission>()
                .Property(e => e.OrganizationId)
                .HasColumnName("ListId");
            modelBuilder.Entity<EnduserListPermission>()
                .HasKey(e => new { e.OrganizationId, e.DocumentId });

            modelBuilder.Entity<User>()
                .Property(e => e.Name)
                .IsUnicode(false);
            modelBuilder.Entity<User>()
                .Property(e => e.FirstName)
                .IsUnicode(false);
            modelBuilder.Entity<User>()
                .Property(e => e.LastName)
                .IsUnicode(false);
            modelBuilder.Entity<User>()
                .Property(e => e.OrganizationId)
                .HasColumnName("CompanyId");

            modelBuilder.Entity<Enduser>()
                .Property(e => e.OrganizationId)
                .HasColumnName("ListId");

            modelBuilder.Entity<Company>()
                .Property(e => e.Name)
                .IsUnicode(false);
        }
    }
}
