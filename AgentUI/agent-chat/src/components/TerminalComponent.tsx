import React, { useState, useRef, useEffect } from 'react';
import { Terminal, X, Minimize2, Maximize2 } from 'lucide-react';

interface TerminalProps {
  isVisible: boolean;
  onToggle: () => void;
}

interface CommandHistory {
  command: string;
  output: string;
  timestamp: Date;
}

export const TerminalComponent: React.FC<TerminalProps> = ({ isVisible, onToggle }) => {
  const [isMinimized, setIsMinimized] = useState(false);
  const [commandHistory, setCommandHistory] = useState<CommandHistory[]>([]);
  const [currentCommand, setCurrentCommand] = useState('');
  const [osInfo, setOsInfo] = useState<string>('');
  const terminalRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Detect OS on component mount
  useEffect(() => {
    const detectOS = () => {
      const userAgent = navigator.userAgent;
      const platform = navigator.platform;
      
      let os = 'Unknown OS';
      
      if (userAgent.indexOf('Win') !== -1) {
        os = 'Windows';
      } else if (userAgent.indexOf('Mac') !== -1) {
        os = 'macOS';
      } else if (userAgent.indexOf('Linux') !== -1) {
        os = 'Linux';
      } else if (userAgent.indexOf('Android') !== -1) {
        os = 'Android';
      } else if (userAgent.indexOf('iPhone') !== -1 || userAgent.indexOf('iPad') !== -1) {
        os = 'iOS';
      }
      
      setOsInfo(`${os} (${platform})`);
    };

    detectOS();
    
    // Add welcome message
    setCommandHistory([{
      command: 'welcome',
      output: `Terminal initialized. Detected OS: ${osInfo}\nType 'help' for available commands.`,
      timestamp: new Date()
    }]);
  }, []);

  // Auto-scroll to bottom when new commands are added
  useEffect(() => {
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight;
    }
  }, [commandHistory]);

  // Focus input when terminal becomes visible
  useEffect(() => {
    if (isVisible && !isMinimized && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isVisible, isMinimized]);

  const executeCommand = (command: string): string => {
    const cmd = command.trim().toLowerCase();
    
    switch (cmd) {
      case 'help':
        return `Available commands:
  help     - Show this help message
  clear    - Clear terminal history
  date     - Show current date and time
  os       - Show operating system information
  whoami   - Show current user
  pwd      - Show current directory (simulated)
  ls       - List directory contents (simulated)
  echo     - Echo back the input (usage: echo <message>)`;
      
      case 'clear':
        setCommandHistory([]);
        return '';
      
      case 'date':
        return new Date().toString();
      
      case 'os':
        return `Operating System: ${osInfo}
User Agent: ${navigator.userAgent}
Platform: ${navigator.platform}
Language: ${navigator.language}`;
      
      case 'whoami':
        return 'react-user';
      
      case 'pwd':
        return '/home/react-user/agent-chat';
      
      case 'ls':
        return `src/
public/
node_modules/
package.json
README.md
vite.config.ts`;
      
      default:
        if (cmd.startsWith('echo ')) {
          return command.substring(5);
        }
        return `Command not found: ${command}. Type 'help' for available commands.`;
    }
  };

  const handleCommandSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!currentCommand.trim()) return;
    
    const output = executeCommand(currentCommand);
    
    if (currentCommand.trim().toLowerCase() !== 'clear') {
      setCommandHistory(prev => [...prev, {
        command: currentCommand,
        output,
        timestamp: new Date()
      }]);
    }
    
    setCurrentCommand('');
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowUp') {
      e.preventDefault();
      // Get last command from history
      if (commandHistory.length > 0) {
        const lastCommand = commandHistory[commandHistory.length - 1].command;
        if (lastCommand !== 'welcome') {
          setCurrentCommand(lastCommand);
        }
      }
    }
  };

  if (!isVisible) return null;

  return (
    <div className={`fixed right-4 bottom-4 bg-gray-900 text-green-400 rounded-lg shadow-2xl border border-gray-700 z-50 ${
      isMinimized ? 'w-80 h-12' : 'w-96 h-96'
    } transition-all duration-300`}>
      {/* Terminal Header */}
      <div className="flex items-center justify-between bg-gray-800 px-3 py-2 rounded-t-lg border-b border-gray-700">
        <div className="flex items-center space-x-2">
          <Terminal size={16} />
          <span className="text-sm font-medium text-gray-300">Terminal</span>
          <span className="text-xs text-gray-500">({osInfo})</span>
        </div>
        <div className="flex items-center space-x-1">
          <button
            onClick={() => setIsMinimized(!isMinimized)}
            className="p-1 hover:bg-gray-700 rounded text-gray-400 hover:text-gray-200"
          >
            {isMinimized ? <Maximize2 size={14} /> : <Minimize2 size={14} />}
          </button>
          <button
            onClick={onToggle}
            className="p-1 hover:bg-gray-700 rounded text-gray-400 hover:text-gray-200"
          >
            <X size={14} />
          </button>
        </div>
      </div>

      {/* Terminal Content */}
      {!isMinimized && (
        <div className="flex flex-col h-80">
          {/* Command History */}
          <div 
            ref={terminalRef}
            className="flex-1 p-3 overflow-y-auto text-sm font-mono"
          >
            {commandHistory.map((entry, index) => (
              <div key={index} className="mb-2">
                {entry.command !== 'welcome' && (
                  <div className="text-green-400">
                    <span className="text-blue-400">$</span> {entry.command}
                  </div>
                )}
                {entry.output && (
                  <div className="text-gray-300 whitespace-pre-line">
                    {entry.output}
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Command Input */}
          <form onSubmit={handleCommandSubmit} className="border-t border-gray-700 p-3">
            <div className="flex items-center space-x-2">
              <span className="text-blue-400">$</span>
              <input
                ref={inputRef}
                type="text"
                value={currentCommand}
                onChange={(e) => setCurrentCommand(e.target.value)}
                onKeyDown={handleKeyDown}
                className="flex-1 bg-transparent text-green-400 outline-none font-mono"
                placeholder="Type a command..."
                autoComplete="off"
              />
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

