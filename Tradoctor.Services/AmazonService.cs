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

namespace Tradoctor.Services
{
    public class AmazonService : IAmazonService
    {
        private readonly IConfiguration _configuration;

        public AmazonService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TextractResult> GetTextFromDocument()
        {
            var awsAccessKey = _configuration["Credentials:AWSAccessKey"];
            var awsSecretKey = _configuration["Credentials:AWSSecretKey"];
            var region = RegionEndpoint.USEast2;

            await CreateObjectS3Async(awsAccessKey, awsSecretKey, region);

            DetectDocumentTextResponse result = await ProcessingTextract(awsAccessKey, awsSecretKey, region);

            TextractResult textractResult = new TextractResult();

            textractResult.PrescriptionText = string.Join(" ", result.Blocks.Select(b => b.Text));

            IEnumerable<float> confidenceValues = result.Blocks.Select(b => b.Confidence);
            textractResult.Confidence = confidenceValues.Average();

            return textractResult;
        }

        private async Task<DetectDocumentTextResponse> ProcessingTextract(string awsAccessKey, string awsSecretKey, RegionEndpoint region)
        {
            var client = new AmazonTextractClient(awsAccessKey, awsSecretKey, region);

            var request = new DetectDocumentTextRequest
            {
                Document = new Document
                {
                    S3Object = new S3Object
                    {
                        Bucket = "textract-console-us-east-2-7a438fab-112f-422b-98ba-cbc0d7f642e8",
                        Name = "Textract/2.jpg"
                    }
                }
            };

            var response = await client.DetectDocumentTextAsync(request);

            return response;
        }

        private async Task<bool> CreateObjectS3Async(string awsAccessKey, string awsSecretKey, RegionEndpoint region)
        {
            var bucketName = "textract-console-us-east-2-7a438fab-112f-422b-98ba-cbc0d7f642e8";
            var filePath = "C:\\Users\\Nuri\\Git\\Tradoctor.API\\Tradoctor.Data\\Prescriptions\\1.jpg";
            var objectKey = "Textract/2.jpg";

            var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

            var transferUtility = new TransferUtility(client);

            try
            {
                await transferUtility.UploadAsync(filePath, bucketName, objectKey);

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
    }
}
