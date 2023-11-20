namespace ManejoPresupuesto.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Emailnormalizado { get; set; }
        public string PasswordHash { get; set; }
    }
}
