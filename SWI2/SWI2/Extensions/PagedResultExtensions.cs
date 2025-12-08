using SWI2.Controllers;
using SWI2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Extensions
{
  public static class PagedResultExtensions
  {
    public abstract class PagedResultBase
    {
      public int CurrentPage { get; set; } = 1;
      public int PageCount { get; set; }
      public int PageSize { get; set; }
      public int RowCount { get; set; }

      public int FirstRowOnPage
      {

        get { return (CurrentPage - 1) * PageSize + 1; }
      }

      public int LastRowOnPage
      {
        get { return Math.Min(CurrentPage * PageSize, RowCount); }
      }

    }

    public class PagedResult<T> : PagedResultBase where T : class
    {
      public IList<T> Results { get; set; }

      public PagedResult()
      {
        Results = new List<T>();
      }

    }
    public static PagedResult<T> GetPaged<T>(this IQueryable<T> query,
                                     int pageNumber, int pageSize) where T : class
    {
      var result = new PagedResult<T>();
      result.CurrentPage = pageNumber;
      result.PageSize = pageSize;
      if (query.Any()) result.RowCount = query.Count();
      else result.RowCount = 0;


      var pageCount = (double)result.RowCount / pageSize;
      result.PageCount = (int)Math.Ceiling(pageCount);

      var skip = pageNumber  * pageSize;
      result.Results = query.Skip(skip).Take(pageSize).ToList();

      return result;
    }
    public static IQueryable<T> ToQueryable<T>(this T instance)
    {
      return new[] { instance }.AsQueryable();
    }

  }
}
