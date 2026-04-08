using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GovcoreBse.Common;

public static class FieldHelper
{
    public static string[] GetTimeIntervals(DateTime start, DateTime end, int stepinMins)
    {
        var nofIntervals = (int)((end - start).Ticks / (stepinMins * TimeSpan.TicksPerMinute));
        DateTime[] intervals = new DateTime[nofIntervals + 1];
        for (int i = 0; i < nofIntervals + 1; i++)
        {

            intervals[i] = start.AddMinutes(i * stepinMins);
        }
        return intervals.Select(e => e.ToString("HH:mm")).ToArray();
    }
    public static KeyValuePair<DateTime, DateTime> GetNearestTimeFrame(this DateTime date)
    {
        var nearest = date.Trim(TimeSpan.TicksPerMinute);
        var nearestminute = (nearest.Minute % 15) > 0 ? (nearest.Minute / 15) * 15 + 15 : nearest.Minute;
        var nearestStart = nearest.AddMinutes(-nearest.Minute + nearestminute);
        var toswitchsession = (nearestStart > DateTime.Today.AddHours(12).AddMinutes(30)) && (nearestStart < DateTime.Today.AddHours(13));
        nearestStart = toswitchsession ? DateTime.Today.AddHours(13) : nearestStart;

        var sessionEnd = nearestStart.AddMinutes(30);
        if (sessionEnd > DateTime.Today.AddHours(18))
        {
            nearestStart = DateTime.Today.AddDays(1).AddHours(9);
            sessionEnd = nearestStart.AddMinutes(30);
        }
        return new KeyValuePair<DateTime, DateTime>(nearestStart, sessionEnd);

    }

    public static DateTime ParseDateOrDefault(string datestr, string fmt, DateTime defaultval)
    {
        var newvalue = defaultval;
        var success = DateTime.TryParseExact(datestr, fmt, null, System.Globalization.DateTimeStyles.None, out newvalue);
        if (success)
            return newvalue;
        return defaultval;

    }

