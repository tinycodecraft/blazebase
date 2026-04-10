using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GovcoreBse.Common;

public static class StringHelper
{

    public static string GetFileType(this string fileNamen)
    {
        var fileName = fileNamen?.ToLower();
        if (fileName.EndsWith(".xlsx") || fileName.EndsWith(".xls"))
            return "application/vnd.ms-excel";
        else if (fileName.EndsWith(".pdf"))
            return "application/pdf";
        else if (fileName.EndsWith(".doc") || fileName.EndsWith(".docx"))
            return "application/vnd.ms-word";
        else if (fileName.EndsWith(".zip"))
            return "application/zip";
        else if (fileName.EndsWith(".jpg"))
            return "image/jpeg";
        else if (fileName.EndsWith(".png"))
            return "image/png";
        else if (fileName.EndsWith(".tif"))
            return "image/tiff";
        else if (fileName.EndsWith(".gif"))
            return "image/gif";
        else if (fileName.EndsWith(".mp4"))
            return "video/mp4";



        return "application/octet-stream";
    }

    public static string GetBaseName(string file)
    {
        var basename = Path.GetFileNameWithoutExtension(file);

        if (basename.LastIndexOf(".") > 0 && (basename.Length - basename.LastIndexOf(".")) > 5)
        {
            basename = basename.Substring(0, basename.LastIndexOf("."));
            return basename;
        }
        return basename;
    }

    public static IConfigurationSection RevertPathSlash<T>(this IConfigurationSection config) where T : class
    {

        foreach (var n in typeof(T).GetProperties().Select(e => e.Name))
        {

            if (config[n]!.Contains(":"))
            {
                config[n] = config[n]!.Replace("/", "\\");
            }
        }
        return config;
    }
    public static string SplitCamelCase(this string str)
    {
        return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
    }

