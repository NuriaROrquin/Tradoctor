using Amazon.Textract.Model;
using Amazon.Textract;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Tradoctor.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Buffers.Text;
using Amazon.S3.Model;

namespace Tradoctor.Services
{
    public class AmazonService : IAmazonService
    {
        private readonly IConfiguration _configuration;

        public AmazonService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TextractResult> GetTextFromDocument(ImageTextract imageTextrack)
        {
            var awsAccessKey = _configuration["Credentials:AWSAccessKey"];
            var awsSecretKey = _configuration["Credentials:AWSSecretKey"];
            var region = RegionEndpoint.USEast2;
            var uniqueId = getUniqueId();

            //Vamos a subir el objeto a AWS
            await CreateObjectS3Async(awsAccessKey, awsSecretKey, region, imageTextrack, uniqueId);

            //Analizamos el documento con textract
            DetectDocumentTextResponse result = await ProcessingTextract(awsAccessKey, awsSecretKey, region, uniqueId);

            //Preparamos los datos para devolverlos
            TextractResult textractResult = new TextractResult();

            textractResult.PrescriptionText = string.Join(" ", result.Blocks.Where(b => b.BlockType == BlockType.LINE).Select(b => b.Text));

            IEnumerable<float> confidenceValues = result.Blocks.Select(b => b.Confidence);
            textractResult.Confidence = confidenceValues.Average();

            return textractResult;
        }

        private async Task<DetectDocumentTextResponse> ProcessingTextract(string awsAccessKey, string awsSecretKey, RegionEndpoint region, string uniqueId)
        {
            var client = new AmazonTextractClient(awsAccessKey, awsSecretKey, region);

            var request = new DetectDocumentTextRequest
            {
                Document = new Document
                {
                    S3Object = new Amazon.Textract.Model.S3Object
                    {
                        Bucket = "textract-console-us-east-2-7a438fab-112f-422b-98ba-cbc0d7f642e8",
                        Name = $"Textract/{uniqueId}.jpg"
                    }
                }
            };

            var response = await client.DetectDocumentTextAsync(request);

            return response;
        }

        private async Task<bool> CreateObjectS3Async(string awsAccessKey, string awsSecretKey, RegionEndpoint region, ImageTextract imageTextrack, string uniqueId)
        {
            var bucketName = "textract-console-us-east-2-7a438fab-112f-422b-98ba-cbc0d7f642e8";
            var objectKey = $"Textract/{uniqueId}.jpg";

            var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

            var transferUtility = new TransferUtility(client);

            //Archivo en b64 lo pasamos a Stream
            var file = imageTextrack.ImageBase64;
            byte[] imageData = Convert.FromBase64String(file);
            Stream imageStream = new MemoryStream(imageData);

            //Se genera request para insertar en el object del bucket
            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = imageStream
            };

            try
            {
                await client.PutObjectAsync(request);

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine("Error al subir el objeto a Amazon S3: " + ex.Message);

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);

                return false;
            }

        }

        public String getUniqueId()
        {
            long timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            string uniqueId = $"{timestamp}_{Guid.NewGuid()}";
            return uniqueId;
        }

    }
}
