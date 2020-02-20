namespace pomodoapi.Controllers

open pomodotypes
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open pomodoapi.Filters.AuthFilter
open System.IO
open FSharp.Data
open pomodoapi.Dal
open pomodoapi.Helpers
open pomodoapi.Exceptions
open System

[<ApiController>]
[<Route("api/[controller]")>]
type PomodoroController (logger : ILogger<PomodoroController>) =
    inherit ControllerBase()

    [<HttpGet>]
    [<AuthFilter>]
    member __.Get() : Pomodoro[] =
        getUserFromHttpContext __.HttpContext
            |> pomodoroDAL.get

    [<HttpGet("{startDate}/{endDate}")>]
    [<AuthFilter>]
    member __.GetByDateRange(startDate: DateTime, endDate: DateTime) : Pomodoro[] =
        getUserFromHttpContext __.HttpContext
            |> pomodoroDAL.getByDateRange startDate endDate

    [<HttpPost>]
    [<AuthFilter>]
    member __.Post() : string =
        let user = getUserFromHttpContext __.HttpContext
        __.HttpContext.Response.StatusCode <- 201
        try
            (new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodoroDAL.fromJson
                |> pomodoroDAL.fillWithUser user
                |> pomodoroDAL.create).ToString()
        with
            | _ -> raise (BadRequestException "Bad request")

    [<HttpPut>]
    [<AuthFilter>]
    member __.Put() : unit =
        let user = getUserFromHttpContext __.HttpContext
        try
            new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodoroDAL.fromJson
                |> pomodoroDAL.fillWithUser user
                |> pomodoroDAL.update
        with
            | _ -> raise (BadRequestException "Bad request")

    [<HttpDelete>]
    [<AuthFilter>]
    member __.Delete() : unit =
        let user = getUserFromHttpContext __.HttpContext
        try
            new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodoroDAL.fromJson
                |> pomodoroDAL.fillWithUser user
                |> pomodoroDAL.delete
        with
            | _ -> raise (BadRequestException "Bad request")