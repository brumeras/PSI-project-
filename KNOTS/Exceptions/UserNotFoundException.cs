namespace KNOTS.Exceptions;

public class UserNotFoundException : Exception{
    public string Username { get; }
    public UserNotFoundException(string username) 
        : base($"User '{username}' not found."){Username = username;}
}