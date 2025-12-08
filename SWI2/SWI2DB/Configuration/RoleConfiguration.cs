using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWI2DB.Configuration
{
  public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
  {
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
      /*builder.HasData(
          new IdentityRole
          {
            Name = "Client",
            NormalizedName= "CLIENT"
          },
          new IdentityRole
          {
            Name = "ArchivedClient",
            NormalizedName = "ARCHIVEDCLIENT"

          },
          new IdentityRole
          {
            Name = "Employee",
            NormalizedName = "EMPLOYEE"

          },
          new IdentityRole
          {
            Name = "ArchivedEmployee",
            NormalizedName = "ARCHIVEDEMPLOYEE"

          },
          new IdentityRole
          {
            Name = "Administrator",
            NormalizedName = "ADMINISTRATOR"

          },
          new IdentityRole
          {
            Name = "Guest",
            NormalizedName = "GUEST"

          }
      );*/
    }
  }
}
