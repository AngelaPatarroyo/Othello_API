Othello API

📌 Overview

Othello API is a backend service built with ASP.NET Core, Entity Framework Core, and SQLite to manage user authentication, game logic, leaderboards, and more for an Othello game. The API follows RESTful principles and implements authentication using JWT tokens.

🚀 Features

User Management: Registration, login, and role-based authentication (Admin, Player).

Game Management: Create, update, and delete Othello games.

Moves Handling: Players can make moves in an Othello game.

Leaderboards: Track player rankings and statistics.

Security: Implements authentication with JWT, CORS policies, and authorization middleware.

Error Handling: Global error handling using middleware.

Pagination & Query Optimization: For efficient data retrieval.

🏗️ Tech Stack

Backend: ASP.NET Core 7.0

Database: SQLite (with EF Core)

Authentication: JWT-based authentication with Identity

Logging: Built-in ASP.NET Core logging

API Documentation: Swagger (OpenAPI)

📂 Project Structure

Othello_API/
│── Controllers/         # API Endpoints
│── Middleware/          # Custom Middleware (Global Error Handling, etc.)
│── Models/              # Entity Models
│── Services/            # Business Logic Layer
│── Interfaces/          # Service Interfaces
│── Repositories/        # Data Access Layer
│── DTOs/                # Data Transfer Objects
│── Program.cs           # Main Entry Point
│── appsettings.json     # Configuration Settings

⚙️ Setup & Installation

1️⃣ Clone the Repository

git clone https://github.com/your-username/Othello_API.git
cd Othello_API

2️⃣ Set Up Environment Variables

Create a .env file in the root directory and add:

DEFAULT_CONNECTION=Data Source=OthelloDB.sqlite
JWT_SECRET=your-secret-key

3️⃣ Install Dependencies

dotnet restore

4️⃣ Apply Database Migrations

dotnet ef database update

5️⃣ Run the API

dotnet run

By default, the API runs on http://localhost:5000 (or https://localhost:5001 if HTTPS is enabled).

🔑 Authentication & Authorization

The API uses JWT tokens for authentication.

🔹 User Roles

Admin: Can manage users, delete games, view leaderboards.

Player: Can create/join games, make moves, and view leaderboards.

🔹 Get JWT Token

Register a New User

POST /api/user/register

{
  "userName": "player1",
  "email": "player1@example.com",
  "password": "SecurePass123!"
}

Login to Get Token

POST /api/user/login

{
  "email": "player1@example.com",
  "password": "SecurePass123!"
}

Response:

{
  "token": "your.jwt.token.here"
}

Use this token in the Authorization header for protected routes:

Authorization: Bearer your.jwt.token.here

🛠️ API Endpoints

🔹 User Endpoints

Method

Endpoint

Description

POST

/api/user/register

Register a new user

POST

/api/user/login

Authenticate and get a JWT token

PUT

/api/user/{id}

Update user (only self)

DELETE

/api/user/{id}

Delete user (Admin only)

🔹 Game Endpoints

Method

Endpoint

Description

POST

/api/game/start

Start a new game

GET

/api/game/{gameId}

Get a specific game

GET

/api/game

Get all games (with pagination)

PUT

/api/game/{gameId}

Update game details

DELETE

/api/game/{gameId}

Delete game (Admin only)

🔹 Move Endpoints

Method

Endpoint

Description

POST

/api/game/{gameId}/move

Make a move

GET

/api/game/{gameId}/moves

Get all moves in a game

🔹 Leaderboard Endpoints

Method

Endpoint

Description

GET

/api/leaderboard

Get leaderboard rankings

GET

/api/leaderboard/{userId}

Get specific user's ranking

🔐 Security Features

Authentication: JWT tokens required for protected routes.

Authorization: Role-based access control ([Authorize(Roles = "Admin")]).

CORS Policies: Restricts frontend origins.

Rate Limiting: Prevents excessive API calls.

Global Error Handling: Middleware ensures proper error responses.

🔍 Testing the API

Use Postman or cURL to test endpoints.
Example:

curl -X GET http://localhost:5000/api/game -H "Authorization: Bearer your.jwt.token"

To test with Swagger:

Run dotnet run

Open http://localhost:5000/swagger

📜 License

This project is licensed under the MIT License.

📌 Future Improvements

Implement WebSockets for real-time game updates.

Add Redis caching for performance optimization.

Extend admin dashboard for managing users and games.

👩‍💻 Author

Developed by Angela Patarroyo

For questions or contributions, feel free to open an issue or fork the repository!

