# A command line tool to execute WIQL and pull the heirarchy

This tool allows you to execute ad-hoc WIQL from the command line and returns JSON for usage with tools like `jq`. 

The result takes the form of the original work items returned from Azure DevOps but with added hierarchy of their parent items. 

```json
[
    {
        "Object": {the object from the wiql},
        "Url": {url from Azure DevOps}
        "Parents": [
            {
                "Object": {the object from the wiql},
                "Url": {url from Azure DevOps}
            },
            {
                "Object": {the object from the wiql},
                "Url": {url from Azure DevOps}
            }
        ]
    }
]
```

Parents are flat, but ordered from most senior to immediate. I.e. the first item is the top of the hierarchy. 

## Requirements

You'll need to install the .NET Core 2.2 SDK (for now - still working on builds etc). 

[.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)

You'll need an Azure DevOps account and will need to grab a [PAT](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops) 

You'll need your Organization, Project and Team name from Azure DevOps. The easiest way to get these I find is to navigate to your sprint board and look at the url:

```
https://dev.azure.com/<org>/<project>/_backlogs/backlog/<team name>/Stories
```

## Docker

I've popped a docker container up that you can use to run this

```
cat  SimpleQuery.txt |  docker run -i jakkaj/wiql ./wiql --pat <pat> --user-email "user@user.com" --org <org> --project <project> --team "<team>" >> out.json
```

## Usage

For now you'll need to run with the dotnet command

```
dotnet run -- --help
```

Note the -- after `dotnet run` which means to pass whatever follows to the app that is being run as parameters. 

```
Options:
  --pat <PAT>                  Azure DevOps Personal Access token - see https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops
  --user-email <USER-EMAIL>    Email address of Azure DevOps user
  --org <ORG>                  The organization from Azure DevOps
  --project <PROJECT>          The project from Azure DevOps
  --team <TEAM>                The team from Azure DevOps
  --query <QUERY>              A WIQL query. Do not use with teh workItems flag
  --version                    Display version information
```

You can pass in a query here with the --query option, but it's best to pass in as a piped input. 

## Sample

In bash you can:

```
cat somefile.txt | dotnet run -- dotnet run -- --pat <pat> --user-email "user@user.com" --org yourorg --project yourproject --team "your team"
```

In powershell you can:

```
Get-Content somefile.txt | & dotnet run -- dotnet run -- --pat <pat> --user-email "user@user.com" --org yourorg --project yourproject --team "your team"
```

Then pipe your output to JQ and off you go. 

## Alternative inputs. 

This project supports `.env` files:

```
AzureDevOpsSettings__PersonalAccessToken=
AzureDevOpsSettings__UserEmail=
AzureDevOpsSettings__Organization=
AzureDevOpsSettings__Project=
AzureDevOpsSettings__Team=

```


Any of these can be set as regular environment variables then omitted from the console input parameters. 

As a last resort you can use the regular .net style ```appsettings.json``` file:


```json
{
  //please do not set these here, use this -> https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows or environment vars for tests and command line options for runtime
  "AzureDevOpsSettings": {
    "PersonalAccessToken": "PAT info: https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops",
    "UserEmail": "",
    "Organization": "",
    "Project": "",
    "Project": ""
  }
}

```

