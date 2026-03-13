# hm-auth-service

Independent microservice repository for Hospital Management.

## Local run

`ash
dotnet restore
dotnet build
dotnet run --project src/AuthService.Api/AuthService.Api.csproj
`

## Docker

`ash
docker build -t hm-auth-service:local .
docker run -p 8081:8080 hm-auth-service:local
`

## Database migrations

Apply SQL migration scripts before starting the API in environments where the schema is managed outside application runtime:

`ash

# Example with sqlcmd (adjust server/database/auth options for your environment)

sqlcmd -S <server> -d <database> -i scripts/migrations/001_create_refresh_tokens.sql
`

Current migration files:

- scripts/migrations/001_create_refresh_tokens.sql

## GitHub setup later

`ash
git branch -M main
git remote add origin <your-github-repo-url>
git add .
git commit -m "Initial scaffold"
git push -u origin main
`
