using EmployeeDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeDemo.Infrastructure.FluentAPIs
{
    public class AccountConfig : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            // Account table
            builder.ToTable("Accounts");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            // Core fields
            builder.Property(a => a.Email)
                   .HasMaxLength(256);

            builder.Property(a => a.UserName)
                   .HasMaxLength(256);

            // Authentication fields
            builder.Property(a => a.PasswordHash)
                   .IsRequired();

            builder.Property(a => a.SecurityStamp)
                   .HasMaxLength(256);

            builder.Property(a => a.EmailConfirmed)
                   .HasDefaultValue(false);

            // Audit trail (inherited from BaseEntity)
            builder.Property(a => a.CreationDate).IsRequired();
            builder.Property(a => a.IsDeleted).HasDefaultValue(false);

            // Relationships
            // AccountRoles (1:N)
            builder.HasMany(a => a.AccountRoles)
                   .WithOne(ar => ar.Account)
                   .HasForeignKey(ar => ar.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            // MeetingsAsHost (1:N)
            builder.HasMany(a => a.MeetingsAsHost)
                   .WithOne(m => m.Host)
                   .HasForeignKey(m => m.HostId)
                   .OnDelete(DeleteBehavior.Restrict);

            // MeetingParticipations (1:N to MeetingParticipant)
            builder.HasMany(a => a.MeetingParticipations)
                   .WithOne(mp => mp.Account)
                   .HasForeignKey(mp => mp.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(a => a.Email).IsUnique(false);
            builder.HasIndex(a => a.UserName).IsUnique(false);
        }
    }
}
