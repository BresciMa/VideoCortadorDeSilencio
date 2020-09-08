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
        public string pressetsFfmpeg { get; set; }
        public string videoBitRate { get; set; }
        public string audioBitRate { get; set; }
        public string minDbDetect { get; set; }
        public string minSilenceDuration { get; set; }
        public Double duracaoDeUmaVoz { get; set; }
        public Double softCut { get; set; }
        public bool modoTeste {get; set;}
        public bool modoDebug { get; set; }

        public ArquivoVideo(){

            minDbDetect = "-32dB";
            minSilenceDuration = "0.34";
            duracaoDeUmaVoz = 0.10;
            softCut = 0.025;

            modoDebug = false;
            modoTeste = false;

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