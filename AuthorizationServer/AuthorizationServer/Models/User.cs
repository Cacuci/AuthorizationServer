namespace AuthorizationServer.Models
{
    public class User
    {
        public int ID { get; private set; }

        public string Login { get; private set; }

        public string Name { get; private set; }

        public string Password { get; private set; }

        public EUserClass UserClass { get; private set; }

        public bool IsValidPassword(string password)
        {
            return Password == password;
        }
    }
}
