namespace pomodoapi

open Microsoft.Extensions.Configuration
open System.IO

module appConfig = 

    let builder = 
        let ret = new ConfigurationBuilder()
        FileConfigurationExtensions.SetBasePath(ret, Directory.GetCurrentDirectory()) |> ignore
        JsonConfigurationExtensions.AddJsonFile(ret, "appSettings.json")

    let appConfig = builder.Build()