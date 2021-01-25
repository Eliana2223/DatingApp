using API.Entities;

namespace API.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);//vamos a devolver un string porque eso es lo que es basicamente el token una cadena
        //Este metodo recibira un usuario de la palicación como parametro y traeremos un
        //usuario de la aplicación de las entidades API
    }
}