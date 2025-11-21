import React, { useEffect, useRef } from 'react';
import { Terminal } from 'xterm';
import { FitAddon } from '@xterm/addon-fit';
import { WebglAddon } from '@xterm/addon-webgl';
import { Icons } from './icons';
import { useStore } from '../store';
import { MOCK_TERMINAL_WELCOME } from '../types';
import { translations } from '../locales';

export const TerminalPanel: React.FC = () => {
  const terminalRef = useRef<HTMLDivElement>(null);
  const xtermRef = useRef<Terminal | null>(null);
  const fitAddonRef = useRef<FitAddon | null>(null);
  const isTerminalOpen = useStore((s) => s.isTerminalOpen);
  const toggleTerminal = useStore((s) => s.toggleTerminal);
  const terminalWidth = useStore(s => s.terminalWidth);
  const language = useStore(s => s.language);
  
  const t = translations[language];

  useEffect(() => {
    if (!terminalRef.current || xtermRef.current) return;

    const term = new Terminal({
      cursorBlink: true,
      cursorStyle: 'bar',
      fontSize: 13,
      fontFamily: '"JetBrains Mono", monospace',
      theme: {
        background: '#1e1e1e', // Keep terminal dark for contrast
        foreground: '#f3f4f6',
        cursor: '#ffffff',
        selectionBackground: 'rgba(255, 255, 255, 0.3)',
        black: '#000000',
        red: '#ef4444',
        green: '#22c55e', 
        yellow: '#eab308',
        blue: '#3b82f6',
        magenta: '#a855f7',
        cyan: '#06b6d4',
        white: '#ffffff',
        brightBlack: '#6b7280',
        brightRed: '#f87171',
        brightGreen: '#4ade80',
        brightYellow: '#fde047',
        brightBlue: '#60a5fa',
        brightMagenta: '#c084fc',
        brightCyan: '#22d3ee',
        brightWhite: '#ffffff',
      },
      allowTransparency: false,
    });

    const fitAddon = new FitAddon();
    term.loadAddon(fitAddon);
    
    try {
      const webglAddon = new WebglAddon();
      webglAddon.onContextLoss(() => {
        webglAddon.dispose();
      });
      term.loadAddon(webglAddon);
    } catch (e) {
      console.warn("WebGL not supported in this environment, falling back to canvas/dom");
    }

    term.open(terminalRef.current);
    fitAddon.fit();
    
    term.write(MOCK_TERMINAL_WELCOME);
    term.write('\r\n$ ');

    xtermRef.current = term;
    fitAddonRef.current = fitAddon;

    let currentLine = '';
    term.onData((e) => {
      switch (e) {
        case '\r':
          term.write('\r\n');
          processCommand(currentLine, term);
          currentLine = '';
          break;
        case '\u007F':
          if (currentLine.length > 0) {
            currentLine = currentLine.substring(0, currentLine.length - 1);
            term.write('\b \b');
          }
          break;
        default:
          if (e >= String.fromCharCode(32) && e <= String.fromCharCode(126)) {
            currentLine += e;
            term.write(e);
          }
      }
    });

    return () => {
      term.dispose();
    };
  }, []);

  useEffect(() => {
    const handleResize = () => {
        if (isTerminalOpen && fitAddonRef.current) {
            setTimeout(() => {
                try {
                    fitAddonRef.current?.fit();
                } catch(e) {}
            }, 10);
        }
    };
    
    // Observe resizing of the container
    const observer = new ResizeObserver(handleResize);
    if (terminalRef.current) {
        observer.observe(terminalRef.current);
    }

    window.addEventListener('resize', handleResize);
    return () => {
        window.removeEventListener('resize', handleResize);
        observer.disconnect();
    };
  }, [isTerminalOpen, terminalWidth]);

  const processCommand = (cmd: string, term: Terminal) => {
    const command = cmd.trim();
    const parts = command.split(' ');
    
    if (command === '') {
      term.write('$ ');
      return;
    }

    setTimeout(() => {
        switch (parts[0]) {
        case 'help':
            term.write('  \x1b[38;2;224;224;224mAgent-OS Commands:\x1b[0m\r\n');
            term.write('  help     Show this help\r\n');
            term.write('  clear    Clear terminal\r\n');
            term.write('  echo     Print arguments\r\n');
            term.write('  ls       List files\r\n');
            term.write('  whoami   Current user\r\n');
            term.write('  date     Current date\r\n');
            break;
        case 'clear':
            term.clear();
            break;
        case 'echo':
            term.write(parts.slice(1).join(' ') + '\r\n');
            break;
        case 'ls':
            term.write('Documents  Downloads  Projects  agent-secrets.txt  README.md\r\n');
            break;
        case 'whoami':
            term.write('agent-user\r\n');
            break;
        case 'date':
            term.write(new Date().toString() + '\r\n');
            break;
        default:
            term.write(`\x1b[31mcommand not found: ${parts[0]}\x1b[0m\r\n`);
        }
        term.write('$ ');
    }, 20);
  };

  if (!isTerminalOpen) return null;

  return (
    <div className="h-full w-full flex flex-col bg-white/90 backdrop-blur-md relative overflow-hidden border-l border-gray-300">
      <div className="flex items-center justify-between px-4 py-3 bg-[#F3F4F6] border-b border-gray-300">
        <div className="flex items-center space-x-2 text-xs font-mono text-gray-600 select-none">
          <Icons.Terminal className="w-3 h-3 text-black" />
          <span className="font-medium">{t.terminalTitle}</span>
        </div>
        <div className="flex items-center space-x-2">
          <button onClick={toggleTerminal} className="text-gray-400 hover:text-black transition-colors">
            <Icons.ClosePanelRight className="w-4 h-4" />
          </button>
        </div>
      </div>

      <div className="flex-1 bg-[#1e1e1e] p-4 min-h-0 relative shadow-inner">
         <div ref={terminalRef} className="h-full w-full" />
      </div>
    </div>
  );
};