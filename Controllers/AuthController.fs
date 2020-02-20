namespace pomodoapi.Controllers

open pomodotypes
open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Text
open System.IO
open FSharp.Data
open pomodoapi.Exceptions
open pomodoapi.JwtHandler
open pomodoapi.Helpers
open pomodoapi.Dal
open pomodoapi.Filters.AuthFilter

type model(test) =
    member this.test: string = test

[<ApiController>]
type AuthController (logger : ILogger<AuthController>) =
    inherit ControllerBase()

    [<HttpGet>]
    [<Route("api/login")>]
    member __.Login() : string =
        let mutable credential: string = null

        match __.HttpContext.Request.Headers.TryGetValue("authorization") with
        | true, token -> credential <- string token
        | _           -> credential <- ""

        if credential = "" then
            raise (BadRequestException "no credentials")

        let splitedAndDecodedCredentials =
            ((credential.Split ' ').[1]
                |> Convert.FromBase64String
                |> Encoding.UTF8.GetString).Split ':'

        try
            let user = pomodouserDAL.getByCrendentials splitedAndDecodedCredentials.[0] splitedAndDecodedCredentials.[1]
            let payload = Map ["id", user.Id :> obj]
            let token = payload |> createToken
            token
        with
            | _ -> raise (ConflictException "conflit")

    [<HttpGet>]
    [<AuthFilter>]
    [<Route("api/renew")>]
    member __.Renew() : string =
        let user = getUserFromHttpContext __.HttpContext
        Map ["id", user.Id :> obj]
            |> createToken

    [<HttpPost>]
    [<Route("api/register")>]
    member __.Register() : string =

        let body =
            new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse

        let user = body |> pomodouserDAL.fromJson

        let password = body.GetProperty("password").AsString()

        try
            __.HttpContext.Response.StatusCode <- 201 // created
            (user |> pomodouserDAL.create password).ToString()
        with
            | _ -> raise (ConflictException "allready used email")
