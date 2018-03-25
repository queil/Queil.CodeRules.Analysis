open Queil.CodeRules.Analyzer.Roslyn

[<EntryPoint>]
let main argv = 

    let slnPath = argv.[0]
    let typeToFind = argv.[1]
    let allReferences = slnPath |> findReferencesOf typeToFind |> Async.RunSynchronously

    printfn "Looking for type '%s' in solution '%s'" typeToFind slnPath

    match allReferences with
     | NoReferences -> printfn "No references found."
     | Found a -> a |> Seq.iter (fun x -> 
                    printfn "%s" (x.Definition.ToDisplayString())
                    x.Locations |> Seq.iter (fun l -> printfn "  %s" (l.Location.ToString())))
     | MultipleTypeDeclaration -> printfn "Multiple type declaration - a bug?"
     | UnknownType -> printfn "Type declaration not found."
    0