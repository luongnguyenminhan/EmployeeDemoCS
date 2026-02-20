using EmployeeDemo.Domain.Entities;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Account>().ToTable("Accounts");
            builder.Entity<Role>().ToTable("Roles");
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
        }

        // Product and Student removed from DbContext (entities/tables deleted)
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Student> Students { get; set; }
    }
}
