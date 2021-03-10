module RebootInBot.Mentions

let buildMentionList author participants =
    participants
    |> Seq.except (Seq.singleton author) 