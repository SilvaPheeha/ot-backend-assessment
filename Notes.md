## Vertical Slice Architecture 
- Instead of going the tradtional route of layered architectures, I Choose VSA where I group everything related to features
- Paired the Architecture with Cortex.Mediator using CQRS and Minimal APIs for lightweight endpoints


## WHY 
- VSA help keep related code together, so you dont break other features "because you didnt know it was a shared service".
- Cortex.Mediator ? Its FREE, the well known (MediatR and Automapper) are nolonger free.
- using Stored Proc ? 
    - Preference  - Some logic and validation needs to be handles by procs, and also to avoid N+1 query problem
    - Flexibility - Sometimes we dont want to raise a PR, Run Pipeline for minor changes.
- Init.sql ? Code Migrations can be a problem when some colleague still have the flexibility to update the db manually. 


## Outstanding Tasks/ Enhancements
- Add VaidationResult  - Better error handling, we dont throw 500 in prod.
- Add Logging (Serilog) -  NO database logging, "We dont want to delete logs every week"
- Add Validation (FluentValidation)
- Add API Auth (integrate with Ping Identity)
- Change memory caching (use Radis)
- Add a Lot of Db Indexing


## Setup
- Docker 
    + cd docker-setup
        - `docker-compose up --build` (Start the RabbitMQ and SQL Server)

Open another terminal to Run the API and Consumer App

- API
    + cd src/OT.Assessment.App/Feature/Wager
        -  dotnet run 
    + `dotnet run --project src/OT.Assessment.App/Feature/Wager`
- Consumer
    + cd src/OT.Assessment.Consumer
        -  dotnet run 
    + `dotnet run --project src/OT.Assessment.Consumer`

## Final Touches 
- Create pipeline, Release 