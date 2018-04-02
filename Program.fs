open Queil.CodeRules.Analyzer.Roslyn

[<EntryPoint>]
let main argv = 

    let slnPath = argv.[0]
    let typeToFind = argv.[1]
    let allReferences = slnPath |> findReferencesOf typeToFind |> Async.RunSynchronously

    printfn "Type: %s" typeToFind
    printfn "Solution: %s\n" slnPath

    match allReferences with
     | NoReferences d -> 
                    printfn "Match: %s\n" (d.ToDisplayString())
                    printfn "No references found."
     | SingleMatch (d, refs) -> 
                    printfn "Match: %s\n" (d.ToDisplayString())
                    refs |> Seq.iter (fun x -> 
                    x.Locations |> Seq.iter (fun l -> printfn "  %s" (l.Location.ToString())))
     | MultipleMatch ds -> ds |> Seq.iter (fun x -> 
                    printfn "Match: %s" (x.ToDisplayString()))
     | NoMatch -> printfn "Type declaration not found."
    0