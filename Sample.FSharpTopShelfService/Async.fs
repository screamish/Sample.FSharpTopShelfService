namespace Sample.FSharpTopShelfService

    open System.Threading
    open FSharp.Control.Observable

    [<AutoOpen>]
    module Async = 
      /// Returns an asynchronous workflow 'Async<Async<unit>>'. When called
      /// using 'let!', it starts the workflow provided as an argument and returns
      /// a token that can be used to cancel the started work - this is an
      /// (asynchronously) blocking operation that waits until the workflow
      /// is actually cancelled 
      let StartCancellable work = async {
        let cts = new CancellationTokenSource()
        // Creates an event used for notification
        let evt = new Event<_>()
        // Wrap the workflow with TryCancelled and notify when cancelled
        Async.Start(Async.TryCancelled(work, ignore >> evt.Trigger), cts.Token)
        // Return a workflow that waits for 'evt' and triggers 'Cancel'
        // after it attaches the event handler (to avoid missing event occurrence)
        let waitForCancel = Async.GuardedAwaitObservable evt.Publish cts.Cancel
        return async.TryFinally(waitForCancel, cts.Dispose) }