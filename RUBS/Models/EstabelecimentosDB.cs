using SQLite;

namespace RUBS
{
    public class EstabelecimentosDB
    {
        [PrimaryKey, NotNull]
        public long codigo_cnes {  get; set; }
        [NotNull]
        public string nome_fantasia {  get; set; }
        [NotNull]
        public string endereco_estabelecimento {  get; set; }
        [NotNull]
        public string numero_estabelecimento { get; set; }
        [NotNull]
        public string bairro_estabelecimento { get; set; }
        [NotNull]
        public string descricao_turno_atendimento { get; set; }
        [NotNull]
        public string numero_telefone_estabelecimento { get; set; }
        public double? latitude_estabelecimento_decimo_grau { get; set; } 
        public double? longitude_estabelecimento_decimo_grau { get; set; }
        [NotNull]
        public long codigo_tipo_unidade {  get; set; }
        [NotNull]
        public string descricao_natureza_juridica_estabelecimento { get; set; }
        public string TipoImagem { get; set; }
    }
}