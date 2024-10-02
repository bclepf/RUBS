namespace RUBS.Models
{
    public class MunicipioResponse
    {
        public List<Cidade> macrorregiao_regiao_saude_municipios { get; set; }
    }

    public class Cidade
    {
        public string municipio { get; set; }
        public string codigo_municipio { get; set; }
    }
}
