namespace Blog.ClassValue
{
    public class UserValue
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string image { get; set; }
        public int verify { get; set; }
        public string verify_token { get; set; }
        public string newPassword { get; set; }
        public string oldPassword { get; set; }
        public string comfirmPassword { get; set; }
        public int isAdmin { get; set; }
        public int registerVerifyCode { get; set; }
        public int confirmVerifyCode { get; set; }
        public int skip { get; set; }
        public int limit { get; set; }
    }
}
