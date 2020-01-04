module RebootInBot.Processing.Tests.UpdatesHandlingTests

open FsUnit
open RebootInBot.Processing
open RebootInBot.Processing.Types
open RebootInBot.Processing.UpdatesHandling
open Xunit

let private defaultCommand: IncomingCommand = { Type = StartTimer
                                                Source = Telegram
                                                ProcessId = 1L
                                                Data = { FromUser = "@testUser"
                                                         MachineName = Some "TestMachine" } }
let private defaultProcess: Process = { Id = defaultCommand.ProcessId
                                        State = IdleState
                                        Config = { ExcludeMembers = [| |] } }

[<Fact>]
let ``Start timer in new process`` () =
    let getProcess: int64 -> Process option =
        fun _ -> None
    let command = defaultCommand
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = All
                              MessageText = sprintf Constants.startingCountTemplate command.Data.MachineName.Value
                              ProcessId = command.ProcessId }
    proc |> should equal (Some { Id = command.ProcessId
                                 State = CountingState { Count = 10
                                                         UserStarted = command.Data.FromUser  }
                                 Config = { ExcludeMembers = [||] } })
  
[<Fact>]
let ``Start timer in idle process with excluded mentions`` () =
    let command = defaultCommand
    let baseProc = { defaultProcess with Config = { ExcludeMembers = [| "@someUser" |] } }
    let expectedProc = { baseProc with State = CountingState { Count = 10
                                                               UserStarted = command.Data.FromUser  } }
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = AllWithExcluded [| "@someUser" |]
                              MessageText = sprintf Constants.startingCountTemplate command.Data.MachineName.Value
                              ProcessId = command.ProcessId }
    proc |> should equal (Some expectedProc)
 
[<Fact>]
let ``Fail to start timer in running process`` () =
    let command = defaultCommand
    let proc = { defaultProcess with State = CountingState { Count = 5
                                                             UserStarted = "@testUser" } }
    let getProcess: int64 -> Process option =
        fun _ -> Some proc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = Specific [| command.Data.FromUser |]
                              MessageText = Constants.processAlreadyRunning
                              ProcessId = command.ProcessId }
    proc |> should equal None
    
[<Fact>]
let ``Fail to start timer in running process and do not mention excluded author`` () =
    let command = defaultCommand
    let proc = { defaultProcess with State = CountingState { Count = 5
                                                             UserStarted = "@testUser" }
                                     Config = { ExcludeMembers = [| command.Data.FromUser |] } }
    let getProcess: int64 -> Process option =
        fun _ -> Some proc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = Specific [| |]
                              MessageText = Constants.processAlreadyRunning
                              ProcessId = command.ProcessId }
    proc |> should equal None
    
    
[<Fact>]
let ``Cancel running count`` () =
    let command = { defaultCommand with Type = CancelTimer }
    let baseProc = { defaultProcess with State = CountingState { Count = 5
                                                                 UserStarted = "@startedUser" } }
    let expectedProc = { baseProc with State = IdleState }
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = Specific [| "@startedUser" |]
                              MessageText = Constants.cancelProcessMessage
                              ProcessId = command.ProcessId }
    proc |> should equal (Some expectedProc)
    
[<Fact>]
let ``Fail to cancel idle process`` () =
    let command = { defaultCommand with Type = CancelTimer }
    let baseProc = defaultProcess
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = NoOne
                              MessageText = Constants.errorCancelMessage
                              ProcessId = command.ProcessId }
    proc |> should equal None
    
[<Fact>]
let ``Exclude user for new process`` () =
    let command = { defaultCommand with Type = ExcludeMember }
    let getProcess: int64 -> Process option =
        fun _ -> None
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = NoOne
                              MessageText = Constants.excludedMentions
                              ProcessId = command.ProcessId }
    proc |> should equal (Some {defaultProcess with Config = { ExcludeMembers = [| command.Data.FromUser |] }})

[<Fact>]
let ``Exclude user for existing process`` () =
    let command = { defaultCommand with Type = ExcludeMember }
    let baseProc = { defaultProcess with Config = {ExcludeMembers = [| "@excludedUser" |]} }
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = NoOne
                              MessageText = Constants.excludedMentions
                              ProcessId = command.ProcessId }
    proc |> should equal (Some {baseProc with Config = { ExcludeMembers = [| "@excludedUser"; command.Data.FromUser |] }})
    
[<Fact>]
let ``Exclude already excluded user`` () =
    let command = { defaultCommand with Type = ExcludeMember }
    let baseProc = { defaultProcess with Config = {ExcludeMembers = [| command.Data.FromUser |]} }
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = NoOne
                              MessageText = Constants.excludedMentions
                              ProcessId = command.ProcessId }
    proc |> should equal (Some baseProc)
    
[<Fact>]
let ``Include user for new process`` () =
    let command = { defaultCommand with Type = IncludeMember }
    let getProcess: int64 -> Process option =
        fun _ -> None
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = Specific [| command.Data.FromUser |]
                              MessageText = Constants.includeMentions
                              ProcessId = command.ProcessId }
    proc |> should equal (Some defaultProcess)
    
[<Fact>]
let ``Include user for existing process`` () =
    let command = { defaultCommand with Type = IncludeMember }
    let baseProc = { defaultProcess with Config = { ExcludeMembers = [| command.Data.FromUser; "@excluded" |] } }
    let getProcess: int64 -> Process option =
        fun _ -> Some baseProc
    
    let (message, proc) = handleIncomingCommand command getProcess
    
    message |> should equal { Mentions = Specific [| command.Data.FromUser |]
                              MessageText = Constants.includeMentions
                              ProcessId = command.ProcessId }
    proc |> should equal (Some {baseProc with Config = { ExcludeMembers = [| "@excluded" |] }})