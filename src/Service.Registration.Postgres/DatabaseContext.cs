using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Postgres;
using MyJetWallet.Sdk.Service;
using Service.Registration.Postgres.Models;

namespace Service.Registration.Postgres
{
public class DatabaseContext : MyDbContext
    {
        public const string Schema = "authorization";

        private const string LogsTableName = "registerlogs";
        private const string ExternalDataTableName = "externaldata";
        
        private Activity _activity;

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RegisterLogModel> RegisterLogModelDbModels { get; set; }
        public DbSet<ExternalDataEntity> ExternalDataEntities { get; set; }

        public static DatabaseContext Create(DbContextOptionsBuilder<DatabaseContext> options)
        {
            var activity = MyTelemetry.StartActivity($"Database context {Schema}")?.AddTag("db-schema", Schema);

            var ctx = new DatabaseContext(options.Options) {_activity = activity};

            return ctx;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetLogsEntry(modelBuilder);
            SetExternalDataEntry(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetLogsEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegisterLogModel>().ToTable(LogsTableName);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Id).UseIdentityColumn(); 
            modelBuilder.Entity<RegisterLogModel>().HasKey(e => e.Id);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.TraderId).HasMaxLength(512).IsRequired();
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Email).HasMaxLength(512).IsRequired();
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.RegistrationCountryCode).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Phone).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Ip).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.UserAgent).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Owner).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.RedirectedFrom).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.LandingPageId).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Language).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.CountryCodeByIp).HasMaxLength(512).IsRequired(false);
            modelBuilder.Entity<RegisterLogModel>().Property(e => e.Registered);
        }
        
        private void SetExternalDataEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalDataEntity>().ToTable(ExternalDataTableName);
            modelBuilder.Entity<ExternalDataEntity>().Property(e => e.Id);
            modelBuilder.Entity<ExternalDataEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<ExternalDataEntity>().Property(e => e.TraderId).HasMaxLength(512).IsRequired();
            modelBuilder.Entity<ExternalDataEntity>().Property(e => e.Key).HasMaxLength(512).IsRequired();
            modelBuilder.Entity<ExternalDataEntity>().Property(e => e.Value).HasMaxLength(2048).IsRequired();
            
            modelBuilder.Entity<ExternalDataEntity>().HasIndex(e => e.TraderId).IsUnique(false);
            modelBuilder.Entity<ExternalDataEntity>().HasIndex(e => new {e.TraderId, e.Key}).IsUnique(false);
        }
        
    }
}