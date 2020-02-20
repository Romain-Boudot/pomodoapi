namespace pomodoapi.Dal

open pomodoapi.database
open pomodotypes
open System
open FSharp.Data
open pomodoapi.Helpers

module pomodouserDAL =

    let fromJson (jsonValue: JsonValue): User =
        let id = tryGetProp jsonValue "id"

        {
            Id = if id = JsonValue.Null then Guid.Empty else id.AsGuid()
            Email = jsonValue.["email"].AsString()
            FullName = jsonValue.["fullname"].AsString()
        }
    
    let GET_USER_BY_ID = "SELECT Id, Email, FullName FROM pomodouser WHERE id = @id"
    let GET_USER_BY_CREDENTIAL = "SELECT Id, Email, FullName FROM pomodouser WHERE email = @email AND password = @password"
    let CREATE_USER = "INSERT INTO pomodouser (password, email, fullname) OUTPUT inserted.id VALUES (@password, @email, @fullname)"
    let GET_USERS = "SELECT Id, Email, FullName FROM pomodouser"
    let DELETE_USER = "DELETE FROM pomodouser WHERE id = @id"
    let DELETE_TAGS = "DELETE FROM pomodotag WHERE userId = @id"
    let DELETE_POMODOROS = "DELETE FROM pomodoro WHERE userId = @id"

    let getByCrendentials (email: string) (password: string): User =
        databaseMapParameterizedQuery<User>
            GET_USER_BY_CREDENTIAL
            (Map ["email", email :> obj; "password", password |> SHA256 :> obj])
        |> Seq.head<User>

    let getById (id: Guid): User =
        databaseMapParameterizedQuery<User>
            GET_USER_BY_ID
            (Map ["id", id :> obj])
        |> Seq.head<User>

    let get (): User[] =
        databaseQuery<User>
            GET_USERS
        |> Seq.toArray

    let create (password: string) (user: User): Guid =        
        databaseMapParameterizedQuerySingleOutputId
            CREATE_USER
            (Map [
                "email", user.Email :> obj
                "password", password |> SHA256 :> obj
                "fullname", user.FullName :> obj
            ])
        
    let delete (user: User): unit =
        databaseMapParameterizedExecute
            DELETE_TAGS
            (Map [ "id", user.Id :> obj ])
        |> ignore

        databaseMapParameterizedExecute
            DELETE_POMODOROS
            (Map [ "id", user.Id :> obj ])
        |> ignore

        databaseMapParameterizedExecute
            DELETE_USER
            (Map [ "id", user.Id :> obj ])
        |> ignore