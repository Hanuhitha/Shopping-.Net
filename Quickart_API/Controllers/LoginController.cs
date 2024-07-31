using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Quickart_API.Models;
using MySqlConnector;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Quickart_API.Controllers
{

    [Route("Login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void SendEmail(string to_addr, string from_addr)
        {
            try
            {
                string gmail = from_addr; // _configuration.GetConnectionString("gmail");
                //string gmail_password = _configuration.GetConnectionString("gmail_password");
                string gmail_password = "LifeLine@11";
                MailMessage msg = new MailMessage();
                //Add your email address to the recipients
                msg.To.Add(to_addr);
                //Configure the address we are sending the mail from
                MailAddress address = new MailAddress(from_addr);
                msg.From = address;
                msg.Subject = "Thank you for signing up with Quickart.";
                msg.Body = "Thank you for registering with Quickart. Your account has been activated.";

                //Configure an SmtpClient to send the mail.
                SmtpClient SmtpClient = new SmtpClient();
                SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpClient.EnableSsl = false;
                SmtpClient.Host = "relay-hosting.secureserver.net";
                SmtpClient.Port = 25;

                //Setup credentials to login to our sender email address ("UserName", "Password")
                NetworkCredential credentials = new NetworkCredential(gmail, gmail_password);
                SmtpClient.UseDefaultCredentials = true;
                SmtpClient.Credentials = credentials;

                //Send the msg
                SmtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }


        public String CreateToken(int UserID, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("email", email), new Claim("UserID", UserID.ToString() ) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        [HttpPost("LoginUser", Name = nameof(LoginUserAsync))]
        public async Task<ActionResult<LoginResponse>> LoginUserAsync([FromBody] LoginRequest request)
        {
            try
            {
                string Email = request.Email;
                string Password = request.Password;

                string st = ("SELECT user_id FROM Users where email='" + Email + "' and Password='" + Password + "'");

                DataTable table = new DataTable();
                DataLogin dt = new DataLogin();
                string DataSource = _configuration.GetConnectionString("QuickartCon");
                MySqlDataReader myReader;
                bool flag = false;
                using (MySqlConnection mycon = new MySqlConnection(DataSource))
                {
                    mycon.Open();
                    using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                    {
                        myReader = mycommand.ExecuteReader();
                        table.Load(myReader);

                        myReader.Close();
                        mycon.Close();
                    }

                    if (table.Rows.Count > 0)
                    {
                        flag = true;
                        dt.user_id = Convert.ToInt32(table.Rows[0]["user_id"]);
                    }
                }

                string token = CreateToken(dt.user_id, Email);
                dt.token = token;

                var response = new LoginResponse
                {
                    response_message = (flag == true) ? "User Found" : "User Not Found",
                    response_code = (flag == true) ? 200 : 404,
                    data = dt
                };

                return response;
            }
            catch (Exception e)
            {
                var response = new LoginResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };

                return response;
            }
            //commen
        }




        [HttpPost("SignupUser", Name = nameof(SignupUserAsync))]
        public async Task<ActionResult<SignupResponse>> SignupUserAsync([FromBody] SignupRequest request)
        {
            string? final_message = null;
            int resp_code = 0;
            string? FirstName = request.FirstName;
            string? LastName = request.LastName;
            string Email = request.Email;
            string Password = request.Password;
            string UserType = request.UserType;
            Data Dt = new Data();

            //check if email exists
            string st = ("SELECT * FROM Users where email='" + Email + "'");
            DataTable table = new DataTable();
            string DataSource = _configuration.GetConnectionString("QuickartCon");
            MySqlDataReader myReader;

            using (MySqlConnection mycon = new MySqlConnection(DataSource))
            {
                mycon.Open();
                using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                {
                    myReader = mycommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    mycon.Close();
                }
            }

            if (table != null && table.Rows.Count > 0)
            {
                final_message = "User already exists";
                resp_code = 404;
            }

            //if email not exists
            if (String.IsNullOrEmpty(final_message))
            {
                // Insert new user into DB
                st = ("Insert into Users (first_name, last_name, email, Password, user_type) values ('" + FirstName + "','" + LastName + "','" + Email + "','" + Password + "','" + UserType + "')");
                using (MySqlConnection mycon = new MySqlConnection(DataSource))
                {
                    mycon.Open();
                    using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                    {
                        myReader = mycommand.ExecuteReader();
                        myReader.Close();
                        mycon.Close();
                    }
                }

                // check if the user got inserted
                st = ("SELECT user_id FROM Users where email='" + Email + "'");
                table.Clear();
                using (MySqlConnection mycon = new MySqlConnection(DataSource))
                {
                    mycon.Open();
                    using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                    {
                        myReader = mycommand.ExecuteReader();
                        table.Load(myReader);
                        myReader.Close();
                        mycon.Close();
                    }

                    if (table == null)
                    {
                        final_message = "some error in inserting";
                        resp_code = 404;
                    }
                    else
                    {
                        //send email to user
                        SendEmail(Email, "lifelineteam11@gmail.com");

                        Dt.user_id = Convert.ToInt32(table.Rows[0]["user_id"]);
                        //Generate token
                        string token = CreateToken(Dt.user_id, Email);
                        Dt.token = token;
                        final_message = "user inserted";
                        resp_code = 200;
                        
                    }
                }

            }

            var response = new SignupResponse
            {
                response_code = resp_code,
                data = Dt,
                response_message = final_message
            };

            return response;

        }


    }
}