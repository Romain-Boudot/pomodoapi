namespace pomodoapi.Controllers

open pomodotypes
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open pomodoapi.Filters.AuthFilter
open pomodoapi.Helpers
open pomodoapi.Dal

[<ApiController>]
[<Route("api/[controller]")>]
type UserController (logger : ILogger<UserController>) =
    inherit ControllerBase()

    [<HttpGet>]
    [<AuthFilter>]
    member __.Get() : User =
        getUserFromHttpContext __.HttpContext

    [<HttpDelete>]
    [<AuthFilter>]
    member __.Delete() : unit =
        getUserFromHttpContext __.HttpContext
            |> pomodouserDAL.delete
