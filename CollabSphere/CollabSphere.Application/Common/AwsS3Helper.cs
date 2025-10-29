using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public enum AwsS3HelperPaths
    {
        Checkpoint,
        Milestone,
        Class,
    }

    public static class AwsS3Helper
    {
        public struct UploadResponse
        {
            /// <summary>
            /// New contructed file name <see cref="string"/> from the upload
            /// </summary>
            public required string FileName { get; set; }

            /// <summary>
            /// Key <see cref="string"/> for AWS S3 Object (File)
            /// </summary>
            public required string ObjectKey { get; set; }

            /// <summary>
            /// A expirable pre-signed URL <see cref="string"/> for downloading the file
            /// </summary>
            public required string PresignedUrl { get; set; }

            /// <summary>
            /// The expire <see cref="DateTime"/> for the pre-signed URL
            /// </summary>
            public required DateTime UrlExpireTime { get; set; }
        }

        private const string BUCKET_NAME = "collab-sphere-bucket";
        private const long FILE_SIZE_LIMIT = 20 * 1024 * 1024; // Hard coded file size limit (20 MB)
        private const int EXPIRATION_COUNT_DOWN = 5; // (Hours)

        /// <summary>
        /// Construct a S3 bucket folder path based on the <paramref name="pathEnum"/> and <paramref name="prefix"/>.
        /// </summary>
        private static string ConstructFolderPath(AwsS3HelperPaths pathEnum, string prefix = "uploads/")
            => ConstructFolderPath(pathEnum, 0, prefix);

        /// <summary>
        /// Construct a S3 bucket folder path based on the <paramref name="pathEnum"/>, <paramref name="sepertationId"/> and <paramref name="prefix"/>.
        /// </summary>aaaaa
        private static string ConstructFolderPath(AwsS3HelperPaths pathEnum, int sepertationId, string prefix = "uploads/")
        {
            var sepertationString = sepertationId != 0 ? $"{sepertationId}" : "";
            return $"{prefix}{_enumPath[pathEnum]}{sepertationString}";
        }

        /// <summary>
        /// Path mappings for each <seealso cref="AwsS3HelperPaths"/> enum.
        /// </summary>
        private readonly static Dictionary<AwsS3HelperPaths, string> _enumPath = new()
        {
            {
                AwsS3HelperPaths.Checkpoint,
                "checkpoints/"
            },
            {
                AwsS3HelperPaths.Milestone,
                "milestones/"
            },
            {
                AwsS3HelperPaths.Class,
                "classes/"
            },
        };

        /// <summary>
        /// Extended function for <see cref="IAmazonS3"/> to upload form files.
        /// <para>
        /// <c>NOTE: </c> 
        /// Restrict <paramref name="formFile"/>'s size to 20 MB.
        /// </para>
        /// </summary>
        /// <param name="formFile">The form file to upload</param>
        /// <param name="pathEnum">The <see cref="AwsS3HelperPaths"/> enum for contructing the destination bucket path</param>
        /// <param name="seperationId">Id for seperation inside the selected folder</param>
        /// <param name="currentTime">DateTime value used to contruct new unique file name.<br/>
        /// Also used as starting point for expiration count-down.<br/>
        /// If <see cref="null"/> then defaults to <see cref="DateTime.UtcNow"/>
        /// </param>
        /// <returns></returns>
        public static async Task<UploadResponse> UploadFileToS3Async(this IAmazonS3 s3Client, IFormFile formFile, AwsS3HelperPaths pathEnum, int seperationId = 0, DateTime? currentTime = null)
        {
            if (formFile.Length > FILE_SIZE_LIMIT)
            {
                throw new ArgumentException($"{nameof(formFile)} can't be larger than 20.0 MB.");
            }

            if (!currentTime.HasValue)
            {
                currentTime = DateTime.UtcNow;
            }

            // Construct unique file name
            string fileName = Path.GetFileNameWithoutExtension(formFile.FileName);
            string extension = Path.GetExtension(formFile.FileName);
            string timestamp = currentTime.Value.ToString("yyyyMMddHHmmss"); // Create a file-safe timestamp
            var newFileName = $"{fileName}_{timestamp}{extension}"; // New unique file name, format : fileName_yyyyMMddmmss.ext

            string objectKey = $"{ConstructFolderPath(pathEnum)}/{newFileName}"; // new bucket's object key

            // Upload file to AWS
            await using var stream = formFile.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = objectKey,
                InputStream = stream,
                ContentType = formFile.ContentType,
            };

            var putObjectResponse = await s3Client.PutObjectAsync(putRequest);

            // Generate pre-signed URL for file download
            var expireTime = currentTime.Value.AddHours(EXPIRATION_COUNT_DOWN);
            var preSignedRequest = new GetPreSignedUrlRequest()
            {
                BucketName = BUCKET_NAME,
                Key = objectKey,
                Expires = expireTime,
            };

            var preSignedUrl = await s3Client.GetPreSignedURLAsync(preSignedRequest);

            return new UploadResponse()
            {
                FileName = newFileName,
                ObjectKey = objectKey,
                PresignedUrl = preSignedUrl,
                UrlExpireTime = expireTime,
            };
        }

        private static async Task<string> GetPresignedUrlFromS3Async(this IAmazonS3 s3Client, string objectKey)
        {
            // Generate pre-signed URL for file download
            var expireTime = DateTime.Now.AddHours(EXPIRATION_COUNT_DOWN);
            var preSignedRequest = new GetPreSignedUrlRequest()
            {
                BucketName = BUCKET_NAME,
                Key = objectKey,
                Expires = expireTime,
            };

            var preSignedUrl = await s3Client.GetPreSignedURLAsync(preSignedRequest);
            return preSignedUrl;
        }

        public static async Task DeleteFileFromS3Async(this IAmazonS3 s3Client, string objectKey)
        {
            await s3Client.DeleteObjectAsync(BUCKET_NAME, objectKey);
        }
    }
}
