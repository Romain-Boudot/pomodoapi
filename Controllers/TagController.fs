namespace pomodoapi.Controllers

open pomodotypes
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FSharp.Data
open System.IO
open pomodoapi.Dal
open pomodoapi.Helpers
open pomodoapi.Exceptions
open pomodoapi.Filters.AuthFilter
open System

[<ApiController>]
[<Route("api/[controller]")>]
type TagController (logger : ILogger<TagController>) =
    inherit ControllerBase()

    [<HttpGet>]
    [<AuthFilter>]
    member __.Get() : Tag[] =
        getUserFromHttpContext __.HttpContext
            |> pomodotagDAL.get

    [<HttpDelete>]
    [<AuthFilter>]
    member __.Delete() : unit =
        let user = getUserFromHttpContext __.HttpContext
        try
            new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodotagDAL.fromJson
                |> pomodotagDAL.fillWithUser user
                |> pomodotagDAL.delete
        with
            | _ -> raise (BadRequestException "Bad request")

    [<HttpPut>]
    [<AuthFilter>]
    member __.Put() : unit  =
        let user = getUserFromHttpContext __.HttpContext
        try
            new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodotagDAL.fromJson
                |> pomodotagDAL.fillWithUser user
                |> pomodotagDAL.update
        with
            | _ -> raise (BadRequestException "Bad request")

    [<HttpPost>]
    [<AuthFilter>]
    member __.Post() : string  =
        let user = getUserFromHttpContext __.HttpContext
        try
            __.HttpContext.Response.StatusCode <- 201
            (new StreamReader(__.HttpContext.Request.Body)
                |> readStream
                |> JsonValue.Parse
                |> pomodotagDAL.fromJson
                |> pomodotagDAL.fillWithUser user
                |> pomodotagDAL.create).ToString()
        with
            | _ -> raise (BadRequestException "Bad request")
