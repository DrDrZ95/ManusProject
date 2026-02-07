
import React, { useState, useEffect, useRef } from 'react';
import { Terminal as TerminalIcon, X, Maximize2, Minimize2, ChevronDown, Server, Wifi, Activity, RefreshCw, Square } from 'lucide-react';
import { TerminalLine, ServerContext } from '../types';
import { analyzeCommand } from '../services/geminiService';
import { mcpClient } from '../services/mcp';
import { api } from '../services/api';

interface TerminalProps {
  isOpen: boolean;
  onToggle: () => void;
}

const Terminal: React.FC<TerminalProps> = ({ isOpen, onToggle }) => {
  const [lines, setLines] = useState<TerminalLine[]>([
    { id: 'init', type: 'system', content: 'AgentProject Core Terminal v4.2.0 initialized...', timestamp: Date.now() },
    { id: 'init2', type: 'system', content: 'Secure Tunnel established via AES-256-GCM.', timestamp: Date.now() },
    { id: 'init3', type: 'system', content: 'Type "mcp status" to check agent orchestration.', timestamp: Date.now() },
  ]);
  const [input, setInput] = useState('');
  const [isMaximized, setIsMaximized] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [activeContext, setActiveContext] = useState<ServerContext>('local');
  const [contextMenuOpen, setContextMenuOpen] = useState(false);
  const [latency, setLatency] = useState(12);
  
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  
  useEffect(() => {
    const timer = setInterval(() => {
      setLatency(prev => Math.max(8, Math.min(25, prev + (Math.random() > 0.5 ? 1 : -1))));
    }, 3000);
    return () => clearInterval(timer);
  }, []);

  const getContextLabel = (ctx: ServerContext) => {
    switch(ctx) {
      case 'local': return 'LOCAL_WORKSPACE';
      case 'remote-aws': return 'AWS_NODE_ORegon';
      case 'remote-aliyun': return 'ALIYUN_HK_GPu';
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

  const handleCancel = () => {
    if (!isProcessing) return;
    
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    setIsProcessing(false);
    setLines(prev => [...prev, {
      id: Date.now().toString(),
      type: 'error',
      content: '[SIGINT] Operation terminated by user.',
      timestamp: Date.now()
    }]);
  };

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
    
    // Setup abort controller for this specific call
    const controller = new AbortController();
    abortControllerRef.current = controller;

    let responseContent = '';
    const lowerCmd = cmd.toLowerCase().trim();

    try {
      if (lowerCmd === 'clear') {
        setLines([]);
        setIsProcessing(false);
        return;
      } 
      else if (lowerCmd.startsWith('mcp')) {
        const query = lowerCmd.replace('mcp', '').trim();
        if (query) {
          responseContent = await mcpClient.simulateAgentExecution(query);
        } else {
          const tools = mcpClient.listTools();
          responseContent = `MCP Registry Tools Detected:\n${tools.map(t => `[+] ${t.name.padEnd(20)} | ${t.description}`).join('\n')}`;
        }
      }
      else if (lowerCmd === 'api test') {
        const data = await api.get('/system/health', { 
          mockData: { status: 'ok', latency: 12, region: 'us-west-2' } 
        });
        responseContent = `API Response Matrix:\n${JSON.stringify(data, null, 2)}`;
      }
      else {
          // Pass signal to the service if needed (simulated here)
          responseContent = await analyzeCommand(cmd, `Context: ${activeContext}`);
      }
    } catch (err: any) {
      if (err.name === 'AbortError') return;
      responseContent = `[FATAL] Exception: ${err.message}`;
    }

    // Only commit output if not aborted
    if (!controller.signal.aborted) {
      setLines(prev => [...prev, {
        id: (Date.now() + 1).toString(),
        type: 'output',
        content: responseContent.trim(),
        timestamp: Date.now()
      }]);
      setIsProcessing(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !isProcessing) {
      handleCommand(input);
    }
    if (e.key === 'c' && e.ctrlKey) {
      e.preventDefault();
      handleCancel();
    }
  };

  const changeContext = (ctx: ServerContext) => {
    setActiveContext(ctx);
    setContextMenuOpen(false);
    setLines(prev => [...prev, {
      id: Date.now().toString(),
      type: 'system',
      content: `Context re-aligned: ${getContextLabel(ctx)}`,
      timestamp: Date.now()
    }]);
    inputRef.current?.focus();
  };

  if (!isOpen) return null;

  return (
    <div className={`absolute bottom-0 left-0 right-0 bg-white dark:bg-nexus-terminal border-t border-slate-200 dark:border-nexus-700/50 shadow-[0_-20px_50px_rgba(0,0,0,0.15)] dark:shadow-[0_-20px_50px_rgba(0,0,0,0.5)] transition-all duration-500 z-[100] flex flex-col ${isMaximized ? 'h-[90%]' : 'h-80'}`}>
      
      {/* Terminal Header */}
      <div className="flex items-center justify-between px-6 py-3 bg-slate-50/80 dark:bg-nexus-800/80 backdrop-blur-xl border-b border-slate-200 dark:border-nexus-700/50 select-none relative group">
        <div className="flex items-center space-x-6">
           <div className="flex items-center space-x-3 text-slate-500 dark:text-nexus-400">
             <div className="p-1.5 bg-nexus-accent rounded-lg shadow-sm">
                <TerminalIcon size={14} className="text-white" />
             </div>
             <span className="text-[10px] font-mono font-black tracking-[0.2em] uppercase text-slate-700 dark:text-nexus-300">AgentProject_Console_v1</span>
           </div>
           
           <div className="relative">
              <button 
                onClick={() => setContextMenuOpen(!contextMenuOpen)}
                className="flex items-center space-x-3 px-3 py-1.5 bg-white dark:bg-nexus-terminal rounded-xl border border-slate-200 dark:border-nexus-700/50 hover:border-nexus-accent/50 text-[9px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 transition-all shadow-sm shadow-slate-200/50 dark:shadow-none"
              >
                 <Server size={12} className={activeContext === 'local' ? 'text-green-500' : 'text-blue-500'} />
                 <span className="truncate max-w-[100px]">{getContextLabel(activeContext)}</span>
                 <ChevronDown size={12} className="opacity-50" />
              </button>
              
              {contextMenuOpen && (
                 <div className="absolute top-full left-0 mt-2 w-56 bg-white dark:bg-nexus-800 border border-slate-200 dark:border-nexus-700 rounded-2xl shadow-xl z-50 overflow-hidden p-1 animate-fade-in">
                    <button onClick={() => changeContext('local')} className="w-full text-left px-4 py-3 text-[9px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 hover:bg-slate-100 dark:hover:bg-nexus-700 hover:text-slate-900 dark:hover:text-white flex items-center rounded-xl transition-colors">
                       <div className="w-2 h-2 bg-green-500 rounded-full mr-3 shadow-[0_0_8px_#10b981]"></div> LOCAL_RUNTIME
                    </button>
                    <button onClick={() => changeContext('remote-aws')} className="w-full text-left px-4 py-3 text-[9px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 hover:bg-slate-100 dark:hover:bg-nexus-700 hover:text-slate-900 dark:hover:text-white flex items-center rounded-xl transition-colors">
                       <div className="w-2 h-2 bg-blue-500 rounded-full mr-3 shadow-[0_0_8px_#3b82f6]"></div> AWS_OPS_NODE
                    </button>
                    <button onClick={() => changeContext('remote-aliyun')} className="w-full text-left px-4 py-3 text-[9px] font-black uppercase tracking-widest text-slate-600 dark:text-nexus-300 hover:bg-slate-100 dark:hover:bg-nexus-700 hover:text-slate-900 dark:hover:text-white flex items-center rounded-xl transition-colors">
                       <div className="w-2 h-2 bg-orange-500 rounded-full mr-3 shadow-[0_0_8px_#f59e0b]"></div> ALIYUN_HUB_01
                    </button>
                 </div>
              )}
           </div>

           <div className="hidden lg:flex items-center space-x-6 border-l border-slate-200 dark:border-nexus-700 pl-6 h-6">
              <div className="flex items-center text-[9px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500">
                 <Wifi size={12} className="mr-2 text-green-500" />
                 Secure Tunnel: <span className="text-slate-900 dark:text-white ml-2">Active</span>
              </div>
              <div className="flex items-center text-[9px] font-black uppercase tracking-widest text-slate-400 dark:text-nexus-500">
                 <Activity size={12} className="mr-2 text-blue-500" />
                 Latency: <span className="text-slate-900 dark:text-white ml-2">{latency}ms</span>
              </div>
           </div>
        </div>

        <div className="flex items-center space-x-4">
          <button onClick={() => setIsMaximized(!isMaximized)} className="p-2 text-slate-400 dark:text-nexus-400 hover:text-slate-900 dark:hover:text-white hover:bg-slate-200 dark:hover:bg-nexus-700 rounded-xl transition-all">
            {isMaximized ? <Minimize2 size={18} /> : <Maximize2 size={18} />}
          </button>
          <button onClick={onToggle} className="p-2 text-slate-400 dark:text-nexus-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 rounded-xl transition-all">
            <X size={18} />
          </button>
        </div>
      </div>

      {/* Terminal Viewport */}
      <div 
        ref={scrollRef}
        className="flex-1 overflow-y-auto p-8 font-mono text-xs bg-white dark:bg-nexus-terminal text-slate-600 dark:text-nexus-300 custom-scrollbar shadow-inner"
        onClick={() => inputRef.current?.focus()}
      >
        {lines.map((line) => (
          <div key={line.id} className="mb-2 break-words whitespace-pre-wrap flex">
            {line.type === 'input' && (
              <span className="text-nexus-accent font-black mr-4 shrink-0">➜ [{activeContext === 'local' ? '~' : 'remote'}]</span>
            )}
            {line.type === 'system' && (
              <span className="text-blue-500 mr-4 font-black shrink-0">[SYS_LOG]</span>
            )}
            {line.type === 'error' && (
              <span className="text-red-500 mr-4 font-black shrink-0">[FATAL]</span>
            )}
            <span className={`leading-relaxed ${line.type === 'input' ? 'text-slate-900 dark:text-white font-bold' : 'text-slate-600 dark:text-nexus-300 opacity-90'}`}>
              {line.content}
            </span>
          </div>
        ))}
        
        <div className="flex items-center mt-4">
          <span className="text-nexus-accent font-black mr-4 animate-pulse">➜ [{activeContext === 'local' ? '~' : 'remote'}]</span>
          <div className="relative flex-1">
             <input
              ref={inputRef}
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={isProcessing}
              className="w-full bg-transparent border-none outline-none text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-nexus-700 caret-nexus-accent font-bold"
              autoComplete="off"
              spellCheck="false"
              placeholder="Execute command..."
            />
            {isProcessing && (
              <div className="absolute right-0 top-0 flex items-center space-x-4">
                <div className="flex items-center text-nexus-accent text-[9px] font-black uppercase tracking-widest animate-pulse">
                  <RefreshCw size={12} className="mr-2 animate-spin" />
                  Processing...
                </div>
                <button 
                  onClick={handleCancel}
                  className="px-3 py-1 bg-red-50 dark:bg-red-500/10 hover:bg-red-500 text-red-500 hover:text-white border border-red-200 dark:border-red-500/20 rounded-lg text-[9px] font-black uppercase tracking-[0.1em] transition-all flex items-center group/stop"
                >
                  <Square size={10} className="mr-2 fill-current" />
                  Cancel
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
      
      <div className="h-1 bg-slate-100 dark:bg-nexus-700/20">
         <div className={`h-full bg-nexus-accent transition-all duration-300 ${isProcessing ? 'w-full opacity-100 animate-pulse' : 'w-0 opacity-0'}`}></div>
      </div>
    </div>
  );
};

export default Terminal;
