# 🎲 Othello API

## 📌 Overview

Othello API is a backend service built with **ASP.NET Core**, **Entity Framework Core**, and **SQLite** to manage **user authentication, game logic, leaderboards, and more** for an Othello game. The API follows **RESTful principles** and implements **secure authentication using JWT tokens**.

## 🚀 Features

✅ **User Management**: Registration, login, and role-based authentication (**Admin, Player**).  
✅ **Game Management**: Start, update, and delete Othello games.  
✅ **Moves Handling**: Players can make moves in an Othello game.  
✅ **Leaderboards**: Track player rankings and statistics.  
✅ **Security**: Implements JWT authentication, CORS policies, and authorization middleware.  
✅ **Error Handling**: Global error handling via middleware.  
✅ **Pagination & Query Optimization**: For efficient data retrieval.  
✅ **CI/CD & Deployment Ready**: Dockerized API with GitHub Actions for CI/CD.

## 🏗️ Tech Stack

- **Backend**: ASP.NET Core 7.0
- **Database**: SQLite (with Entity Framework Core)
- **Authentication**: JWT-based authentication with ASP.NET Core Identity
- **Logging**: Built-in ASP.NET Core logging (`ILogger`)
- **API Documentation**: Swagger (OpenAPI)

## 📂 Project Structure

```
Othello_API/
│── Controllers/         # API Endpoints
│── Middleware/          # Custom Middleware (Global Error Handling, etc.)
│── Models/              # Entity Models
│── Services/            # Business Logic Layer
│── Interfaces/          # Service Interfaces
│── Repositories/        # Data Access Layer
│── DTOs/                # Data Transfer Objects
│── Tests/               # Unit Tests for API & Services
│── Program.cs           # Main Entry Point
│── appsettings.json     # Configuration Settings
│── Dockerfile           # Containerization
```

## ⚙️ Setup & Installation

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/your-username/Othello_API.git
cd Othello_API
```

### 2️⃣ Set Up Environment Variables
Create a `.env` file in the root directory and add:
```env
DEFAULT_CONNECTION=Data Source=OthelloDB.sqlite
JWT_SECRET=your-secret-key
```

### 3️⃣ Install Dependencies
```bash
dotnet restore
```

### 4️⃣ Apply Database Migrations
```bash
dotnet ef database update
```

### 5️⃣ Run the API
```bash
dotnet run
```
The API runs by default at:  
- **http://localhost:5000**  
- **https://localhost:5001** (if HTTPS is enabled)

## 🔑 Authentication & Authorization

The API uses **JWT tokens** for authentication.

### 🔹 User Roles
- **Admin**: Can manage users, delete games, and view leaderboards.
- **Player**: Can create/join games, make moves, and view leaderboards.

### 🔹 Get JWT Token
#### Register a New User
```http
POST /api/user/register
```
**Request Body:**
```json
{
  "userName": "player1",
  "email": "player1@example.com",
  "password": "SecurePass123!"
}
```

#### Login to Get Token
```http
POST /api/user/login
```
**Request Body:**
```json
{
  "email": "player1@example.com",
  "password": "SecurePass123!"
}
```
**Response:**
```json
{
  "token": "your.jwt.token.here"
}
```
Use this token in the **Authorization header**:
```
Authorization: Bearer your.jwt.token.here
```

## 🛠️ API Endpoints

### 🔹 User Endpoints
| Method | Endpoint              | Description                      |
|--------|----------------------|----------------------------------|
| POST   | `/api/user/register` | Register a new user             |
| POST   | `/api/user/login`    | Authenticate and get a JWT token |
| PUT    | `/api/user/{id}`     | Update user (only self)         |
| DELETE | `/api/user/{id}`     | Delete user (Admin only)        |

### 🔹 Game Endpoints
| Method | Endpoint           | Description                     |
|--------|------------------|---------------------------------|
| POST   | `/api/game/start` | Start a new game               |
| GET    | `/api/game/{id}`  | Get details of a specific game |
| DELETE | `/api/game/{id}`  | Delete a game (Admin only)     |

### 🔹 Move Endpoints
| Method | Endpoint              | Description            |
|--------|----------------------|------------------------|
| POST   | `/api/game/{id}/move` | Make a move           |
| GET    | `/api/game/{id}/moves` | Get all moves in a game |

### 🔹 Leaderboard Endpoints
| Method | Endpoint               | Description                 |
|--------|----------------------|-----------------------------|
| GET    | `/api/leaderboard`    | Get leaderboard rankings   |
| GET    | `/api/leaderboard/{id}` | Get specific user's ranking |

## 🔐 Security Features

✅ **Authentication**: JWT tokens required for protected routes.  
✅ **Authorization**: Role-based access control (`[Authorize(Roles = "Admin")]`).  
✅ **CORS Policies**: Restricts frontend origins.  
✅ **Rate Limiting**: Prevents excessive API calls.  
✅ **Global Error Handling**: Middleware ensures proper error responses.  

## 🔍 Testing the API

You can test the API using **Postman** or **cURL**:
```bash
curl -X GET http://localhost:5000/api/game -H "Authorization: Bearer your.jwt.token"
```
Or use **Swagger**:
1. Run `dotnet run`
2. Open [Swagger UI](http://localhost:5000/swagger)

## 🚀 Deployment
### 🔹 Docker Deployment
To run the API inside a **Docker container**:
```bash
docker build -t othello-api .
docker run -p 5000:5000 othello-api
```

### 🔹 Deploy to Azure (Example)
```bash
az webapp create --resource-group MyResourceGroup --plan MyPlan --name OthelloAPI --runtime "DOTNET:7.0"
az webapp config appsettings set --resource-group MyResourceGroup --name OthelloAPI --settings DEFAULT_CONNECTION="your-db-url" JWT_SECRET="your-secret"
```

## 🤝 Contributing
1. **Fork** the repository.
2. **Clone** your forked repo:
   ```bash
   git clone https://github.com/your-username/Othello_API.git
   ```
3. **Create a feature branch**:
   ```bash
   git checkout -b new-feature
   ```
4. **Commit your changes**:
   ```bash
   git commit -m "Added a new feature"
   ```
5. **Push and submit a PR**:
   ```bash
   git push origin new-feature
   ```

## 📜 License
This project is licensed under the **MIT License**.

## 👩‍💻 Author
Developed by **Angela Patarroyo**.  
For questions or contributions, feel free to **open an issue** or **fork the repository**!

