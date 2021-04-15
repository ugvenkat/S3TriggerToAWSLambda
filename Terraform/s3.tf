# Bucket where users drop the file. 
resource "aws_s3_bucket" "bucket-src4" {
  bucket = "ugvenkat-src4"
  acl = "public-read"

  tags = {
    Name  = "Venkat Bucket"
    Environment = "Development"
  }
}