using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

public class TestAssemblyResolver : IAssemblyResolver
{
    List<string> gacPaths;
    List<string> directories;

    public TestAssemblyResolver(string targetPath, string projectPath)
    {
        var versionReader = new VersionReader(projectPath);
        directories = new List<string>();
        if (string.IsNullOrEmpty(versionReader.TargetFrameworkProfile))
        {
            directories.Add($@"{GetProgramFilesPath()}\Reference Assemblies\Microsoft\Framework\.NETFramework\{versionReader.FrameworkVersionAsString}\");
        }
        else
        {
            directories.Add($@"{GetProgramFilesPath()}\Reference Assemblies\Microsoft\Framework\.NETFramework\{versionReader.FrameworkVersionAsString}\Profile\{versionReader.TargetFrameworkProfile}");
        }
        if (versionReader.IsFSharp)
        {
            //C:\Program Files (x86)\Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.0.0\FSharp.Core.dll
            var path = $@"{GetProgramFilesPath()}\Reference Assemblies\Microsoft\FSharp\.NETFramework\{versionReader.FrameworkVersionAsString}\{versionReader.TargetFSharpCoreVersion}\";
            directories.Add(path);
        }
        directories.Add(Path.GetDirectoryName(targetPath));

        GetGacPaths();
    }

    void GetGacPaths()
    {
        gacPaths = GetDefaultWindowsGacPaths().ToList();
    }

    IEnumerable<string> GetDefaultWindowsGacPaths()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
        if (environmentVariable != null)
        {
            yield return Path.Combine(environmentVariable, "assembly");
            yield return Path.Combine(environmentVariable, Path.Combine("Microsoft.NET", "assembly"));
        }
    }

    public string GetProgramFilesPath()
    {
        var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        if (programFiles == null)
        {
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        return programFiles;
    }


    string SearchDirectory(string name)
    {
        foreach (var directory in directories)
        {
            var dllFile = Path.Combine(directory, name + ".dll");
            if (File.Exists(dllFile))
            {
                return dllFile;
            }
            var exeFile = Path.Combine(directory, name + ".exe");
            if (File.Exists(exeFile))
            {
                return exeFile;
            }
        }
        return null;
    }


    public string Find(AssemblyNameReference assemblyNameReference)
    {
        var file = SearchDirectory(assemblyNameReference.Name);
        if (file == null)
        {
            file = GetAssemblyInGac(assemblyNameReference);
        }
        if (file != null)
        {
            return file;
        }
        throw new FileNotFoundException();
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        return AssemblyDefinition.ReadAssembly(Find(name));
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        return AssemblyDefinition.ReadAssembly(Find(name));
    }

    public string Find(string assemblyName)
    {
        var file = SearchDirectory(assemblyName);
        if (file != null)
        {
            return file;
        }
        throw new FileNotFoundException();
    }

    string GetAssemblyInGac(AssemblyNameReference reference)
    {
        if (reference.PublicKeyToken == null || reference.PublicKeyToken.Length == 0)
        {
            return null;
        }
        return GetAssemblyInNetGac(reference);
    }

    string GetAssemblyInNetGac(AssemblyNameReference reference)
    {
        var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC" };
        var prefixes = new[] { string.Empty, "v4.0_" };

        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < gacs.Length; j++)
            {
                var gac = Path.Combine(gacPaths[i], gacs[j]);
                var file = GetAssemblyFile(reference, prefixes[i], gac);
                if (Directory.Exists(gac) && File.Exists(file))
                {
                    return file;
                }
            }
        }

        return null;
    }


    static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
    {
        var builder = new StringBuilder();
        builder.Append(prefix);
        builder.Append(reference.Version);
        builder.Append("__");
        for (var i = 0; i < reference.PublicKeyToken.Length; i++)
        {
            builder.Append(reference.PublicKeyToken[i].ToString("x2"));
        }
        return Path.Combine(Path.Combine(Path.Combine(gac, reference.Name), builder.ToString()), reference.Name + ".dll");
    }


    public void Dispose()
    {
    }
}