namespace Queil.CodeRules.Analyzer

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.FindSymbols

module Roslyn =

    let openSolution solutionPath =
        async {
            let ws = MSBuildWorkspace.Create()
            return! ws.OpenSolutionAsync(solutionPath)
                     |> Async.AwaitTask
        }

    let declarationOfType (name:string) (p:Project) : Async<Option<ISymbol>> = 
        async {
            let nameSegments = name.Split('.') |> List.ofSeq |> List.rev
            let! symbols = SymbolFinder.FindDeclarationsAsync(p, nameSegments.[0], true, SymbolFilter.Type)
                            |> Async.AwaitTask

            let isMatch symbol =
                let rec checkNs sgm (s:ISymbol) =
                    match (sgm,s) with
                        | ([], a) when (a.ContainingNamespace |> isNull) -> true
                        | ([], _) -> false
                        | (h::t, a) when a.Name = h -> checkNs t (s.ContainingNamespace)
                        | _ -> false
                checkNs nameSegments symbol

            return match symbols |> Seq.toList with
                    | [] -> None
                    | a -> match a |> List.filter isMatch with 
                            | [] -> None
                            | h::t -> Some h
        }

    let referencesOfType (s:ISymbol) (solution:Solution) =
        async {
            return! SymbolFinder.FindReferencesAsync(s, solution) |> Async.AwaitTask
        }

    let findReferencesOf typeName slnPath = 
        async {
            let! solution = openSolution slnPath
            let! wantedSymbol = solution.Projects
                                 |> Seq.map (declarationOfType typeName)
                                 |> Seq.head

            match wantedSymbol with
             | None -> return []
             | Some s -> let! res =  solution |> referencesOfType s
                         return res |> Seq.toList
        }
