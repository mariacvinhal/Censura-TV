using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GravadorCensura
{
    public partial class Form1 : Form
    {
        // ================================
        // VARIÁVEIS DE DETECÇÃO DE ERRO
        // ================================

        // Envio de alertas para o Telegram

        private bool _alertaCriticoEnviado = false;
        private double _tempoLimiteAlerta = 30.0; // Exemplo: 30 segundos de tela preta para avisar

        // Guarda o horário do último frame recebido
        private DateTime ultimoFrame = DateTime.MinValue;

        // Guarda o último bitmap para comparação de frames congelados
        private Bitmap ultimoBitmap = null;

        // Contador de frames iguais (usado para detectar congelamento)
        private int framesCongelados = 0;

        // Controle de intervalo entre avisos do mesmo erro
        private DateTime _ultimoErroSemSinal = DateTime.MinValue;
        private DateTime _ultimoErroTelaPreta = DateTime.MinValue;
        private DateTime _ultimoErroFrameCongelado = DateTime.MinValue;

        // Controle de detecção de tela preta
        private DateTime _inicioTelaPreta = DateTime.MinValue;
        private bool _detectandoTelaPreta = false;

        // Quantos segundos de tela preta antes de disparar alerta
        private int _segundosTelaPreta = 8;


        // Intervalo mínimo entre logs do mesmo erro
        private TimeSpan _intervaloErros = TimeSpan.FromMinutes(10);

        // ================================
        // LOGS
        // ================================
        private string _logPath = @"C:\Censuras\Logs"; // Caminho onde os logs serão armazenados

        // Evento acionado sempre que um erro é detectado
        public Action<string> AoDetectarErro;


        // ================================
        // CONTROLE DE SISTEMA
        // ================================

        // Define se o sistema iniciou automaticamente
        private bool _inicioAutomatico = false;
        // Monitor de uso de CPU do sistema
        private PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        // Quantos dias manter as gravações antes de apagar
        private int _diasDeRetencao = 90;
        // Hora em que a gravação começou
        private DateTime _horaInicio;
        // Cronômetro para mostrar tempo de gravação
        private Stopwatch _cronometro = new Stopwatch();

        // ================================
        // VARIÁVEIS DE GRAVAÇÃO (FFMPEG)
        // ================================

        // Indica se o sistema está gravando
        private bool _gravando = false;
        // Processo do FFmpeg responsável pela captura
        private Process _ffmpegProcess;

        // Pasta base onde as gravações serão salvas
        private string _basePath = @"C:\Censuras";

        // Pasta referente ao dia atual
        private string _currentToday;

        // Timer que roda o monitoramento do sistema
        private System.Windows.Forms.Timer _monitorTimer;

        public Form1()
        {
            InitializeComponent();

            // Botão de gravação transparente sobre a interface
            button1.Parent = this;
            button1.BackColor = Color.Transparent; // Importante para não ter fundo sólido

            ConfigurarPastaBase();
            InicializarInterface();
            DeixarBotaoRedondo();

            // Timer de monitoramento (1 segundo)
            _monitorTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _monitorTimer.Tick += (s, e) => MonitoramentoGeral();
            AoDetectarErro = (erro) =>
            {
                ExibirErroPreview(erro);
                EscreverLog("⚠️ " + erro);
            };

            // Autostart após exibir a janela
            this.Shown += (s, e) => {
                _inicioAutomatico = true;
                button1.PerformClick();
                _inicioAutomatico = false;
            };
        }
        // Cria pastas base do sistema se não existirem
        private void ConfigurarPastaBase()
        {
            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
            if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
        }

        #region Interface e Estética

        private void DeixarBotaoRedondo()
        {
            // Cria o recorte circular para o botão
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(1, 1, button1.Width - 3, button1.Height - 3);
            button1.Region = new Region(gp);

            // Estilização Flat
            button1.Parent = this;
            button1.BackColor = Color.Transparent;
            button1.TabStop = false;
            button1.FlatStyle = FlatStyle.Flat;
            // Remove bordas e efeitos padrão do botão
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255); // Transparente real
            button1.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;

            button1.Text = "";
            button1.BackgroundImageLayout = ImageLayout.Zoom;

            // Ícone inicial (play)
            button1.BackgroundImage = CarregarImagemAltaQualidade(Properties.Resources.icon_play, 64);
        }

        // Carrega PNG com alta fidelidade e transparência
        private Image CarregarImagemAltaQualidade(object recurso, int tamanho)
        {
            if (recurso is byte[] bytes)
            {
                using (var ms = new MemoryStream(bytes))
                {
                    using (Bitmap original = new Bitmap(ms))
                    {
                        Bitmap destino = new Bitmap(tamanho, tamanho, PixelFormat.Format32bppPArgb);
                        using (Graphics g = Graphics.FromImage(destino))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.Clear(Color.Transparent);
                            g.DrawImage(original, 0, 0, tamanho, tamanho);
                        }
                        return destino;
                    }
                }
            }
            return null;
        }
        // Adiciona linha no log visual
        private void AdicionarLinhaLog(string msg)
        {
            string linha = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {msg}";
            lstLog.Items.Insert(0, linha);

            // grava no arquivo
            EscreverLogArquivo(msg);
            if (lstLog.Items.Count > 100) lstLog.Items.RemoveAt(100);
        }

        // Método seguro para escrever no log vindo de outras threads
        private void EscreverLog(string mensagem)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            if (lstLog.InvokeRequired)
                lstLog.Invoke(new Action(() => AdicionarLinhaLog(mensagem)));
            else
                AdicionarLinhaLog(mensagem);
        }
        // Exibe mensagem de erro no preview de vídeo
        private void ExibirErroPreview(string erro)
        {
            Bitmap bmp = new Bitmap(picPreview.Width, picPreview.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.DarkRed);
                using (Font f = new Font("Arial", 12, FontStyle.Bold))
                {
                    string msg = "⚠️ SEM SINAL DA PLACA\n" + erro;
                    SizeF size = g.MeasureString(msg, f);
                    g.DrawString(msg, f, Brushes.White, (bmp.Width - size.Width) / 2, (bmp.Height - size.Height) / 2);
                }
            }
            picPreview.Invoke(new Action(() => picPreview.Image = bmp));
        }

        #endregion

        #region Lógica de Gravação e FFmpeg

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_gravando)
            {
                // Iniciar REC
                _horaInicio = DateTime.Now;
                _cronometro.Restart();
                try
                {
                    IniciarGravacao();
                }
                catch (Exception ex)
                {
                    ExibirErroPreview(ex.Message);
                }
                _monitorTimer.Start();

                button1.BackgroundImage = CarregarImagemAltaQualidade(Properties.Resources.icon_rec, 64);
                _gravando = true;

                if (_inicioAutomatico) EscreverLog("🚀 Sistema iniciado automaticamente.");
                else EscreverLog("👤 Gravação iniciada manualmente.");
            }
            else
            {
                // Parar REC
                PararGravacao();
                _monitorTimer.Stop();
                _cronometro.Stop();

                button1.BackgroundImage = CarregarImagemAltaQualidade(Properties.Resources.icon_play, 64);
                _gravando = false;

                lblStatus.Text = $"Pronto | Última: {_cronometro.Elapsed:hh\\:mm\\:ss}";
                if (picPreview.Image != null) { picPreview.Image.Dispose(); picPreview.Image = null; }

                EscreverLog("🔴 Gravação encerrada pelo usuário.");
            }
        }

        private void IniciarGravacao()
        {
            Thread.Sleep(2000); // Tempo para a placa estabilizar
            _currentToday = DateTime.Now.ToString("yyyy-MM-dd");
            string targetDir = Path.Combine(_basePath, _currentToday);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            string drawTV = "drawtext=text='TV Universitaria':x=20:y=20:fontsize=24:fontcolor=white:shadowcolor=black:shadowx=2:shadowy=2";
            string drawRelogio = "drawtext=text='%{localtime\\:%d/%m/%Y  -  %H\\\\\\:%M\\\\\\:%S}':x=w-tw-20:y=20:fontsize=24:fontcolor=white:shadowcolor=black:shadowx=2:shadowy=2";

            string args = $"-y -f dshow -rtbufsize 1500M -use_wallclock_as_timestamps 1 " +
                          $"-pixel_format uyvy422 -video_size 1920x1080 -framerate 29.97 " +
                          $"-i video=\"Blackmagic WDM Capture\":audio=\"Blackmagic WDM Capture\" " +
                          $"-vf \"yadif,scale=720:480,{drawTV},{drawRelogio}\" " +
                          $"-c:v libx264 -preset veryfast -b:v 450k -maxrate 600k -bufsize 1M -pix_fmt yuv420p " +
                          $"-c:a aac -b:a 64k -ar 44100 -ac 1 " +
                          $"-f segment -segment_time 3600 -segment_atclocktime 1 -reset_timestamps 1 -strftime 1 \"{targetDir}\\Censura_%Y-%m-%d_%H-%M-%S.ts\" " +
                          $"-f image2pipe -vcodec mjpeg -vf \"fps=10,scale=480:270\" -";

            _ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _ffmpegProcess.Start();
            new Thread(LerStreamPreview) { IsBackground = true }.Start();
        }

        private void PararGravacao()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                try { _ffmpegProcess.Kill(); _ffmpegProcess.WaitForExit(1000); } catch { }
            }
            lblStatus.Text = "Gravação Interrompida.";
        }

        private void LerStreamPreview()
        {
            using (Stream stream = _ffmpegProcess.StandardOutput.BaseStream)
            {
                byte[] buffer = new byte[1024 * 512];
                MemoryStream ms = new MemoryStream();

                while (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    try
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read <= 0) break;

                        ms.Write(buffer, 0, read);
                        byte[] data = ms.ToArray();
                        int endPos = EncontrarByteFinal(data);

                        if (endPos != -1)
                        {
                            ultimoFrame = DateTime.Now;
                            byte[] frameData = new byte[endPos + 2];
                            Array.Copy(data, frameData, frameData.Length);

                            using (MemoryStream frameMs = new MemoryStream(frameData))
                            {
                                Image img = Image.FromStream(frameMs);
                                Bitmap bmpFrame = new Bitmap(img);

                                // atualiza detector
                                FrameRecebido(bmpFrame, false);

                                picPreview.Invoke(new Action(() =>
                                {
                                    picPreview.Image?.Dispose();
                                    picPreview.Image = (Bitmap)bmpFrame.Clone();
                                }));
                            }
                            ms.SetLength(0);
                            if (data.Length > endPos + 2)
                                ms.Write(data, endPos + 2, data.Length - (endPos + 2));
                        }
                    }
                    catch { /* Erro de stream ignorado para manter thread viva */ }
                }
            }
        }

        private int EncontrarByteFinal(byte[] data)
        {
            for (int i = 0; i < data.Length - 1; i++)
                if (data[i] == 0xFF && data[i + 1] == 0xD9) return i;
            return -1;
        }

        #endregion

        #region Monitoramento e Manutenção

        private void MonitoramentoGeral()
        {
            AtualizarStatusDiscoECpu();
            //VerificarTimeout();

            if (!_gravando)
            {
                lblStatus.Text = "Sistema Pronto";
                lblStatus.ForeColor = Color.White;
                return;
            }

            lblStatus.Text = $"REC: {_cronometro.Elapsed:hh\\:mm\\:ss} | Local: {_currentToday}";

            // Virada de Dia
            if (DateTime.Now.ToString("yyyy-MM-dd") != _currentToday)
            {
                EscreverLog("📅 Virada de dia. Tudo certo por enquanto.");
                ExecutarAutoPurge();
                ReiniciarSistema();
            }
        }

        private void AtualizarStatusDiscoECpu()
        {
            try
            {
                float cpuUso = _cpuCounter.NextValue();
                lblCPU.Text = $"Uso de CPU: {cpuUso:0}%";
                lblCPU.ForeColor = cpuUso > 90 ? Color.Red : Color.White;

                DriveInfo drive = new DriveInfo("C");
                long totalGB = drive.TotalSize / 1073741824;
                long livreGB = drive.AvailableFreeSpace / 1073741824;
                int percentualUsado = (int)(((totalGB - livreGB) * 100) / totalGB);

                pbDisco.Value = percentualUsado;
                lblDiscoPorcentagem.Text = $"Disco: {livreGB} GB Livres ({percentualUsado}% usado)";
                lblDiscoPorcentagem.ForeColor = livreGB < 20 ? Color.Red : Color.LightGreen;
            }
            catch { }
        }

        private void ExecutarAutoPurge()
        {
            try
            {
                if (!Directory.Exists(_basePath)) return;
                string[] pastas = Directory.GetDirectories(_basePath);
                foreach (string pasta in pastas)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(pasta);
                    if (dirInfo.CreationTime < DateTime.Now.AddDays(-_diasDeRetencao))
                    {
                        Directory.Delete(pasta, true);
                        EscreverLog($"🗑️ Purge: {dirInfo.Name} removida.");
                    }
                }
            }
            catch (Exception ex) { EscreverLog("⚠️ Erro Purge: " + ex.Message); }
        }

        private void ReiniciarSistema()
        {
            PararGravacao();
            Thread.Sleep(10000); // Aguarda liberação do hardware Blackmagic
            IniciarGravacao();
        }

        #endregion

        private void InicializarInterface()
        {
            lblCPU.Text = "CPU: --";
            lblStatus.Text = "Aguardando...";
            lstLog.Items.Clear();
            EscreverLog("Monitor pronto.");
        }

        public void FrameRecebido(Bitmap frame, bool semInputFlag)
        {
            ultimoFrame = DateTime.Now;

            if (semInputFlag)
            {
                if (DateTime.Now - _ultimoErroSemSinal > _intervaloErros)
                {
                    _ultimoErroSemSinal = DateTime.Now;
                    AoDetectarErro?.Invoke("Cabo desconectado ou sem sinal");
                }
                return;
            }
            if (TelaPreta(frame))
            {
                if (!_detectandoTelaPreta)
                {
                    _inicioTelaPreta = DateTime.Now;
                    _detectandoTelaPreta = true;
                    _alertaCriticoEnviado = false;
                }
                // Não faz nada aqui além de esperar a tela voltar ao normal
                var tempoPassado = (DateTime.Now - _inicioTelaPreta).TotalSeconds;

                if (tempoPassado >= _tempoLimiteAlerta && !_alertaCriticoEnviado)
                {
                    AoDetectarErro?.Invoke($"Tela preta detectada por mais de {_segundosTelaPreta}s!");
                    // Dispara o alerta sem travar o processamento do frame
                    Task.Run(() => EnviarTelegram($"🚨 *ERRO CRÍTICO*: Tela preta detectada há mais de {tempoPassado:N0}s!"));
                    _alertaCriticoEnviado = true;
                }
            }
            else
            {
                if (_detectandoTelaPreta)
                {
                    var duracaoTotal = (DateTime.Now - _inicioTelaPreta).TotalSeconds;

                    if (_alertaCriticoEnviado)
                    {
                        Task.Run(() => EnviarTelegram($"✅ *SISTEMA RECUPERADO*: A tela voltou ao normal após {duracaoTotal:N2}s."));
                    }

                    _detectandoTelaPreta = false;



                    // Verifica se a duração total foi longa o suficiente para ser considerada um erro
                    if (duracaoTotal >= _segundosTelaPreta)
                    {
                        // Opcional: manter o intervalo entre erros se houver várias quedas seguidas
                        if (DateTime.Now - _ultimoErroTelaPreta > _intervaloErros)
                        {
                            _ultimoErroTelaPreta = DateTime.Now;

                            AoDetectarErro?.Invoke(
                                $"Tela preta finalizada. Duração total: {Math.Round(duracaoTotal, 2)} segundos."
                            );
                        }
                    }

                    // Importante: resetar a flag para a próxima detecção
                    _detectandoTelaPreta = false;
                }
            }

            if (framesCongelados > 30)
            {
                if (DateTime.Now - _ultimoErroFrameCongelado > _intervaloErros)
                {
                    _ultimoErroFrameCongelado = DateTime.Now;
                    AoDetectarErro?.Invoke("Frame congelado");
                }
            }
            else
            {
                framesCongelados = 0;
            }

            ultimoBitmap?.Dispose();
            ultimoBitmap = (Bitmap)frame.Clone();
        }

        /*public void VerificarTimeout()
        {
            if ((DateTime.Now - ultimoFrame).TotalSeconds > 10)
            {
                if (DateTime.Now - _ultimoErroSemSinal > _intervaloErros)
                {
                    _ultimoErroSemSinal = DateTime.Now;
                    AoDetectarErro?.Invoke("Nenhum frame recebido");
                }
            }
        }
        */
        private bool TelaPreta(Bitmap bmp)
        {
            long soma = 0;
            int amostras = 0;
            int pixelsClaros = 0;

            for (int y = 0; y < bmp.Height; y += 40)
            {
                for (int x = 0; x < bmp.Width; x += 40)
                {
                    Color c = bmp.GetPixel(x, y);
                    int brilho = (c.R + c.G + c.B) / 3;

                    soma += brilho;
                    amostras++;

                    if (brilho > 40) // pixel claro
                        pixelsClaros++;
                }
            }

            int media = (int)(soma / amostras);
            double percentualClaros = (double)pixelsClaros / amostras;

            // tela preta se média baixa E quase nenhum pixel claro
            return media < 15 && percentualClaros < 0.01;
        }


        private void EscreverLogArquivo(string mensagem)
        {
            try
            {
                string arquivo = Path.Combine(_logPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                string linha = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {mensagem}";
                File.AppendAllText(arquivo, linha + Environment.NewLine);
            }
            catch
            {
                // evita travar o sistema se der erro de disco
            }
        }
        private async Task EnviarTelegram(string mensagem)
        {
            try
            {
                string token = "TOKEN";
                string chatId = "ID";
                string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(mensagem)}&parse_mode=Markdown";

                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    await client.GetAsync(url);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro Telegram: " + ex.Message);
            }
        }

    }
}
