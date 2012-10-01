using System;
using System.Linq.Expressions;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using Xunit.Sdk;

namespace Tests
{
    public static class Helper
    {
        public static string GetFieldName<T, TField>(this IClient client, Expression<Func<T, TField>> expression)
        {
            return client.Conventions.FieldNameConvention.GetFieldName(expression);
        }

        public static string GetFieldNameForLowercase<T, TField>(this IClient client, Expression<Func<T, TField>> expression)
        {
            return client.Conventions.FieldNameConvention.GetFieldNameForLowercase(expression);
        }

        public static void ShouldEqual(this FieldFilterValue actual, string expected)
        {
            if(!actual.Equals(expected))
            {
                throw new AssertException("Expected \"" + expected + "\" to equal " + actual);
            }
        }
    }
}