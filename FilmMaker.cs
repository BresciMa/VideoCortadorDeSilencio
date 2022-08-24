using System;
using System.IO;

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

            
            
            //PROCESSAMENTO POR AQUIVO UNICO
            if(File.Exists(Entrada.NomeArquivo))
            {
                // 1 - DETECTA O SILENCIO NO ARQUIVO
                objSilence.Detectar(Entrada, Saida, FFConfig);

                // 2 - ANALISA AS FALAS DENTRO DO ARQUIVO - GERA ARQUIVOS CORTADOS
                Processo proc = objSilence.Analisar(Entrada, Saida, FFConfig);

                // 3 - CONVERTE TODOS OS ARQUIVOS CORTADOS EM UM SÓ ARQUIVO
                if (!Entrada.ModoTeste){
                    proc.Concatena(Entrada, Saida, FFConfig);
                }
            }
            
            //PROCESSAMENTO POR DIRETORIO
            else if(Directory.Exists(Entrada.NomeArquivo))
            {
                string [] fileEntries = Directory.GetFiles(Entrada.NomeArquivo, "*.mp4");
                Array.Sort(fileEntries, StringComparer.InvariantCulture);


                Processo processoPrincipal = new Processo();


                foreach(string fileName in fileEntries){
                    
                    Entrada.NomeArquivo = fileName;
                    
                    // 1 - DETECTA O SILENCIO NO ARQUIVO
                    objSilence.Detectar(Entrada, Saida, FFConfig);

                    // 2 - ANALISA AS FALAS DENTRO DO ARQUIVO - GERA ARQUIVOS CORTADOS
                    Processo _proc = objSilence.Analisar(Entrada, Saida, FFConfig);
                    processoPrincipal.ArquivosAdd(_proc.getArquivosLista() );

                }

                // 3 - CONVERTE TODOS OS ARQUIVOS CORTADOS EM UM SÓ ARQUIVO
                if (!Entrada.ModoTeste){
                    processoPrincipal.Concatena(Entrada, Saida, FFConfig);
                }


            }
            else
            {
                Console.WriteLine("Arquivo ou diretório não encontrado: ", Entrada.NomeArquivo);
            }

            return true;
        }


    }
}