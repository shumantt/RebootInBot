module RebootInBot.Tests.MentionsTests

open RebootInBot.Mentions
open Xunit
open FsUnit.Xunit


[<Fact>]
let  ``buildMentionList should return empty if no participant`` () =
    let actual = buildMentionList "author" List.empty
    
    Assert.Equal(List.empty, actual)
    
[<Fact>]
let ``buildMentionList should except author from list`` () =
    let actual = buildMentionList "author" [ "author"; "notAuthor" ]
    
    Assert.Equal(["notAuthor"], actual)