using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common;

public class Constants
{


    public enum AutoSuggestType
    {
        Engineers,
        Posts,
        Emails,
    }
    public enum AutocompleteGroup
    {
        suggests,
        weathers,
        login,
        upload,
        download,
        removefile,
        getrandom,
        stream,
        streambyname,
        fileremove,
        fileupload,

    }

    public enum PathType
    {
        Share,
        Upload,
        Docs,
        Stream,
        StreamByName,
        Template
    }

    //describe the field using 
    public enum FieldType
    {

    }

    public static class Op
    {

        public const string equal = "eq";
        public const string greaterThanOrEqual = "gte";
        public const string lessThanOrEqual = "lte";
        public const string lessThan = "lt";
        public const string itLikes = "ct";
        public const string Between = "bw";
        public const string BeginWith = "bt";
        public const string EndWith = "et";
        public const string Within = "in";
        public const string CheckListIn = "at";
    }
    public enum QueryOpType
    {
        Equal,
        StartsWith,
        EndsWith,
        ContainsWith,
        LikesWith,
        NotEq,
        GreaterOrEq,
        LessOrEq,
        Less,
        InListOp,
        OrderBy,
        ThenBy,
    }

    public static class Setting
    {
        public const string DBRCUSetting = nameof(DBRCUSetting);
        public const string PathSetting = nameof(PathSetting);
        public const string AuthSetting = nameof(AuthSetting);
        public const string TemplateSetting = nameof(TemplateSetting);
        public const string CorsPolicySetting = nameof(CorsPolicySetting);
        
        public static int PageSize = 20;
        public static int PageStart = 1;
        public static string AppName = typeof(Setting).Assembly.GetName().Name!.Replace(".Shared", "");
        public static string AuthorizeCookieKey = $"HYD.AuthorizeCookie_Key";
        
        public const string SecretKey = "HYD.abcqwe123";
        public const string Issuer = "HYD";
        public const string Audience = "";
        public const string Subject = "HYD.ENG";

        public const string SEARCH_SEPARATOR = "$";
        public const string CONTENT_SEPARATOR = "|";
        public const string KEY_SEPARATOR = "!";
        public const string GROUP_SEPARATOR = "--";
        public const string FILEPOND_ATTCHTYPE = "AttachType";

    }

    public static class DataKey
    {
        public const string SETT_USERGUIDE = "userguidefile";
        public const string SETT_SIMPUSRGUIDE = "simrguidefile";
        public const string NOTIFY_SUBJ = "NOTIFY.SUBJECT";
        public const string NOTIFY_CC = "NOTIFY.CC";
        public const string NOTIFY_SERVER = "NOTIFY.SERVER";
        public const string NOTIFY_PORT = "NOTIFY.PORT";
        public const string NOTIFY_TO = "NOTIFY.TO";


        public const string SETT_DOCTYPE = "DocType";
        public const string SETT_DOCMAXCNT = "DocMaxCnt";

        public const string DOCTYPE_GENERAL = "G";

        

    }

    public static class SessionKey
    {
        public const string SESSION_USERID = "HYDbz.Session.UserId";


    }


}