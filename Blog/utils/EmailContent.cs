namespace Blog.utils
{
    public class EmailContent
    {
        private readonly string _name;
        private readonly string _code;
        public EmailContent(string name, string code)
        {
            _name = name;
            _code = code;
        }

        public string GenerateEmailContent()
        {
            var html = @"
                <div style=""font-family: Avenir, Helvetica, sans-serif; box-sizing: border-box; background-color: #f5f8fa; color: #74787e; height: 100%; line-height: 1.4; margin: 0; width: 100% !important; word-break: break-word;"">
                    <p>Hi, <span style=""text-transform: capitalize;"">{{name}}</span></p>
                    <p>Please enter 6 number to confirm registration!</p>
                    <p>{{code}}</p>
                    <p>Keep Creating,<br />The Booking Team</p>
                </div>
            ";

            // Replace placeholders with actual values
            html = html.Replace("{{name}}", _name)
                       .Replace("{{code}}", _code);


            return html;
        }
    }
}
