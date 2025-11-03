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
        MilestoneReturn,
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
            /// A temporally pre-signed URL <see cref="string"/> for accessing the AWS S3 object file
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
        /// Construct a S3 bucket folder path based on the <paramref name="pathEnum"/>, <paramref name="sepertationId"/> and <paramref name="prefix"/>.
        /// </summary>aaaaa
        private static string ConstructFolderPath(AwsS3HelperPaths pathEnum, int sepertationId, string prefix = "uploads/")
        {
            var hasPath = _enumPath.TryGetValue(pathEnum, out var enumPathString);
            if (!hasPath)
            {
                throw new ArgumentException($"No path definition for {nameof(pathEnum)} of value {pathEnum.ToString()}");
            }

            var sepertationString = sepertationId != 0 ? $"{sepertationId}/" : "";
            return $"{prefix}{enumPathString}{sepertationString}";
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
            {
               AwsS3HelperPaths.MilestoneReturn,
               "milestone-returns/"
            }
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
        public static async Task<UploadResponse> UploadFileToS3Async(this IAmazonS3 s3Client, IFormFile formFile, AwsS3HelperPaths pathEnum, int seperationId, DateTime currentTime)
        {
            if (formFile.Length > FILE_SIZE_LIMIT)
            {
                throw new ArgumentException($"{nameof(formFile)} can't be larger than 20.0 MB.");
            }

            // Construct unique file name
            string fileName = Path.GetFileNameWithoutExtension(formFile.FileName);
            string extension = Path.GetExtension(formFile.FileName);
            string timestamp = currentTime.ToString("yyyyMMddHHmmss"); // Create a file-safe timestamp
            var newFileName = $"{fileName}_{timestamp}{extension}"; // New unique file name, format : fileName_yyyyMMddmmss.ext

            string objectKey = $"{ConstructFolderPath(pathEnum, seperationId)}{newFileName}"; // new bucket's object key

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

            var preSignResponse = await s3Client.GetPresignedUrlFromS3Async(objectKey, currentTime);

            return new UploadResponse()
            {
                FileName = newFileName,
                ObjectKey = objectKey,
                PresignedUrl = preSignResponse.url,
                UrlExpireTime = preSignResponse.expireTime,
            };
        }

        /// <summary>
        /// Get a temporally pre-signed URL <see cref="string"/> for other clients <br/>
        /// to access the AWS S3 object file mapped with the <paramref name="objectKey"/>
        /// </summary>
        /// <param name="objectKey">The key <see cref="string"/> for the AWS S3 object file to get a pre-signed URL</param>
        /// <param name="startTime">The time 5 hours from which the URL path expires</param>
        /// <returns>An pre-signed URL <see cref="string"/> and it's <see cref="DateTime"/> expire time</returns>
        public static async Task<(string url, DateTime expireTime)> GetPresignedUrlFromS3Async(this IAmazonS3 s3Client, string objectKey, DateTime startTime)
        {
            // Generate pre-signed URL for file download
            var expireTime = startTime.AddHours(EXPIRATION_COUNT_DOWN);
            var preSignedRequest = new GetPreSignedUrlRequest()
            {
                BucketName = BUCKET_NAME,
                Key = objectKey,
                Expires = expireTime,
            };

            var preSignedUrl = await s3Client.GetPreSignedURLAsync(preSignedRequest);
            return (preSignedUrl, expireTime);
        }

        /// <summary>
        /// Delete the AWS S3 object file mappead with the <paramref name="objectKey"/>
        /// </summary>
        /// <param name="objectKey">The key <see cref="string"/> for the AWS S3 object file to delete</param>
        public static async Task DeleteFileFromS3Async(this IAmazonS3 s3Client, string objectKey)
        {
            await s3Client.DeleteObjectAsync(BUCKET_NAME, objectKey);
        }
    }
}
