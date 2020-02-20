namespace pomodoapi

open System.Text
open Jose
open System
open Exceptions
open FSharp.Data

module JwtHandler =
    let key = "MySecretPasswordSuperStrong" |> Encoding.UTF8.GetBytes

    let now (): Int64 =
        DateTimeOffset.Now.ToUnixTimeSeconds()
    
    let createToken (payload: Map<string, obj>): string =
        let finalPayload =
            payload
                .Add("iat", now())
                .Add("exp", now() + int64 86400)

        JWT.Encode(finalPayload, key, JwsAlgorithm.HS256)

    let decodeAndVerifyToken (token: string): JsonValue =
        let payload = JWT.Decode(token, key, JwsAlgorithm.HS256)
        let parsedPayload = JsonValue.Parse(payload)
        if (parsedPayload.GetProperty("exp").AsInteger64() >= now()) then
            parsedPayload
        else
            raise (TokenParseFailedException "Failed to parse token")

    let getPayload (token: string) =
        JWT.Decode(token, key, JwsAlgorithm.HS256)
