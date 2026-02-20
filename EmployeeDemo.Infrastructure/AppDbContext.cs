using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Infrastructure.FluentAPIs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Role configuration (inline)
            builder.Entity<Role>().ToTable("Roles");

            // AccountRole configuration (inline)
            builder.Entity<AccountRole>().ToTable("AccountRoles");
            builder.Entity<AccountRole>().HasKey(ar => new { ar.AccountId, ar.RoleId });

            builder.Entity<AccountRole>()
                   .HasOne(ar => ar.Account)
                   .WithMany(a => a.AccountRoles)
                   .HasForeignKey(ar => ar.AccountId);

            builder.Entity<AccountRole>()
                   .HasOne(ar => ar.Role)
                   .WithMany(r => r.AccountRoles)
                   .HasForeignKey(ar => ar.RoleId);

            // Apply Fluent API configurations
            builder.ApplyConfiguration(new AccountConfig());
            builder.ApplyConfiguration(new MeetingConfig());
            builder.ApplyConfiguration(new MeetingParticipantConfig());
        }

        // Product and Student removed from DbContext (entities/tables deleted)
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Student> Students { get; set; }
    }
}
