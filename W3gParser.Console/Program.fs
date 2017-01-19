module W3gParser.Console.Program

open System
open System.IO

open Deerchao.War3Share.W3gParser

type private Stat = {
    name : string
    time : TimeSpan
    actions : int64
} with
    member this.Apm = (double this.actions) / this.time.TotalMinutes

let private getPlayers directory =
    Directory.EnumerateFiles (directory, "*.w3g", SearchOption.AllDirectories)
    |> Seq.collect (fun file ->
        let r = Replay(file)
        r.Players)

let private initStat name =
    { name = name
      time = TimeSpan.Zero
      actions = 0L }

let private combineStat (stat : Stat) (player : Player) =
     { stat with time = stat.time + player.GetTime()
                 actions = stat.actions + (int64 player.Actions) }

let private getStats (players : Player seq) =
    players
    |> Seq.groupBy (fun p -> p.Name)
    |> Seq.map (fun (name, players) -> Seq.fold combineStat (initStat name) players)

[<EntryPoint>]
let main argv =
    if argv.Length < 1
    then
        printfn "Usage: W3gParser.Console <replay-directory-path>"
        1
    else
        let players = getPlayers argv.[0]
        let stats = getStats players
        stats
        |> Seq.iter (fun stat ->
            printfn "%s: %d actions / %A = %g APM" stat.name stat.actions stat.time stat.Apm)

        0
