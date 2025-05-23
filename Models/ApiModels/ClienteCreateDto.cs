namespace GestorEventos.Models.ApiModels
{
    public class ClienteCreateDto
    {
        public string TipoCliente { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ruc { get; set; }
        public string RazonSocial { get; set; }
    }
}