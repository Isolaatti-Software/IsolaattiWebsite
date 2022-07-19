using System;

namespace Isolaatti.Utils
{
    public class GoogleCloudStorageUrlUtils
    {
        public static string GetFileRefFromUrl(string url)
        {
            // input example https://firebasestorage.googleapis.com/v0/b/isolaatti-b6641.appspot.com/o/audio_posts%2F4%2FFri%20Jun%2011%202021-19-1-42_audio.webm?alt=media&token=61504140-6e3c-4788-8ddd-831c144f06e1

            var result =
                url.Replace("https://firebasestorage.googleapis.com/v0/b/isolaatti-b6641.appspot.com/o/", String.Empty);

            result = result.Replace("%2F", "/");
            result = result.Replace("%20", " ");
            result = result.Split("?")[0];

            return result;
        }
    }
}