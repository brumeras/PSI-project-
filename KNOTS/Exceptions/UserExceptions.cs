namespace KNOTS.Exceptions;
public class UserAlreadyExistsException : Exception{
    public string Username { get; }
    public UserAlreadyExistsException(string username) 
        : base($"User '{username}' already exists in the system.") {Username = username;}
}
public class InvalidCredentialsException : Exception{ 
    public InvalidCredentialsException(string message = "Invalid username or password.") 
        : base(message) { }
}
public class UserNotFoundException : Exception{
    public string Username { get; }
    public UserNotFoundException(string username) 
        : base($"User '{username}' not found."){Username = username;}
}