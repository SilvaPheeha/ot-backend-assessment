using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OT.Assessment.Data.Entities;

namespace OT.Assessment.Data;

public partial class AppDbContext : DbContext
{
    private const string Schema = "dbo";
    private const string DateTimeOffsetColumnType = "datetimeoffset(7)";
    private const string CurrentDateTimeOffsetSql = "SYSDATETIMEOFFSET()";

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountEntity> Accounts { get; set; }
    public virtual DbSet<BetEntity> Bets { get; set; }
    public virtual DbSet<BrandEntity> Brands { get; set; }
    public virtual DbSet<TransactionEntity> Transactions { get; set; }
    public virtual DbSet<TransactionTypeEntity> TransactionTypes { get; set; }
    public virtual DbSet<UserEntity> Users { get; set; }
    public virtual DbSet<WagerEntity> Wagers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionTypeEntity>(entity =>
        {
            entity.ToTable("TransactionType", Schema);
            entity.HasKey(e => e.TransactionTypeId);
            entity.Property(e => e.TransactionTypeId).ValueGeneratedNever();
            entity.Property(e => e.TransactionType).HasMaxLength(100);
            entity.Property(e => e.TypeName).HasMaxLength(100);
            ConfigureAuditColumns(entity);
        });

        modelBuilder.Entity<BetEntity>(entity =>
        {
            entity.ToTable("Bets", Schema);
            entity.HasKey(e => e.BetId);
            entity.Property(e => e.BetId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(18, 6);
            ConfigureAuditColumns(entity);
        });

        modelBuilder.Entity<BrandEntity>(entity =>
        {
            entity.ToTable("Brands", Schema);
            entity.HasKey(e => e.BrandId);
            entity.Property(e => e.BrandId).ValueGeneratedNever();
            entity.Property(e => e.BrandName).HasMaxLength(100);
            ConfigureAuditColumns(entity);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users", Schema);
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Surname).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.IdNumber).HasMaxLength(200);
            entity.Property(e => e.IdType).HasMaxLength(200);
            entity.Property(e => e.CountryCode).HasMaxLength(10).IsFixedLength().IsUnicode(false);
            ConfigureAuditColumns(entity);
        });

        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("Accounts", Schema);
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.AccountName).HasMaxLength(200);
            entity.Property(e => e.AccountNumber).HasMaxLength(200);
            entity.Property(e => e.AccountType).HasMaxLength(200);
            entity.Property(e => e.BranchCode).HasMaxLength(200);
            ConfigureAuditColumns(entity);

            entity.HasIndex(e => new { e.AccountId, e.CreatedDateTime }, "IX_Accounts_AccountId_CreatedDateTime")
                .IsDescending(false, true);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_user_id");
        });

        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.ToTable("Transactions", Schema);
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.TransactionDate)
                .HasColumnType(DateTimeOffsetColumnType)
                .HasDefaultValueSql(CurrentDateTimeOffsetSql);
            entity.Property(e => e.Description).HasMaxLength(255).IsUnicode(false);
            ConfigureAuditColumns(entity);

            entity.HasOne(e => e.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_transactions_account_id");

            entity.HasOne(e => e.TransactionType)
                .WithMany(t => t.Transactions)
                .HasForeignKey(e => e.TransactionTypeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_transactions_transaction_type_id");
        });

        modelBuilder.Entity<WagerEntity>(entity =>
        {
            entity.ToTable("Wagers", Schema);
            entity.HasKey(e => e.WagerId);
            entity.Property(e => e.WagerId).ValueGeneratedNever();
            entity.Property(e => e.Theme).HasMaxLength(100);
            entity.Property(e => e.Provider).HasMaxLength(200);
            entity.Property(e => e.GameName).HasMaxLength(200);
            entity.Property(e => e.CountryCode).HasMaxLength(10).IsFixedLength().IsUnicode(false);
            ConfigureAuditColumns(entity);

            entity.HasIndex(e => new { e.WagerId, e.CreatedDateTime }, "IX_Wagers_WagerId_CreatedDateTime")
                .IsDescending(false, true);

            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.Wagers)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_transaction_id");

            entity.HasOne(e => e.Brand)
                .WithMany(b => b.Wagers)
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_brand_id");

            entity.HasOne(e => e.Account)
                .WithMany(a => a.Wagers)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_account_id");

            entity.HasOne(e => e.TransactionType)
                .WithMany(t => t.Wagers)
                .HasForeignKey(e => e.TransactionTypeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_transaction_type_id");

            entity.HasOne(e => e.Bet)
                .WithMany(b => b.Wagers)
                .HasForeignKey(e => e.BetId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_bet_id");
        });
    }

    private static void ConfigureAuditColumns<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.Property<DateTimeOffset>("CreatedDateTime")
            .HasColumnType(DateTimeOffsetColumnType)
            .HasDefaultValueSql(CurrentDateTimeOffsetSql);

        entity.Property<DateTimeOffset?>("UpdatedDateTime")
            .HasColumnType(DateTimeOffsetColumnType);
    }
}
