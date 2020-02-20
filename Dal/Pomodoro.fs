namespace pomodoapi.Dal

open pomodoapi.database
open pomodotypes
open System
open System.Security.Cryptography
open System.Text
open FSharp.Data
open pomodoapi.Helpers

module pomodoroDAL =

    let fromJson (jsonValue: JsonValue): Pomodoro =
        let id = tryGetProp jsonValue "id"
        let completionDate = tryGetProp jsonValue "completionDate"
        let elapsedTime = tryGetProp jsonValue "elapsedTime"
        let complete = tryGetProp jsonValue "complete"
        let userId = tryGetProp jsonValue "userId"
        let name = tryGetProp jsonValue "name"
        let tags = tryGetProp jsonValue "tags"

        {
            Id = if id = JsonValue.Null then Guid.Empty else id.AsGuid()
            CompletionDate = if completionDate = JsonValue.Null then DateTime.MinValue else completionDate.AsDateTime()
            ElapsedTime = if elapsedTime = JsonValue.Null then 1500 else elapsedTime.AsInteger()
            Complete = if complete = JsonValue.Null then true else complete.AsBoolean()
            UserId = if userId = JsonValue.Null then Guid.Empty else userId.AsGuid()
            Name = if name = JsonValue.Null then "default name" else name.AsString()
            Tags = if tags = JsonValue.Null then Array.empty else tags.AsArray() |> Array.map (fun (tag: JsonValue) -> tag |> pomodotagDAL.fromJson )
        }

    let fillWithUser (user: User) (pomo: Pomodoro): Pomodoro =
        {
            Id = pomo.Id
            CompletionDate = pomo.CompletionDate
            ElapsedTime = pomo.ElapsedTime
            Complete = pomo.Complete
            UserId = user.Id
            Name = pomo.Name
            Tags = pomo.Tags
        }

    let GET_TAGS_FROM_POMO = "SELECT Id, UserId, Name FROM pomodotag WHERE EXISTS (SELECT * FROM pomodorotag WHERE Id = TagId AND PomodoroId = @pomodoroId)"
    let DELETE_TAGS = "DELETE FROM pomodorotag WHERE pomodoroid = @id"
    let CREATE_TAG = "INSERT INTO pomodorotag (tagId, pomodoroId) VALUES (@tagId, @pomodoroId)"
    
    let GET_POMODORO = "SELECT Id, CompletionDate, ElapsedTime, Complete, UserId, Name FROM pomodoro WHERE userId = @userId"
    let CREATE_POMODORO = "INSERT INTO pomodoro (completionDate, elapsedTime, complete, userId, name) OUTPUT inserted.id VALUES (@completionDate, @elapsedTime, @complete, @userId, @name)"
    let UPDATE_POMODORO = "UPDATE pomodoro SET completionDate = @completionDate, elapsedTime = @elapsedTime, complete = @complete, userId = @userId, name = @name WHERE id = @id"
    let DELETE_POMODORO = "DELETE FROM pomodoro WHERE id = @id and userId = @userId"
    let GET_POMODORO_BY_DATERANGE = "SELECT Id, CompletionDate, ElapsedTime, Complete, UserId, Name FROM pomodoro WHERE userId = @userId AND completionDate <= @endDate AND completionDate >= @startDate"

    let fillWithDatabaseTag (list: DatabasePomodoro[]): Pomodoro[] =
        list |> Array.map (fun (pomo: DatabasePomodoro) ->
            {
                Id = pomo.Id
                CompletionDate = pomo.CompletionDate
                ElapsedTime = pomo.ElapsedTime
                Complete = pomo.Complete
                UserId = pomo.UserId
                Name = pomo.Name
                Tags =
                    databaseMapParameterizedQuery<Tag> GET_TAGS_FROM_POMO (Map [ "pomodoroId", pomo.Id :> obj ])
                        |> Seq.toArray
            })

    let deleteTagFromPomodoro (pomodoro: Pomodoro): unit =
        databaseMapParameterizedExecute DELETE_TAGS (Map [ "id", pomodoro.Id :> obj ]) |> ignore

    let createTagFromPomodoro (pomodoro: Pomodoro): unit =
            pomodoro.Tags
                |> Array.iter (fun tag -> databaseMapParameterizedExecute CREATE_TAG (Map [ "tagId", tag.Id :> obj; "pomodoroId", pomodoro.Id :> obj ]) |> ignore)

    let get (user: User): Pomodoro[] =
        databaseMapParameterizedQuery<DatabasePomodoro> GET_POMODORO (Map [ "userId", user.Id :> obj ])
            |> Seq.toArray
            |> fillWithDatabaseTag

    let getByDateRange (startDate: DateTime) (endDate: DateTime) (user: User): Pomodoro[] =
        databaseMapParameterizedQuery<DatabasePomodoro>
            GET_POMODORO_BY_DATERANGE
            (Map [
                "userId", user.Id :> obj
                "startDate", startDate :> obj
                "endDate", endDate :> obj
            ])
        |> Seq.toArray
        |> fillWithDatabaseTag

    let update (pomodoro: Pomodoro): unit =
        databaseMapParameterizedExecute
            UPDATE_POMODORO
            (Map [
                "id", pomodoro.Id :> obj
                "completionDate", pomodoro.CompletionDate :> obj
                "elapsedTime", pomodoro.ElapsedTime :> obj
                "complete", pomodoro.Complete :> obj
                "userId", pomodoro.UserId :> obj
                "name", pomodoro.Name :> obj
            ]) |> ignore


        pomodoro |> deleteTagFromPomodoro |> ignore
        pomodoro |> createTagFromPomodoro |> ignore
        
    let create (pomodoro: Pomodoro): Guid =

        let pomoId =
            databaseMapParameterizedQuerySingleOutputId
                CREATE_POMODORO
                (Map [
                    "completionDate", pomodoro.CompletionDate :> obj
                    "elapsedTime", pomodoro.ElapsedTime :> obj
                    "complete", pomodoro.Complete :> obj
                    "userId", pomodoro.UserId :> obj
                    "name", pomodoro.Name :> obj
                ])

        let newPomo =
            {
                Id = pomoId
                CompletionDate = pomodoro.CompletionDate
                ElapsedTime = pomodoro.ElapsedTime
                Complete = pomodoro.Complete
                UserId = pomodoro.UserId
                Name = pomodoro.Name
                Tags = pomodoro.Tags
            }

        newPomo |> createTagFromPomodoro |> ignore

        newPomo.Id

    let delete (pomodoro: Pomodoro): unit = // cascade in db so don't delete tags
        databaseMapParameterizedExecute
            DELETE_POMODORO
            (Map [
                "id", pomodoro.Id :> obj 
                "userId", pomodoro.UserId :> obj
            ]) |> ignore