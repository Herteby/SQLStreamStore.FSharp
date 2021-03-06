namespace SqlStreamStore.FSharp.Postgres

open SqlStreamStore.FSharp

type PostgresConfig =
    { host: string
      port: string
      username: string
      password: string
      database: string }

type PoolingConfig =
    { minPoolSize: int option
      maxPoolSize: int option
      connectionIdleLifeTime: int option
      connectionPruningInterval: int option }

module Postgres =
    let private storeSettings (config: PostgresConfig): string =
        sprintf
            "Host=%s;Port=%s;User Id=%s;Password=%s;Database=%s"
            config.host
            config.port
            config.username
            config.password
            config.database

    let private poolingSettings (pooling: PoolingConfig): string =
        let minSize =
            pooling.minPoolSize |> Option.defaultValue 0

        let maxSize =
            pooling.maxPoolSize |> Option.defaultValue 100

        let connectionPruningInterval =
            pooling.connectionPruningInterval
            |> Option.defaultValue 300

        let connectionIdleLifeTime =
            pooling.connectionIdleLifeTime
            |> Option.defaultValue 10

        sprintf
            "Minimum Pool Size=%d;Maximum Pool Size=%d;Connection Idle Lifetime=%d;Connection Pruning Interval%d"
            minSize
            maxSize
            connectionPruningInterval
            connectionIdleLifeTime

    let private storeSettingsWithPooling (config: PostgresConfig) (pooling: PoolingConfig): string =
        sprintf "%s;%s" (storeSettings config) (poolingSettings pooling)

    let createStore (config: PostgresConfig): SqlStreamStore.PostgresStreamStore =
        new SqlStreamStore.PostgresStreamStore(SqlStreamStore.PostgresStreamStoreSettings(storeSettings config))

    let createStoreWithPoolingConfig (config: PostgresConfig) (pooling: PoolingConfig): SqlStreamStore.PostgresStreamStore =
        new SqlStreamStore.PostgresStreamStore(SqlStreamStore.PostgresStreamStoreSettings
                                                   (storeSettingsWithPooling config pooling))

    let createStoreWithConfigString (config: string): SqlStreamStore.PostgresStreamStore =
        new SqlStreamStore.PostgresStreamStore(SqlStreamStore.PostgresStreamStoreSettings(config))

    let createSchemaRaw (store: SqlStreamStore.PostgresStreamStore): Async<unit> =
        async.Delay(fun () -> store.CreateSchemaIfNotExists() |> Async.AwaitTask)

    let createSchema (store: SqlStreamStore.PostgresStreamStore): Async<Result<unit, string>> =
        createSchemaRaw store
        |> ExceptionsHandler.asyncExceptionHandler
