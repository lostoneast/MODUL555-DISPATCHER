using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Domain.Entities;

namespace DispatcherApp.Infrastructure.Services;

// Only explicitly exposed DTO properties are filterable. No raw SQL or arbitrary navigation paths.
public static class ProductListFilters
{
    private static readonly Dictionary<string, PropertyInfo> Fields = typeof(ProductListDto).GetProperties()
        .Where(x => x.Name != nameof(ProductListDto.Identifiers))
        .ToDictionary(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name), StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PropertyInfo> IdentifierFields = typeof(ProductListIdentifierDto).GetProperties()
        .ToDictionary(x => "identifiers." + JsonNamingPolicy.CamelCase.ConvertName(x.Name),
            x => typeof(ProductIdentifier).GetProperty(x.Name)!, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<ProductFieldInfo> Describe() => Fields.Select(x => Describe(x.Key, x.Value, true))
        .Concat(IdentifierFields.Select(x => Describe(x.Key, x.Value, false))).ToList();

    private static ProductFieldInfo Describe(string field, PropertyInfo property, bool sortable)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var nullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null;
        List<string> operators = ["Eq", "NotEq"];
        if (type == typeof(string)) operators.Add("Contains");
        else if (!type.IsEnum && type != typeof(bool)) operators.AddRange(["Gte", "Lte"]);
        if (nullable) operators.AddRange(["IsEmpty", "IsNotEmpty"]);
        return new(field, type.IsEnum ? "enum" : type == typeof(string) ? "text" : type == typeof(bool) ? "boolean"
            : type == typeof(DateOnly) ? "date" : type == typeof(DateTimeOffset) ? "datetime" : "number",
            nullable, sortable, operators, type.IsEnum ? Enum.GetNames(type) : []);
    }

    public static IQueryable<ProductListDto> Apply(IQueryable<ProductListDto> query, ProductListQuery request)
    {
        if (request.Page is < 1 or > 1000000 || request.PageSize is < 1 or > 200 || request.Filters.Count > 50)
            throw new ArgumentException("Страница должна быть положительной, размер страницы — 1–200, фильтров — не более 50.");
        foreach (var filter in request.Filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field)) throw new ArgumentException("Не указано поле фильтра.");
            if (IdentifierFields.ContainsKey(filter.Field)) continue;
            var property = Find(Fields, filter.Field);
            var row = Expression.Parameter(typeof(ProductListDto), "row");
            query = query.Where(Expression.Lambda<Func<ProductListDto, bool>>(Condition(row, property, filter), row));
        }
        // Validate sorting before sending any database query.
        Find(Fields, request.SortBy);
        return query;
    }

    public static IQueryable<ProductIdentifier> FilterIdentifiers(IQueryable<ProductIdentifier> query, List<ProductFieldFilter> filters)
    {
        foreach (var filter in filters)
        {
            var row = Expression.Parameter(typeof(ProductIdentifier), "identifier");
            query = query.Where(Expression.Lambda<Func<ProductIdentifier, bool>>(
                Condition(row, Find(IdentifierFields, filter.Field), filter), row));
        }
        return query;
    }

    public static IOrderedQueryable<ProductListDto> Sort(IQueryable<ProductListDto> query, string field, bool descending)
    {
        var property = Find(Fields, field);
        var row = Expression.Parameter(typeof(ProductListDto), "row");
        var key = Expression.Lambda(Expression.Property(row, property), row);
        var call = Expression.Call(typeof(Queryable), descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy),
            [typeof(ProductListDto), property.PropertyType], query.Expression, Expression.Quote(key));
        return ((IOrderedQueryable<ProductListDto>)query.Provider.CreateQuery<ProductListDto>(call)).ThenBy(x => x.Id);
    }

    private static PropertyInfo Find(Dictionary<string, PropertyInfo> fields, string field) =>
        !string.IsNullOrWhiteSpace(field) && fields.TryGetValue(field, out var property)
            ? property : throw new ArgumentException($"Неизвестное поле: «{field}».");

    private static Expression Condition(ParameterExpression row, PropertyInfo property, ProductFieldFilter filter)
    {
        var metadata = Describe(filter.Field, property, false);
        if (!metadata.Operators.Contains(filter.Operator.ToString()))
            throw new ArgumentException($"Операция {filter.Operator} недоступна для поля {filter.Field}.");
        Expression member = Expression.Property(row, property);
        var underlying = Nullable.GetUnderlyingType(property.PropertyType);
        var type = underlying ?? property.PropertyType;
        if (filter.Operator is ProductFilterOperator.IsEmpty or ProductFilterOperator.IsNotEmpty)
        {
            Expression empty = Expression.Equal(member, Expression.Constant(null, property.PropertyType));
            if (type == typeof(string)) empty = Expression.OrElse(empty, Expression.Equal(member, Expression.Constant("")));
            return filter.Operator == ProductFilterOperator.IsEmpty ? empty : Expression.Not(empty);
        }
        if (filter.Value == null) throw new ArgumentException($"Не указано значение фильтра {filter.Field}.");
        var value = Parse(type, filter.Value, filter.Field);
        Expression? guard = null;
        if (underlying != null)
        {
            guard = Expression.Property(member, "HasValue");
            member = Expression.Property(member, "Value");
        }
        if (type == typeof(string))
        {
            guard = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            member = Expression.Call(member, nameof(string.ToUpper), Type.EmptyTypes);
            value = ((string)value).ToUpperInvariant();
        }
        var constant = Expression.Constant(value, type);
        Expression condition = filter.Operator switch
        {
            ProductFilterOperator.Eq => Expression.Equal(member, constant),
            ProductFilterOperator.NotEq => Expression.NotEqual(member, constant),
            ProductFilterOperator.Gte => Expression.GreaterThanOrEqual(member, constant),
            ProductFilterOperator.Lte => Expression.LessThanOrEqual(member, constant),
            ProductFilterOperator.Contains => Expression.Call(member, nameof(string.Contains), Type.EmptyTypes, constant),
            _ => throw new ArgumentException("Неизвестная операция фильтра."),
        };
        return guard == null ? condition : Expression.AndAlso(guard, condition);
    }

    private static object Parse(Type type, string value, string field)
    {
        try
        {
            if (type == typeof(string)) return value.Trim();
            if (type == typeof(int)) return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (type == typeof(long)) return long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (type == typeof(decimal)) return decimal.Parse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            if (type == typeof(bool)) return bool.Parse(value);
            if (type == typeof(DateOnly)) return DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (type == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            if (type.IsEnum && Enum.TryParse(type, value, true, out var parsed) && Enum.IsDefined(type, parsed!)) return parsed!;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        { throw new ArgumentException($"Некорректное значение «{value}» для поля {field}.", ex); }
        throw new ArgumentException($"Некорректное значение «{value}» для поля {field}.");
    }
}
