using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using RUBS.Models;
using RUBS.Services;

namespace RUBS.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public readonly DatabaseService _databaseService;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _databaseService = new DatabaseService();
        }

        // Novo método para fazer a busca com código de município e controle de paginação
        public async Task GetEstabelecimentosPaginadosAsync(string codigoMunicipio)
        {
            int offset = 1;
            int limit = 1;
            bool continuar = true;

            while (continuar)
            {
                string url = $"https://apidadosabertos.saude.gov.br/cnes/estabelecimentos?codigo_municipio={codigoMunicipio}&status=1&limit={limit}&offset={offset}";
                var response = await _httpClient.GetStringAsync(url);

                // Aqui estamos deserializando o objeto que contém a lista de estabelecimentos
                var result = JsonConvert.DeserializeObject<EstabelecimentosWrapper>(response);

                // Se a resposta vier vazia, interrompe o loop
                if (result == null || result.estabelecimentos == null || result.estabelecimentos.Count == 0)
                {
                    continuar = false;
                }
                else
                {
                    // Salva os estabelecimentos no banco
                    foreach (var estabelecimento in result.estabelecimentos)
                    {
                        // Verifica se os campos importantes estão preenchidos
                        if (estabelecimento.codigo_cnes != 0 &&
                            !string.IsNullOrEmpty(estabelecimento.nome_fantasia) &&
                            !string.IsNullOrEmpty(estabelecimento.endereco_estabelecimento) &&
                            !string.IsNullOrEmpty(estabelecimento.numero_estabelecimento) &&
                            !string.IsNullOrEmpty(estabelecimento.bairro_estabelecimento) &&
                            !string.IsNullOrEmpty(estabelecimento.descricao_turno_atendimento) &&
                            !string.IsNullOrEmpty(estabelecimento.numero_telefone_estabelecimento) &&
                            estabelecimento.latitude_estabelecimento_decimo_grau.HasValue &&
                            estabelecimento.longitude_estabelecimento_decimo_grau.HasValue)
                        {
                            // Salva no banco apenas se todos os campos são válidos
                            await _databaseService.SaveEstabelecimentoAsync(estabelecimento);
                        }
                    }

                    // Incrementa o offset para a próxima página
                    offset += limit;
                }
            }
        }
    }
}
