module RebootInBot.Mentions

let buildMentionList author participants =
    List.except (List.singleton author) participants