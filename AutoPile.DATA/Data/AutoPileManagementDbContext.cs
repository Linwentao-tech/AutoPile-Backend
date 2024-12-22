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

            modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOne(c => c.Category).HasForeignKey(c => c.CategoryId);
            modelBuilder.Entity<Order>().HasMany(o => o.OrderItems).WithOne(o => o.Order).HasForeignKey(o => o.OrderId);
            modelBuilder.Entity<Product>().HasMany(p => p.OrderItems).WithOne(p => p.Product).HasForeignKey(p => p.ProductId);
            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Orders).WithOne(u => u.User).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<Product>().HasMany(p => p.ProductMedias).WithOne(p => p.Product).HasForeignKey(p => p.ProductId);
            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Reviews).WithOne(u => u.User).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<Product>().HasMany(p => p.Reviews).WithOne(p => p.Product).HasForeignKey(p => p.ProductId);
            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.ShoppingCartItems).WithOne(u => u.User).HasForeignKey(u => u.UserId);
            modelBuilder.Entity<Product>().HasMany(p => p.ShoppingCartItems).WithOne(p => p.Product).HasForeignKey(p => p.ProductId);

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.ProductPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.ComparePrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductMedia> ProductMedias { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}