namespace pomodoapi.Filters

open Microsoft.AspNetCore.Mvc.Filters
open System.Threading.Tasks
open FSharp.Data
open pomodoapi.JwtHandler
open pomodoapi.Exceptions
open pomodoapi.Dal

module AuthFilter =

    type AuthFilter() =
        inherit ActionFilterAttribute() with
    
        override x.OnActionExecutionAsync(ctx: ActionExecutingContext, next: ActionExecutionDelegate) =
            match ctx.HttpContext.Request.Headers.TryGetValue("authorization") with
                | true, stringToken ->
                    try
                        let token =
                            ((string stringToken).Split ' ').[1]
                                |> decodeAndVerifyToken

                        let user =
                            token.GetProperty("id").AsGuid()
                                |> pomodouserDAL.getById

                        ctx.HttpContext.Items.Add("user", user)

                        next.Invoke() :> Task
                    with
                        | e -> Task.FromException(UnauthorizedException e.Message)
                | _ -> Task.FromException(UnauthorizedException "invalid token or no token provided")