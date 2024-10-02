using Newtonsoft.Json;
using RUBS.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RUBS.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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
                CidadeButton.IsVisible = true; // Torna o botão de cidade visível
            }
        }

        private async Task CarregarCidades(string estado)
        {
            string url = $"https://apidadosabertos.saude.gov.br/macrorregiao-e-regiao-de-saude/municipio?sigla_uf={estado}&limit=860&offset=1";
            var httpClient = new HttpClient();
            var resposta = await httpClient.GetStringAsync(url);

            var resultado = JsonConvert.DeserializeObject<MunicipioResponse>(resposta);

            MunicipioPicker.ItemsSource = resultado.macrorregiao_regiao_saude_municipios;
            MunicipioPicker.ItemDisplayBinding = new Binding("municipio");
            MunicipioPicker.IsVisible = true; 
        }


        private void OnCidadeButtonClicked(object sender, EventArgs e)
        {
            MunicipioPicker.Focus();
        }
    }
}
