import React, { useState, useEffect, useRef } from 'react';
import { Terminal as TerminalIcon, X, Maximize2, Minimize2, ChevronDown, Server } from 'lucide-react';
import { TerminalLine, ServerContext } from '../types';
import { analyzeCommand } from '../services/geminiService';
import { TRANSLATIONS } from '../constants';

interface TerminalProps {
  isOpen: boolean;
  onToggle: () => void;
}

const Terminal: React.FC<TerminalProps> = ({ isOpen, onToggle }) => {
  const [lines, setLines] = useState<TerminalLine[]>([
    { id: 'init', type: 'system', content: 'OpsNexus CLI v1.0.4 initialized...', timestamp: Date.now() },
    { id: 'init3', type: 'system', content: 'Type "help" for available commands.', timestamp: Date.now() },
  ]);
  const [input, setInput] = useState('');
  const [isMaximized, setIsMaximized] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [activeContext, setActiveContext] = useState<ServerContext>('local');
  const [contextMenuOpen, setContextMenuOpen] = useState(false);
  
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  
  // Translation Helper for context
  const getContextLabel = (ctx: ServerContext) => {
    switch(ctx) {
      case 'local': return TRANSLATIONS.zh.localContext;
      case 'remote-aws': return `${TRANSLATIONS.zh.remoteServer} (AWS)`;
      case 'remote-aliyun': return `${TRANSLATIONS.zh.remoteServer} (Aliyun)`;
      default: return 'Unknown';
    }
  };

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [lines, isOpen]);

  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  const handleCommand = async (cmd: string) => {
    if (!cmd.trim()) return;

    const newLine: TerminalLine = {
      id: Date.now().toString(),
      type: 'input',
      content: cmd,
      timestamp: Date.now()
    };

    setLines(prev => [...prev, newLine]);
    setInput('');
    setIsProcessing(true);

    // Mock local command handling
    let responseContent = '';
    const lowerCmd = cmd.toLowerCase().trim();

    if (lowerCmd === 'clear') {
      setLines([]);
      setIsProcessing(false);
      return;
    } else {
        responseContent = await analyzeCommand(cmd, `Context: ${activeContext}`);
    }

    setLines(prev => [...prev, {
      id: (Date.now() + 1).toString(),
      type: 'output',
      content: responseContent.trim(),
      timestamp: Date.now()
    }]);
    setIsProcessing(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !isProcessing) {
      handleCommand(input);
    }
  };

  const changeContext = (ctx: ServerContext) => {
    setActiveContext(ctx);
    setContextMenuOpen(false);
    setLines(prev => [...prev, {
      id: Date.now().toString(),
      type: 'system',
      content: `Context switched to: ${getContextLabel(ctx)}`,
      timestamp: Date.now()
    }]);
    inputRef.current?.focus();
  };

  if (!isOpen) return null;

  return (
    <div className={`fixed bottom-0 left-0 right-0 bg-nexus-900 border-t border-nexus-700 shadow-2xl transition-all duration-300 z-50 flex flex-col ${isMaximized ? 'h-[80vh]' : 'h-64'}`}>
      {/* Terminal Header */}
      <div className="flex items-center justify-between px-4 py-2 bg-nexus-800 border-b border-nexus-700 select-none relative">
        <div className="flex items-center space-x-4">
           <div className="flex items-center space-x-2 text-nexus-300">
             <TerminalIcon size={16} />
             <span className="text-xs font-mono font-bold tracking-wide">OPSNEXUS_CLI</span>
           </div>
           
           {/* Context Selector */}
           <div className="relative">
              <button 
                onClick={() => setContextMenuOpen(!contextMenuOpen)}
                className="flex items-center space-x-2 px-2 py-1 bg-nexus-900 rounded border border-nexus-600 hover:border-nexus-400 text-xs text-nexus-200 transition-colors"
              >
                 <Server size={12} className={activeContext === 'local' ? 'text-green-400' : 'text-blue-400'} />
                 <span>{getContextLabel(activeContext)}</span>
                 <ChevronDown size={12} />
              </button>
              
              {contextMenuOpen && (
                 <div className="absolute top-full left-0 mt-1 w-48 bg-nexus-800 border border-nexus-600 rounded shadow-xl z-50 overflow-hidden">
                    <button onClick={() => changeContext('local')} className="w-full text-left px-3 py-2 text-xs text-nexus-200 hover:bg-nexus-700 flex items-center">
                       <div className="w-2 h-2 bg-green-500 rounded-full mr-2"></div> {TRANSLATIONS.zh.localContext}
                    </button>
                    <button onClick={() => changeContext('remote-aws')} className="w-full text-left px-3 py-2 text-xs text-nexus-200 hover:bg-nexus-700 flex items-center">
                       <div className="w-2 h-2 bg-blue-500 rounded-full mr-2"></div> {TRANSLATIONS.zh.remoteServer} (AWS)
                    </button>
                    <button onClick={() => changeContext('remote-aliyun')} className="w-full text-left px-3 py-2 text-xs text-nexus-200 hover:bg-nexus-700 flex items-center">
                       <div className="w-2 h-2 bg-orange-500 rounded-full mr-2"></div> {TRANSLATIONS.zh.remoteServer} (Aliyun)
                    </button>
                 </div>
              )}
           </div>
        </div>

        <div className="flex items-center space-x-3">
          <button onClick={() => setIsMaximized(!isMaximized)} className="text-nexus-400 hover:text-white">
            {isMaximized ? <Minimize2 size={16} /> : <Maximize2 size={16} />}
          </button>
          <button onClick={onToggle} className="text-nexus-400 hover:text-red-400">
            <X size={16} />
          </button>
        </div>
      </div>

      {/* Terminal Body */}
      <div 
        ref={scrollRef}
        className="flex-1 overflow-y-auto p-4 font-mono text-sm bg-nexus-900/95 backdrop-blur text-nexus-300"
        onClick={() => inputRef.current?.focus()}
      >
        {lines.map((line) => (
          <div key={line.id} className="mb-1 break-words whitespace-pre-wrap">
            {line.type === 'input' && (
              <span className="text-green-400 font-bold mr-2">➜ [{activeContext === 'local' ? '~' : 'remote'}]</span>
            )}
            {line.type === 'system' && (
              <span className="text-blue-400 mr-2">[SYS]</span>
            )}
            <span className={line.type === 'error' ? 'text-red-400' : line.type === 'input' ? 'text-white' : 'text-nexus-300'}>
              {line.content}
            </span>
          </div>
        ))}
        
        <div className="flex items-center mt-2">
          <span className="text-green-400 font-bold mr-2">➜ [{activeContext === 'local' ? '~' : 'remote'}]</span>
          <div className="relative flex-1">
             <input
              ref={inputRef}
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={isProcessing}
              className="w-full bg-transparent border-none outline-none text-white placeholder-nexus-600"
              autoComplete="off"
              spellCheck="false"
            />
            {isProcessing && <span className="absolute right-0 top-0 text-yellow-400 animate-pulse">Processing...</span>}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Terminal;