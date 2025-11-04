namespace KNOTS.Exceptions;

/// <summary>
/// Thrown when attempting to register a username that already exists.
/// </summary>
public class UserAlreadyExistsException : Exception
{
    public string Username { get; }
    
    public UserAlreadyExistsException(string username) 
        : base($"User '{username}' already exists in the system.")
    {
        Username = username;
    }
}

/// <summary>
/// Thrown when user credentials are invalid during authentication.
/// </summary>
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message = "Invalid username or password.") 
        : base(message) { }
}

/// <summary>
/// Thrown when a requested user cannot be found in the database.
/// </summary>
public class UserNotFoundException : Exception
{
    public string Username { get; }
    
    public UserNotFoundException(string username) 
        : base($"User '{username}' not found.")
    {
        Username = username;
    }
}