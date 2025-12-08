using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWI2DB.Models.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWI2DB.Configuration
{
    public class SellDateNameConfiguration : IEntityTypeConfiguration<SellDateName>
    {
        public void Configure(EntityTypeBuilder<SellDateName> builder)
        {
            builder.HasData(
                new SellDateName
                {
                    Id =1,
                    Name = "Data wykonania us³ugi"
                },
                new SellDateName
                {
                    Id = 2,
                    Name = "Data dostawy"
                }
            );;
        }
    }
}