    public static Dictionary<String, Object> Dyn2Dict(dynamic dynObj)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
        {
            object obj = propertyDescriptor.GetValue(dynObj);
            dictionary.Add(propertyDescriptor.Name, obj);
        }
        return dictionary;
    }

    public static Dictionary<string, string> ZipStrPair(string values, string fields, string sep = "!")
    {
        if (string.IsNullOrEmpty(values) || string.IsNullOrEmpty(fields))
            return new Dictionary<string, string>();

        var valuelist = values.Split(sep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var fieldlist = fields.Split(sep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (fieldlist.Length > 0)

            return fieldlist.Select((v, i) => new KeyValuePair<string, string>(v, valuelist[i])).ToDictionary(e => e.Key, e => e.Value);

        return new Dictionary<string, string>();
    }

    public static string ItRevertAmpSign(this string str)
    {
        return (str ?? "").Replace("&", "`3").Replace("\\", "`1").Replace("/", "`8").Replace("!", "`2");
    }
    public static string ItRestoreAmpSign(this string str)
    {
        return (str ?? "").Replace("`3", "&").Replace("`1", "\\").Replace("`8", "/").Replace("`2", "!");
    }
    public static string ItSubStr(this string strval, string sep = "_")
    {
        if (string.IsNullOrEmpty(strval))
            return "";
        return strval.Substring(strval.IndexOf(sep) + 1);
    }


    public static dynamic GetDynamicObjFromJSON(string jsonvalues)
    {
        var d = JsonSerializer.Deserialize<dynamic>(jsonvalues);
        return d;
    }

    public static string PascaltoCamel(this string str, bool invert = false)
    {
        if (string.IsNullOrEmpty(str)) return null;
        if (invert)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);

        }
        return str.Substring(0, 1).ToLower() + str.Substring(1);
    }

    public static async IAsyncEnumerable<T> CreateJSONObjList<T>(string strJSON) where T : class
    {
        bool skip = false;
        if (string.IsNullOrEmpty(strJSON) || strJSON == "{}")
            skip = true;

        if (!skip)
        {
            var formattedValues = strJSON.Replace(Environment.NewLine, string.Empty);
            if (!formattedValues.TrimStart().StartsWith("["))
            {
                formattedValues = $"[{formattedValues}]";
            }
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(formattedValues));
            var arrJSON = JsonSerializer.DeserializeAsyncEnumerable<T>(stream);

            await foreach (T item in arrJSON)
            {
                yield return item;
            }

        }



    }
    public static T GetEnum<T>(this string val) where T : Enum
    {
        try
        {
            var enumT = (T)Enum.Parse(typeof(T), val); ;
            return enumT;
        }
        catch (Exception ex)
        {
            return default(T);
        }


    }

    public static bool CanInline(string fileName)
    {
        var fileNamelwr = fileName.ToLower();
        if (fileNamelwr.EndsWith(".jpg"))
            return true;
        else if (fileNamelwr.EndsWith(".jpg"))
            return true;
        else if (fileNamelwr.EndsWith(".png"))
            return true;
        else if (fileNamelwr.EndsWith(".tif"))
            return true;
        else if (fileNamelwr.EndsWith(".gif"))
            return true;
        else if (fileNamelwr.EndsWith("pdf"))
            return true;
        return false;

    }

    public static Func<T, bool> GetCanBe<T>(T bevalue) where T : struct
    {
        return (T datum) => EqualityComparer<T>.Default.Equals(bevalue, datum);
    }

    public static R Coalesce<T, R>(T targetvalue, List<R> values, params T[] canReturns) where T : struct
    {
        int i = -1;
        foreach (var canBevalue in canReturns)
        {
            i++;
            if (i >= values.Count)
            {
                return default(R);
            }
            if (GetCanBe(canBevalue)(targetvalue))
                return values[i];
        }

        return default(R);
    }

    public static string GetRandomFileInType(string type)
    {
        var filerandom = Path.GetRandomFileName();
        filerandom = Path.GetFileNameWithoutExtension(filerandom);
        switch (type)
        {
            case "word":
                return filerandom + ".docx";
            case "excel":
                return filerandom + ".xlsx";
            default:
                return filerandom + ".txt";
        }
    }


    public static DateTime Trim(this DateTime date, long ticks)
    {
        return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
    }

    public static string TrimStartAt(this string input, string find, int count = 1)
    {
        var tmpinput = input;
        if (string.IsNullOrEmpty(input))
            return input;
        while (tmpinput.IndexOf(find) > -1 && tmpinput.StartsWith(find))
        {
            tmpinput = tmpinput.Substring(tmpinput.IndexOf(find) + find.Length);
            count--;
            if (count <= 0)
                break;
        }

        return tmpinput;
    }

    public static string TrimEndAt(this string input, string find)
    {
        var tmpinput = input;
        if (string.IsNullOrEmpty(input))
            return input;
        while (tmpinput.LastIndexOf(find) > -1 && tmpinput.EndsWith(find))
        {
            tmpinput = tmpinput.Substring(0, tmpinput.LastIndexOf(find));
        }

        return tmpinput;
    }

    public static bool Contains(this string input, string find, StringComparison comparisonType)
    {
        return String.IsNullOrWhiteSpace(input) ? false : input.IndexOf(find, comparisonType) > -1;
    }

    public static IEnumerable<string> NoEmpty(this IEnumerable<string> input)
    {
        foreach (var i in input.Where(e => !string.IsNullOrEmpty(e)))
            yield return i;
    }

    public static IEnumerable<string> ItSplit(this string str, string sep = ",")
    {

        if (string.IsNullOrEmpty(str))
            yield return "";
        else if (string.IsNullOrEmpty(sep))
            yield return str;
        else
        {
            var sb = new StringBuilder(str);
            while (sb.ToString().IndexOf(sep) >= 0)
            {
                var sbstr = sb.ToString();
                var sepindex = sbstr.IndexOf(sep);
                sbstr = sbstr.Substring(0, sepindex);
                yield return sbstr.Trim();

                sb = sb.Remove(0, sepindex + sep.Length);
            }
            if (sb.Length > 0)
            {
                yield return sb.ToString().Trim();
            }

        }


    }

}