using System;

namespace VideoCortadorDeSilencio
{
    public static class Util
    {
        
        public static string TimeFormat(double tempo){
            return TimeSpan.FromSeconds(tempo).ToString(@"hh\:mm\:ss\.fff");
        }

        public static string TimeFormat(TimeSpan tempo){
            return tempo.ToString(@"hh\:mm\:ss\.fff");
        }


    }
}