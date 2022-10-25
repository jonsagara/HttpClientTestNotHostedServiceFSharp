namespace HttpClientTestNotHostedServiceFSharp

open Serilog.Core
open Serilog.Events

/// Serilog enricher that convers the log event's timestamp to UTC.
type UtcTimestampEnricher() =
    interface ILogEventEnricher with
        member this.Enrich (logEvent : LogEvent, lepf : ILogEventPropertyFactory) =
            logEvent.AddPropertyIfAbsent(lepf.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime))