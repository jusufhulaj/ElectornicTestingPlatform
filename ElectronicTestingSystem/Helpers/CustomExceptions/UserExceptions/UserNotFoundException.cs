namespace ElectronicTestingSystem.Helpers.CustomExceptions.UserExceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userName) :base(String.Format("User not found: {0}", userName))
        {
        
        }
    }
}
