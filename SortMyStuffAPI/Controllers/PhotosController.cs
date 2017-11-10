using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using IFileService = Microsoft.EntityFrameworkCore.Scaffolding.Internal.IFileService;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class PhotosController : Controller
    {
        private readonly IPhotoFileService _photoService;
        private readonly ApiConfigs _apiConfigs;

        public PhotosController(IPhotoFileService photoService, IOptions<ApiConfigs> apiConfigs)
        {
            _photoService = photoService;
            _apiConfigs = apiConfigs.Value;
        }

        // GET /photos/{assetId}
        [HttpGet("{assetId}", Name = nameof(GetPhotoByIdAsync))]
        public async Task<IActionResult> GetPhotoByIdAsync(string assetId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{assetId}", Name = nameof(UploadPhotoAsync))]
        public async Task<IActionResult> UploadPhotoAsync(
            string assetId,
            IFormFile photo,
            CancellationToken ct)
        {
            if (photo == null) return BadRequest(new ApiError("No file uploaded."));

            if (photo.Length > _apiConfigs.ImageMaxBytes)
                return BadRequest(
                    new ApiError($"File over size.(limit: {_apiConfigs.ImageMaxBytes / 1024 / 1024} mb)"));

            var fileName = Path.GetTempFileName();
            using (var photoFile = new FileStream(fileName, FileMode.Create))
            {
                await photo.CopyToAsync(photoFile, ct);
                bool uploaded = await _photoService.UploadPhoto(assetId, photoFile, ct);

                if (uploaded) return Ok();
            }

            return BadRequest(new ApiError("Uploading image file failed."));
        }
    }
}
