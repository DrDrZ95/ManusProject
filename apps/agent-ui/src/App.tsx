import { useState } from 'react';
import { Terminal, Menu, X } from 'lucide-react';
import { Sidebar } from './components/Sidebar';
import { ChatArea } from './components/ChatArea';
import { TerminalComponent } from './components/TerminalComponent';
import { OSInfoDisplay } from './components/OSInfoDisplay';
import { ChatProvider } from './contexts/ChatContext';
import './App.css';

function App() {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);
  const [isTerminalVisible, setIsTerminalVisible] = useState(false);

  return (
    <ChatProvider>
      <div className="flex h-screen bg-white overflow-hidden font-sans text-gray-900">
        {/* Mobile sidebar overlay */}
        {isMobileSidebarOpen && (
          <div 
            className="md:hidden fixed inset-0 bg-black bg-opacity-30 z-5"
            onClick={() => setIsMobileSidebarOpen(false)}
          />
        )}

        {/* Sidebar - Notion-style */}
        <div 
          className={`
            md:relative fixed inset-y-0 left-0 z-10 transform 
            ${isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full'} 
            md:translate-x-0 transition-transform duration-300 ease-in-out
            w-64 flex-shrink-0
          `}
        >
          <Sidebar />
        </div>

        {/* Main content area */}
        <div className="flex-1 flex flex-col h-screen overflow-hidden">
          {/* Top bar with controls */}
          <div className="px-4 py-3 border-b border-gray-200 bg-white flex items-center justify-between flex-shrink-0">
            <div className="flex items-center gap-3">
              {/* Mobile menu toggle */}
              <button
                className="md:hidden p-2 hover:bg-gray-100 rounded-md transition-colors"
                onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
                title="Toggle sidebar"
              >
                {isMobileSidebarOpen ? <X size={20} /> : <Menu size={20} />}
              </button>
              
              {/* OS Info Display */}
              <OSInfoDisplay />
            </div>
            
            {/* Terminal toggle button */}
            <button
              className="p-2 hover:bg-gray-100 text-gray-600 rounded-md transition-colors"
              onClick={() => setIsTerminalVisible(!isTerminalVisible)}
              title="Toggle Terminal"
            >
              <Terminal size={20} />
            </button>
          </div>
          
          {/* Chat Area - Takes all remaining space */}
          <div className="flex-1 overflow-hidden">
            <ChatArea />
          </div>
        </div>

        {/* Terminal Component */}
        <TerminalComponent 
          isVisible={isTerminalVisible} 
          onToggle={() => setIsTerminalVisible(!isTerminalVisible)} 
        />
      </div>
    </ChatProvider>
  );
}

export default App;

