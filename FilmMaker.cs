using System;

namespace VideoCortadorDeSilencio
{
    public class FilmMaker
    {
        public bool Start(ArquivoVideo Entrada)
        {
            ArquivoVideo Saida = Entrada.GenerateArquivoSaida();

            //private string ffmpegProgram = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"library\ffmpeg\bin\ffmpeg.exe");
            string ffmpegProgram = @"E:\Java\ffmpeg-4.3.2\bin\ffmpeg.exe";

            FFconfig FFConfig = new FFconfig();
            FFConfig.silenceDetect = @"\TMP\CUT.txt";
            FFConfig.pastaDeTrabalho = @"E:\TMP\";
            FFConfig.nomeArquivoMergeTemporario = @"\TMP\arquivoconcatenacao.txt";
            FFConfig.ffmpegProgram = ffmpegProgram;

            Console.WriteLine ("Arquivo de Silencio:" + FFConfig.silenceDetect);

            Console.WriteLine ("Arquivo de Corte gerado em: " + FFConfig.silenceDetect);

            // DETECTA O SILENCIO E SALVA EM ARQUIVO DE SILENCIO
            Silencedetect objSilence = new Silencedetect();

            // 1 - DETECTA O SILENCIO NO ARQUIVO
            objSilence.Detectar(Entrada, Saida, FFConfig);

            // 2 - ANALISA AS FALAS DENTRO DO ARQUIVO - GERA ARQUIVOS CORTADOS
            Processo proc = objSilence.Analisar(Entrada, Saida, FFConfig);

            // 3 - CONVERTE TODOS OS ARQUIVOS CORTADOS EM UM SÓ ARQUIVO
            if (!Entrada.ModoTeste){
                proc.Concatena(Entrada, Saida, FFConfig);
            }

            return true;
        }


    }
}