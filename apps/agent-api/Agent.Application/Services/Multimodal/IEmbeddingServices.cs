namespace Agent.Application.Services.Multimodal;

/// <summary>
/// Service for generating image embeddings (e.g., using CLIP).
/// 用于生成图像嵌入的服务（例如，使用CLIP）。
/// </summary>
public interface IImageEmbeddingService
{
    /// <summary>
    /// Generates a vector embedding for a given image file.
    /// 为给定的图像文件生成向量嵌入。
    /// </summary>
    /// <param name="imagePath">The local path to the image file.</param>
    /// <returns>The image embedding as a float array.</returns>
    Task<float[]> GenerateImageEmbeddingAsync(string imagePath);
}

/// <summary>
/// Service for generating audio embeddings (e.g., using CLAP).
/// 用于生成音频嵌入的服务（例如，使用CLAP）。
/// </summary>
public interface IAudioEmbeddingService
{
    /// <summary>
    /// Generates a vector embedding for a given audio file.
    /// 为给定的音频文件生成向量嵌入。
    /// </summary>
    /// <param name="audioPath">The local path to the audio file.</param>
    /// <returns>The audio embedding as a float array.</returns>
    Task<float[]> GenerateAudioEmbeddingAsync(string audioPath);
}

/// <summary>
/// Service for converting speech to text (e.g., using Whisper).
/// 用于将语音转换为文本的服务（例如，使用Whisper）。
/// </summary>
public interface ISpeechToTextService
{
    /// <summary>
    /// Transcribes the speech in a given audio file to text.
    /// 将给定音频文件中的语音转录为文本。
    /// </summary>
    /// <param name="audioPath">The local path to the audio file.</param>
    /// <returns>The transcribed text.</returns>
    Task<string> TranscribeAsync(string audioPath);
}

