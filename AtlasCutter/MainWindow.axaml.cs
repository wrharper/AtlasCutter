using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AtlasCutter
{
    public partial class MainWindow : Window
    {
        private const int DefaultTileSizeX = 16;
        private const int DefaultTileSizeY = 16;
        private const int DefaultRowCount = 100;
        private const string InputFile = "example.png";
        private const string OutputFolder = "Output";

        public MainWindow()
        {
            InitializeComponent();

            // Set default values for input fields
            var sizeXBox = this.FindControl<TextBox>("SizeXInput");
            var sizeYBox = this.FindControl<TextBox>("SizeYInput");
            var rowCountBox = this.FindControl<TextBox>("RowCountInput");

            if (sizeXBox != null) sizeXBox.Text = DefaultTileSizeX.ToString();
            if (sizeYBox != null) sizeYBox.Text = DefaultTileSizeY.ToString();
            if (rowCountBox != null) rowCountBox.Text = DefaultRowCount.ToString();

            var generateButton = this.FindControl<Button>("GenerateButton");
            if (generateButton != null)
            {
                generateButton.Click += OnGenerateClick;
            }
        }

        private void OnGenerateClick(object? sender, RoutedEventArgs e)
        {
            // Ensure input retrieval doesn't cause null reference exceptions
            var sizeXBox = this.FindControl<TextBox>("SizeXInput");
            var sizeYBox = this.FindControl<TextBox>("SizeYInput");
            var rowCountBox = this.FindControl<TextBox>("RowCountInput");

            int sizeX = sizeXBox != null && int.TryParse(sizeXBox.Text, out var sx) ? sx : DefaultTileSizeX;
            int sizeY = sizeYBox != null && int.TryParse(sizeYBox.Text, out var sy) ? sy : DefaultTileSizeY;
            int rowCount = rowCountBox != null && int.TryParse(rowCountBox.Text, out var rows) ? rows : DefaultRowCount;

            if (sizeX > 0 && sizeY > 0 && rowCount > 0)
            {
                ProcessAtlas(sizeX, sizeY, rowCount);
            }
            else
            {
                ShowDialog("Invalid inputs! Please enter positive numbers.");
            }
        }

        private async void ShowDialog(string message)
        {
            var dialog = new Window
            {
                Content = new TextBlock { Text = message, Margin = new Thickness(20) },
                Width = 300,
                Height = 150
            };

            await dialog.ShowDialog(this);
        }

        private void ProcessAtlas(int sizeX, int sizeY, int rowCount)
        {
            if (!File.Exists(InputFile))
            {
                Console.WriteLine("Error: Input file not found!");
                return;
            }

            // Ensure the output folder exists
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            using SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(InputFile);
            int columns = image.Width / sizeX;
            int totalTiles = rowCount * columns;

            for (int i = 0; i < totalTiles; i++)
            {
                int x = (i % columns) * sizeX;
                int y = (i / columns) * sizeY;

                if (y + sizeY > image.Height) break; // Prevent out-of-bounds slicing

                using SixLabors.ImageSharp.Image<Rgba32> tile = image.Clone(ctx => ctx.Crop(new Rectangle(x, y, sizeX, sizeY)));
                string outputPath = Path.Combine(OutputFolder, $"{i}.png");
                tile.Save(outputPath);
            }

            Console.WriteLine($"Atlas processing complete! {totalTiles} tiles saved.");
        }
    }
}
