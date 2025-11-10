namespace KNOTS.Exceptions;

public class UserAlreadyExistsException : Exception{
    public string Username { get; }
    public UserAlreadyExistsException(string username) 
        : base($"User '{username}' already exists in the system.") {Username = username;}
}