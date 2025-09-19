using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class UserUpdateDto
    {
        /// Essa função se está sendo criada principalmente para futuramente , Quando existia estiver Mais estruturado e robusto Não só atualizar
        /// o nome de usuário Mas também inserir nos dados do banco de dados SQL server, O link de referência talvez de alguma imagem ou arquivo 
        /// de perfil do usuário vindo do front end e do subabase 
        [StringLength(160)]
        public string? Name { get; set; }

        //[Url]
        //public string? PhotoUrl { get; set; }  // link de imagem (pode vir do Firebase ou Supabase)
    }
}
