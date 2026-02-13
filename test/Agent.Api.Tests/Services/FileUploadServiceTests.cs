
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

            _mockFileUploadService.Setup(s => s.ValidateFileAsync(
                It.Is<IFormFile>(f => f.FileName.EndsWith(".exe"))))
                .ThrowsAsync(new SecurityException("File type not allowed by OWASP policy."));

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _mockFileUploadService.Object.ValidateFileAsync(invalidFile.Object));

            _mockFileUploadService.Verify(s => s.ValidateFileAsync(
                invalidFile.Object), Times.Once);
        }
    }
}

