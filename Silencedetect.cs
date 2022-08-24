using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoCortadorDeSilencio
{
    public class Silencedetect
    {


        private String AjustarNomeDoArquivo (String nome){

            return nome.ToLower();

            /*return nome.ToLower().Replace(" ", "")
                .Replace("á","")
                .Replace("é","")
                .Replace("í","")
                .Replace("ó","")
                .Replace("ú","")
                .Replace("ç","")
                .Replace("ã","")
                .Replace("õ","");
            */

        }


        public void Detectar(ArquivoVideo entrada, ArquivoVideo saida, FFconfig configs)
        {

            String arquivoASerAnalizado = entrada.NomeArquivo;
            if (entrada.ArquivoAudioParaCorte != null && entrada.ArquivoAudioParaCorte != "")
            {
                arquivoASerAnalizado = entrada.ArquivoAudioParaCorte;
            }

            string silencedetect = configs.silenceDetect.ToUpper().Replace("C:", "").Replace(@"\", "/");

            string ffmpeg = " -i \"" + arquivoASerAnalizado + "\" -af \"silencedetect=n=" +
                entrada.minDbDetect + ":d=" +
                entrada.minSilenceDuration +
                ",ametadata=mode=print:file=" + silencedetect + "\" -f null - ";

            if (entrada.ModoDebug)
            {
                Console.WriteLine(ffmpeg);
            }

            Render obj = new Render();

            obj = new Render();
            obj.setFfmpegProgram(configs.ffmpegProgram);
            obj.Renderiza(ffmpeg);

        }

        public Processo Analisar(ArquivoVideo entrada, ArquivoVideo saida, FFconfig configs)
        {

            Processo proc = new Processo();

            Boolean voiceDetected = false;
            Boolean avancouCorte = false;
            Double silenceStart = 0;
            Double silenceEnd = 0;
            Double voiceDuration = 0;
            Double silenceDuration = 0;
            int videoCounter = 1;
            string arquivoSaida;
            string ffmpeg;
            Render obj;
            string line;
            string lastCommand = "";
            Double duracaoDoClip = 0;
            Boolean firstCut = true;
            Boolean realStartWithSilenceZero = false;

            Double corteStart = 0;
            Double corteEnd = 0;

            int contagemDeLinhas = 0;
            int recortesCriados = 0;

            List<string> objLinhas = new List<string>();
            List<Recorte> objRecortes = new List<Recorte>();
            double validadorDeCorte = 0;

            String timeStamp = GetTimestamp(DateTime.Now);

            /*
            // LE O ARQUIVO DE CORTE E JOGA DENTRO DE UMA VARIAVEL
            */

            System.IO.StreamReader file = new System.IO.StreamReader(configs.silenceDetect);
            string lineFile;
            while ((lineFile = file.ReadLine()) != null)
            {
                if (lineFile.Trim().Length > 1)
                {
                    objLinhas.Add(lineFile);
                    contagemDeLinhas++;
                }
            }
            file.Close();


            for (int i = 0; i <= contagemDeLinhas - 1; i++)
            {
                line = objLinhas[i];

                if (line.IndexOf("silence_start") > 0)
                {
                    lastCommand = "start";

                    silenceStart = Convert.ToDouble(line.Substring(line.IndexOf("=") + 1, (line.Length - line.IndexOf("=") - 1)).Replace('.', ','));

                    /* 
                    Detectar vídeos que não começam no Silencio
                    Estes vídeos representam que o silencio está no começo do arquivo 
                    */

                    if (firstCut)
                    {
                        if (silenceStart <= 0)
                        {
                            realStartWithSilenceZero = true;
                        }
                        if (silenceStart > 1)
                        {
                            voiceDetected = true;
                            silenceEnd = 0;
                        }
                        firstCut = false;
                    }

                    if (voiceDetected)
                    {
                        voiceDuration = silenceStart - silenceEnd;
                        corteStart = silenceEnd;
                        corteEnd = silenceStart;

                        // Para evitar cortes frequentes, foi definido um intervalo minimo para cortes, porque
                        // O Silencio pode ser uma Pausa de Linguagem. Exceto para o ultimo corte, que pode ter qualquer comprimento

                        if (entrada.ModoTeste)
                        {
                            Console.Write("SS:" + Util.TimeFormat(silenceEnd) + " TO " + Util.TimeFormat(silenceStart) + ";");
                            Console.Write("Corte:;" + (videoCounter).ToString("D8") + ";");
                            Console.Write("Duração do Silêncio:;" + silenceDuration + ";");
                            Console.Write("Duração da Voz:; " + voiceDuration + ";");
                        }

                        if (voiceDuration >= entrada.duracaoDeUmaVoz || (i == contagemDeLinhas - 1))
                        {

                            if (entrada.ModoTeste)
                            {
                                Console.WriteLine("Cortado");
                            }

                            voiceDetected = false;
                            avancouCorte = false;
                            duracaoDoClip = duracaoDoClip + voiceDuration;

                            //VALIDADOR DE RECORTE
                            if (corteStart < validadorDeCorte)
                            {
                                Console.WriteLine("ALERTA INCONSISTÊNCIA DE ARQUIVO");
                            }
                            validadorDeCorte = corteEnd;

                            // MONTAGEM DO ARQUIVO A SER CORTADO
                            arquivoSaida = configs.pastaDeTrabalho + timeStamp +  " " + (videoCounter++).ToString("D8") + ".mp4";
                              
                            Recorte recorte = new Recorte();
                            recorte.nomeDoArquivo = arquivoSaida;
                            recorte.inicio = corteStart;
                            recorte.fim = corteEnd;

                            recorte.log = "Voice Detected " + (videoCounter).ToString("D3") + ": " + voiceDuration.ToString("N3") +
                                " Start: " + Util.TimeFormat(corteStart) +
                                " End: " + Util.TimeFormat(corteEnd);

                            objRecortes.Add(recorte);
                            proc.ArquivoAdd(arquivoSaida);

                            recortesCriados++;
                            if (entrada.numeroCortesModoTeste > 0
                                && recortesCriados >= entrada.numeroCortesModoTeste)
                            {
                                break;
                            }


                        }
                        else
                        {
                            avancouCorte = true;
                            if (entrada.ModoTeste)
                            {
                                Console.WriteLine("");
                            }
                        }
                    }
                }

                // IDENTIFICA A LINHA DE FINAL DE SILENCIO
                if (line.IndexOf("silence_end") > 0)
                {
                    lastCommand = "end";
                    double tmpSilenceEnd = Convert.ToDouble(line.Substring(line.IndexOf("=") + 1, (line.Length - line.IndexOf("=") - 1)).Replace('.', ','));

                    if ((firstCut || realStartWithSilenceZero) && entrada.PreserveInitialSilence)
                    {
                        tmpSilenceEnd = 0;
                        firstCut = false;
                        realStartWithSilenceZero = false;
                    }

                    silenceDuration = tmpSilenceEnd - silenceStart;

                    //FORÇA O CORTE PARA SILENCIO MAIOR QUE 1 SEGUNDO
                    if (silenceDuration > 1 && avancouCorte)
                    {

                        if (entrada.ModoTeste)
                        {
                            Console.WriteLine("SS:" + Util.TimeFormat(silenceEnd) + " TO " + Util.TimeFormat(silenceStart) + ";"
                                + "Corte:; " + (videoCounter).ToString("D8") + ";"
                                + "Duração do Silêncio:;" + silenceDuration + ";"
                            );
                        }

                        corteStart = silenceEnd;
                        corteEnd = silenceStart;

                        //VALIDADOR DE RECORTE
                        if (corteStart < validadorDeCorte)
                        {
                            Console.WriteLine("ALERTA INCONSISTÊNCIA DE ARQUIVO: " + corteStart + " : " + corteEnd);
                        }
                        validadorDeCorte = corteEnd;

                        //arquivoSaida = AjustarNomeDoArquivo(saida.NomeArquivo + (videoCounter++).ToString("D8") + ".mp4");
                        arquivoSaida = configs.pastaDeTrabalho + timeStamp +  " " + (videoCounter++).ToString("D8") + ".mp4";


                        Recorte recorte = new Recorte();
                        recorte.nomeDoArquivo = arquivoSaida;
                        recorte.inicio = corteStart;
                        recorte.fim = corteEnd;
                        recorte.log = "Voice Detected " + (videoCounter).ToString("D3") + ": " + voiceDuration.ToString("N3") +
                            " Start: " + Util.TimeFormat(corteStart) +
                            " End: " + Util.TimeFormat(corteEnd);

                        objRecortes.Add(recorte);
                        proc.ArquivoAdd(arquivoSaida);

                        recortesCriados++;
                        if (entrada.numeroCortesModoTeste > 0
                            && recortesCriados >= entrada.numeroCortesModoTeste)
                        {
                            break;
                        }

                        voiceDetected = true;
                        silenceEnd = tmpSilenceEnd;

                    }

                    if (!voiceDetected)
                    {
                        silenceEnd = tmpSilenceEnd;
                        voiceDetected = true;
                    }
                }

            }


            // IDENTIFICANDO UMA PARTE FINAL DO VÍDEO NÃO CORTADO
            if (lastCommand == "end")
            {
                corteStart = silenceEnd;
                corteEnd = -1;

                //arquivoSaida = AjustarNomeDoArquivo(saida.NomeArquivo + (videoCounter++).ToString("D8") + ".mp4");
                arquivoSaida = configs.pastaDeTrabalho + timeStamp +  " " + (videoCounter++).ToString("D8") + ".mp4";

                Recorte recorte = new Recorte();
                recorte.nomeDoArquivo = arquivoSaida;
                recorte.inicio = corteStart;
                recorte.fim = corteEnd;

                recorte.log = "Voice Detected " + (videoCounter).ToString("D3") +
                    " Start: " + Util.TimeFormat(corteStart) +
                    " End: " + Util.TimeFormat(corteEnd);

                objRecortes.Add(recorte);

                proc.ArquivoAdd(arquivoSaida);
            }

            string duracaoDoClipStr = TimeSpan.FromSeconds(duracaoDoClip).ToString(@"hh\:mm\:ss\.fff");
            Console.WriteLine("Duração do Novo Arquivo: " + duracaoDoClipStr);

            int novoContador = 1;
            validadorDeCorte = 0;

            foreach (Recorte recorte in objRecortes)
            {
                Console.WriteLine("[" + (novoContador++) + "/" + (videoCounter - 1) + "] " +
                    " Tamanho: " + (recorte.fim - recorte.inicio) + " - " +
                    Util.TimeFormat(recorte.inicio) + " - " + Util.TimeFormat(recorte.fim));

                if (recorte.inicio < validadorDeCorte)
                {
                    Console.WriteLine("ALERTA INCONSISTÊNCIA DE ARQUIVO");
                    Console.WriteLine("RECORTE: " + " Start: " + Util.TimeFormat(recorte.inicio) +
                                " End: " + Util.TimeFormat(recorte.fim));
                }
                validadorDeCorte = recorte.inicio;

                //Suaviza o corte conforme parametro
                if (entrada.softCut > 0)
                {
                    recorte.inicio = recorte.inicio - entrada.softCut;
                    if (recorte.inicio < 0)
                    {
                        recorte.inicio = 0;
                    }
                    if (recorte.fim > 0)
                    {
                        recorte.fim = recorte.fim + entrada.softCut;
                    }
                }

                ffmpeg = " -i \"" + entrada.NomeArquivo + "\"";

                //NOVA ENTRADA PARA UM ARQUIVO EXTERNO DE AUDIO
                if (entrada.AudioTratado != null && entrada.AudioTratado != "")
                {
                    ffmpeg += " -i \"" + entrada.AudioTratado + "\"";
                }
                if (!String.IsNullOrEmpty(entrada.pressetsFfmpeg))
                {
                    ffmpeg += " -filter_complex \"[0:v]" + entrada.pressetsFfmpeg + "[vout]\" -map \"[vout]\"";
                }
                else
                {
                    ffmpeg += " -map 0:v";
                }

                //PARA UM ARQUIVO EXTERNO DE AUDIO
                if (entrada.AudioTratado != null && entrada.AudioTratado != "")
                {
                    ffmpeg += " -map 1:a ";
                }
                else
                {
                    ffmpeg += " -map 0:a ";
                }

                //ffmpeg += "-c:v libx264 ";
                ffmpeg +=   "-c:v h264_qsv ";

                ffmpeg += "-b:v " + entrada.videoBitRate + " ";
                ffmpeg += "-pix_fmt yuv420p -y ";
                //ffmpeg += "-b:a " + entrada.audioBitRate + " ";
                ffmpeg += "-b:a 320k "; // ASSUMINDO 320kbps com este encoding

                ffmpeg += " -ss " + Util.TimeFormat(recorte.inicio);
                if (recorte.fim > 0)
                {
                    ffmpeg += " -to " + Util.TimeFormat(recorte.fim);
                }
                ffmpeg += " \"" + recorte.nomeDoArquivo + "\"";

                if (!entrada.ModoTeste)
                {
                    obj = new Render();
                    obj.setFfmpegProgram(configs.ffmpegProgram);
                    obj.Renderiza(ffmpeg);
                }

                if (entrada.ModoDebug)
                {
                    Console.WriteLine(configs.ffmpegProgram + " " + ffmpeg);
                }

                //Finalizar o Processamento em um Numero menor de Cortes para testes
                if (entrada.numeroCortesModoTeste > 0 && novoContador > entrada.numeroCortesModoTeste)
                {
                    break;
                }

            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(configs.pastaDeTrabalho, configs.nomeArquivoMergeTemporario), true))
            {
                outputFile.WriteLine(proc.getArquivoMerge());
            }
            Console.WriteLine("Duração do Novo Arquivo: " + duracaoDoClipStr);
            return proc;
        }



        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }



    }
}