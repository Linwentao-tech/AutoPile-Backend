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
        private readonly IConfiguration _configuration;

        public AutoPileManagementDbContext(DbContextOptions<AutoPileManagementDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
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