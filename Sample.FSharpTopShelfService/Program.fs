namespace Sample.FSharpTopShelfService

open log4net.Config
open Topshelf
open FSharp.Control

module Program =

    let counterFactory n = async {
        let inner i = async {
            let log = getLogger()
            log.Info "Entered inner with %d" i
            do! Async.Sleep 500
            log.Info "Leaving inner with %d" i
        }
        let log = getLogger()
        log.Info "Starting to count"
        do! seq { 1 .. n }
            |> AsyncSeq.ofSeq
            |> AsyncSeq.iterAsync inner
        log.Info "Finished counting"
    }

    [<EntryPoint>]
    let main argv = 
        XmlConfigurator.Configure() |> ignore
        let log = LogManager.getLogger()

        configureTopShelf <| fun conf ->
            conf |> useLog4Net
            conf |> runAsLocalSystem
                
            "A simple counting service that can be cancelled" |> description conf
            "FSharpTopShelfService" |> displayName conf
            "FSharpTopShelfService" |> serviceName conf

            let testSCFactory() = TopshelfEx.createServiceControlFromAsyncWorkflow <| fun () -> counterFactory 30
            service conf testSCFactory
