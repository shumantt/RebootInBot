module RebootInBot.Processing.Types

[<CLIMutable>]
type CountingState = {
    Count: int
    UserStarted: string
}

type ProcessState =
| CountingState of CountingState
| IdleState

[<CLIMutable>]
type ProcessConfig = {
    ExcludeMembers: array<string>
}

[<CLIMutable>]
type Process = {
    Id: int64
    State: ProcessState
    Config: ProcessConfig
}