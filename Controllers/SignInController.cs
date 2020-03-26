/*
 File written by erik
 Handles the data to create a new account for users
 */
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SignIn : ControllerBase
    {
        private readonly DbContextApp dbContextApp;
        public SignIn (DbContextApp context)
        {
            dbContextApp = context;
        }

        /*
         Return codes:
         1 => An existing user has used the same email address
         2 => An existing user has the same username
         3 => By an unknown reason, one or more of the fields are empty
         0 => Everything is OK, the user is now recorded
         Exception => Returns the exception string if something didn't go well on the server
        */
        [HttpPost]
        public string Index([FromForm] string username, [FromForm] string email, [FromForm] string password)
        {
            if(dbContextApp.Users.Any(user => user.Email.Equals(email)))
            {
                return "1";
            }
            if(dbContextApp.Users.Any(user => user.Name.Equals(username)))
            {
                return "2";
            }
            if(username == "" || password == "" || email == "")
            {
                return "3";
            }
            User newUser = new User()
            {
                Name = username,
                Email = email,
                Password = password,
                Uid = Guid.NewGuid().ToString()
            };
            try
            {
                dbContextApp.Users.Add(newUser);
                dbContextApp.SaveChanges();
                sendValidationEmail(newUser.Id, newUser.Uid);
                return "0";
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }
        public bool sendValidationEmail(int id, string validationCode)
        {
            string link = String.Format("http://isolaattiapi.azurewebsites.net/validateAccount?id={0}&code={1}",id,validationCode);
            string userEmailAddress = dbContextApp.Users.Find(id).Email;
            string htmlBody = String.Format( @"
                <html>
                    <body>
                        <h1>Welcome to Isolaatti</h1>
                        <h1>Bienvenido a Isolaatti</h1>
                        <p>Recently, you used this email address to create a new account on Isolaatti. Click in the link below to activate your account</p>
                        <p>Recientemente, usaste esta dirección de correo electrónico para crear una nueva cuenta en Isolaatti. Haz clic en el link de abajo para activarla</p>
                        <a href='{0}'>{0}</a>
                        <p>If you didn't, please ommit this</p>
                        <p>Si no lo hiciste, omite esto por favor</p>
                    </body>
                </html>
                ",link);

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Isolaatti","validation.isolaatti@gmail.com"));
            message.To.Add(new MailboxAddress(userEmailAddress));
            message.Subject = "Welcome to Isolaatti | Bienvenido a Isolaatti";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("validation.isolaatti@gmail.com","0203_0302_" );
                client.Send(message);
                client.Disconnect(true);
            }
            return true;
        }
    }
}