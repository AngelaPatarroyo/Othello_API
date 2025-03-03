# ==============================
# Stage 1: Build
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy only the project file first (better caching)
COPY Othello_API.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Build and publish the application
RUN dotnet publish Othello_API.csproj -c Release -o /app/publish --no-restore

# ==============================
# Stage 2: Runtime
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the compiled output from the build stage
COPY --from=build /app/publish .

# Copy SQLite database (if needed)
COPY OthelloDB.sqlite /app/OthelloDB.sqlite

# Expose the application port
EXPOSE 5000

# Run the application
CMD ["dotnet", "Othello_API.dll", "--urls", "http://0.0.0.0:5000"]
