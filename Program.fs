open Queil.CodeRules.Analyzer.Roslyn

[<EntryPoint>]
let main argv = 

    let slnPath = argv.[0]
    let typeToFind = argv.[1]
    let allReferences = slnPath |> findReferencesOf typeToFind |> Async.RunSynchronously

    match allReferences with
     | [] -> printfn "No references of type '%s' found." typeToFind
     | a -> a |> Seq.iter (fun x -> 
                    printfn "%s" (x.Definition.ToDisplayString())
                    x.Locations |> Seq.iter (fun l -> printfn "  %s" (l.Location.ToString()))) 
    0