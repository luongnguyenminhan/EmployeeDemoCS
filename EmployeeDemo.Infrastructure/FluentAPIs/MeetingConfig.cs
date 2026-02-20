using EmployeeDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeDemo.Infrastructure.FluentAPIs
{
    public class MeetingConfig : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            // Meeting table
            builder.ToTable("Meetings");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();

            // Required fields
            builder.Property(m => m.Title)
                   .IsRequired()
                   .HasMaxLength(255);
            
            builder.Property(m => m.Description)
                   .HasMaxLength(1000);

            builder.Property(m => m.StartTime).IsRequired();
            builder.Property(m => m.EndTime).IsRequired();

            // Host FK relationship (1 Host : Many Meetings)
            builder.HasOne(m => m.Host)
                   .WithMany(a => a.MeetingsAsHost)
                   .HasForeignKey(m => m.HostId)
                   .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

            // Participants relationship (M2M through MeetingParticipant)
            builder.HasMany(m => m.Participants)
                   .WithOne(mp => mp.Meeting)
                   .HasForeignKey(mp => mp.MeetingId)
                   .OnDelete(DeleteBehavior.Cascade);  // Delete participants if meeting deleted

            // Audit trail (inherited from BaseEntity)
            builder.Property(m => m.CreationDate).IsRequired();
            builder.Property(m => m.IsDeleted).HasDefaultValue(false);
        }
    }

    public class MeetingParticipantConfig : IEntityTypeConfiguration<MeetingParticipant>
    {
        public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
        {
            // Join table
            builder.ToTable("MeetingParticipants");
            builder.HasKey(mp => new { mp.MeetingId, mp.AccountId });

            // FK relationships
            builder.HasOne(mp => mp.Meeting)
                   .WithMany(m => m.Participants)
                   .HasForeignKey(mp => mp.MeetingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mp => mp.Account)
                   .WithMany(a => a.MeetingParticipations)
                   .HasForeignKey(mp => mp.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Metadata
            builder.Property(mp => mp.IsHost).HasDefaultValue(false);

            // Indexes for query optimization
            builder.HasIndex(mp => mp.MeetingId);
            builder.HasIndex(mp => mp.AccountId);
            builder.HasIndex(mp => new { mp.MeetingId, mp.IsHost });  // For finding host of meeting
        }
    }
}
