using System;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;


public static class DefinitionFinder
{

    static PropertyDefinition FindType(Type declaringType, string name)
    {
        var typeDefinition = FindType(declaringType);
        return typeDefinition.Properties.First(x => x.Name == name);
    }

    static TypeDefinition FindType(Type typeToFind)
    {
        var moduleDefinition = ModuleDefinition.ReadModule(typeToFind.Assembly.Location);
        foreach (var type in moduleDefinition.Types)
        {

            if (type.Name == typeToFind.Name)
            {
                return type;
            }
            foreach (var nestedType in type.NestedTypes)
            {
                if (nestedType.Name == typeToFind.Name && nestedType.DeclaringType.Name == typeToFind.DeclaringType.Name)
                {
                    return nestedType;
                }
            }
        }
        throw new Exception();
    }
}