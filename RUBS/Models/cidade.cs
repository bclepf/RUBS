using SQLite;

namespace RUBS
{
    public class cidade
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int ID { get; set; }
        [NotNull]
        public string municipio { get; set; }
        [NotNull]
        public string codigo_municipio { get; set; }
    }
}