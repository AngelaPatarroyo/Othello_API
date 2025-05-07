# Use the .NET 9 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore Othello_API.csproj

# Build and publish
RUN dotnet publish Othello_API.csproj -c Release -o /app/publish

# Use the ASP.NET runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "Othello_API.dll"]
