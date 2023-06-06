namespace Blog.utils
{
    public class GenerateCode
    {
        public static string VerifyCode()
        {
            Random r = new Random();
            var x = r.Next(0, 1000000);
            string s = x.ToString("000000");

            return s;
        }
    }
}
