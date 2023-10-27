using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class S3Service
    {
        static TransferUtility fileTransferUtility;

        static S3Service()
        {
            fileTransferUtility = new TransferUtility(AWSClients.s3Client);
        }

        internal static async Task<string> UploadMovie(string key, IFormFile video)
        {
            // validate if video file
            if (!video.ContentType.Contains(Constants.VIDEO.ToLower()))
            {
                return $"{Constants.ERROR} while uploading movie: Movie file should be a video.";
            }

            try
            {
                //check if video bucket exists
                if (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(AWSClients.s3Client, Constants.MOVIE_BUCKET_NAME))
                {
                    await AWSClients.s3Client.PutBucketAsync(Constants.MOVIE_BUCKET_NAME);
                }

                //upload movie
                Stream fileStream = video.OpenReadStream();                
                await fileTransferUtility.UploadAsync(fileStream, Constants.MOVIE_BUCKET_NAME, Constants.VIDEO_FOLDER+key);
                
                return Constants.SUCCESS;
            }
            catch(Exception e)
            {
                return $"{Constants.ERROR} while uploading movie file: {e.Message}";
            }
        }

        internal static async Task<string> UploadThumbnail(string key, IFormFile thumbnail)
        {
            // validate if thumbnail file
            if (!thumbnail.ContentType.Contains(Constants.IMAGE.ToLower()))
            {
                return $"{Constants.ERROR} while uploading thumbnail file: Thumbnail file should be a image.";
            }

            try
            {
                //check if thumbnail bucket exists
                if (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(AWSClients.s3Client, Constants.MOVIE_BUCKET_NAME))
                {
                    await AWSClients.s3Client.PutBucketAsync(Constants.MOVIE_BUCKET_NAME);
                }

                //upload thumbnail
                Stream fileStream = thumbnail.OpenReadStream();
                await fileTransferUtility.UploadAsync(fileStream, Constants.MOVIE_BUCKET_NAME, Constants.THUMBNAIL_FOLDER+key);

                return Constants.SUCCESS;
            }
            catch (Exception e)
            {
                return $"{Constants.ERROR} while uploading thumbnail: {e.Message}";
            }
        }

        internal static async Task<MemoryStream> GetMovie(string key)
        {
            MemoryStream movieStream = new();
            try
            {
                GetObjectResponse res = await AWSClients.s3Client.GetObjectAsync(Constants.MOVIE_BUCKET_NAME, Constants.VIDEO_FOLDER+key);
                
                res.ResponseStream.CopyTo(movieStream);
            }
            catch(Exception e)
            {
                // only logging on the console, not to users
                Console.WriteLine($"{Constants.ERROR} while getting movie from S3: {e.Message}");
            }
            return movieStream;
        }

        internal static async Task<string> DeleteMovie(string key)
        {
            try
            {
                await AWSClients.s3Client.DeleteObjectAsync(Constants.MOVIE_BUCKET_NAME, Constants.VIDEO_FOLDER + key);

                return Constants.SUCCESS;
            }
            catch (Exception e)
            {
                return $"{Constants.ERROR} while deleting movie: {e.Message}";
            }
        }

        internal static async Task<string> DeleteThumbnail(string key)
        {
            try
            {
                await AWSClients.s3Client.DeleteObjectAsync(Constants.MOVIE_BUCKET_NAME, Constants.THUMBNAIL_FOLDER + key);

                return Constants.SUCCESS;
            }
            catch (Exception e)
            {
                return $"{Constants.ERROR} while deleting thumbnail: {e.Message}";
            }
        }
    }
}
