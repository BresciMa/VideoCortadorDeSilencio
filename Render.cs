using System;

namespace VideoCortadorDeSilencio
{
    class Render
    {
        public string ffmpegProgram;

        public void setFfmpegProgram(String caminho){
            this.ffmpegProgram = caminho;
        }


        public bool Renderiza(String parameters)
        {

            System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.FileName = ffmpegProgram;
            ffmpeg.StartInfo.Arguments = parameters;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.Start();

            // Use asynchronous read operations on at least one of the streams.
            // Reading both streams synchronously would generate another deadlock.
            ffmpeg.BeginOutputReadLine();
            string tmpErrorOut = ffmpeg.StandardError.ReadToEnd();

            //Gravar isso aqui em um arquivo de log, porque é o resultado do processamento da Aplicação... 
            //conforme tela de Prompt de Comando

            ffmpeg.WaitForExit();
            ffmpeg.Close();

            return true;


            /*
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "Write500Lines.exe";
            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            */

        }


    }


}
