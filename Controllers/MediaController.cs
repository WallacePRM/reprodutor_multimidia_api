using System.Diagnostics;
using ATL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReprodutorMultimia.Database;
using ReprodutorMultimia.Models;
using ReprodutorMultimia.Utils;
using ReprodutorMultimidia.Models;

namespace ReprodutorMultimia.Controllers;

[ApiController]
[Route("[controller]")]
public class MediasController : ControllerBase
{
    private AppDbContext _dbContext;
    public MediasController(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetMedias()
    {
        try
        {
            // this._dbContext.Medias.RemoveRange(this._dbContext.Medias.ToList());
            // await this._dbContext.SaveChangesAsync();

            var dbMedias = await this._dbContext.Medias.ToListAsync();
            var medias = dbMedias.Select(x => MapMedia(x)).ToList();

            return Ok(medias);
        }
        catch(Exception error)
        {
            return BadRequest(new ResponseError(error.Message));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMedia(int id)
    {

        var media = await this._dbContext.Medias.Where(x => x.Id == id).FirstOrDefaultAsync();
        if (media == null) return NotFound();

        var filePath = GetFilePath(media.FileName);
        var fileStream = System.IO.File.OpenRead(filePath);

        return this.File(fileStream, GetContentTypeFromFileName(media.FileName), enableRangeProcessing: true);
    }

    [HttpGet("{id:int}/thumbnail")]
    public async Task<IActionResult> GetMediaThumbnail(int id)
    {
        var media = await this._dbContext.Medias.Where(x => x.Id == id).FirstOrDefaultAsync();
        if (media == null) return NotFound();

        var filePath = Path.Combine(GetPicturesFolder(), GetThumbnailNameFromFilename(media.FileName));
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var fileStream = System.IO.File.OpenRead(filePath);
        var mimeType = GetThumbailTypeFromFilePath(filePath);
        return new FileStreamResult(fileStream, mimeType);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(1_048_566_000)]
    [RequestFormLimits(ValueLengthLimit = 1_048_566_000 , MultipartBodyLengthLimit = 1_048_566_000)] // 1000 mb
    public async Task<IActionResult> UploadMedia(List<IFormFile> files)
    {
        try
        {
            var medias = new List<Database.Entities.Media>();
            foreach (var file in files)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = GetFilePath(fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var picturePath = await GenerateMediaThumbnail(filePath);
                var media  = new Database.Entities.Media
                {
                    Name = Path.GetFileNameWithoutExtension(file.FileName),
                    Type = GetMediaTypeFromFileName(file.FileName),
                    FileName = fileName,
                    ThumbName = picturePath != null ? GetThumbnailNameFromFilename(fileName) : null,
                };

                if (GetMediaTypeFromFileName(file.FileName) == Media.TypeVideo)
                {
                    var video = new VideoTrack().GetVideoInfo(filePath, picturePath);
                    media.Duration = video.Duration;
                }

                if (GetMediaTypeFromFileName(file.FileName) == Media.TypeMusic)
                {
                    var track = new Track(filePath);
                    media.Author = String.IsNullOrEmpty(track.Artist) ? null : track.Artist;
                    media.Album = String.IsNullOrEmpty(track.Album) ? null : track.Album;
                    media.Title = String.IsNullOrEmpty(track.Title) ? null : track.Title;
                    media.Genre = String.IsNullOrEmpty(track.Genre) ? null : track.Genre;
                    media.ReleaseDate = track.PublishingDate;
                    media.Duration = track.Duration;
                }

                this._dbContext.Medias.Add(media);
                medias.Add(media);
            }

            await this._dbContext.SaveChangesAsync();
            var result = medias.Select(media => MapMedia(media)).ToList();
            return Ok(result);
        }
        catch(Exception error)
        {
            return BadRequest(new ResponseError(error.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteMedia(int id)
    {
        try {

            var media = await this._dbContext.Medias.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (media != null)
            {
                this._dbContext.Medias.Remove(media);
                await this._dbContext.SaveChangesAsync();

                var filePath = GetThumbnailNameFromFilename(media.FileName);

                System.IO.File.Delete(media.FileName);
                System.IO.File.Delete(filePath);
            }

            return Ok();
        }
        catch(Exception error) {

            return BadRequest(new ResponseError(error.Message));
        }
    }


    /* --------- UTILS --------- */

    private string GetContentTypeFromFileName(string fileName)
    {
        if (fileName.EndsWith("mp3")) return "audio/mpeg";
        if (fileName.EndsWith("mp4")) return "video/mp4";

        return "application/octet-stream";
    }

    private string GetMediaTypeFromFileName(string fileName)
    {
        if (fileName.EndsWith("mp3")) return Media.TypeMusic;
        if (fileName.EndsWith("mp4")) return Media.TypeVideo;

        throw new Exception("Arquivo não suportado: " + fileName);
    }

    private string GetThumbailTypeFromFilePath(string fileName)
    {
        if (fileName.EndsWith("jpg")) return "application/octet-stream";

        throw new Exception("Arquivo não suportado: " + fileName);
    }

    private string GetThumbnailNameFromFilename(string filename)
        {
        var imageName = Path.GetFileNameWithoutExtension(filename);
        imageName = imageName + ".artwork.jpg";

        return imageName;
    }

    private string GetFilePath(string fileName)
    {

        var filePath = Path.Combine(Environment.CurrentDirectory, "Out", "Medias", fileName);
        return filePath;
    }

    private string GetMediasFolder()
    {
        return Path.Combine(Environment.CurrentDirectory, "out", "Medias");
    }

    private string GetPicturesFolder()
    {
        return Path.Combine(Environment.CurrentDirectory, "Out", "Thumbnails");
    }

    private async Task<string?> GenerateMediaThumbnail(string filePath)
    {
        var fileType = GetMediaTypeFromFileName(filePath);
        var picturePath = Path.Combine(GetPicturesFolder(), GetThumbnailNameFromFilename(filePath));

        if (fileType == Media.TypeMusic)
        {
            var track = new Track(filePath);
            var picture = track.EmbeddedPictures.FirstOrDefault();
            if (picture != null)
            {
                using(var fileStream = System.IO.File.Create(picturePath))
                using (var pictureStream = new System.IO.MemoryStream(picture.PictureData))
                {
                    await pictureStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                return null;
            }
        }
        else if (fileType == Media.TypeVideo)
        {
            var cmd = "ffmpeg  -itsoffset -1  -i " + '"' + filePath + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -s 320x240 " + '"' + picturePath + '"';
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            if (!process.WaitForExit(5000)) return null;
        }
        else {
            return null;
        }

        return picturePath;
    }

    private Media MapMedia(Database.Entities.Media media)
    {
        return new Media
        {
            Id = media.Id,
            Name = media.Name,
            Type = GetMediaTypeFromFileName(media.FileName),
            Src =  $"{Request.Scheme}://{Request.Host}/medias/{media.Id}",
            Author = media.Author,
            Album = media.Album,
            Title = media.Title,
            Genre = media.Genre,
            Duration = media.Duration,
            Thumbnail = media.ThumbName != null ? $"{Request.Scheme}://{Request.Host}/medias/{media.Id}/thumbnail" : null,
        };
    }
}