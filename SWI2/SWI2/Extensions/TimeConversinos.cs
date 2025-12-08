using SWI2.Controllers;
using SWI2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Extensions
{
  public class TimeConversinos
  {
    public DateTime JSTimeToDateTime(long? dateto)
    {
      DateTime dateTo;
      if (dateto != null)
      {
        dateTo = new DateTime(((long)dateto * 10000) + 621355968000000000, DateTimeKind.Utc);

        if (dateTo.Hour > 0 && dateTo.Hour <= 6)
        {
          dateTo = dateTo.AddHours(dateTo.Hour * (-1));
        }
        else if (dateTo.Hour >= 18)
        {
          dateTo = dateTo.AddHours(dateTo.Hour * (-1));
          dateTo = dateTo.AddDays(1);
        }
      }
      else
      {
        dateTo = DateTime.Today;
      }
      return dateTo;
    }
  }
}
