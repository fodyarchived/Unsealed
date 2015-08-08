using System.Linq;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class WeaverTests:BaseTaskTests
{
    public WeaverTests()
        : base(@"AssemblyToProcess\AssemblyToProcess.fsproj")
    {
        
    }

    [Test]
    public void Record_should_nolonger_be_sealed()
    {
        var type = TheAssembly.GetTypes().Single(t => t.FullName == "AssemblyToProcess.Record");
        type.IsSealed.Should().BeFalse();
    }
}