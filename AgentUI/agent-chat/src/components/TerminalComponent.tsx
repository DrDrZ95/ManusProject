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
      
      let os = '未知系统';
      
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
      output: `终端已初始化。检测到操作系统：${osInfo}\n输入 'help' 查看可用命令。`,
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
        return `可用命令：
  help     - 显示此帮助信息
  clear    - 清除终端历史记录
  date     - 显示当前日期和时间
  os       - 显示操作系统信息
  whoami   - 显示当前用户
  pwd      - 显示当前目录（模拟）
  ls       - 列出目录内容（模拟）
  echo     - 回显输入内容（用法：echo <消息>）`;
      
      case 'clear':
        setCommandHistory([]);
        return '';
      
      case 'date':
        return new Date().toLocaleString('zh-CN');
      
      case 'os':
        return `操作系统：${osInfo}
用户代理：${navigator.userAgent}
平台：${navigator.platform}
语言：${navigator.language}`;
      
      case 'whoami':
        return 'react-用户';
      
      case 'pwd':
        return '/home/react-用户/agent-chat';
      
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
        return `命令未找到：${command}。输入 'help' 查看可用命令。`;
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
    <div className={`fixed right-4 top-16 bottom-4 bg-gray-900 text-green-400 rounded-lg shadow-2xl border border-gray-700 z-50 ${
      isMinimized ? 'w-80 h-12' : 'w-96'
    } transition-all duration-300`}>
      {/* Terminal Header */}
      <div className="flex items-center justify-between bg-gray-800 px-3 py-2 rounded-t-lg border-b border-gray-700">
        <div className="flex items-center space-x-2">
          <Terminal size={16} />
          <span className="text-sm font-medium text-gray-300">终端</span>
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
        <div className="flex flex-col h-full">
          {/* Command History */}
          <div 
            ref={terminalRef}
            className="flex-1 p-3 overflow-y-auto text-sm font-mono"
            style={{ height: 'calc(100vh - 200px)' }}
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
                placeholder="输入命令..."
                autoComplete="off"
              />
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

