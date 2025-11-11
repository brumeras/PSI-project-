namespace KNOTS.Exceptions;
public class InvalidCredentialsException : Exception{ 
    public InvalidCredentialsException(string message = "Invalid username or password.") 
        : base(message) { }
}
