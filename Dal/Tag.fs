namespace pomodoapi.Dal

open pomodoapi.database
open pomodotypes
open System
open FSharp.Data
open pomodoapi.Helpers

module pomodotagDAL =

    let fromJson (jsonValue: JsonValue): Tag =
        let id = tryGetProp jsonValue "id"
        let userId = tryGetProp jsonValue "userId"
        let name = tryGetProp jsonValue "name"

        {
            Id = if id = JsonValue.Null then Guid.Empty else id.AsGuid()
            Name = if name = JsonValue.Null then "default name" else name.AsString()
            UserId = if userId = JsonValue.Null then Guid.Empty else userId.AsGuid()
        }

    let fillWithUser (user: User) (tag: Tag): Tag =
        {
            Id = tag.Id
            UserId = user.Id
            Name = tag.Name
        }
    
    let GET_TAG = "SELECT Id, UserId, Name FROM pomodotag WHERE userId = @userId"
    let UPDATE_TAG = "UPDATE pomodotag SET name = @name, userId = @userId WHERE id = @id"
    let CREATE_TAG = "INSERT INTO pomodotag (name, userId) OUTPUT inserted.id VALUES (@name, @userId)"
    let DELETE_TAG = "DELETE FROM pomodotag WHERE id = @id and userId = @userId"

    let get (user: User): Tag[] =
        databaseMapParameterizedQuery<Tag>
            GET_TAG
            (Map [ "userId", user.Id :> obj ])
        |> Seq.toArray

    let update (tag: Tag): unit =        
        databaseMapParameterizedExecute
            UPDATE_TAG
            (Map [
                "id", tag.Id :> obj
                "userId", tag.UserId :> obj
                "name", tag.Name :> obj
            ]) |> ignore

    let create (tag: Tag): Guid =        
        databaseMapParameterizedQuerySingleOutputId
            CREATE_TAG
            (Map [
                "userId", tag.UserId :> obj
                "name", tag.Name :> obj
            ])
        
    let delete (tag: Tag): unit =
        databaseMapParameterizedExecute
            DELETE_TAG
            (Map [
                "userId", tag.UserId :> obj
                "id", tag.Id :> obj
            ]) |> ignore