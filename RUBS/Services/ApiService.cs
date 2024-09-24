using Newtonsoft.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Estabelecimento>> GetEstabelecimentosAsync()
    {
        var url = "https://apidadosabertos.saude.gov.br/cnes/estabelecimentos?codigo_municipio=317070&limit=100&offset=1";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var estabelecimentosResponse = JsonConvert.DeserializeObject<EstabelecimentosResponse>(content);
            return estabelecimentosResponse?.estabelecimentos;
        }

        return null;
    }
}
