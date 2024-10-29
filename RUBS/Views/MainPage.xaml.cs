using Newtonsoft.Json;
using RUBS.Models;
using RUBS.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RUBS.Views
{
    public partial class MainPage : ContentPage
    {
        // Vari�vel para armazenar o c�digo do munic�pio selecionado
        private string codigoMunicipioSelecionado;
        private readonly DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            CarregarEstados();
        }

        private async void CarregarEstados()
        {
            // Lista de estados
            var estados = new List<string>
            {
                "AC", "AL", "AP", "AM", "BA", "CE",
                "DF", "ES", "GO", "MA", "MT", "MS",
                "MG", "PA", "PB", "PR", "PE", "PI",
                "RJ", "RN", "RS", "RO", "RR", "SC",
                "SP", "SE", "TO"
            };
            EstadoPicker.ItemsSource = estados;
        }

        private void OnEstadoButtonClicked(object sender, EventArgs e)
        {
            EstadoPicker.IsVisible = true;
            EstadoPicker.Focus();
        }

        private async void OnEstadoSelected(object sender, EventArgs e)
        {
            if (EstadoPicker.SelectedItem != null)
            {
                var estadoSelecionado = EstadoPicker.SelectedItem.ToString();
                await CarregarCidades(estadoSelecionado);
                CidadeButton.IsVisible = true; // Torna o bot�o de cidade vis�vel
            }
        }

        private async Task CarregarCidades(string estado)
        {
            string url = $"https://apidadosabertos.saude.gov.br/macrorregiao-e-regiao-de-saude/municipio?sigla_uf={estado}&limit=860&offset=1";
            var httpClient = new HttpClient();
            var resposta = await httpClient.GetStringAsync(url);

            var resultado = JsonConvert.DeserializeObject<MunicipioResponse>(resposta);

            // Define a fonte de itens do Picker, exibindo o nome do munic�pio
            MunicipioPicker.ItemsSource = resultado.macrorregiao_regiao_saude_municipios;
            MunicipioPicker.ItemDisplayBinding = new Binding("municipio"); // Mostra o nome do munic�pio
            MunicipioPicker.IsVisible = true;
        }

        private void OnCidadeButtonClicked(object sender, EventArgs e)
        {
            MunicipioPicker.Focus();
        }

        private async void OnMunicipioSelected(object sender, EventArgs e)
        {
            if (MunicipioPicker.SelectedItem != null)
            {
                var municipioSelecionado = (Cidade)MunicipioPicker.SelectedItem;
                MunicipioService.Instance.CodigoMunicipioSelecionado = municipioSelecionado.codigo_municipio;

                // Limpa os registros atuais no banco de dados antes de carregar os novos dados da cidade
                await _databaseService.RemoverEstabelecimetnosAsync();

                // Torna o bot�o de confirma��o vis�vel ap�s a sele��o do munic�pio
                ConfirmarButton.IsVisible = true;
            }
        }

        private async void OnConfirmarButtonClicked(object sender, EventArgs e)
        {
            if (MunicipioPicker.SelectedItem != null)
            {
                var municipioSelecionado = (Cidade)MunicipioPicker.SelectedItem;
                MunicipioService.Instance.CodigoMunicipioSelecionado = municipioSelecionado.codigo_municipio;
                new ListagemEstabelecimentos();
            }
        }
    }
}
