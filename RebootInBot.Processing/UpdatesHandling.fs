module RebootInBot.Processing.UpdatesHandling

open RebootInBot.Processing.Types

let private buildDefaultProcess id = { Id = id
                                       State = IdleState
                                       Config = { ExcludeMembers = [||] } }

let private buildMentions (toMention:array<string> option) (toExclude:array<string> option) =
    match toMention with
    | Some someToMention -> match toExclude with
                            | Some someToExclude -> Mentions.Specific (Array.except someToExclude someToMention)
                            | _ -> Mentions.Specific someToMention
    | _ ->  match toExclude with
            | Some someToExclude -> Mentions.AllWithExcluded someToExclude
            | _ -> Mentions.All

let private handleStartTimer (command:IncomingCommand) getProcess: NewMessage * Process option =
    let buildAlreadyRunningCommand userStarted excludedMentions =
        { Mentions = buildMentions (Some [| userStarted |]) (Some excludedMentions)
          MessageText = Constants.processAlreadyRunning
          ProcessId = command.ProcessId }
    
    let startNewCounting (savedProcess:Process option) =
        let buildCommand () =
            let exludedMentions = savedProcess |> Option.bind(fun x -> Some x.Config.ExcludeMembers)                                  
            { Mentions = buildMentions None exludedMentions
              MessageText = match command.Data.MachineName with
                            | Some x ->  sprintf Constants.startingCountTemplate x
                            | _ -> Constants.noMachineStartCount
              ProcessId = command.ProcessId }
        
        let buildProcessToUpdate () =
            let buildCountingState =
                CountingState { Count = 10
                                UserStarted = command.Data.FromUser }
            match savedProcess with
            | Some x -> { x with State = buildCountingState }
            | _ -> { buildDefaultProcess command.ProcessId with State = buildCountingState }
        
        (buildCommand (), Some (buildProcessToUpdate ()))
        
    
    let proc = getProcess command.ProcessId
    match proc with
    | Some savedProcess ->
                       match savedProcess.State with
                       | CountingState _ -> buildAlreadyRunningCommand command.Data.FromUser savedProcess.Config.ExcludeMembers, None
                       | IdleState -> startNewCounting (Some savedProcess)
    | _ -> startNewCounting None
   
let private handleCancelTimer (command:IncomingCommand) getProcess =
    let buildCanceledMessage startedUser excludeMentions =
        { Mentions = buildMentions (Some [|startedUser|]) (Some excludeMentions)
          MessageText = Constants.cancelProcessMessage
          ProcessId = command.ProcessId }
    
    let handleCancel (proc:Process) (user) =
        (buildCanceledMessage user proc.Config.ExcludeMembers, Some { proc with State = IdleState })
    
    let buildErrorCommand =
        { ProcessId = command.ProcessId
          MessageText = Constants.errorCancelMessage
          Mentions = Mentions.NoOne }
    
    let proc = getProcess command.ProcessId
    match proc with
    | Some savedProcess ->
                       match savedProcess.State with
                       | CountingState counting ->  handleCancel savedProcess counting.UserStarted
                       | IdleState -> buildErrorCommand, None
    | _ -> buildErrorCommand, None

let private handlerExcludeMember (command:IncomingCommand) getProcess =
    let proc = getProcess command.ProcessId
    let updateProc: Process =
        let updateExcludeMembers excluded =
            if excluded |> Array.contains command.Data.FromUser
            then
                excluded
            else
                Array.concat [excluded; [| command.Data.FromUser |]]
        
        match proc with
        | Some savedProcess -> { savedProcess with Config = { ExcludeMembers = updateExcludeMembers savedProcess.Config.ExcludeMembers } }
        | _ -> { buildDefaultProcess command.ProcessId with Config = { ExcludeMembers = [| command.Data.FromUser |] } }
        
    let message = 
            { Mentions = Mentions.NoOne
              MessageText = Constants.excludedMentions
              ProcessId = command.ProcessId }
    message, Some updateProc

let private handleIncludeMember (command:IncomingCommand) getProcess =
    let proc = getProcess command.ProcessId
    let updateProc: Process =
        match proc with
        | Some savedProcess -> { savedProcess with Config = { ExcludeMembers =
                                                                savedProcess.Config.ExcludeMembers
                                                                |> Array.filter(fun x -> x <> command.Data.FromUser) } }
        | _ -> buildDefaultProcess command.ProcessId
        
    let message = 
        { Mentions = Mentions.Specific [| command.Data.FromUser |]
          MessageText = Constants.includeMentions
          ProcessId = command.ProcessId }
    message, Some updateProc

let handleIncomingCommand (command: IncomingCommand) (getProcess: int64-> Process option) =
    match command.Type with
    | IncomingCommandType.StartTimer -> handleStartTimer command getProcess
    | IncomingCommandType.CancelTimer -> handleCancelTimer command getProcess
    | IncomingCommandType.ExcludeMember -> handlerExcludeMember command getProcess
    | IncomingCommandType.IncludeMember -> handleIncludeMember command getProcess
    
let handleUpdate (command: IncomingCommand): OutgoingCommand =
    let getProcess = ProcessStorage.getProcess ProcessStorage.defaultRedisClientFactory
    let (outgoingCommand, processToSave) = handleIncomingCommand command getProcess
    match processToSave with
    | Some x -> ProcessStorage.createOrUpdateProcess ProcessStorage.defaultRedisClientFactory x
    | _ -> ()
    NewMessage outgoingCommand