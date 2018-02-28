open System.Linq
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild

let compile projectFilter solutionPath = 
    async {
            let workspace = MSBuildWorkspace.Create();
            
            let! solution = workspace.OpenSolutionAsync(solutionPath) |> Async.AwaitTask
            
            return! solution.Projects 
                     |> Seq.filter projectFilter
                     |> Seq.map (fun p -> p.GetCompilationAsync() |> Async.AwaitTask)
                     |> Async.Parallel
          }

[<EntryPoint>]
let main argv = 
    
    let slnPath = argv.[0]

    let projectFilter (p:Project) = 
        match p.FilePath with
         | _ -> true   

    let namespaceFilter (n:INamespaceOrTypeSymbol) =
        match n.Name with
          | "Queil" -> true
          | _ -> false

    let compilations = slnPath 
                        |> compile projectFilter 
                        |> Async.RunSynchronously
    
    let rec folder (acc:INamespaceOrTypeSymbol list) (s:INamespaceOrTypeSymbol) = 
        let symbols = s.GetMembers().OfType<INamespaceOrTypeSymbol>() |> List.ofSeq
        List.append acc (symbols |> List.fold folder symbols)
       
    let typesFrom (c:Compilation) = 
        c.GlobalNamespace.GetNamespaceMembers()
            |> Seq.filter namespaceFilter
            |> Seq.fold folder ([] : INamespaceOrTypeSymbol list)
            |> Enumerable.OfType<INamedTypeSymbol>
            |> List.ofSeq
   
    let all = compilations |> Seq.collect typesFrom  |> List.ofSeq
        
    all |> Seq.iter (fun x -> printf "%s\n" (x.ToDisplayString()))

    0