namespace Queil.CodeRules.Analyzer

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.FindSymbols

module Roslyn =

    type ReferencesResult =
     | NoMatch
     | MultipleMatch of ISymbol seq
     | SingleMatch of ISymbol * ReferencedSymbol seq
     | NoReferences of ISymbol

    let openSolution solutionPath =
        async {
            let ws = MSBuildWorkspace.Create()
            return! ws.OpenSolutionAsync(solutionPath)
                     |> Async.AwaitTask
        }

    let rec ancestors (s:ISymbol) = 
        seq {   
                yield s
                match s.ContainingSymbol with
                 | :? INamespaceSymbol as ns when ns.IsGlobalNamespace -> ()
                 | c -> yield! ancestors c
            }
    
    let declarationOfType (fullName:string) (solution:Solution) =
      async {
        let nameParts = fullName.Split('.') |> Seq.rev
        let nameFilter n = nameParts |> Seq.head = n
        let! symbols = SymbolFinder.FindSourceDeclarationsAsync(solution, nameFilter) |> Async.AwaitTask

        let isMatch symbol = (symbol |> ancestors |> Seq.map (fun x -> x.Name), nameParts) ||> Seq.compareWith Operators.compare = 0
            
        return symbols |> Seq.filter isMatch |> Seq.toList
      }

    let referencesOfType (s:ISymbol) (solution:Solution) =
        async {
            let! ret = SymbolFinder.FindReferencesAsync(s, solution) |> Async.AwaitTask
            return ret |> Seq.toList
        }

    let findReferencesOf typeName slnPath = 
        async {
            let! solution = openSolution slnPath
            let! wantedSymbols = solution |> declarationOfType typeName 

            match wantedSymbols with
             | [] -> return NoMatch
             | [s] -> let! refs = solution |> referencesOfType s
                      match refs with
                       | [] -> return NoMatch 
                       | _ -> return SingleMatch (s, refs)
             | _ -> return MultipleMatch (wantedSymbols)
        }
