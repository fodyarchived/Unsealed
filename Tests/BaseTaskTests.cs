using System.Reflection;

using NUnit.Framework;

public abstract class BaseTaskTests
{
    readonly string projectPath;
    Assembly assembly;
    WeaverHelper weaverHelper;

    protected BaseTaskTests(string projectPath)
    {

#if (RELEASE)
            projectPath = projectPath.Replace("Debug", "Release");
#endif
        this.projectPath = projectPath;
    }

    public string ProjectPath
    {
        get
        {
            return projectPath;
        }
    }

    public WeaverHelper WeaverHelper
    {
        get
        {
            return weaverHelper;
        }
    }

    public Assembly TheAssembly
    {
        get
        {
            return assembly;
        }
    }

    [TestFixtureSetUp]
    public void Setup()
    {
        weaverHelper = new WeaverHelper(projectPath);
        assembly = weaverHelper.Assembly;
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(weaverHelper.BeforeAssemblyPath, weaverHelper.AfterAssemblyPath);
    }
}