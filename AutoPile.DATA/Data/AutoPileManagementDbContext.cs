using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPile.DATA.Configurations;
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
            modelBuilder.ApplyConfiguration(new OrderConfigurations());
            modelBuilder.ApplyConfiguration(new OrderItemConfigurations());

            modelBuilder.ApplyConfiguration(new ShoppingCartItemConfigurations());

            modelBuilder.Entity<Order>().HasMany(o => o.OrderItems).WithOne(o => o.Order).HasForeignKey(o => o.OrderId);

            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.Orders).WithOne(u => u.User).HasForeignKey(u => u.UserId);

            modelBuilder.Entity<ApplicationUser>().HasMany(u => u.ShoppingCartItems).WithOne(u => u.User).HasForeignKey(u => u.UserId);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}