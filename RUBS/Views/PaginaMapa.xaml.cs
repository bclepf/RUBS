using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SQLite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System;
using RUBS.Services;

namespace RUBS.Views
{
    public partial class PaginaMapa : ContentPage
    {
        private SQLiteConnection _db;
        private ILogger<PaginaMapa> _logger;
        private bool SusLigado = false;
        private List<EstabelecimentosDB> todosEstabelecimentos;
        private List<EstabelecimentosDB> estabelecimentosSUS;
        private HashSet<string> idsDePinsAdicionados = new HashSet<string>();
        private string codigoMunicipioAnterior = "";

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

            var codigosSUS = new HashSet<string> { "1015", "1023", "1031", "1040", "1058", "1066", "1074", "1082", "1104", "1112", "1120", "1139", "1147", "1155", "1163", "1171", "1180", "1198", "1210", "1228", "1236", "1244", "1252", "1260", "1279", "1287", "1295", "1309", "1317", "1325", "1333", "1341" };
            estabelecimentosSUS = todosEstabelecimentos
                .Where(e => !string.IsNullOrEmpty(e.descricao_natureza_juridica_estabelecimento) && codigosSUS.Contains(e.descricao_natureza_juridica_estabelecimento))
                .ToList();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var codigoMunicipioAtual = MunicipioService.Instance.CodigoMunicipioSelecionado;
            if (codigoMunicipioAtual != codigoMunicipioAnterior)
            {
                codigoMunicipioAnterior = codigoMunicipioAtual;
                await CarregarPinsEmBackgroundAsync(SusLigado ? estabelecimentosSUS : todosEstabelecimentos);
            }

            var geolocationRequest = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                _logger?.LogInformation($"Movendo mapa para a localização do usuário: Latitude {location.Latitude}, Longitude {location.Longitude}");
                map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
            }
        }

        private async Task CarregarPinsEmBackgroundAsync(List<EstabelecimentosDB> estabelecimentos)
        {
            map.Pins.Clear();
            idsDePinsAdicionados.Clear();

            const int loteSize = 30;
            var estabelecimentosParaAdicionar = estabelecimentos.ToList();

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
                                continue;
                            }

                            var pin = new Pin
                            {
                                Label = estabelecimento.nome_fantasia,
                                Address = $"{estabelecimento.endereco_estabelecimento}, {estabelecimento.numero_estabelecimento}, {estabelecimento.bairro_estabelecimento}",
                                Location = new Location(estabelecimento.latitude_estabelecimento_decimo_grau.Value, estabelecimento.longitude_estabelecimento_decimo_grau.Value)
                            };

                            pinsParaAdicionar.Add(pin);
                            idsDePinsAdicionados.Add(idPin);
                        }
                    }

                    await Dispatcher.DispatchAsync(() =>
                    {
                        foreach (var pin in pinsParaAdicionar)
                        {
                            map.Pins.Add(pin);
                        }
                    });

                    await Task.Delay(100);
                }
            });
        }

        private async void OnBotaoSUSClicked(object sender, EventArgs e)
        {
            SusLigado = !SusLigado;

            BotaoSUS.Text = SusLigado ? "SUS" : "Todos";
            BotaoSUS.BackgroundColor = SusLigado ? Color.FromArgb("#39559E") : Color.FromArgb("#E1395F");

            map.Pins.Clear();
            idsDePinsAdicionados.Clear();

            if (SusLigado)
            {
                if (estabelecimentosSUS.Any())
                {
                    await CarregarPinsEmBackgroundAsync(estabelecimentosSUS);
                }
                else
                {
                    await DisplayAlert("Atenção", "Nenhum estabelecimento SUS encontrado", "Ok");
                    _logger?.LogWarning("Nenhum estabelecimento SUS encontrado.");
                }
            }
            else
            {
                await CarregarPinsEmBackgroundAsync(todosEstabelecimentos);
            }
        }
    }
}
