using System.Diagnostics;
using ReprodutorMultimia.Models;

namespace ReprodutorMultimia.Utils;

public class VideoTrack
{
    public VideoInfo GetVideoInfo(string filePath, string picturePath)
    {

        var ffmpegPath = Path.Combine(Environment.CurrentDirectory, "ffmpeg.exe");
        var fileName = Path.GetFileName(filePath);
        var startTimeAt = 5;
        var process = new Process();
        var media = new VideoInfo {};

        process.StartInfo.FileName = ffmpegPath;
        process.StartInfo.Arguments = $"-ss {startTimeAt} -i \"{filePath}\" -f image2 -vframes 1 -y \"{picturePath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        var handleData = new DataReceivedEventHandler((sender, e) =>
        {
            if (e.Data == null)
                return;

            var strMatch = "Duration: ";
            var durationStartIndex = e.Data.IndexOf(strMatch);
            if (durationStartIndex > -1)
            {
                var durationStr = e.Data.Substring(durationStartIndex + strMatch.Length, 11);
                var durationSplit = durationStr.Split(':');
                var hour = int.Parse(durationSplit[0]);
                var minute = int.Parse(durationSplit[1]);
                var second =  (int)float.Parse(durationSplit[2].Replace(".", ","));
                var duration = int.Parse(new TimeSpan(hour, minute, second).TotalSeconds.ToString());

                if (minute <= 0) {
                    duration = second;
                }

                media.Duration = duration;
            }
        });

        process.OutputDataReceived += handleData;
        process.ErrorDataReceived += handleData;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return media;
    }
}

public class VideoInfo
{
    public int Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string ThumbName { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
    public string Type { get; set; }
}