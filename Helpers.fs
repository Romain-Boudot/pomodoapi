namespace pomodoapi

open FSharp.Data
open Microsoft.AspNetCore.Http
open pomodotypes
open pomodoapi.Exceptions
open System.IO
open System
open System.Text
open System.Linq
open System.Security.Cryptography

module Helpers = 
    
    let tryGetProp (value: JsonValue) (propertieName: string): JsonValue =
        match value.TryGetProperty(propertieName) with
            | Some prop -> prop
            | _ -> JsonValue.Null

    let getUserFromHttpContext (httpcontext: HttpContext): User =
        match httpcontext.Items.TryGetValue("user") with
            | true, user -> user :?> User
            | _ -> raise(TokenParseFailedException "No user found for this token")

    let readStream (stream: StreamReader) =
        let mutable valid = true
        let mutable string = new StringWriter()
        while (valid) do
            let line = stream.ReadLine()
            if (line = null) then
                valid <- false
            else
                string.Write line
        string.ToString()

    let SHA256 (value: string): string =
        String.Join("",
            (value
                |> Encoding.UTF8.GetBytes
                |> SHA256.Create().ComputeHash
                ).Select(fun item -> item.ToString("x2"))
        )