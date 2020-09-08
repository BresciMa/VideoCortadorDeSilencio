using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoCortadorDeSilencio
{
    public class Processo
    {
        
        public string multipleFiles { get ; set ;}
        private List<string> arquivos = new  List<string>();
        private StringBuilder stringBuilder = new StringBuilder();
        public void ArquivoAdd(String arquivo){
                        
            stringBuilder.Append("file "  + arquivo.Replace("\\", "/") + Environment.NewLine);
            arquivos.Add(arquivo);
            
        }
        public List<string> getArquivosLista(){
            return arquivos;
        }
        public string getArquivoMerge(){
            return stringBuilder.ToString();
        }
        public void Concatena(ArquivoVideo entrada, ArquivoVideo saida, FFconfig configs){

            string ffmpeg = " -f concat -safe 0 -i " + configs.pastaDeTrabalho + configs.nomeArquivoMergeTemporario + 
                " -c copy -y " + saida.NomeArquivo + "_complete.mp4";

            Console.WriteLine (ffmpeg);
            Console.WriteLine ("Iniciando o agrupamento.......");
            
            Render obj = new Render();

            obj = new Render();
            obj.setFfmpegProgram(configs.ffmpegProgram);
            obj.Renderiza(ffmpeg);

            // DELETA OS ARQUIVOS TEMPORARIOS
            foreach (string f in arquivos)
            {
                File.Delete(f);
            }
        }
    }
}