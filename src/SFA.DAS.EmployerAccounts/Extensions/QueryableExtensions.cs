using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;
using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PaginatedList<T>> GetPagedAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            string sortColumn,
            bool isAscending = true,
            CancellationToken token = default)
        {
            int totalRecords = await query.CountAsync(token);

            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                var param = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(param, sortColumn);
                var lambda = Expression.Lambda(property, param);

                string methodName = isAscending ? "OrderBy" : "OrderByDescending";
                var orderByExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    [typeof(T), property.Type],
                    query.Expression,
                    Expression.Quote(lambda)
                );

                query = query.Provider.CreateQuery<T>(orderByExpression);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);

            return new PaginatedList<T>(items, totalRecords, pageNumber, pageSize);
        }
    }
}
