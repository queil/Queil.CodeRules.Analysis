namespace Queil.CodeRules.Analyzer

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.FindSymbols

module Roslyn =

    type ReferencesResult =
     | UnknownType
     | MultipleTypeDeclaration
     | Found of ReferencedSymbol seq
     | NoReferences

    let openSolution solutionPath =
        async {
            let ws = MSBuildWorkspace.Create()
            return! ws.OpenSolutionAsync(solutionPath)
                     |> Async.AwaitTask
        }

    let declarationOfType (name:string) (solution:Solution) =
      async {
        let nameSegments = name.Split('.') |> List.ofSeq |> List.rev
        let! symbols = SymbolFinder.FindSourceDeclarationsAsync(solution, (fun n -> n = nameSegments.[0])) |> Async.AwaitTask

        let isMatch symbol =
            let rec checkNs sgm (s:ISymbol) =
                match (sgm,s) with
                    | ([], a) when (a.ContainingNamespace |> isNull) -> true
                    | ([], _) -> false
                    | (h::t, a) when a.Name = h -> checkNs t (s.ContainingNamespace)
                    | _ -> false
            checkNs nameSegments symbol

        return match symbols |> Seq.toList with
                    | [] -> []
                    | a -> match a |> List.filter isMatch with 
                            | [] -> []
                            | a -> a
      }

    let referencesOfType (s:ISymbol) (solution:Solution) =
        async {
            let! ret = SymbolFinder.FindReferencesAsync(s, solution) |> Async.AwaitTask
            return ret |> Seq.toList
        }

    let findReferencesOf typeName slnPath = 
        async {
            let! solution = openSolution slnPath
            let! wantedSymbol = solution |> declarationOfType typeName 

            match wantedSymbol with
             | [] -> return UnknownType
             | [s] -> let! refs = solution |> referencesOfType s
                      match refs with
                       | [] -> return NoReferences
                       | _ -> return Found (refs)
             | _ -> return MultipleTypeDeclaration
        }
