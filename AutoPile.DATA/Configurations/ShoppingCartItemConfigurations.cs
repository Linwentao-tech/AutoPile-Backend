using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Configurations
{
    public class ShoppingCartItemConfigurations : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.Property(s => s.ProductId).HasMaxLength(24);
        }
    }
}