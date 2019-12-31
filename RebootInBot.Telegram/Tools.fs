module RebootInBot.Telegram.Tools

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let private serializeSettings =
    JsonSerializerSettings(
        ContractResolver =
            DefaultContractResolver(
                NamingStrategy =
                    SnakeCaseNamingStrategy()))
let internal parseJson<'a> (data: string) =
  JsonConvert.DeserializeObject<'a>(data, serializeSettings) 