open System
open System.Linq
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.CSharp

let syntaxNodes projectFilter documentFilter solutionPath = 
    async {

            let workspace = MSBuildWorkspace.Create();
            
            let! solution = workspace.OpenSolutionAsync(solutionPath) |> Async.AwaitTask
                
            let! roots = solution.Projects 
                            |> Seq.filter projectFilter
                            |> Seq.collect (fun p -> p.Documents)
                            |> Seq.filter documentFilter
                            |> Seq.map (fun d -> d.GetSyntaxRootAsync() |> Async.AwaitTask)
                            |> Async.Parallel

            return roots |> Seq.map (fun r -> r.SyntaxTree)
        }

[<EntryPoint>]
let main argv = 
    
    let slnPath = argv.[0]

    let typeName = "Container"
    let ns = "Queil.CodeRules.Analyzed.RemoveThis.Container"


    let projectFilter (p:Project) = true

    let documentFilter (d:Document) =
        match d.Name with
         | n when n.EndsWith(".AssemblyAttributes.cs") -> false
         | "AssemblyInfo.cs" -> false
         | _ -> true
    

    let allNodes = slnPath 
                    |> syntaxNodes projectFilter documentFilter 
                    |> Async.RunSynchronously
    

    let compilation = CSharpCompilation.Create("ignore-assembly", allNodes)

    let semanticModels = allNodes 
                          |> Seq.map (fun n -> (compilation.GetSemanticModel(n, true), n))
                          |> Seq.toList

    let symbols = semanticModels
                          |> Seq.collect (fun (model,tree) -> tree.GetRoot().DescendantNodes() 
                                                               |> Enumerable.OfType<IdentifierNameSyntax>
                                                               |> Seq.filter (fun n -> n.ToString() = typeName)
                                                               |> Seq.map (fun n -> match model.GetSymbolInfo(n).Symbol with
                                                                                     | :? INamedTypeSymbol as s -> Some(s)
                                                                                     |_ -> None))                                                         
                                                               |> Seq.choose id
                                                               |> Seq.filter (fun n -> n.ContainingNamespace.ToDisplayString() = ns)
                          |> Seq.toList

    symbols 
     |> Seq.iter (fun s -> printf "%s\n" (s.ToDisplayString()))
    
    0