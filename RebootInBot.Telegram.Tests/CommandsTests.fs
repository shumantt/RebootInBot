module Tests

open RebootInBot.Processing.Types
open RebootInBot.Telegram
open System
open System
open Xunit
open RebootInBot.Telegram.Commands
open RebootInBot.Telegram.Types

let commonMessage: Message = { MessageId = 1L
                               From = Some { Id = 1L
                                             IsBot = false
                                             FirstName = "TestName"
                                             LastName = None
                                             Username = Some "testUser"
                                             LanguageCode = None }
                               Date = DateTime.Now
                               Chat = { Id = 1L
                                        Type = Group }
                               EditDate = None
                               Text = Some "/reboot testMachine"
                               Entities = Some (seq { { Type = botCommandType
                                                        Offset = 0L
                                                        Length = int64("/reboot".Length)
                                                        Url = None
                                                        User = None} })  }

[<Fact>]
let ``None when parsing message without command`` () =
    let message = { commonMessage with Entities = None}
    
    let actual = parseIncomingCommand message
    
    Assert.Equal<IncomingCommand option>(actual, None)
    
[<Fact>]
let ``None when parsing message with unknown command`` () =
    let message = { commonMessage with Text = Some "/unknown testMachine";
                                        Entities = Some (seq { { Type = botCommandType
                                                                 Offset = 0L
                                                                 Length = int64("/unknown".Length)
                                                                 Url = None
                                                                 User = None} }) }
    
    let actual = parseIncomingCommand message
    
    Assert.Equal<IncomingCommand option>(actual, None)