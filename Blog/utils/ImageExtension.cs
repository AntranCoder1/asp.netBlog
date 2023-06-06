namespace Blog.utils
{
    public class ImageExtension
    {
        public ImageExtension()
        {

        }

        public bool IsImageExtension(string fileName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            return allowedExtensions.Any(ext => fileName.EndsWith(ext));
        }
    }
}