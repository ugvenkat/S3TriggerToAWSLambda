using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using System.IO;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3TriggerToAWSLambda
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }
        
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            if (s3Event == null) { return null; }

            try
            {
                context.Logger.LogLine($"Handler Process Begins: Getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}.");

                String sLine = "";
                int Recordcounter = 1;
                string[] DeatailLine;

                var response = await this.S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
                StreamReader streamReader = new StreamReader(response.ResponseStream);
                while ((sLine = streamReader.ReadLine()) != null)
                {
                    context.Logger.LogLine($"File Content of: {s3Event.Object.Key} = ");

                    context.Logger.LogLine(sLine);

                    if ((Recordcounter == 1) && !sLine.Contains("RateType|RateValue|TimeStamp|"))
                    {
                        //File missin header record.
                        //Move the file to Bucket Invalid-Files
                        break;
                    }

                    /* Sample format of the valid file 
                     * 
                     * RateType|RateValue|TimeStamp|
                     * Fixed|3.5|2021-04-15 06:54:58.870|
                     * Varibe|2.5|2021-04-15 06:54:58.870|
                     */

                    //check if the ratetype falls in allowed 7 ratetype
                    if (sLine.StartsWith("Fixed")     || 
                         sLine.StartsWith("Variable")  || sLine.StartsWith("Compound")  || 
                         sLine.StartsWith("Simple")    || sLine.StartsWith("Amortized") || 
                         sLine.StartsWith("PrimaRate") ||  sLine.StartsWith("DisCount") 
                    )
                    {
                        DeatailLine = sLine.Split("|");
                        if ((DeatailLine[0] == "") || (DeatailLine[1] == "") || (DeatailLine[2] == ""))
                        {
                            //Skip this record as it does not have all the 3 values. 
                            Recordcounter++;
                            continue;
                        }

                        // Check RateValue to be only number and .
                        if (!isOnly_Digit_And_Dot(DeatailLine[1]))
                        {
                            //Skip this record as it should only have "Numbers and ."
                            Recordcounter++;
                            continue;
                        }

                        //Check TimeStamp.
                        if (!isValid_TimeStamp(DeatailLine[2]))
                        {
                            //Skip this record as it should only have "Numbers and ."
                            Recordcounter++;
                            continue;
                        }

                        // All Validations for the record was successful - So Insert to sql Server Table.
                        
                        //Insert Logic  for the Database. 
                    }
                    Recordcounter++;
                }
                context.Logger.LogLine($"Handler Process Ends -: Processed object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}.");
                return "";
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name} ");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw new Exception("Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name} ");
            }
        }

        //Return true if only numbers and . 
        bool isOnly_Digit_And_Dot(string checkstring)
        {
            foreach (char c in checkstring)
            {
                if (c == '.')
                    continue;

                if ((c < '0') || (c > '9'))
                    return false;
            }
            return true;
        }

        //Venkat: *Caution*   Revisit this code. 
        bool isValid_TimeStamp(string checkstring)
        {
            return true;
        }

    }
}
