open System.Net
open System.Net.Http
open System.IO
open FSharp.Control

let client = new HttpClient()

let getFragment (baseUrl: string) (num: string) = async {
    let! result = client.GetAsync(baseUrl + num) |> Async.AwaitTask
    match result.StatusCode with
    | System.Net.HttpStatusCode.OK ->
        let! bytes = result.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
        return Some(bytes)
    | _ ->
        return None
}

let getFragements baseUrl =
    let rec loop (start: int) = asyncSeq { 
        let! result = getFragment baseUrl (start.ToString())
        match result with 
        | Some(bytes) ->
            yield (start, bytes)
            yield! loop (start + 1)
        | None ->
            () 
    }
    loop 1

let writeToFile bytes outputPath fileName =
    printfn "Writing file %s..." fileName
    File.WriteAllBytes(Path.Combine(outputPath, fileName), bytes)

[<EntryPoint>]
let main argv = 
    match argv with
    | [| baseUrl; outputPath; baseOutputFilename |] ->
        getFragements baseUrl
        |> AsyncSeq.iter (fun (i, bytes) -> writeToFile bytes outputPath (baseOutputFilename + i.ToString()))
        |> Async.RunSynchronously

        printfn "Done!"
        0
    | _ ->
        printfn "Expecting 3 arguments: base livestream URL, output path, base output filename"
        2
