module RebootInBot.TimerProcess

let createTimer timerInterval runningTime =
    let timer = new System.Timers.Timer(float timerInterval)

    timer.AutoReset <- true

    let observable = timer.Elapsed

    let task =
        async {
            timer.Start()
            do! Async.Sleep runningTime
            timer.Stop()
        }

    (task, observable)
