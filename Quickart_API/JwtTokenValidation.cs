using System;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Quickart_API
{
	public class JwtTokenValidation
	{
		private readonly IConfiguration _configuration;
		public JwtTokenValidation(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string validate(string token)
		{
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                string Email = jwtToken.Claims.First(x => x.Type == "id").ToString();
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                return Email;
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

	}
}

