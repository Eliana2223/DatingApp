using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(), //ToLower todos nuestros nombres de usuario se van a almacenar en minuscula
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),//lo que hace aca es codificar la contraseña
                PasswordSalt = hmac.Key,//codifica la contraseña ya codificada, segunda codificación
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                 .SingleOrDefaultAsync(x => x.UserName == loginDto.Username); /*lo que hace aca es comparar el username que pusieron 
            en el login con losde la base de datos y si coincide lo deja logear*/
            /*SingleOrDefaultAsync retorna el unico elemento de una secuencia o un valor
            default si la secuencia esta vacía y tira una excepción si hay más de un
            elemento en la secuencia, FirstOrDefaultAsync es igual al primero exepto
            por la parte de la excepción*/
            if (user == null) return Unauthorized("Invalid username"); //si el usuario no existe tira como error que es invalido

            using var hmac = new HMACSHA512(user.PasswordSalt);//en esta parte se compara la traducción de la password salt en la passwordhash con la passwordhash almacenada en la base de datos

            var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));// en esta parte se otiene la passwordhash?

            for (int i = 0; i < ComputedHash.Length; i++)//si la contraseña no concuerda?
            {
                if (ComputedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
            //si concuerda
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

        }

        private async Task<bool> UserExists(string username)// esto sirve para que el nombre de usuario sea unico
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

    }
}