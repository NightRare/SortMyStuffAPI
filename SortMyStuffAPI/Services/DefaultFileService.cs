using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using SortMyStuffAPI.Utils;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SortMyStuffAPI.Models;
using SixLabors.ImageSharp.Formats;

namespace SortMyStuffAPI.Services
{
    public class DefaultFileService : IThumbnailFileService, IPhotoFileService
    {
        // Make sure this is consistent with ApiConfigs.ImageFormat
        private readonly static IImageFormat imageFormat = ImageFormats.Jpeg;

        private FirebaseStorage _storage;
        private ApiConfigs _apiConfigs;
        private ILocalResourceService _localRes;

        public DefaultFileService(
            IOptions<ApiConfigs> apiConfigs, 
            ILocalResourceService localResourceService)
        {
            _apiConfigs = apiConfigs.Value;
            _localRes = localResourceService;
        }


        #region IThumbnailFileService METHODS

        public async Task<Stream> DownloadThumbnail(
            string userId,
            string id, 
            CancellationToken ct)
        {
            await InitialiseFirebaseStorage();

            try
            {
                var downloadUrl = await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StorageThumbnails)
                    .Child(id + _apiConfigs.ImageFormat)
                    .GetDownloadUrlAsync();

                var response = await new HttpClient().GetAsync(downloadUrl, ct);
                return await response.Content.ReadAsStreamAsync();
            }
            catch (FirebaseStorageException)
            {
                return await _localRes.GetResourceAsync(LocalResources.DefaultThumbnail, ct);
            }
        }

        #endregion


        #region IPhotoFileService METHODS

        public async Task<Stream> DownloadPhoto(
            string userId,
            string id, 
            CancellationToken ct)
        {
            await InitialiseFirebaseStorage();

            try
            {
                var downloadUrl = await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StoragePhotos)
                    .Child(id + _apiConfigs.ImageFormat)
                    .GetDownloadUrlAsync();

                var response = await new HttpClient().GetAsync(downloadUrl, ct);
                return await response.Content.ReadAsStreamAsync();
            }
            catch(FirebaseStorageException) 
            {
                return await _localRes.GetResourceAsync(LocalResources.DefaultPhoto, ct);
            }
        }

        public async Task<(bool Succeeded, string Error)> UploadPhoto(
            string userId,
            string id, 
            Stream photo, 
            CancellationToken ct)
        {
            await InitialiseFirebaseStorage();

            var format = Image.DetectFormat(photo);
            if (!format.Equals(imageFormat))
            {
                return (false, $"Incorrect photo format. Only allow " +
                    $"[{imageFormat.FileExtensions.ToEnumString()}].");
            }

            bool photoUploaded = false;
            try
            {
                photo.Position = 0;
                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StoragePhotos)
                    .Child(id + _apiConfigs.ImageFormat)
                    .PutAsync(photo, ct);
                photoUploaded = true;

                photo.Position = 0;
                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StorageThumbnails)
                    .Child(id + _apiConfigs.ImageFormat)
                    .PutAsync(ToThumbnail(photo), ct);
            }
            catch (FirebaseStorageException ex)
            {
                // if uploading thumbnail failed, then delete the photo
                // to remain consistency
                if (photoUploaded)
                {
                    await _storage
                        .Child(_apiConfigs.StorageUserData)
                        .Child(userId)
                        .Child(_apiConfigs.StoragePhotos)
                        .Child(id + _apiConfigs.ImageFormat)
                        .DeleteAsync();
                }

                return (false, ex.Message);
            }
            return (true, null);
        }

        public async Task<(bool Succeeded, string Error)> DeletePhoto(
            string userId,
            string id, 
            CancellationToken ct)
        {
            await InitialiseFirebaseStorage();

            try
            {
                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StoragePhotos)
                    .Child(id + _apiConfigs.ImageFormat)
                    .DeleteAsync();

                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(userId)
                    .Child(_apiConfigs.StorageThumbnails)
                    .Child(id + _apiConfigs.ImageFormat)
                    .DeleteAsync();
            }
            catch (FirebaseStorageException ex)
            {
                return (false, ex.Message);
            }
            return (true, null);
        }

        #endregion


        #region PRIVATE METHODS

        private async Task InitialiseFirebaseStorage()
        {
            if (_storage != null) return;

            var bucket = Environment.GetEnvironmentVariable(ApiStrings.EnvFirebaseStorageUrl);
            var apiKey = Environment.GetEnvironmentVariable(ApiStrings.EnvFirebaseApiKey);
            var email = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperEmail);
            var password = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperPassword);

            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(email, password);

            _storage = new FirebaseStorage(
                bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken)
                });
        }

        private Stream ToThumbnail(Stream photo)
        {
            var thumbnail = new MemoryStream();

            var image = Image.Load<Rgba32>(photo, new JpegDecoder());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(_apiConfigs.ThumbnailWidth, _apiConfigs.ThumbnailHeight),
                Mode = ResizeMode.Max
            }));

            image.Save(thumbnail, new JpegEncoder
            {
                Quality = _apiConfigs.ImageQuality
            });

            thumbnail.Position = 0;
            return thumbnail;
        }

        #endregion

    }
}
