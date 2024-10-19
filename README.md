# `SocialNet` ![Static Badge](https://img.shields.io/badge/ASP.NET-socialnet-purple) ![GitHub commit activity](https://img.shields.io/github/commit-activity/w/luis-domingues/SocialNet)

**Connect with others by writing your ideas.**

SocialNET is a web application inspired by Twitter/X and Bluesky, allowing users to connect and share their experiences. This project serves as the frontend for my API [`SocialNetApi`](https://github.com/luis-domingues/social-net-api), focusing on user registration and login functionalities.

[Getting Started](#usage) •
[Installation](#installation) •
[Contributing](#contributing)

## Key features

- User Registration and Login - Users can create an account or log in if they already have one.
- Security - User passwords are stored securely using hashing techniques, ensuring that passwords are encrypted before being saved to the database.
- Scalable - This project is designed to expand with additional functionalities in the future.

## Updates
* Implemented the `user's personal feed` controller, with a post posting section, `likes`, and `comments` on posts.
* Implemented the `user profile`, where the user's information is detailed.

## Usage

### Installation

To install SocialNET, you can clone the repository and run it locally.

1. Clone this repository:
   ```sh
   git clone https://github.com/luis-domingues/SocialNet.git
   ```
2. Navigate into the project directory:
    ```sh
   cd SocialNet
    ```
3. Open the project in your favorite IDE or editor, and run it using the .NET Core CLI:
    ```sh
    dotnet run
    ```

### Features in development
Expanding functionalities such as post creation, comments, and news feeds.

## Contributing
Feel free to contribute by opening an issue to report a bug or suggest improvements or features.

How to contribute in 5 steps
1. Fork this project.
2. Clone your fork on your machine.
3. Make your changes and commit them following conventional commits.
4. Push your changes to sync with your fork.
5. Open a pull request specifying what you did.

## License
This project is licensed under the MIT License - See the [LICENSE](https://github.com/luis-domingues/SocialNet/LICENSE.md) for more information.
