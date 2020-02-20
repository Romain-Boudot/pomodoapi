namespace pomodoapi

open pomodoapi.appConfig
open System.Data.SqlClient
open Dapper
open System.Dynamic
open System.Collections.Generic
open System.Threading.Tasks
open System

module database =

    let connSTR = appConfig.Item("database:connstr")
    let db = new SqlConnection(connSTR)

    let databaseQuerySingleOutputId (query: string): Guid =
        db.QuerySingle<Guid> query

    let databaseMapParameterizedQuerySingleOutputId (query: string) (param: Map<string, obj>) : Guid =
        let expando = ExpandoObject()
        let expandoDictionary = expando :> IDictionary<string, obj>
        for paramValue in param do
            expandoDictionary.Add(paramValue.Key, paramValue.Value)
        db.QuerySingle<Guid> (query, expando)

    /// Execute a query `query` on the database and return the number of affected lines
    let databaseExecute (query: string): int =
        db.Execute query

    /// Execute a query `query` on the database with param `param` and return the number of affected lines
    let databaseMapParameterizedExecute (query: string) (param: Map<string, obj>) : int =
        let expando = ExpandoObject()
        let expandoDictionary = expando :> IDictionary<string, obj>
        for paramValue in param do
            expandoDictionary.Add(paramValue.Key, paramValue.Value)
        db.Execute (query, expando)

    let databaseQuery<'R> (query: string) : 'R seq  =
        db.Query<'R>(query)
    
    let databaseMapParameterizedQuery<'R> (query: string) (param: Map<string, obj>) : 'R seq =
        let expando = ExpandoObject()
        let expandoDictionary = expando :> IDictionary<string, obj>
        for paramValue in param do
            expandoDictionary.Add(paramValue.Key, paramValue.Value)
        db.Query<'R>(query, expando)