using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoPile.DATA.Data
{
    public class AutoPileManagementDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AutoPileManagementDbContext(DbContextOptions<AutoPileManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.Property(e => e.ProductPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.ComparePrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<ProductMedia> ProductMedias { get; set; }
        public DbSet<Products> Products { get; set; }

        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<ShoppingCartItems> ShoppingCartItems { get; set; }
    }
}