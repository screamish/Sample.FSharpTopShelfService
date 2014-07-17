namespace Sample.FSharpTopShelfService

    open log4net.Config
    open Topshelf
    open FSharp.Control
    
    [<AutoOpen>]
    module TopshelfEx =
        let createServiceControlFromAsyncWorkflow workflowFactory =
            let cancellationSequence = ref <| async.Zero()
            let startAsyncWork() = async {
                let wrapped = async {
                    do! workflowFactory()
                    cancellationSequence := async.Zero()
                }
                let! ct = Async.StartCancellable <| wrapped
                cancellationSequence := ct
            }
            let cancelAsyncWork() = async {
                // evaluating the cancellationSequence will trigger the async workflow started in the 'start' function to cancel
                do! !cancellationSequence
                cancellationSequence := async.Zero()
            }
            let log = LogManager.getLogger()

            let start hc =
                log.Info "Starting async workflow"
                Async.RunSynchronously <| startAsyncWork()
                log.Info "Async workflow started"; true

            let stop hc =
                log.Info "Stopping async workflow"
                Async.RunSynchronously <| cancelAsyncWork()
                log.Info "Async workflow stopped"; true

            serviceControl start stop
