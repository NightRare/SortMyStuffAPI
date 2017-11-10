using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace SortMyStuffAPI.Services
{
    public class DefaultFileService : IThumbnailFileService, IPhotoFileService
    {
        // TODO: to be removed
        private const string TestUser = "test_user";

        private FirebaseStorage _storage;
        private ApiConfigs _apiConfigs;
        private ILocalResourceService _localRes;

        public DefaultFileService(IOptions<ApiConfigs> apiConfigs, ILocalResourceService localResourceService)
        {
            _apiConfigs = apiConfigs.Value;
            _localRes = localResourceService;
        }


        #region IThumbnailFileService METHODS

        public Task<Stream> DownloadThumbnail(string id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadDefaultThumbnail(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IPhotoFileService METHODS

        public Task<Stream> DownloadPhoto(string id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadDefaultPhoto(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UploadPhoto(string id, Stream photo, CancellationToken ct)
        {
            await InitialiseFirebaseStorage();

            try
            {
                photo.Position = 0;
                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(TestUser)
                    .Child(_apiConfigs.StoragePhotos)
                    .Child(id + _apiConfigs.ImageFormat)
                    .PutAsync(photo, ct);

                await _storage
                    .Child(_apiConfigs.StorageUserData)
                    .Child(TestUser)
                    .Child(_apiConfigs.StorageThumbnails)
                    .Child(id + _apiConfigs.ImageFormat)
                    .PutAsync(ToThumbnail(photo), ct);

                //TODO: In order to keep the sync between photo and thumbnail, should retry upload thumbnail if failed, or delete the photo
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public Task<bool> DeletePhoto(string id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region PRIVATE METHODS

        private async Task InitialiseFirebaseStorage()
        {
            if (_storage != null) return;

            var bucket = Environment.GetEnvironmentVariable(ApiStrings.ENV_FIREBASE_STORAGE_URL);
            var apiKey = Environment.GetEnvironmentVariable(ApiStrings.ENV_FIREBASE_API_KEY);
            var email = Environment.GetEnvironmentVariable(ApiStrings.ENV_FIREBASE_AUTH_EMAIL);
            var password = Environment.GetEnvironmentVariable(ApiStrings.ENV_FIREBASE_AUTH_PASSWORD);

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
            photo.Position = 0;

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
