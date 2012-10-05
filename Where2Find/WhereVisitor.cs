using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Helpers.Reflection;

namespace Where2Find
{
    public class WhereVisitor<T> : ExpressionVisitor
    {
        private Stack<Action<Filter>> addStack = new Stack<Action<Filter>>(); 

        private IClientConventions conventions;
        public Filter ParseFilter(Expression<Func<T, bool>> expression, IClientConventions conventions)
        {
            this.conventions = conventions;
            Filter filterToReturn = null;
            addStack.Push(filter => filterToReturn = filter);
            Visit(expression);
            return filterToReturn;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Method == typeof(string).GetMethod("StartsWith", new [] {typeof(string)}))
            {
                var fieldName = GetFilterableFieldName(node.Object);
                var argument = (string) EvaluateExpression(node.Arguments[0]);
                AddFilter(new PrefixFilter(fieldName, argument));
            }
            else if (node.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) }))
            {
                string fieldName;
                var argument = (string)EvaluateExpression(node.Arguments[0]);
                var comparisonOptions = (StringComparison)EvaluateExpression(node.Arguments[1]);
                if (comparisonOptions == StringComparison.OrdinalIgnoreCase || comparisonOptions == StringComparison.CurrentCultureIgnoreCase || comparisonOptions == StringComparison.InvariantCultureIgnoreCase)
                {
                    fieldName = GetFilterableFieldName(node.Object, knownToBeLowercase: true);
                    argument = argument.ToLowerInvariant();
                }
                else
                {
                    fieldName = GetFilterableFieldName(node.Object);
                }
                AddFilter(new PrefixFilter(fieldName, argument));
            }
            else if (node.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(bool), typeof(CultureInfo) }))
            {
                string fieldName;
                var argument = (string)EvaluateExpression(node.Arguments[0]);
                var ignoreCase = (bool)EvaluateExpression(node.Arguments[1]);
                if(ignoreCase)
                {
                    fieldName = GetFilterableFieldName(node.Object, knownToBeLowercase: true);
                    argument = argument.ToLowerInvariant();
                }
                else
                {
                    fieldName = GetFilterableFieldName(node.Object);
                }
                AddFilter(new PrefixFilter(fieldName, argument));
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                var addScope = addStack.Peek();
                addStack.Push(filter => addScope(new NotFilter(filter)));
            }
            return base.VisitUnary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.GetReturnType() == typeof(bool))
            {
                var fieldName = conventions.FieldNameConvention.GetFieldName(node);
                AddFilter(CreateTermFilter(fieldName, Expression.Constant(true)));
                return node;
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            if (node.NodeType == ExpressionType.TypeIs)
            {
                var fieldName = conventions.FieldNameConvention.GetFieldName(node.Expression);
                if(!string.IsNullOrEmpty(fieldName))
                {
                    fieldName += ".";
                }
                fieldName += TypeHierarchyInterceptor.TypeHierarchyJsonPropertyName;
                var typeHiearchyFilter = new TermFilter(fieldName,
                                                        TypeHierarchyInterceptor.GetTypeName(node.TypeOperand));
                AddFilter(typeHiearchyFilter);
            }
            return base.VisitTypeBinary(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.And)
            {
                if (node.Left is BinaryExpression && node.Right is BinaryExpression)
                {
                    var leftBinary = (BinaryExpression)node.Left;
                    var rightBinary = (BinaryExpression)node.Right;
                    if (leftBinary.NodeType == ExpressionType.LessThan && rightBinary.NodeType == ExpressionType.GreaterThan)
                    {

                    }
                }
                var andFilter = new AndFilter();
                AddFilter(andFilter);
                return VisitWithFilterInScope(() => base.VisitBinary(node), filter => andFilter.Filters.Add(filter));
            }
            if (node.NodeType == ExpressionType.OrElse || node.NodeType == ExpressionType.Or)
            {
                var orFilter = new OrFilter();
                AddFilter(orFilter);
                return VisitWithFilterInScope(() => base.VisitBinary(node), filter => orFilter.Filters.Add(filter));
            }
            
            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual || NodeTypeIsSizeComparison(node.NodeType))
            {
                Expression valueExpression = null;
                var fieldName = GetFilterableFieldName(node.Left);
                
                if (fieldName != null)
                {
                    valueExpression = node.Right;
                }
                else
                {
                    fieldName = GetFilterableFieldName(node.Right);
                    valueExpression = node.Left;
                }
                if (fieldName != null)
                {
                    Filter filterToAdd = null;
                    if (node.NodeType == ExpressionType.Equal)
                    {
                        var value = EvaluateExpression(valueExpression);
                        if (value != null)
                        {
                            filterToAdd = CreateTermFilter(fieldName, value);
                        }
                        else
                        {
                            filterToAdd = new NotFilter(new ExistsFilter(fieldName));
                        }
                        
                    }
                    else if (node.NodeType == ExpressionType.NotEqual)
                    {
                        var value = EvaluateExpression(valueExpression);
                        if (value != null)
                        {
                            var termFilter = CreateTermFilter(fieldName, valueExpression);
                            filterToAdd = new NotFilter(termFilter);
                        }
                        else
                        {
                            filterToAdd = new ExistsFilter(fieldName);
                        }
                        
                    }
                    else if (node.NodeType == ExpressionType.GreaterThan)
                    {
                        filterToAdd = CreateGreaterThanFilter(fieldName, valueExpression, false);
                    }
                    else if (node.NodeType == ExpressionType.GreaterThanOrEqual)
                    {
                        filterToAdd = CreateGreaterThanFilter(fieldName, valueExpression, true);
                    }
                    else if (node.NodeType == ExpressionType.LessThan)
                    {
                        filterToAdd = CreateLessThanFilter(fieldName, valueExpression, false);
                    }
                    else if (node.NodeType == ExpressionType.LessThanOrEqual)
                    {
                        filterToAdd = CreateLessThanFilter(fieldName, valueExpression, true);
                    }

                    AddFilter(filterToAdd);
                    return node;
                }
            }
            return base.VisitBinary(node);
        }

        private string GetFilterableFieldName(Expression expression, bool knownToBeLowercase = false)
        {
            var memberExpression = expression as MemberExpression;
            if(memberExpression != null)
            {
                var objectType = memberExpression.Expression.Type;
                if(objectType == typeof(T))
                {
                    return GetFieldName(expression, knownToBeLowercase);
                }
            }
            var unaryExpression = expression as UnaryExpression;
            if(unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert && typeof(Enum).IsAssignableFrom(unaryExpression.Operand.Type))
            {
                return GetFieldName(unaryExpression, false);
            }
            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.Name == "GetType")
                {
                    var fieldPath = GetFieldName(methodCallExpression.Object, knownToBeLowercase);
                    if(!string.IsNullOrEmpty(fieldPath))
                    {
                        fieldPath += ".";
                    }
                    return fieldPath + "$type";
                }
                if(methodCallExpression.Object.Type == typeof(string))
                {
                    if(methodCallExpression.Method.Name == "ToLower" || methodCallExpression.Method.Name == "ToLowerInvariant")
                    {
                        return GetFieldName(methodCallExpression.Object, true);
                    }
                }
            }
            return null;
        }

        private string GetFieldName(Expression expression, bool lowercase)
        {
            if(lowercase)
            {
                return conventions.FieldNameConvention.GetFieldNameForLowercase(expression);
            }
            return conventions.FieldNameConvention.GetFieldName(expression);
        }

        private Expression VisitWithFilterInScope(Func<Expression> visitMethod, Action<Filter> addMethod)
        {
            addStack.Push(addMethod);
            var returnValue = visitMethod();
            addStack.Pop();
            return returnValue;
        }

        private void AddFilter(Filter filterToAdd)
        {
            addStack.Peek()(filterToAdd);
        }

        private TermFilter CreateTermFilter(string fieldName, Expression valueExpression)
        {
            var value = EvaluateExpression(valueExpression);

            return CreateTermFilter(fieldName, value);
        }

        private static object EvaluateExpression(Expression valueExpression)
        {
            var valueGetter = Expression.Lambda(valueExpression, false, null).Compile();
            var value = valueGetter.DynamicInvoke();
            return value;
        }

        private TermFilter CreateTermFilter(string fieldName, object value)
        {
            FieldFilterValue filterValue = null;

            if (value is string)
            {
                filterValue = (string) value;
            }
            else if (value is int)
            {
                filterValue = (int) value;
            }
            else if (value is double)
            {
                filterValue = (double) value;
            }
            else if (value is bool)
            {
                filterValue = (bool) value;
            }
            else if (value is DateTime)
            {
                filterValue = (DateTime) value;
            }
            else if (value is Enum)
            {
                filterValue = (Enum)value;
            }
            else if (value is Type)
            {
                var typeValue = (Type) value;
                filterValue = typeValue + ", " + typeValue.Assembly.GetName().Name;
            }

            var termFilter = new TermFilter(fieldName, filterValue);
            return termFilter;
        }

        private static bool NodeTypeIsSizeComparison(ExpressionType expressionType)
        {
            return
                expressionType == ExpressionType.LessThan
                || expressionType == ExpressionType.LessThanOrEqual
                || expressionType == ExpressionType.GreaterThan
                || expressionType == ExpressionType.GreaterThanOrEqual;
        }

        private Filter CreateGreaterThanFilter(string fieldName, Expression valueExpression, bool includeLower)
        {
            var valueGetter = Expression.Lambda(valueExpression, false, null).Compile();
            var value = valueGetter.DynamicInvoke();
            if (value is string)
            {
            }
            else if (value is int)
            {
                var rangeFilter = RangeFilter.Create(fieldName, (int)value, Int32.MaxValue);
                rangeFilter.IncludeLower = includeLower;
                return rangeFilter;
            }
            else if (value is DateTime)
            {
                var rangeFilter = RangeFilter.Create(fieldName, (DateTime)value, DateTime.MaxValue);
                rangeFilter.IncludeLower = includeLower;
                return rangeFilter;
            }
            return null;
        }

        private Filter CreateLessThanFilter(string fieldName, Expression valueExpression, bool includeUpper)
        {
            var valueGetter = Expression.Lambda(valueExpression, false, null).Compile();
            var value = valueGetter.DynamicInvoke();
            if (value is string)
            {

            }
            else if (value is int)
            {
                var rangeFilter = RangeFilter.Create(fieldName, Int32.MinValue, (int)value);
                rangeFilter.IncludeUpper = includeUpper;
                return rangeFilter;
            }
            else if (value is DateTime)
            {
                var rangeFilter = RangeFilter.Create(fieldName, DateTime.MinValue, (DateTime)value);
                rangeFilter.IncludeUpper = includeUpper;
                return rangeFilter;
            }
            return null;
        }
    }
}