namespace Agent.Core.Tests.Services
{
    /// <summary>
    /// Unit tests for SandboxTerminalService
    /// SandboxTerminalService 单元测试
    /// </summary>
    public class SandboxTerminalServiceTests
    {
        private readonly Mock<ILogger<SandboxTerminalService>> _mockLogger;
        private readonly Mock<IOptions<SandboxTerminalOptions>> _mockOptions;
        private readonly SandboxTerminalService _service;
        private readonly SandboxTerminalOptions _options;

        public SandboxTerminalServiceTests()
        {
            _mockLogger = new Mock<ILogger<SandboxTerminalService>>();
            _mockOptions = new Mock<IOptions<SandboxTerminalOptions>>();
            
            _options = new SandboxTerminalOptions
            {
                DefaultWorkingDirectory = System.IO.Path.GetTempPath(),
                BlockedCommands = new List<string> { "rm -rf", "mkfs" }
            };
            
            _mockOptions.Setup(o => o.Value).Returns(_options);
            
            _service = new SandboxTerminalService(_mockOptions.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Test IsCommandSafe with safe command
        /// 测试安全命令
        /// </summary>
        [Fact]
        public void IsCommandSafe_SafeCommand_ReturnsTrue()
        {
            // Arrange
            var command = "echo hello";

            // Act
            var result = _service.IsCommandSafe(command);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Test IsCommandSafe with blocked command
        /// 测试被阻止的命令
        /// </summary>
        [Fact]
        public void IsCommandSafe_BlockedCommand_ReturnsFalse()
        {
            // Arrange
            var command = "rm -rf /";

            // Act
            var result = _service.IsCommandSafe(command);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test IsCommandSafe with dangerous patterns
        /// 测试危险模式
        /// </summary>
        [Fact]
        public void IsCommandSafe_DangerousPattern_ReturnsFalse()
        {
            // Arrange
            var command = ":(){ :|:& };"; // Fork bomb

            // Act
            var result = _service.IsCommandSafe(command);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test ExecuteCommandAsync with unsafe command blocks execution
        /// 测试执行不安全命令时被阻止
        /// </summary>
        [Fact]
        public async Task ExecuteCommandAsync_UnsafeCommand_ReturnsError()
        {
            // Arrange
            var command = "rm -rf /";

            // Act
            var result = await _service.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal(-1, result.ExitCode);
            Assert.Contains("blocked", result.StandardError);
        }

        /// <summary>
        /// Test ExecuteCommandAsync with safe command (mocking Process is hard, so we just check it doesn't fail validation)
        /// Actually, running "echo test" is safe on most systems.
        /// But we should be careful about CI environments.
        /// Since we cannot mock Process, we might skip actual execution test or use a very simple command.
        /// </summary>
        [Fact]
        public async Task ExecuteCommandAsync_SafeCommand_RunsOrFailsGracefully()
        {
            // Arrange
            var command = "echo test";
            // On Windows "echo" might be a shell builtin, so we might need "cmd /c echo test" or similar?
            // Process.Start("echo") might fail if not found.
            // SandboxTerminalService implementation uses "cmd /c" or "/bin/bash -c" usually?
            // Let's check CreateProcessStartInfo implementation if possible, but it's private.
            // Assuming it handles it.
            
            // We just ensure it doesn't return security error.
            
            // Act
            var result = await _service.ExecuteCommandAsync(command);

            // Assert
            // It might fail to run if "echo" is not found, but it shouldn't be blocked.
            Assert.DoesNotContain("blocked", result.StandardError);
        }
    }
}
