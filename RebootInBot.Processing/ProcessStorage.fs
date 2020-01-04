module RebootInBot.Processing.ProcessStorage

open System.Text.RegularExpressions
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open RebootInBot.Processing.Types
open ServiceStack.Redis

module private StorageSerialization =    
    let settings =
        let settings = JsonSerializerSettings()
        settings.Converters.Add(DiscriminatedUnionConverter())
        settings
    
    let deserialize<'a> (value:string) (settings:JsonSerializerSettings) =
        Regex.Unescape value
        |> fun x -> x.Trim('\"')
        |> fun x -> JsonConvert.DeserializeObject<'a>(x, settings)

    let serialize (value) (settings:JsonSerializerSettings) =
        JsonConvert.SerializeObject(value, settings)

let defaultRedisClientFactory =
    fun () -> new RedisClient("192.168.99.100")

let getProcess (redisClientFactory:unit -> RedisClient) (processId: int64) : Process option =
    use client = redisClientFactory()
    let key = processId.ToString()
    if client.ContainsKey(key) then
        let processObj = StorageSerialization.deserialize (client.GetValue(key)) (StorageSerialization.settings)
        Some(processObj)
    else
        None
        
let createOrUpdateProcess (redisClientFactory:unit -> RedisClient) (processObj: Process) =
    let create (client: RedisClient) key =
        StorageSerialization.serialize (processObj) (StorageSerialization.settings)
        |> fun serialized -> client.Add (key, serialized)

    let update (client: RedisClient) key =
        StorageSerialization.serialize (processObj) (StorageSerialization.settings)
        |> fun serialized -> client.Replace (key, serialized)
    
    use client = redisClientFactory()
    let key = processObj.Id.ToString()
    if client.ContainsKey(key) then
        update client key |> ignore
    else
        create client key |> ignore
        
let removeProcess (redisClientFactory:unit -> RedisClient) processId =
    use client = redisClientFactory()    
    client.RemoveEntry(processId.ToString())