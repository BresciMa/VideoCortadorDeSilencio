using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoCortadorDeSilencio
{
    public class Silencedetect
    {
        
        public void Detectar(ArquivoVideo entrada, ArquivoVideo saida, FFconfig configs){

            string silencedetect = configs.silenceDetect.ToUpper().Replace("C:", "").Replace(@"\", "/");

            string ffmpeg = " -i " + entrada.NomeArquivo + " -af \"silencedetect=n=" + 
                entrada.minDbDetect + ":d=" + 
                entrada.minSilenceDuration +  
                ",ametadata=mode=print:file=" + silencedetect + "\" -f null - ";

            if (entrada.modoDebug){
                Console.WriteLine(ffmpeg);
            }
            
            Render obj = new Render();

            obj = new Render();
            obj.setFfmpegProgram(configs.ffmpegProgram);
            obj.Renderiza(ffmpeg);

        }

        public Processo Analisar(ArquivoVideo entrada, ArquivoVideo saida, FFconfig configs){

            Processo proc = new Processo();

            Boolean voiceDetected = false;
            Boolean avancouCorte = false;
            Double silenceStart = 0;
            Double silenceEnd = 0;            
            Double voiceDuration = 0;
            Double silenceDuration = 0;
            int videoCounter= 1;
            string arquivoSaida;
            string ffmpeg;
            Render obj;
            string line;
            string lastCommand = "";
            Double duracaoDoClip = 0;
            Boolean firstCut = true;

            Double corteStart = 0;
            Double corteEnd = 0;

            int contagemDeLinhas = 0;
            List<string> objLinhas = new  List<string>();
            List<Recorte> objRecortes = new  List<Recorte>();
            double validadorDeCorte = 0;


            /*
            // LE O ARQUIVO DE CORTE E JOGA DENTRO DE UMA VARIAVEL
            */

            System.IO.StreamReader file = new System.IO.StreamReader(configs.silenceDetect);  
            string lineFile;  
            while((lineFile = file.ReadLine()) != null)  
            {  
                if (lineFile.Trim().Length > 1){
                    objLinhas.Add(lineFile);
                    contagemDeLinhas++;
                }
            }
            file.Close();  


            for (int i = 0; i <= contagemDeLinhas-1; i++){
                line = objLinhas[i];
                
                if (line.IndexOf("silence_start") > 0){
                    lastCommand = "start";

                    silenceStart = Convert.ToDouble(line.Substring( line.IndexOf("=") + 1,  (line.Length - line.IndexOf("=")-1)).Replace('.', ','));

                    /* 
                    Detectar vídeos que não começam no Silencio
                    Estes vídeos representam que o silencio está no começo do arquivo 
                    */

                    if (firstCut){
                        if (silenceStart > 1){
                            voiceDetected = true;
                            silenceEnd = 0;
                        }
                        firstCut = false;
                    }

                    if (voiceDetected){
                        voiceDuration = silenceStart-silenceEnd;
                        corteStart = silenceEnd;
                        corteEnd = silenceStart;

                        // Para evitar cortes frequentes, foi definido um intervalo minimo para cortes, porque
                        // O Silencio pode ser uma Pausa de Linguagem. Exceto para o ultimo corte, que pode ter qualquer comprimento

                        if(entrada.modoTeste){
                            Console.Write("SS:" + Util.TimeFormat(silenceEnd) + " TO " + Util.TimeFormat(silenceStart) + ";");
                            Console.Write("Corte:;" + (videoCounter).ToString("D8") + ";" )  ;
                            Console.Write("Duração do Silêncio:;" + silenceDuration  + ";" );
                            Console.Write("Duração da Voz:; " + voiceDuration  + ";");
                        }                        

                        if (voiceDuration >=  entrada.duracaoDeUmaVoz || (i == contagemDeLinhas-1)) {
                            
                            if(entrada.modoTeste){
                                Console.WriteLine("Cortado");
                            }

                            voiceDetected = false; 
                            avancouCorte  = false;
                            duracaoDoClip = duracaoDoClip + voiceDuration;
                            
                            //VALIDADOR DE RECORTE
                            if (corteStart < validadorDeCorte){
                                Console.WriteLine ("ALERTA INCONSISTÊNCIA DE ARQUIVO");
                            }
                            validadorDeCorte = corteEnd;


                            // MONTAGEM DO ARQUIVO A SER CORTADO
                            arquivoSaida = saida.NomeArquivo + (videoCounter++).ToString("D8") + ".mp4";

                            Recorte recorte = new Recorte();
                            recorte.nomeDoArquivo = arquivoSaida;
                            recorte.inicio = corteStart;
                            recorte.fim = corteEnd;

                            recorte.log = "Voice Detected " + (videoCounter).ToString("D3")  +  ": " + voiceDuration.ToString("N3") + 
                                " Start: " + Util.TimeFormat(corteStart) + 
                                " End: " + Util.TimeFormat(corteEnd);

                            objRecortes.Add(recorte);
                            proc.ArquivoAdd(arquivoSaida);

                        }
                        else{
                            avancouCorte  = true;
                            if(entrada.modoTeste){
                                Console.WriteLine("");
                            }
                        }
                    }
                }

                // IDENTIFICA A LINHA DE FINAL DE SILENCIO
                if (line.IndexOf("silence_end") > 0 ){
                    lastCommand = "end";

                    double tmpSilenceEnd = Convert.ToDouble(line.Substring( line.IndexOf("=") + 1,  (line.Length - line.IndexOf("=")-1) ).Replace('.', ','));
                    silenceDuration = tmpSilenceEnd-silenceStart;

                    //FORÇA O CORTE PARA SILENCIO MAIOR QUE 1 SEGUNDO
                    if (silenceDuration > 1 && avancouCorte){
                        Console.WriteLine("SS:" + Util.TimeFormat(silenceEnd) + " TO " + Util.TimeFormat(silenceStart) + ";"
                            + "Corte:; " + (videoCounter).ToString("D8") + ";"
                            + "Duração do Silêncio:;" + silenceDuration  + ";"
                        );

                        corteStart = silenceEnd;
                        corteEnd = silenceStart;

                        //VALIDADOR DE RECORTE
                        if (corteStart < validadorDeCorte){
                            Console.WriteLine ("ALERTA INCONSISTÊNCIA DE ARQUIVO: " + corteStart + " : " + corteEnd);
                        }
                        validadorDeCorte = corteEnd;

                        arquivoSaida = saida.NomeArquivo + (videoCounter++).ToString("D8") + ".mp4";

                        Recorte recorte = new Recorte();
                        recorte.nomeDoArquivo = arquivoSaida;
                        recorte.inicio = corteStart;
                        recorte.fim = corteEnd;
                        recorte.log = "Voice Detected " + (videoCounter).ToString("D3")  +  ": " + voiceDuration.ToString("N3") + 
                            " Start: " + Util.TimeFormat(corteStart) + 
                            " End: " + Util.TimeFormat(corteEnd);

                        objRecortes.Add(recorte);
 
                        proc.ArquivoAdd(arquivoSaida);

                        voiceDetected = true;
                        silenceEnd = tmpSilenceEnd;

                    }

                    if (!voiceDetected ){
                        silenceEnd = tmpSilenceEnd;
                        voiceDetected = true;
                    }
                }

            }


            // IDENTIFICANDO UMA PARTE FINAL DO VÍDEO NÃO CORTADO
            if (lastCommand == "end"){
                corteStart = silenceEnd;
                corteEnd = -1;

                arquivoSaida = saida.NomeArquivo + (videoCounter++).ToString("D8") + ".mp4";
                Recorte recorte = new Recorte();
                recorte.nomeDoArquivo = arquivoSaida;
                recorte.inicio = corteStart;
                recorte.fim = corteEnd;

                recorte.log = "Voice Detected " + (videoCounter).ToString("D3")  + 
                    " Start: " + Util.TimeFormat(corteStart) + 
                    " End: " + Util.TimeFormat(corteEnd);                

                objRecortes.Add(recorte);
 
                proc.ArquivoAdd(arquivoSaida);
            }

            string duracaoDoClipStr = TimeSpan.FromSeconds(duracaoDoClip).ToString(@"hh\:mm\:ss\.fff"); 
            Console.WriteLine ("Duração do Novo Arquivo: "  + duracaoDoClipStr);            

            int novoContador = 1;
            validadorDeCorte = 0;

            foreach (Recorte recorte in objRecortes){
                Console.WriteLine ( "[" + (novoContador++) + "/" + (videoCounter-1) + "] "  + recorte.nomeDoArquivo);

                if (recorte.inicio < validadorDeCorte){
                    Console.WriteLine ("ALERTA INCONSISTÊNCIA DE ARQUIVO");
                    Console.WriteLine ("RECORTE: " + " Start: " + Util.TimeFormat(recorte.inicio) + 
                                " End: " + Util.TimeFormat(recorte.fim));
                }
                validadorDeCorte = recorte.inicio;
    
                //Suaviza o corte conforme parametro
                if (entrada.softCut > 0){
                    recorte.inicio = recorte.inicio - entrada.softCut;
                    if (recorte.inicio < 0){
                        recorte.inicio = 0;
                    }
                    if (recorte.fim > 0){
                        recorte.fim = recorte.fim + entrada.softCut;
                    }
                }

                ffmpeg = " -i " + entrada.NomeArquivo ;
                if (!String.IsNullOrEmpty(entrada.pressetsFfmpeg)){
                    ffmpeg += " -filter_complex \"[0:v]" + entrada.pressetsFfmpeg + "[vout]\" -map \"[vout]\"";
                }
                else{
                    ffmpeg += " -map 0:v" ;
                }
                ffmpeg += " -map 0:a ";
                ffmpeg += "-c:v libx264 ";
                ffmpeg += "-b:v " + entrada.videoBitRate + " ";
                ffmpeg += "-b:a " + entrada.audioBitRate + " ";
                ffmpeg += "-pix_fmt yuv420p -y "; 
                ffmpeg += " -ss " + Util.TimeFormat(recorte.inicio);
                if (recorte.fim > 0){
                        ffmpeg += " -to " + Util.TimeFormat(recorte.fim);
                }
                ffmpeg += " " + recorte.nomeDoArquivo;                    
                
                if (!entrada.modoTeste){    
                    obj = new Render();
                    obj.setFfmpegProgram(configs.ffmpegProgram);
                    obj.Renderiza(ffmpeg);
                }

                if (entrada.modoDebug){
                    Console.WriteLine(configs.ffmpegProgram + " " + ffmpeg);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(configs.pastaDeTrabalho, configs.nomeArquivoMergeTemporario), false))
            {
                outputFile.WriteLine(proc.getArquivoMerge());
            }
            Console.WriteLine ("Duração do Novo Arquivo: "  + duracaoDoClipStr);
            return proc;
        }


    }
}