import { createParser } from 'eventsource-parser';
import { fetchEventSource } from '@microsoft/fetch-event-source';

/**
 * Utility for handling SSE (Server-Sent Events) streams from LLM APIs
 */
export class StreamingService {
  /**
   * Parse a ReadableStream from a fetch response into a stream of events
   * @param {ReadableStream} stream - The ReadableStream from fetch response
   * @param {function} onMessage - Callback for each message chunk
   * @param {function} onComplete - Callback when stream is complete
   * @param {function} onError - Callback for errors
   */
  static async parseSSEResponse(
    stream: ReadableStream<Uint8Array>,
    onMessage: (message: string) => void,
    onComplete: () => void,
    onError: (error: Error) => void
  ) {
    const parser = createParser((event) => {
      if (event.type === 'event') {
        onMessage(event.data);
      }
    });

    try {
      const reader = stream.getReader();
      const decoder = new TextDecoder();
      
      let done = false;
      while (!done) {
        const { value, done: doneReading } = await reader.read();
        done = doneReading;
        
        if (done) {
          onComplete();
          break;
        }
        
        const chunk = decoder.decode(value, { stream: true });
        parser.feed(chunk);
      }
    } catch (error) {
      onError(error instanceof Error ? error : new Error(String(error)));
    }
  }

  /**
   * Use fetchEventSource to connect to an SSE endpoint with automatic reconnection
   * @param {string} url - The SSE endpoint URL
   * @param {Object} options - Options for the fetch request
   * @param {function} onMessage - Callback for each message
   * @param {function} onError - Callback for errors
   * @param {function} onClose - Callback when connection closes
   * @returns {function} A function to abort the connection
   */
  static connectToSSE(
    url: string,
    options: RequestInit = {},
    onMessage: (message: string) => void,
    onError: (error: Error) => void,
    onClose: () => void
  ) {
    const controller = new AbortController();
    
    fetchEventSource(url, {
      ...options,
      signal: controller.signal,
      onmessage: (event) => {
        onMessage(event.data);
      },
      onerror: (err) => {
        onError(err);
        return err instanceof Error && err.message.includes('abort') 
          ? 1 // Don't retry on abort
          : 0; // Retry on other errors
      },
      onclose: () => {
        onClose();
      },
    }).catch(onError);
    
    return () => controller.abort();
  }

  /**
   * Send a streaming request to an LLM API and process the response
   * @param {string} url - API endpoint
   * @param {Object} payload - Request payload
   * @param {function} onChunk - Callback for each text chunk
   * @param {function} onComplete - Callback when stream completes
   * @param {function} onError - Callback for errors
   */
  static async streamLLMRequest(
    url: string,
    payload: any,
    onChunk: (text: string) => void,
    onComplete: () => void,
    onError: (error: Error) => void
  ) {
    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`API request failed: ${response.status} - ${errorText}`);
      }

      if (!response.body) {
        throw new Error('Response body is null');
      }

      await this.parseSSEResponse(
        response.body,
        onChunk,
        onComplete,
        onError
      );
    } catch (error) {
      onError(error instanceof Error ? error : new Error(String(error)));
    }
  }
}
