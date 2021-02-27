module RebootInBot.Tests

open RebootInBot.Mentions
open Xunit
open FsUnit


[<Fact>]
let  ``buildMentionList should return empty if no participant`` () =
    let actual = buildMentionList "author" List.empty
    
    actual |> should equivalent List.empty
    
[<Fact>]
let ``buildMentionList should except author from list`` () =
    let actual = buildMentionList "author" [ "author"; "notAuthor" ]
    
    actual |> should equivalent ["notAuthor"]