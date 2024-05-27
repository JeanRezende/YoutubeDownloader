using System;
using System.IO;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader
{
    public partial class MainForm : Form
    {
        private readonly YoutubeClient _youtubeClient; 
        private string _downloadFolderPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
        

        public MainForm()
        {
            InitializeComponent(); //chamada de construcao da GUI
            _youtubeClient = new YoutubeClient(); //instanciando um cliente youtube
        }

        private async void DownloadButton_Click(object sender, EventArgs e) //evento do botão de download
        {
            if (string.IsNullOrWhiteSpace(linkTextBox.Text))
            {
                labelStatus.Text = "Link inválido";
                return;
            }

            bool downloadAsMp3 = checkBoxMp3.Checked;
            var videoId = linkTextBox.Text;

            try
            {
                var video = await _youtubeClient.Videos.GetAsync(videoId);
                var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

                IStreamInfo streamInfo;

                if (downloadAsMp3)
                {
                    streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                }
                else
                {
                    streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                }

                //Limpar titulo do video
                string cleanTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

                if (streamInfo != null)
                {
                    var filePath = Path.Combine(_downloadFolderPath, $"{cleanTitle}.{streamInfo.Container}");

                    labelStatus.Text = "Baixando vídeo...";
                    progressBar.Value = 0;

                    await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePath, progress: new Progress<double>(p => progressBar.Value = (int)(p*100)));

                    labelStatus.Text = "Download concluído!";

                    if (downloadAsMp3)
                    {
                        labelStatus.Text = "Convertendo para MP3";
                        ConvertToMp3(filePath, _downloadFolderPath);

                    }
                }
                else
                {
                    labelStatus.Text = "Falha em achar o video na qualidade escolhida.";
                }
            }
            catch (VideoUnavailableException)
            {
                MessageBox.Show("O video solicitado não esta disponivel.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConvertToMp3(string inputFilePath, string outputFilePath)
        {
            var ffmpeg = new NReco.VideoConverter.FFMpegConverter();
            ffmpeg.ConvertMedia(inputFilePath, outputFilePath, "mp3");
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _downloadFolderPath = folderDialog.SelectedPath;
                    folderPathLabel.Text = _downloadFolderPath;
                }
            }
        }
    }
}
