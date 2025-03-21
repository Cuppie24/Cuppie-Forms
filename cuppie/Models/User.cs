using System.ComponentModel.DataAnnotations;

namespace cuppie.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }        
        public int Age { get; set; }
    }
}
