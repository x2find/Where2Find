using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Api.Querying.Queries;
using FluentAssertions;
using Where2Find;
using Xunit;

//x.Tags.Contains()/Any()
//Unsupported exceptions
//x.Enum == Enum.Value

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void GivenAnEqualityExpressionWithMemberOnLeftAndIntegerOnRight_ShouldReturnATermFilterWithMemberAsFieldAndIntegerAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id == 42);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter) filter;
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithIntegerOnLeftAndMemberOnRight_ShouldAddATermFilterWithMemberAsFieldAndIntegerAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => 42 == x.Id);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter) filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithFieldIntegerOnLeftAndMemberOnRight_ShouldAddATermFilterWithMemberAsFieldAndIntegerAsValue()
        {
            var client = new Client("", "");
            var intField = 42;
            var filter = client.WhereFilter<TestData>(x => intField == x.Id);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithIntegerPropertyFromNonQueriedClassOnLeftAndMemberOnRight_ShouldAddATermFilterWithMemberAsFieldAndIntegerAsValue()
        {
            var client = new Client("", "");
            var filter = client.WhereFilter<TestData>(x => IntProperty == x.Id);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        public int IntProperty
        {
            get { return 42; }
        }

        #region Supported types to filter by equality
        [Fact]
        public void GivenAnEqualityExpressionWithMemberOnLeftAndStringOnRight_ShouldAddATermFilterWithMemberAsFieldAndStringAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name == "Hello");

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter) filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals("Hello").Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithMemberOnLeftAndDateTimeOnRight_ShouldAddATermFilterWithMemberAsFieldAndDateTimeAsValue()
        {
            var client = new Client("", "");

            var filterValue = DateTime.Now;
            var filter = client.WhereFilter<TestData>(x => x.Date == filterValue);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(filterValue).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime>(x => x.Date));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithMemberOnLeftAndNullableDateTimeOnRight_ShouldAddATermFilterWithMemberAsFieldAndNullableDateTimeAsValue()
        {
            var client = new Client("", "");

            var filterValue = DateTime.Now;
            var filter = client.WhereFilter<TestData>(x => x.NullableDate == filterValue);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(filterValue).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime?>(x => x.NullableDate));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithNullableDateTimeMemberOnLeftAndNullOnRight_ShouldAddANotFilterWithAExistsFilterWithMemberAsField()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.NullableDate == null);

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter) filter;
            notFilter.Filter.Should().BeOfType<ExistsFilter>();
            var existsFilter = (ExistsFilter) notFilter.Filter;
            existsFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime?>(x => x.NullableDate));
        }

        [Fact]
        public void GivenAnInequalityExpressionWithNullableDateTimeMemberOnLeftAndNullOnRight_ShouldAddAnExistsFilterWithMemberAsField()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.NullableDate != null);

            filter.Should().BeOfType<ExistsFilter>();
            var existsFilter = (ExistsFilter)filter;
            existsFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime?>(x => x.NullableDate));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithMemberOnLeftAndDoubleOnRight_ShouldAddATermFilterWithMemberAsFieldAndDoubleAsValue()
        {
            var client = new Client("", "");

            var filterValue = 42.42;
            var filter = client.WhereFilter<TestData>(x => x.Double == filterValue);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(filterValue).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, Double>(x => x.Double));
        }
        #endregion

        #region Boolean filters
        [Fact]
        public void GivenAnExpressionWithBooleanMember_ShouldAddATermFilterWithMemberAsFieldAndTrueAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Bool);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(true).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, bool>(x => x.Bool));
        }

        //[Fact]
        //public void GivenAnExpressionWithNegatedBooleanMember_ShouldAddATermFilterWithMemberAsFieldAndFalseAsValue()
        //{
        //    var client = new Client("", "");

        //    var filter = client.WhereFilter<TestData>(x => !x.Bool);

        //    filter.Should().BeOfType<TermFilter>();
        //    var termFilter = (TermFilter)filter;
        //    termFilter.Should().NotBeNull();
        //    termFilter.Value.Equals(false).Should().BeTrue();
        //    termFilter.Field.Should().Be(client.GetFieldName<TestData, bool>(x => x.Bool));
        //}

        [Fact]
        public void GivenAnExpressionWithNegatedBooleanMember_ShouldAddANotFilterWithATermFilterWithMemberAsFieldAndTrueAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => !x.Bool);

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter) filter;
            notFilter.Filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)notFilter.Filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(true).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, bool>(x => x.Bool));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithBooleanMemberOnLeftAndFalseOnRight_ShouldAddATermFilterWithMemberAsFieldAndFalseAsValue()
        {
            var client = new Client("", "");

            var filterValue = false;
            var filter = client.WhereFilter<TestData>(x => x.Bool == filterValue);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(filterValue).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, bool>(x => x.Bool));
        }

        [Fact]
        public void GivenAnEqualityExpressionWithBooleanMemberOnLeftAndFalseOnRightInNegatedParenthesis_ShouldAddANotFilterWithATermFilterWithMemberAsFieldAndFalseAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => !(x.Bool == false));

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter) filter;
            notFilter.Filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)notFilter.Filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Equals(false).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, bool>(x => x.Bool));
        }

        [Fact]
        public void GivenAnEqualityExpressionInNegatedParenthesis_ShouldReturnANotFilterWithATermFilter()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => !(x.Id == 42));

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter) filter;
            var termFilter = (TermFilter)notFilter.Filter;
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        [Fact]
        public void GivenAnInequalityExpressionWithMemberOnRightAndIntegerOnLeft_ShouldReturnANotFilterWithATermFilterWithMembersFieldNameAndIntegerValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id != 42);

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter)filter;
            var termFilter = (TermFilter)notFilter.Filter;
            termFilter.Value.Equals(42).Should().BeTrue();
            termFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
        }

        [Fact]
        public void GivenTwoAndAlsoedEqualityExpressionsInNegatedParenthesis_ShouldReturnANotFilterWithAnAndFilter()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => !(x.Id == 42 && x.Double == 42.42));

            filter.Should().BeOfType<NotFilter>();
            var notFilter = (NotFilter)filter;
        }
        #endregion

        #region Multiple criterias
        [Fact]
        public void GivenTwoAndedEqualityExpressions_ShouldAddAAndFilterWithTwoFilters()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id == 42 & x.Name == "Hello");

            filter.Should().BeOfType<AndFilter>();
            var andFilter = (AndFilter)filter;
            andFilter.Filters.Should().HaveCount(2);
        }

        [Fact]
        public void GivenTwoAndAlsoedEqualityExpressions_ShouldAddAAndFilterWithTwoFilters()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id == 42 && x.Name == "Hello");

            filter.Should().BeOfType<AndFilter>();
            var andFilter = (AndFilter) filter;
            andFilter.Filters.Should().HaveCount(2);
        }

        [Fact]
        public void GivenTwoOredEqualityExpressions_ShouldAddAOrFilterWithTwoFilters()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id == 42 | x.Name == "Hello");

            filter.Should().BeOfType<OrFilter>();
            var orFilter = (OrFilter) filter;
            orFilter.Filters.Should().HaveCount(2);
        }

        [Fact]
        public void GivenTwoOrElsedEqualityExpressions_ShouldAddAOrFilterWithTwoFilters()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>((x => x.Id == 42 || x.Name == "Hello"));

            filter.Should().BeOfType<OrFilter>();
            var orFilter = (OrFilter) filter;
            orFilter.Filters.Should().HaveCount(2);
        }
        #endregion

        #region Grouped criterias
        [Fact]
        public void GivenTwoOrElsedEqualityExpressionsInParenthesisAndAnAndAlsoedEqualityExpression_ShouldAddAAndFilterWithTwoFiltersOutOfWhichOneIsAnOrFilter()
        {
            var client = new Client("", "");

            var dateValue = DateTime.Now;
            var filter = client.WhereFilter<TestData>(x => (x.Id == 42 || x.Name == "Hello") && x.Date == dateValue);

            filter.Should().BeOfType<AndFilter>();
            var andFilter = (AndFilter)filter;
            andFilter.Filters.Should().HaveCount(2);
            andFilter.Filters.First().Should().BeOfType<OrFilter>();
        }
        #endregion

        #region Supported comparison operators
        [Fact]
        public void GivenALessThanExpressionWithMemberOnLeftAndIntegerOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndIntegerAsToValueAndIncludeUpperFalse()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id < 42);

            filter.Should().BeOfType<RangeFilter<int>>();
            var rangeFilter = (RangeFilter<int>)filter;
            rangeFilter.To.Equals(42).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
            rangeFilter.IncludeUpper.Should().BeFalse();
        }

        [Fact]
        public void GivenALessThanOrEqualExpressionWithMemberOnLeftAndIntegerOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndIntegerAsToValueAndIncludeUpperTrue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id <= 42);

            filter.Should().BeOfType<RangeFilter<int>>();
            var rangeFilter = (RangeFilter<int>)filter;
            rangeFilter.To.Equals(42).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
            rangeFilter.IncludeUpper.Should().BeTrue();
        }

        [Fact]
        public void GivenAGreaterThanExpressionWithMemberOnLeftAndIntegerOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndIntegerAsFromValueAndIncludeLowerFalse()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id > 42);

            filter.Should().BeOfType<RangeFilter<int>>();
            var rangeFilter = (RangeFilter<int>)filter;
            rangeFilter.From.Equals(42).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
            rangeFilter.IncludeLower.Should().BeFalse();
        }

        [Fact]
        public void GivenAGreaterThanOrEqualExpressionWithMemberOnLeftAndIntegerOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndIntegerAsFromValueAndIncludeLowerTrue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Id >= 42);

            filter.Should().BeOfType<RangeFilter<int>>();
            var rangeFilter = (RangeFilter<int>)filter;
            rangeFilter.From.Equals(42).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, int>(x => x.Id));
            rangeFilter.IncludeLower.Should().BeTrue();
        }

        [Fact]
        public void GivenALessThanExpressionWithMemberOnLeftAndDateTimeOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndDateTimeAsToValueAndIncludeUpperFalse()
        {
            var client = new Client("", "");
            var filterValue = DateTime.Now;
            var filter = client.WhereFilter<TestData>(x => x.Date < filterValue);

            filter.Should().BeOfType<RangeFilter<DateTime>>();
            var rangeFilter = (RangeFilter<DateTime>)filter;
            rangeFilter.To.Equals(filterValue).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime>(x => x.Date));
            rangeFilter.IncludeUpper.Should().BeFalse();
        }

        [Fact]
        public void GivenAGreaterThanExpressionWithMemberOnLeftAndDateTimeOnRight_ShouldReturnARangeFilterWithMemberAsFieldAndDateTimeAsFromValueAndIncludeLowerFalse()
        {
            var client = new Client("", "");
            var filterValue = DateTime.Now;
            var filter = client.WhereFilter<TestData>(x => x.Date > filterValue);

            filter.Should().BeOfType<RangeFilter<DateTime>>();
            var rangeFilter = (RangeFilter<DateTime>)filter;
            rangeFilter.From.Equals(filterValue).Should().BeTrue();
            rangeFilter.Field.Should().Be(client.GetFieldName<TestData, DateTime>(x => x.Date));
            rangeFilter.IncludeLower.Should().BeFalse();
        }
        #endregion

        #region Type filtering
        [Fact]
        public void GivenAnIsExpressionWithSubTypeOfQueriedTypeOnRight_ShouldReturnATermFilterWithTypesFieldAndTypeNameAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x is TestDataSub);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Value.Equals(TypeHierarchyInterceptor.GetTypeName(typeof(TestDataSub))).Should().BeTrue();
            termFilter.Field.Should().Be(TypeHierarchyInterceptor.TypeHierarchyJsonPropertyName);
        }

        [Fact]
        public void GivenAnIsExpressionWithMemberOnLeftAndSubTypeOfMemberTypeOnRight_ShouldReturnATermFilterWithMembersTypesFieldAndTypeNameAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Another is TestDataSub);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Value.Equals(TypeHierarchyInterceptor.GetTypeName(typeof(TestDataSub))).Should().BeTrue();
            var expectedFieldName = client.GetFieldName<TestData, TestData>(x => x.Another) + "." + TypeHierarchyInterceptor.TypeHierarchyJsonPropertyName;
            termFilter.Field.Should().Be(expectedFieldName);
        }

        [Fact]
        public void GivenATypeComparisonExpressionWithTypeOfQueriedTypeOnRight_ShouldReturnATermFilterWithTypeFieldAndTypeNameAsValue()
        {
            var client = new Client("", "");
            var typeToMatch = typeof (TestDataSub);
            var filter = client.WhereFilter<TestData>(x => x.GetType() == typeToMatch);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Value.Equals(typeToMatch + ", " + typeToMatch.Assembly.GetName().Name).Should().BeTrue();
            termFilter.Field.Should().Be("$type");
        }

        [Fact]
        public void GivenATypeComparisonExpressionWithTypeOfQueriedTypeOnLeft_ShouldReturnATermFilterWithTypeFieldAndTypeNameAsValue()
        {
            var client = new Client("", "");
            var typeToMatch = typeof(TestDataSub);
            var filter = client.WhereFilter<TestData>(x => typeToMatch == x.GetType());

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Value.Equals(typeToMatch + ", " + typeToMatch.Assembly.GetName().Name).Should().BeTrue();
            termFilter.Field.Should().Be("$type");
        }

        [Fact]
        public void GivenATypeComparisonExpressionWithGetTypeInvokedOnMemberOnLeftAndTypeOfQueriedTypeOnRight_ShouldReturnATermFilterWithMembersFieldPlusTypeFieldAndTypeNameAsValue()
        {
            var client = new Client("", "");
            var typeToMatch = typeof(TestDataSub);
            var filter = client.WhereFilter<TestData>(x => x.Another.GetType() == typeToMatch);

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            var memberFieldName = client.GetFieldName<TestData, TestData>(x => x.Another);
            termFilter.Value.ShouldEqual(typeToMatch + ", " + typeToMatch.Assembly.GetName().Name);
            termFilter.Field.Should().Be(memberFieldName + "." + "$type");
        }
        #endregion

        #region String filtering
        [Fact]
        public void GivenEqualityExpressionWithToLowerInvokedOnMemberOnLeftAndStringOnRight_ShouldAddATermFilterWithMemberAsLowercaseFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.ToLower() == "Hello");

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.ShouldEqual("Hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenEqualityExpressionWithToLowerWithCultureInfoInvokedOnMemberOnLeftAndStringOnRight_ShouldAddATermFilterWithMemberAsLowercaseFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.ToLower(CultureInfo.InvariantCulture) == "Hello");

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.ShouldEqual("Hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenEqualityExpressionWithToLowerInvariantInvokedOnMemberOnLeftAndStringOnRight_ShouldAddATermFilterWithMemberAsLowercaseFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.ToLowerInvariant() == "Hello");

            filter.Should().BeOfType<TermFilter>();
            var termFilter = (TermFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.ShouldEqual("Hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }
        #endregion

        #region String.StartsWith() filtering
        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMember_ShouldAddAPrefixFilterWithMemberAsFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello"));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithIgnoreCaseFalse_ShouldAddAPrefixFilterWithMemberAsFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", false, CultureInfo.InvariantCulture));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithIgnoreCaseTrue_ShouldAddAPrefixFilterWithMemberAsLowercaseFieldAndArgumentLowercasedAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", true, CultureInfo.InvariantCulture));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonOrdinal_ShouldAddAPrefixFilterWithMemberAsFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.Ordinal));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonCurrentCulture_ShouldAddAPrefixFilterWithMemberAsFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.CurrentCulture));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonInvariantCulture_ShouldAddAPrefixFilterWithMemberAsFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.InvariantCulture));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldName<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonOrdinalIgnoreCase_ShouldAddAPrefixFilterWithMemberAsLowercaseFieldAndArgumentLowercasedAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.OrdinalIgnoreCase));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonCurrentCultureIgnoreCase_ShouldAddAPrefixFilterWithMemberAsLowercaseFieldAndArgumentLowercasedAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.CurrentCultureIgnoreCase));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnMemberWithStringComparisonInvariantCultureIgnoreCase_ShouldAddAPrefixFilterWithMemberAsLowercaseFieldAndArgumentLowercasedAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.StartsWith("Hello", StringComparison.InvariantCultureIgnoreCase));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }

        [Fact]
        public void GivenExpressionWithStartsWithInvokedOnReturnValueFromToLowerInvokedOnMember_ShouldAddAPrefixFilterWithMemberAsLowercaseFieldAndArgumentAsValue()
        {
            var client = new Client("", "");

            var filter = client.WhereFilter<TestData>(x => x.Name.ToLower().StartsWith("Hello"));

            filter.Should().BeOfType<PrefixFilter>();
            var termFilter = (PrefixFilter)filter;
            termFilter.Should().NotBeNull();
            termFilter.Value.Should().Be("Hello");
            termFilter.Field.Should().Be(client.GetFieldNameForLowercase<TestData, string>(x => x.Name));
        }
        #endregion
    }
}
