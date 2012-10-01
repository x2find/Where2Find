using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Find;
using EPiServer.Find.Helpers.Linq;
using EPiServer.Find.Api.Querying;

namespace Where2Find
{
    public static class SearchExtensions
    {
        public static Filter WhereFilter<T>(this IClient client, Expression<Func<T, bool>> condition)
        {
            var visitor = new WhereVisitor<T>();
            var filter = visitor.ParseFilter(condition, client.Conventions);
            return filter;
        }

        public static ITypeSearch<T> Where<T>(this ITypeSearch<T> search, Expression<Func<T, bool>> condition)
        {
            return search.Filter(search.Client.WhereFilter(condition));
        }
    }
}
