
namespace GovcoreBse.Common.Models;
public class AuthSetting
{

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    public string ExpireInHrs { get; set; } = "4";

}

public class CorsPolicySetting
{
    public string Name { get; set; }
    public string[] AllowHeaders { get; set; }
    public string[] AllowMethods { get; set; }

    public string[] AllowOrigins { get; set; }

}

public class PathSetting
{
    //the share path root for keeping files upload
    public string Share { get; set; }
    //the upload api for keeping the uploaded files
    public string Upload { get; set; }
    //the api path for downloading the doc files
    public string Docs { get; set; }
    //the api path for downloading
    public string Stream { get; set; }
    //the api path for downloading file with save name specified
    public string StreamByName { get; set; }
    //the template path for keeping the template files
    public string Template { get; set; }
    //the api path for removing the file
    public string FileRemove { get; set; }

    //the api path for uploading the file
    public string FileUpload { get; set; }
}

public class DBRCUSetting:IN.IDBSetting
{
    
    public string DBsource { get; set; } = string.Empty;
    public string DBcatalog { get; set; } = string.Empty;
    public string DBuser { get; set; } = string.Empty;
    public string DBpwd { get; set; } = string.Empty;



}


public class TemplateSetting
{
    //the template name for the template file
    public string User { get; set; }

}