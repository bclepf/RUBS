using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SQLite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RUBS.Views
{
    public partial class PaginaMapa : ContentPage
    {
        private SQLiteConnection _db;
        private ILogger<PaginaMapa> _logger;
        private bool SusLigado = false;

        private List<EstabelecimentosDB> todosEstabelecimentos;
        private List<EstabelecimentosDB> estabelecimentosSUS;
        private HashSet<string> idsDePinsAdicionados = new HashSet<string>(); // Para evitar duplicação

        public PaginaMapa()
        {
            InitializeComponent();
            InitializeDatabase();
            CarregarEstabelecimentos();
        }

        public void SetLogger(ILogger<PaginaMapa> logger)
        {
            _logger = logger;
        }

        private void InitializeDatabase()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RUBS.db3");
            _db = new SQLiteConnection(dbPath);
            _db.CreateTable<EstabelecimentosDB>();
        }

        private void CarregarEstabelecimentos()
        {
            todosEstabelecimentos = _db.Table<EstabelecimentosDB>().ToList();

            // Define os códigos de natureza jurídica para SUS como texto
            var codigosSUS = new HashSet<string> { "1015", "1023", "1031", "1040", "1058", "1066", "1074", "1082", "1104", "1112", "1120", "1139", "1147", "1155", "1163", "1171", "1180", "1198", "1210", "1228", "1236", "1244", "1252", "1260", "1279", "1287", "1295", "1309", "1317", "1325", "1333", "1341" };

            estabelecimentosSUS = todosEstabelecimentos
                .Where(e => !string.IsNullOrEmpty(e.descricao_natureza_juridica_estabelecimento) && codigosSUS.Contains(e.descricao_natureza_juridica_estabelecimento))
                .ToList();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var geolocationRequest = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                _logger?.LogInformation($"Movendo mapa para a localização do usuário: Latitude {location.Latitude}, Longitude {location.Longitude}");
                map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
            }

            // Carrega os pins iniciais com todos os estabelecimentos de forma assíncrona
            await CarregarPinsEmBackgroundAsync(todosEstabelecimentos);
        }

        private async Task CarregarPinsEmBackgroundAsync(List<EstabelecimentosDB> estabelecimentos)
        {
            // Limpa os pins existentes
            map.Pins.Clear();
            idsDePinsAdicionados.Clear();

            const int loteSize = 30;  // Tamanho do lote de pins para adicionar por vez
            var estabelecimentosParaAdicionar = estabelecimentos.ToList();

            // Inicia o carregamento em segundo plano
            await Task.Run(async () =>
            {
                while (estabelecimentosParaAdicionar.Any())
                {
                    var lote = estabelecimentosParaAdicionar.Take(loteSize).ToList();
                    estabelecimentosParaAdicionar.RemoveRange(0, lote.Count);

                    var pinsParaAdicionar = new List<Pin>();

                    foreach (var estabelecimento in lote)
                    {
                        if (estabelecimento.latitude_estabelecimento_decimo_grau.HasValue && estabelecimento.longitude_estabelecimento_decimo_grau.HasValue)
                        {
                            var idPin = $"{estabelecimento.latitude_estabelecimento_decimo_grau}_{estabelecimento.longitude_estabelecimento_decimo_grau}";
                            if (idsDePinsAdicionados.Contains(idPin))
                            {
                                continue; // Já existe esse pin, então não adiciona novamente
                            }

                            var pin = new Pin
                            {
                                Label = estabelecimento.nome_fantasia,
                                Address = $"{estabelecimento.endereco_estabelecimento}, {estabelecimento.numero_estabelecimento}, {estabelecimento.bairro_estabelecimento}",
                                Location = new Location(estabelecimento.latitude_estabelecimento_decimo_grau.Value, estabelecimento.longitude_estabelecimento_decimo_grau.Value)
                            };

                            pinsParaAdicionar.Add(pin);
                            idsDePinsAdicionados.Add(idPin); // Marca como adicionado
                        }
                        else
                        {
                            _logger?.LogWarning($"Estabelecimento {estabelecimento.nome_fantasia} não possui coordenadas válidas.");
                        }
                    }

                    // Atualiza a UI de forma segura com um lote de pins
                    await Dispatcher.DispatchAsync(() =>
                    {
                        foreach (var pin in pinsParaAdicionar)
                        {
                            map.Pins.Add(pin);
                        }
                    });

                    _logger?.LogInformation($"Total de PINs exibidos até agora: {map.Pins.Count}");

                    // Aguarda um pouco antes de adicionar o próximo lote para evitar bloqueios
                    await Task.Delay(100); // Ajuste o valor conforme necessário
                }
            });
        }

        private async void OnBotaoSUSClicked(object sender, EventArgs e)
        {
            // Alterna o estado do botão
            SusLigado = !SusLigado;

            // Atualiza a aparência do botão
            BotaoSUS.Text = SusLigado ? "SUS" : "Todos";
            BotaoSUS.BackgroundColor = SusLigado ? Colors.Green : Colors.Gray;

            // Limpa os pins existentes
            map.Pins.Clear();
            idsDePinsAdicionados.Clear();

            // Carrega a lista de estabelecimentos apropriada
            if (SusLigado)
            {
                // Carregar pins dos estabelecimentos SUS
                if (estabelecimentosSUS.Any())
                {
                    await CarregarPinsEmBackgroundAsync(estabelecimentosSUS);
                }
                else
                {
                    _logger?.LogWarning("Nenhum estabelecimento SUS encontrado.");
                }
            }
            else
            {
                // Carregar pins de todos os estabelecimentos
                await CarregarPinsEmBackgroundAsync(todosEstabelecimentos);
            }
        }

        private void CarregarPins(List<EstabelecimentosDB> estabelecimentos)
        {
            foreach (var estabelecimento in estabelecimentos)
            {
                if (estabelecimento.latitude_estabelecimento_decimo_grau.HasValue && estabelecimento.longitude_estabelecimento_decimo_grau.HasValue)
                {
                    var idPin = $"{estabelecimento.latitude_estabelecimento_decimo_grau}_{estabelecimento.longitude_estabelecimento_decimo_grau}";
                    if (idsDePinsAdicionados.Contains(idPin))
                    {
                        continue; // Já existe esse pin
                    }

                    var pin = new Pin
                    {
                        Label = estabelecimento.nome_fantasia,
                        Address = $"{estabelecimento.endereco_estabelecimento}, {estabelecimento.numero_estabelecimento}, {estabelecimento.bairro_estabelecimento}",
                        Location = new Location(estabelecimento.latitude_estabelecimento_decimo_grau.Value, estabelecimento.longitude_estabelecimento_decimo_grau.Value)
                    };

                    map.Pins.Add(pin);
                    idsDePinsAdicionados.Add(idPin);
                    _logger?.LogInformation($"PIN criado: {pin.Label} - Latitude: {estabelecimento.latitude_estabelecimento_decimo_grau}, Longitude: {estabelecimento.longitude_estabelecimento_decimo_grau}");
                }
                else
                {
                    _logger?.LogWarning($"Estabelecimento {estabelecimento.nome_fantasia} não possui coordenadas válidas.");
                }
            }

            _logger?.LogInformation($"Total de PINs exibidos: {map.Pins.Count}");
        }
    }
}
