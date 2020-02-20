namespace pomodotypes

open System

type User =
    { Id: Guid
      Email: string
      FullName: string }

type Tag = 
    { Id: Guid
      UserId: Guid
      Name: string }

type Pomodoro = 
    { Id: Guid
      CompletionDate: System.DateTime
      ElapsedTime: int
      Complete: bool
      UserId: Guid
      Name: string
      Tags: Tag[] }

type DatabasePomodoro =
    { Id: Guid
      CompletionDate: System.DateTime
      ElapsedTime: int
      Complete: bool
      UserId: Guid
      Name: string }