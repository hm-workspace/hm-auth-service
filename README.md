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

## GitHub setup later

`ash
git branch -M main
git remote add origin <your-github-repo-url>
git add .
git commit -m "Initial scaffold"
git push -u origin main
`
