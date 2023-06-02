using Signum.DynamicQuery.Tokens;

namespace Signum.Authorization.ActiveDirectory.Azure;

public class MicrosoftGraphQueryConverter
{
    public virtual string[]? GetOrderBy(IEnumerable<Order> orders)
    {
        return orders.Select(c => ToGraphField(c.Token) + " " + (c.OrderType == OrderType.Ascending ? "asc" : "desc")).ToArray();
    }

    public virtual string ToGraphField(QueryToken token)
    {
        var field = token.Follow(a => a.Parent).Reverse().ToString(a => a.Key.FirstLower(), "/");

        return field;
    }

    public virtual string ToStringValue(object? value)
    {
        return value is string str ? $"'{str}'" :
            value is DateOnly date ? $"{date.ToIsoString()}" :
            value is DateTime dt ? $"{dt.ToIsoString()}" :
            value is DateTimeOffset dto ? $"{dto.DateTime.ToIsoString()}" :
            value is Guid guid ? $"'{guid}'" :
            value is bool b ? b.ToString().ToLower() :
            value?.ToString() ?? "";
    }

    public virtual string? GetFilters(List<Filter> filters)
    {
        return filters.Select(f => ToFilter(f)).Combined(FilterGroupOperation.And);
    }

    public virtual string? ToFilter(Filter f)
    {
        if (f is FilterCondition fc)
        {
            return fc.Operation switch
            {
                FilterOperation.EqualTo => ToGraphField(fc.Token) + " eq " + ToStringValue(fc.Value),
                FilterOperation.DistinctTo => ToGraphField(fc.Token) + " ne " + ToStringValue(fc.Value),
                FilterOperation.GreaterThan => ToGraphField(fc.Token) + " gt " + ToStringValue(fc.Value),
                FilterOperation.GreaterThanOrEqual => ToGraphField(fc.Token) + " ge " + ToStringValue(fc.Value),
                FilterOperation.LessThan => ToGraphField(fc.Token) + " lt " + ToStringValue(fc.Value),
                FilterOperation.LessThanOrEqual => ToGraphField(fc.Token) + " le " + ToStringValue(fc.Value),
                FilterOperation.Contains => null,
                FilterOperation.NotContains => "NOT (" + ToGraphField(fc.Token) + ":" + ToStringValue(fc.Value) + ")",
                FilterOperation.StartsWith => "startswith(" + ToGraphField(fc.Token) + "," + ToStringValue(fc.Value) + ")",
                FilterOperation.EndsWith => "endswith(" + ToGraphField(fc.Token) + "," + ToStringValue(fc.Value) + ")",
                FilterOperation.NotStartsWith => "not startswith(" + ToGraphField(fc.Token) + "," + ToStringValue(fc.Value) + ")",
                FilterOperation.NotEndsWith => "not endswith(" + ToGraphField(fc.Token) + "," + ToStringValue(fc.Value) + ")",
                FilterOperation.IsIn => "(" + ((object[])fc.Value!).ToString(a => ToGraphField(fc.Token) + " eq " + ToStringValue(a), " OR ") + ")",
                FilterOperation.IsNotIn => "not (" + ((object[])fc.Value!).ToString(a => ToGraphField(fc.Token) + " eq " + ToStringValue(a), " OR ") + ")",
                FilterOperation.Like or
                FilterOperation.NotLike or
                _ => throw new InvalidOperationException(fc.Operation + " is not implemented in Microsoft Graph API")
            };
        }
        else if (f is FilterGroup fg)
        {
            return fg.Filters.Select(f2 => ToFilter(f2)).Combined(fg.GroupOperation);
        }
        else
            throw new UnexpectedValueException(f);
    }

    public virtual string? GetSearch(List<Filter> filters)
    {
        return filters.Select(f => ToSearch(f)).Combined(FilterGroupOperation.And);
    }

    public virtual string? ToSearch(Filter f)
    {
        if (f is FilterCondition fc)
        {
            return fc.Operation switch
            {
                FilterOperation.Contains => "\"" + ToGraphField(fc.Token) + ":" + fc.Value?.ToString()?.Replace(@"""", @"\""") + "\"",
                _ => null
            };
        }
        else if (f is FilterGroup fg)
        {
            return fg.Filters.Select(f2 => ToSearch(f2)).Combined(fg.GroupOperation);
        }
        else
            throw new UnexpectedValueException(f);
    }




    public virtual string[]? GetSelect(IEnumerable<Column> columns)
    {
        return columns.Select(c => ToGraphField(c.Token)).Distinct().ToArray();
    }

    public virtual int? GetTop(Pagination pagination)
    {
        var top = pagination switch
        {
            Pagination.All => (int?)null,
            Pagination.Firsts f => f.TopElements,
            Pagination.Paginate p => p.ElementsPerPage * p.CurrentPage,
            _ => throw new UnexpectedValueException(pagination)
        };

        return top;
    }
}

public static class MicrosoftGraphConverterExtensions
{
    public static string? Combined(this IEnumerable<string?> filterEnumerable, FilterGroupOperation groupOperation)
    {
        var filters = filterEnumerable.ToList();
        var cleanFilters = filters.NotNull().ToList();

        if (groupOperation == FilterGroupOperation.And)
        {
            if (cleanFilters.IsEmpty())
                return null;

            return cleanFilters.ToString(" AND ");
        }
        else
        {
            if (cleanFilters.IsEmpty())
                return null;

            if (cleanFilters.Count != filters.Count)
                throw new InvalidOperationException("Unable to convert filter (mix $filter and $search in an OR");

            if (cleanFilters.Count == 1)
                return cleanFilters.SingleEx();

            return "(" + cleanFilters.ToString(" OR ") + ")";
        }
    }
}