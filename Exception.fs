namespace pomodoapi

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open System.Text
open Microsoft.AspNetCore.Diagnostics

module Exceptions =
    
    exception TokenParseFailedException of string // 400

    exception BadRequestException of string     // 400
    exception UnauthorizedException of string   // 401
    exception ForbiddenException of string      // 403
    exception NotFoundException of string       // 404
    exception ConflictException of string       // 409

    let messageToJsonError (code:  int) (message: string) =
        "{\"error\":\"" + message + "\",\"status\":" + code.ToString() + "}"

    let setResponse (code: int) (message: string) (context: HttpContext): Task =
        context.Response.StatusCode <- code
        //context.Response.BodyWriter.WriteAsync(ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(messageToJsonError code message))) |> ignore
        context.Response.Body.WriteAsync(ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(messageToJsonError code message))) |> ignore
        Task.CompletedTask

    let ExceptionHandler (appBuilder: IApplicationBuilder) =
        appBuilder.Run(fun (context: HttpContext) ->
            
            let exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

            match exceptionHandlerPathFeature.Error with
            | BadRequestException   message -> setResponse 400 message context
            | UnauthorizedException message -> setResponse 401 message context
            | ForbiddenException    message -> setResponse 403 message context
            | NotFoundException     message -> setResponse 404 message context
            | ConflictException     message -> setResponse 409 message context
            | TokenParseFailedException msg -> setResponse 400 msg context
            | _ as e -> setResponse 500 ("Unhandled Exception: " + e.Message) context
        )