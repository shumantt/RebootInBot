module CommandTests

open FsUnit
open RebootInBot.Processing.Types
open System
open Xunit
open RebootInBot.Telegram.Commands
open RebootInBot.Telegram.Types

let commonMessage: Message = { MessageId = 1L
                               From = Some { Id = 1L
                                             IsBot = false
                                             FirstName = "TestName"
                                             LastName = None
                                             Username = Some "@testUser"
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
    
    actual |> should equal None
    
[<Fact>]
let ``None when parsing message with unknown command`` () =
    let message = { commonMessage with Text = Some "/unknown testMachine";
                                        Entities = Some (seq { { Type = botCommandType
                                                                 Offset = 0L
                                                                 Length = int64("/unknown".Length)
                                                                 Url = None
                                                                 User = None} }) }
    
    let actual = parseIncomingCommand message
    
    actual |> should equal None
    
[<Theory>]
[<MemberData("knownCommands")>]
let ``Known command with machine param`` (command:string) (expectedType: IncomingCommandType) =
    let testMachine = "testMachine"
    let fullCommandText = command + " " + testMachine;
    let message = { commonMessage with Text = Some fullCommandText;
                                        Entities = Some (seq { { Type = botCommandType
                                                                 Offset = 0L
                                                                 Length = int64(command.Length)
                                                                 Url = None
                                                                 User = None} }) }    
    let actual = parseIncomingCommand message

    let expected = Some { Type = expectedType
                          Source = Telegram
                          ProcessId = message.Chat.Id
                          Data = { MachineName = Some testMachine
                                   FromUser = message.From.Value.Username } }
    actual |> should equal expected

[<Theory>]
[<MemberData("knownCommands")>]
let ``Known command without machine param`` (command:string) (expectedType: IncomingCommandType) =
    let message = { commonMessage with Text = Some command;
                                        Entities = Some (seq { { Type = botCommandType
                                                                 Offset = 0L
                                                                 Length = int64(command.Length)
                                                                 Url = None
                                                                 User = None} }) }    
    let actual = parseIncomingCommand message

    let expected = Some { Type = expectedType
                          Source = Telegram
                          ProcessId = message.Chat.Id
                          Data = { MachineName = None
                                   FromUser = message.From.Value.Username } }
    actual |> should equal expected
  
let knownCommands =
    let reboot: obj[] = [|"/reboot"; IncomingCommandType.StartTimer|]
    let cancel: obj[] = [|"/cancel"; IncomingCommandType.CancelTimer|]
    let exclude: obj[] =  [|"/exclude"; IncomingCommandType.ExcludeMember|]
    [| reboot; cancel; exclude |]