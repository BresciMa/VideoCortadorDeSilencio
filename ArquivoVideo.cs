using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoCortadorDeSilencio
{
    struct sArquivoOriginal {

    }

    public class ArquivoVideo
    {
        public string NomeArquivo { get; set; }

        public string ArquivoAudioParaCorte { get; set; }
        public string AudioTratado { get; set; }

        public string pressetsFfmpeg { get; set; }
        public string videoBitRate { get; set; }
        public string audioBitRate { get; set; }
        public string minDbDetect { get; set; }
        public string minSilenceDuration { get; set; }
        public Double duracaoDeUmaVoz { get; set; }
        public Double softCut { get; set; }
        public bool ModoTeste {get; set;}
        public bool ModoDebug { get; set; }

        public int numeroCortesModoTeste { get; set; }

        public bool PreserveInitialSilence {get; set;}


        public ArquivoVideo(){

            minDbDetect = "-32dB";
            minSilenceDuration = "0.34";
            duracaoDeUmaVoz = 0.10;
            softCut = 0.025;

            ModoDebug = false;
            ModoTeste = false;
            PreserveInitialSilence = false;

        }

        public ArquivoVideo GenerateArquivoSaida(){
            
            ArquivoVideo rt = new ArquivoVideo();

            rt.NomeArquivo =        NomeArquivo.ToUpper().Replace(".MP4", "") + "vss_recortado";
            rt.pressetsFfmpeg =     pressetsFfmpeg;
            rt.videoBitRate =       videoBitRate;
            rt.audioBitRate =       audioBitRate;
            rt. minDbDetect =       minDbDetect;
            rt. minSilenceDuration = minSilenceDuration;
            rt.duracaoDeUmaVoz =    duracaoDeUmaVoz;
            rt.softCut =            softCut;
            return rt;

        }


    }
}