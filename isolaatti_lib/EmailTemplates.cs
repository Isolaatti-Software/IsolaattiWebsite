namespace isolaatti_API.isolaatti_lib
{
    public class EmailTemplates
    {
        public const string SongReady= @"<html><body>
        <h1>Song {0} has been processed!</h1>
        <p>Your 4 tracks are now available on the app. Also, here you have direct links to the files:</p>
        <ul>
            <li>Bass: {1}</li>
            <li>Drums: {2}</li>
            <li>Vocals: {3}</li>
            <li>Other: {4}</li>
        </ul>
        <p>Be careful, these files will be delated 12 hours after they were created, so download them soon!</p>
        </body></html>";

        public const string SongStartedProcessing = @"<html><body>
        <h1>Your song is being processing now</h1>
        <p>Your song {0} is now processing. Please wait, you will receive a message when it's finished</p>
        </body></html>";
    }
}