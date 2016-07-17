using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class WeaverTests
{
    WeaverHelper weaverHelper;

    public WeaverTests()
    {
        var projectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.fsproj");
        weaverHelper = new WeaverHelper(projectPath);
    }

    [Test]
    public void Record_should_nolonger_be_sealed()
    {
        var type = weaverHelper.Assembly.GetTypes().Single(t => t.FullName == "AssemblyToProcess.Record");
        type.IsSealed.Should().BeFalse();
    }
}