module W3gParser.Console.Program

open Deerchao.War3Share.W3gParser

[<EntryPoint>]
let main argv =
    if argv.Length < 1
    then
        printfn "Usage: W3gParser.Console <replay-filepath>"
        1
    else
        let r = Replay(argv.[0])
        r.Players
        |> Seq.iter (fun p -> printfn "%s: %d actions / %A = %g APM" p.Name p.Actions (p.GetTime()) p.Apm)
        0
