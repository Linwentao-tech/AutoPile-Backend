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
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            builder.Property(e => e.SubTotal).HasPrecision(18, 2);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        }
    }
}