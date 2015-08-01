namespace AssemblyToProcess


type Record = 
    {
        First : int
        Second : string    
    }
    override x.ToString() = x.First.ToString() + ":" + x.Second

type RecordList = Record list

type UnionOne = | OneCase

type UnionThree = | CaseOne | CaseTwo | CaseThree

type SomeTuple = int * string * float * int64 // only alias -> not compiled as type

type Union =
    | Animal of string
    | Pair of int * string
    | ComplexPair of int * Record
    override x.ToString() = "Contents:" + (x|> function Animal a -> a | Pair (i,s) -> s | ComplexPair(i,r) -> if (box r) = null then "null" else i.ToString() + "/" + r.Second )
     
//[<KnownTypeAttribute("GetKnownTypes")>]

type Tree =
    | Empty
    //| Node of Root:string * LeftChild:Tree * RightChild:Tree // F# 3.1 syntax
    | Node of string * Tree * Tree
    override x.ToString() = x |> function | Empty -> "~" | Node (s,l,r) -> s + "(" + l.ToString() + " * " + r.ToString() + ")"


type Class1() = 

    let a = Animal "Dog"
    let b = Pair (2,"Arrrr")
    let c = ComplexPair(3, {First = 3;Second = "BBB"})

    let x1 = {First = 4;Second = "X! Nesta"}
    let x2 = {First = 5;Second = "X2222! yea"}

    let lx:RecordList =  [x1;x2]


    member this.X = "F#"
