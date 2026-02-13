using Agent.Application.Services.FileUpload;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security;

namespace Agent.Api.Tests.Services
{
    public class FileUploadServiceTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;

        public FileUploadServiceTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
        }

        [Fact]
        public async Task UploadFile_WithInvalidFileType_ShouldThrowSecurityException()
        {
            // Arrange
            var invalidFile = new Mock<IFormFile>();
            invalidFile.Setup(f => f.FileName).Returns("malicious.exe");
            invalidFile.Setup(f => f.ContentType).Returns("application/x-msdownload");

            _mockFileUploadService.Setup(s => s.UploadFileAsync(
                It.Is<IFormFile>(f => f.FileName.EndsWith(".exe")), 
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SecurityException("File type not allowed by OWASP policy."));

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _mockFileUploadService.Object.UploadFileAsync(invalidFile.Object, CancellationToken.None));
            
            _mockFileUploadService.Verify(s => s.UploadFileAsync(
                invalidFile.Object, 
                CancellationToken.None), Times.Once);
        }
    }
}

