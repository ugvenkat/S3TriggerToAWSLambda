Download and install Terraform v0.14.10
---------------------------------------
C:\Users\Venkat>terraform version
Terraform v0.14.10

In providers.tf
---------------------------------------
Change access_key and secret_key


Navigate to Terraform Directory and execute

Terraform init
Terraform plan
Terraform deploy

Goto lamda functions in aws console using browser. 
select the lambda S3TriggerToAwsLamda
Select Add Trigger 

In Trigger configuration
Select the newly created S3 bucket.    (ugvenkat-src4)
Event Type selcet "All object create events"


Check the checkbox which says. 
I acknowledge that using same checkbox.........................



Go to the bucket and upload a text file. 

watch the cloudwatch log to view the file being read. 









