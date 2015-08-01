// Damjan Namjesnik, August 2013.

namespace Unsealed.Fody

open Mono.Cecil
open Mono.Cecil.Cil
open Mono.Cecil.Rocks

type ModuleWeaver() = 

    // direct translation of C# code by Simon Cropp
    let makeSetter (field:FieldDefinition) name (moduleDefinition:ModuleDefinition) =
        let set = MethodDefinition("set_" + name, MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig, moduleDefinition.TypeSystem.Void)
        let instructions = set.Body.Instructions
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0))
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1))
        instructions.Add(Instruction.Create(OpCodes.Stfld, field))
        instructions.Add(Instruction.Create(OpCodes.Ret))
        set.Parameters.Add(ParameterDefinition(field.FieldType))
        set.SemanticsAttributes <- MethodSemanticsAttributes.Setter
        //TODO: assembly wouldn't PEVerify when adding the attribute - but nevertheless the weaving works :)
        //let msCoreReferenceFinder = MsCoreReferenceFinder(moduleDefinition, moduleDefinition.AssemblyResolver)
        //set.CustomAttributes.Add(CustomAttribute(msCoreReferenceFinder.CompilerGeneratedReference))
        set

    let findBackingField (prop:PropertyDefinition) =
        let getMethod = prop.GetMethod
        if getMethod.HasBody then
            let fo = getMethod.Body.Instructions
                     |> Seq.tryFind (fun i -> i.OpCode = OpCodes.Ldfld)
            fo |> function | None -> None | Some f -> Some (f.Operand |> unbox<FieldDefinition>)   
        else
            None

    let processProperties (nt:TypeDefinition) (moduleDefinition:ModuleDefinition) = 
        nt.Properties 
        |> Seq.where (fun p -> p.SetMethod = null)
        |> Seq.iter (fun p -> let fo = findBackingField p                                                        
                              match fo with
                              | None -> ()
                              | Some f -> let setter = makeSetter f p.Name moduleDefinition
                                          f.IsInitOnly <- false
                                          nt.Methods.Add(setter)                     
                                          p.SetMethod <- setter
                         )
                                                                
                             
    // direct translation of C# code by Simon Cropp, with minor customizations by me                          
    let makeConstructor (t:TypeDefinition) (moduleDefinition:ModuleDefinition) =
        if t.GetConstructors() |> Seq.forall (fun c -> c.HasParameters) then
            let ctor = new MethodDefinition(".ctor", MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.RTSpecialName, moduleDefinition.TypeSystem.Void);        
            let t1 = moduleDefinition.TypeSystem.Object.Resolve()
            let t2 = t1.GetConstructors()
            let t3 = t2 |> Seq.head
            let objectConstructor = moduleDefinition.Import(t3);
            let processor = ctor.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, objectConstructor);
            processor.Emit(OpCodes.Ret);
            t.Methods.Add(ctor);
        ()                    
                            

    let processOneType (t:TypeDefinition) (moduleDefinition:ModuleDefinition) =         
        let cax = t.CustomAttributes |> Seq.tryFind(fun ca -> ca.AttributeType.Name = "CompilationMappingAttribute" )
        match cax with 
        | None -> ()
        | Some ca -> // Union type
                     if Option.isSome (ca.ConstructorArguments |> Seq.tryFind (fun arg -> arg.Type.Name = "SourceConstructFlags" && arg.Value |> unbox<int> = 1)) then 
                        if not t.IsAbstract then 
                            processProperties t moduleDefinition
                        t.NestedTypes |> Seq.iter (fun nt -> processProperties nt moduleDefinition)
                     // Record type -> no need for [<CliMutableAttribute>] anymore! :)
                     if Option.isSome (ca.ConstructorArguments |> Seq.tryFind (fun arg -> arg.Type.Name = "SourceConstructFlags" && arg.Value |> unbox<int> = 2)) then 
                        makeConstructor t moduleDefinition
                        processProperties t moduleDefinition

    let rec processTypes (tl:seq<TypeDefinition>) (moduleDefinition:ModuleDefinition) = tl |> Seq.iter (fun t -> processOneType t moduleDefinition
                                                                                                                 if t.HasNestedTypes then 
                                                                                                                    processTypes t.NestedTypes moduleDefinition
                                                                                                            ) 

    member val ModuleDefinition:ModuleDefinition = null
        with get, set

    member x.Execute() = 
        processTypes x.ModuleDefinition.Types x.ModuleDefinition
        ()