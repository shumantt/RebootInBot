module RebootInBot.Helpers

open RebootInBot.Types

let bindResultAsync binder result =
    async {
        match result with
        | Ok ok -> return! binder ok
        | Error error -> return Error error
    }

let bindAsyncResultAsync binder asyncResult =
    async {
        let! result = asyncResult
        return! bindResultAsync binder result
    }

let bindAsyncResult binder asyncResult =
    async {
        let! result = asyncResult

        match result with
        | Ok ok -> return binder ok
        | Error error -> return Error error
    }

let mapAsyncResult map asyncResult =
    async {
        let! result = asyncResult

        match result with
        | Ok ok -> return Ok(map ok)
        | Error error -> return Error error
    }
    
let mapAsyncError map asyncResult =
    async {
        let! result = asyncResult

        match result with
        | Ok ok -> return Ok ok
        | Error error -> return Error (map error)
    }

let mapAsync map value =
    async {
        let! result = value
        return map result
    }
    
let bindAsync bind value =
    async {
        let! result = value
        return! bind result
    }

let liftAsync x =
    async {
        // lift x to an Async
        return x
    }

type Chat with
    member x.ToTimerId(): TimerId =
        TimerId (x.ChatId.ToString())
    
    