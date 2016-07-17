using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

public class VersionReader
{
    public string FrameworkVersionAsString;
    public string TargetFrameworkProfile;
    public string TargetFSharpCoreVersion;
    public bool IsFSharp;

    public VersionReader(string projectPath)
    {
        var xDocument = XDocument.Load(projectPath);
        xDocument.StripNamespace();
        GetFrameworkVersion(xDocument);
        GetTargetFrameworkProfile(xDocument);
        GetTargetFSharpCoreVersion(xDocument);
    }

    void GetFrameworkVersion(XDocument xDocument)
    {
        FrameworkVersionAsString = xDocument.Descendants("TargetFrameworkVersion")
            .Select(c => c.Value)
            .First();
    }


    void GetTargetFrameworkProfile(XDocument xDocument)
    {
        TargetFrameworkProfile = xDocument.Descendants("TargetFrameworkProfile")
            .Select(c => c.Value)
            .FirstOrDefault();
    }

    void GetTargetFSharpCoreVersion(XDocument xDocument)
    {
        TargetFSharpCoreVersion = xDocument.Descendants("TargetFSharpCoreVersion")
            .Select(c => c.Value)
            .FirstOrDefault();
        if (TargetFSharpCoreVersion != null)
        {
            IsFSharp = true;
        }

    }

}