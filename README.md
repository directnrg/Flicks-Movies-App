# Flicks_App

Flicks_App is a Razor Pages web application for managing and reviewing movies. It allows users to upload, view, review, and rate movies. The app also includes user authentication and authorization features, ensuring secure access to its functionalities.

---

## Features

- **User Authentication**: 
  - Register, log in, and manage accounts using ASP.NET Core Identity.
  - Email confirmation and password reset functionality.

- **Movie Management**:
  - Upload movies with metadata (title, genre, release date, directors, etc.).
  - View a list of movies with thumbnails and details.

- **Review System**:
  - Add comments and rate movies using a star-based rating system.
  - Edit reviews within 24 hours of posting.

- **File Management**:
  - Upload and retrieve movie files and thumbnails using AWS S3.

- **Responsive Design**:
  - Built with Bootstrap for a modern and mobile-friendly user interface.

---

## Technologies Used

- **Backend**:
  - ASP.NET Core Razor Pages
  - Entity Framework Core with SQL Server
  - AWS DynamoDB for movie metadata and reviews
  - AWS S3 for storing movie files and thumbnails

- **Frontend**:
  - Bootstrap for styling
  - JavaScript for interactivity

- **Authentication**:
  - ASP.NET Core Identity for user management

- **Development Tools**:
  - Visual Studio 2022
  - .NET 6

---

## Getting Started

### Prerequisites

- .NET 6 SDK
- SQL Server
- AWS account with S3 and DynamoDB configured
- Visual Studio 2022

### Installation

1. Clone the repository: git clone https://github.com/directnrg/Flicks-Movies-App.git cd Flicks_App
   

2. Configure the database connection string in `appsettings.json`:

```json
   "ConnectionStrings": {
	   "DefaultConnection": "your connection string"
   }
   ```

3. Set up AWS credentials for S3 and DynamoDB:
   - Use environment variables in a .env file or AWS SDK configuration.

4. Run database migrations: 

   ```bash
   dotnet ef database update
   ```

5. Build and run the application:

6. Access the app in your browser at `https://localhost:5001`.

---

## How It Works

### User Registration and Login
- Users can register with their email and password.
- After logging in, users can access all features, including uploading movies and posting reviews.

### Uploading Movies
- Navigate to the "Upload" page.
- Fill in the movie details, upload the video file and thumbnail, and submit.

### Viewing Movies
- Browse the list of movies on the homepage.
- Click on a movie to view its details, including reviews and ratings.

### Reviewing Movies
- Add a comment and rate a movie on its details page.
- Edit your review within 24 hours of posting.

### Downloading Movies
- Download movie files directly from the details page.

---

## Project Structure

- **Models**: Contains data models for movies, reviews, and users.
- **Controllers**: Handles user requests and business logic.
- **Views**: Razor Pages for the user interface.
- **wwwroot**: Static files like JavaScript, CSS, and images.
- **Migrations**: Database migration files for Entity Framework Core.

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

---

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/directnrg/Flicks-Movies-App/blob/master/LICENSE.md) file for details.

---

## Contact

For questions or support, please contact directnrg@gmail.com.