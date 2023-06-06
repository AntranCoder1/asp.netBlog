namespace Blog.utils
{
    public class Common
    {
        public static async Task<string> EncryptPassword(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hash;
        }

        public static async Task<bool> ComparePassword(string hashPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }
    }
}