    public static bool IsInteger(this string s)
    {
        try
        {
            if (s == null)
                return false;
            int t = int.Parse(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsBase64String(this string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

    }

    public static Type GetTypeForDefaultValue(object value)
    {
        DateTime resultvalue = DateTime.Now;
        if (DateTime.TryParse(value.ToString(), out resultvalue))
            return typeof(DateTime);
        else
            return BaseType(value.GetType());
    }

    public static Type BaseType(Type objType)
    {
        // ensure the passed objType 1) is valid, 2) .IsValueType, 3) and is logicially nullable
        if (objType != null && objType.IsValueType && objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(Nullable<>))
            return Nullable.GetUnderlyingType(objType);
        else
            return objType;
    }

    public static bool IsSimpleType(
this Type type)
    {
        return
            type.IsValueType ||
            type.IsPrimitive ||
            new Type[] {
            typeof(String),
            typeof(Decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        }.Contains(type) ||
            Convert.GetTypeCode(type) != TypeCode.Object;
    }

    public static Uri AddQuery(this Uri uri, string name, string value)
    {
        var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

        httpValueCollection.Remove(name);
        httpValueCollection.Add(name, value);

        var ub = new UriBuilder(uri);

        // this code block is taken from httpValueCollection.ToString() method
        // and modified so it encodes strings with HttpUtility.UrlEncode
        if (httpValueCollection.Count == 0)
            ub.Query = String.Empty;
        else
        {
            var sb = new StringBuilder();

            for (int i = 0; i < httpValueCollection.Count; i++)
            {
                string text = httpValueCollection.GetKey(i);
                {
                    text = HttpUtility.UrlEncode(text);

                    string val = (text != null) ? (text + "=") : string.Empty;
                    string[] vals = httpValueCollection.GetValues(i);

                    if (sb.Length > 0)
                        sb.Append('&');

                    if (vals == null || vals.Length == 0)
                        sb.Append(val);
                    else
                    {
                        if (vals.Length == 1)
                        {
                            sb.Append(val);
                            sb.Append(HttpUtility.UrlEncode(vals[0]));
                        }
                        else
                        {
                            for (int j = 0; j < vals.Length; j++)
                            {
                                if (j > 0)
                                    sb.Append('&');

                                sb.Append(val);
                                sb.Append(HttpUtility.UrlEncode(vals[j]));
                            }
                        }
                    }
                }
            }

            ub.Query = sb.ToString();
        }

        return ub.Uri;
    }

    /// <summary>
    /// change the type of value to match with conversion
    /// </summary>
    /// <param name="value">value to be converted</param>
    /// <param name="conversion">conversion type</param>
    /// <returns></returns>
    public static object ChangeType(object value, Type conversion)
    {
        var t = conversion;
        object converted = value;
        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
            {
                return null;
            }

            t = Nullable.GetUnderlyingType(t);
        }
        try
        {
            converted = Convert.ChangeType(value, t);
            return converted;
        }
        catch (Exception)
        {

        }
        return converted;
    }
    public static PropertyInfo GetPropertyFromExpression<T, V>(this Expression<Func<T, V>> GetPropertyLambda)
    {
        MemberExpression Exp = null;

        //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
        if (GetPropertyLambda.Body is UnaryExpression)
        {
            var UnExp = (UnaryExpression)GetPropertyLambda.Body;
            if (UnExp.Operand is MemberExpression)
            {
                Exp = (MemberExpression)UnExp.Operand;
            }
            else
                throw new ArgumentException();
        }
        else if (GetPropertyLambda.Body is MemberExpression)
        {
            Exp = (MemberExpression)GetPropertyLambda.Body;
        }
        else
        {
            throw new ArgumentException();
        }

        return (PropertyInfo)Exp.Member;
    }


    //example : [expression].GetMemberName()
    public static string GetMemberName(this LambdaExpression memberSelector)
    {
        var currentExpression = memberSelector.Body;

        while (true)
        {
            switch (currentExpression.NodeType)
            {
                case ExpressionType.Parameter:
                    return ((ParameterExpression)currentExpression).Name;
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)currentExpression).Member.Name;
                case ExpressionType.Call:
                    return ((MethodCallExpression)currentExpression).Method.Name;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    currentExpression = ((UnaryExpression)currentExpression).Operand;
                    break;
                case ExpressionType.Invoke:
                    currentExpression = ((InvocationExpression)currentExpression).Expression;
                    break;
                case ExpressionType.ArrayLength:
                    return "Length";
                default:
                    throw new Exception("not a proper member selector");
            }
        }
    }

    /// <summary>
    /// Gets an attribute on an enum field value
    /// </summary>
    /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
    /// <param name="enumVal">The enum value</param>
    /// <returns>The attribute of type T that exists on the enum value</returns>
    /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
    public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString()).FirstOrDefault();
        var attributes = memInfo?.GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : null;
    }


    public static PropertyDescriptor FilterPropWithName(Type entitytype, string name)
    {
        foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(entitytype))
        {
            ColumnAttribute attrib = p.Attributes.OfType<Attribute>().Where(x => x.GetType().IsAssignableFrom(typeof(ColumnAttribute))).FirstOrDefault() as ColumnAttribute;

            if (attrib != null && attrib.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return p;
            }
        }
        return null;
    }

    public static IEnumerable<PropertyDescriptor> FilterPropWithAttribute<T>(Type entitytype, bool ignore) where T : Attribute
    {
        foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(entitytype))
        {
            T attrib = p.Attributes.Cast<Attribute>().Where(x => x.GetType().IsAssignableFrom(typeof(T))).FirstOrDefault() as T;

            if (ignore && attrib == null)
                yield return p;
            if (attrib != null && !ignore)
                yield return p;
        }

    }

    public static bool IsAttributeDefined<T>(object entity, string propertyName) where T : Attribute
    {
        bool flag = false;
        var prop = TypeDescriptor.GetProperties(entity).Find(propertyName, true);
        if (prop == null)
            return false;
        T tt = prop.Attributes.Cast<Attribute>().Where(x => x.GetType().IsAssignableFrom(typeof(T))).FirstOrDefault() as T;
        if (tt != null)
            return true;
        return flag;
    }

    /// <summary>
    /// find the getter of the property
    /// </summary>
    /// <param name="targettype">type of entity</param>
    /// <param name="propertyname">property name of entity</param>
    /// <returns>getter of property</returns>
    public static Func<object, object> FindGetter(Type targettype, string propertyname)
    {
        try
        {
            if (!targettype.GetProperties().Any(y => y.Name == propertyname))
                return null;
            var prop = targettype.GetProperty(propertyname);

            return GetGetter(prop);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    /// <summary>
    /// find setter of property by property name
    /// </summary>
    /// <param name="targettype">business object type</param>
    /// <param name="propertyname">property name</param>
    /// <param name="toConvert">convert value to property type if true</param>
    /// <returns>setter of property</returns>
    public static Action<object, object> FindSetter(Type targettype, string propertyname, bool toConvert = false)
    {
        try
        {
            var prop = targettype.GetProperty(propertyname);

            if (prop == null)
                return null;

            if (toConvert)
            {

                return (target, value) =>
                {
                    //var cnt = TypeDescriptor.GetConverter(prop.PropertyType);
                    //var valueAstype = cnt.ConvertFrom(value.ToString());

                    var valueAstype = ChangeType(value, prop.PropertyType);
                    GetSetter(prop)(target, valueAstype);
                };
            }


            return GetSetter(prop);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    internal static Func<object, object> GetGetter(PropertyInfo property)
    {
        // get the get method for the property
        MethodInfo method = property.GetGetMethod(true); // true - non-public accessor should be returned 

        // get the generic get-method generator (ReflectionUtility.GetSetterHelper<TTarget, TValue>)
        MethodInfo genericHelper = typeof(FieldHelper).GetMethod(
            "GetGetterHelper",
            BindingFlags.Static | BindingFlags.NonPublic);

        // reflection call to the generic get-method generator to generate the type arguments
        MethodInfo constructedHelper = genericHelper.MakeGenericMethod(
            method.DeclaringType,
            method.ReturnType);

        // now call it. The null argument is because it's a static method.
        object ret = constructedHelper.Invoke(null, new object[] { method });

        // cast the result to the action delegate and return it
        return (Func<object, object>)ret;
    }

    static Func<object, object> GetGetterHelper<TTarget, TResult>(MethodInfo method)
        where TTarget : class // target must be a class as property sets on structs need a ref param
    {
        // Convert the slow MethodInfo into a fast, strongly typed, open delegate
        Func<TTarget, TResult> func = (Func<TTarget, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), method);

        // Now create a more weakly typed delegate which will call the strongly typed one
        Func<object, object> ret = (object target) => (TResult)func((TTarget)target);
        return ret;
    }

    public static Dictionary<string, Func<object, object>> GetSortedGetters(this Type type, bool onlyDisplay = false)
    {
        int order = int.MaxValue - type.GetProperties().Count();
        var metadataType = type.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
            .OfType<MetadataTypeAttribute>().FirstOrDefault();


        return type.GetProperties()
            .Select(p =>
            {


                var display = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;


                return new
                {
                    display = display,
                    property = p,
                    order = display != null ?
                        (display.GetOrder() ?? ++order) : ++order,

                    getter = FindGetter(type, p.Name)

                };
            })
            .Where(e => !onlyDisplay || e.display != null)
            .OrderBy(o => o.order)
            .ToDictionary(x => x.property.Name, x => x.getter);
    }
    public static Dictionary<PropertyInfo, DisplayAttribute> GetSortedProperties(this Type type, bool onlyDisplay = false)
    {
        int order = int.MaxValue - type.GetProperties().Count();
        var metadataType = type.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
            .OfType<MetadataTypeAttribute>().FirstOrDefault();


        return type.GetProperties()
            .Select(p =>
            {


                var display = p.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;

                return new
                {
                    display = display,
                    property = p,
                    order = display != null ?
                        (display.GetOrder() ?? ++order) : ++order

                };
            })
            .Where(e => !onlyDisplay || e.display != null)
            .OrderBy(o => o.order)
            .ToDictionary(x => x.property, x => x.display);
    }

    internal static Action<object, object> GetSetter(PropertyInfo property)
    {
        // get the get method for the property
        MethodInfo method = property.GetSetMethod(true); // true - non-public accessor should be returned 

        // get the generic get-method generator (ReflectionUtility.GetSetterHelper<TTarget, TValue>)
        MethodInfo genericHelper = typeof(FieldHelper).GetMethod(
            "GetSetterHelper",
            BindingFlags.Static | BindingFlags.NonPublic);

        // reflection call to the generic get-method generator to generate the type arguments
        MethodInfo constructedHelper = genericHelper.MakeGenericMethod(
            method.DeclaringType,
            property.PropertyType);

        // now call it. The null argument is because it's a static method.
        object ret = constructedHelper.Invoke(null, new object[] { method });

        // cast the result to the action delegate and return it
        return (Action<object, object>)ret;
    }

    static Action<object, object> GetSetterHelper<TClass, TProperty>(MethodInfo method)
    {
        Action<TClass, TProperty> act = (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), method);
        Action<object, object> ret = (object instance, object value) => { act((TClass)instance, (TProperty)value); };
        return ret;
    }
}
