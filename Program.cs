using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoCortadorDeSilencio
{
    class Program
    {
        static void Main(string[] args)
        {
            String OriginalFile = @"C:\Users\bresciani\Desktop\Curta\Videos\20200919_082015-critico-interno.mp4";
            ArquivoVideo Entrada = new ArquivoVideo();

            //habilitando o modo teste
            //Entrada.modoTeste = true;
            //Entrada.modoDebug = true;

            //Entrada.pressetsFfmpeg ="unsharp=3:3:1,curves=psfile=/Users/bresciani/Desktop/Curta/Scripts/2020-09-03-escritorio-camiseta-azul.acv";
            Entrada.minDbDetect =  "-30dB";
            Entrada.minSilenceDuration = "0.34";
            Entrada.duracaoDeUmaVoz = 3; //0.20; 
            Entrada.softCut = 0;  //0.025;
            Entrada.videoBitRate = "10000k";
            Entrada.audioBitRate = "256k";
            
            //AMBIENTE CONTROLADO
            //ALTO RUIDO
            Entrada.minDbDetect = "-22dB";
            Entrada.minSilenceDuration = "0.50";

            /*
            [0] Arquivo Original 
            [1] TESTE ou Ajustes de Presset 
            [2] Nível de Ruído em dB
            [3] Milisegundos do Silencio
            [4] Duração minima do tamanho de voz
            [5] Suavização do Corte
            [6] Video Bitrate
            [7] Audio  Bitrate
            */
    
            if (args.Length >= 1 && args[0] != null) {
                OriginalFile = args[0];
            }
            if (args.Length >= 2 && args[1] != null) {
                if (args[1].ToUpper() == "TESTE"){
                    Entrada.modoTeste = true;
                }else{
                    Entrada.pressetsFfmpeg = args[1];
                }
            }

            if (args.Length >= 3 && args[2] != null) {
                Entrada.minDbDetect = args[2];
            }
            if (args.Length >= 4 && args[3] != null) {
                Entrada.minSilenceDuration = args[3];
            }
            if (args.Length >= 5 && args[4] != null) {
                Entrada.duracaoDeUmaVoz = Convert.ToDouble(args[4]);
            }
            if (args.Length >= 6 && args[5] != null) {
                Entrada.softCut = Convert.ToDouble(args[5]);
            }
            if (args.Length >= 7 && args[6] != null) {
                Entrada.videoBitRate = args[6];
            }
            if (args.Length >= 8 && args[7] != null) {
                Entrada.audioBitRate = args[7];
            }            

            FilmMaker fm = new FilmMaker();
            Entrada.NomeArquivo = OriginalFile;

            fm.Start(Entrada);

            Console.WriteLine ("Finalizando tudo.......");

        }

    }
}
