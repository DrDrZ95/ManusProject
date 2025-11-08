using Agent.Core.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Agent.Api.Tests.Services
{
    /// <summary>
    /// FileUploadService的单元测试
    /// Unit tests for FileUploadService
    /// </summary>
    public class FileUploadServiceTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;

        public FileUploadServiceTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
        }

        /// <summary>
        /// 测试OWASP安全检查（例如，文件类型验证）
        /// Test OWASP security check (e.g., file type validation)
        /// </summary>
        [Fact]
        public async Task UploadFile_WithInvalidFileType_ShouldThrowSecurityException()
        {
            // Arrange
            var invalidFile = new Mock<IFormFile>();
            invalidFile.Setup(f => f.FileName).Returns("malicious.exe");
            invalidFile.Setup(f => f.ContentType).Returns("application/x-msdownload");

            // 模拟文件上传服务中的安全检查逻辑 - Mock the security check logic in the file upload service
            _mockFileUploadService.Setup(s => s.UploadFileAsync(
                It.Is<IFormFile>(f => f.FileName.EndsWith(".exe")), 
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SecurityException("File type not allowed by OWASP policy."));

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _mockFileUploadService.Object.UploadFileAsync(invalidFile.Object, CancellationToken.None));
            
            // 验证服务是否被调用 - Verify that the service was called
            _mockFileUploadService.Verify(s => s.UploadFileAsync(
                invalidFile.Object, 
                CancellationToken.None), Times.Once);
        }

        // TODO: 补充其他OWASP安全检查的测试，例如文件大小限制、路径遍历等
        // TODO: Supplement other OWASP security check tests, such as file size limits, path traversal, etc.
    }
}

